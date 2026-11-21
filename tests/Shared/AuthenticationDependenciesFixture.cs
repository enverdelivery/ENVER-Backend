using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers;
using Microsoft.Extensions.Logging;
using Testcontainers.MongoDb;
using Testcontainers.Redis;
using Xunit;

namespace Enver.Testing;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AuthenticationDependenciesCollection : ICollectionFixture<AuthenticationDependenciesFixture>
{
    public const string Name = "authentication-dependencies";
}

public sealed class AuthenticationDependenciesFixture : IAsyncLifetime
{
    public const string MongoImage = "docker.io/library/mongo:8.0.34-noble@sha256:3c64937121b9437095921c4bb2569911084c05f7b88e4a263d62a8959d2b2399";
    public const string RedisImage = "docker.io/library/redis:8.2.11-bookworm@sha256:99ffb973380cc2f47bca06bd094f7f8c7138a0879a63cb8939896a22b7a65bba";
    public const string DynamoDbImage = "amazon/dynamodb-local:3.1.0@sha256:7ef4a2c45b58c2901e70a4f28e0953a422c2c631baaaf5e2c15e0805740c7752";
    public const string RyukImage = "testcontainers/ryuk:0.14.0@sha256:7c1a8a9a47c780ed0f983770a662f80deb115d95cce3e2daa3d12115b8cd28f0";
    public const string ReplicaSetName = "rs0";
    public const long RedisMaxMemoryBytes = 64L * 1024 * 1024;

    private readonly CancellationTokenSource _lifetimeCancellation = new(TimeSpan.FromMinutes(10));
    private readonly HashSet<string> _ownedRedisKeys = new(StringComparer.Ordinal);
    private readonly string _mongoPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24));
    private readonly string _jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private readonly string _rateLimitingKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private MongoDbContainer? _mongo;
    private RedisContainer? _redis;
    private IContainer? _dynamoDb;
    private bool _initialized;
    private bool _cleaned;

    public string RunId { get; } = Guid.NewGuid().ToString("N");
    public string EnvironmentName => $"T015_{RunId}";
    public string MongoDatabase => $"enver_t015_{RunId}";
    public string RedisNamespace => $"enver:t015_{RunId}";
    public string DynamoDbTableName => $"enver_t020_{RunId}";
    public string MongoConnectionString => Mongo.GetConnectionString();
    public string RedisConnectionString => Redis.GetConnectionString();
    // Diagnóstico de infraestructura: permite distinguir restart de recreación sin exponer credenciales.
    public string RedisContainerId => Redis.Id;

    private MongoDbContainer Mongo => _mongo ?? throw new InvalidOperationException("MongoDB de pruebas no inicializado.");
    private RedisContainer Redis => _redis ?? throw new InvalidOperationException("Redis de pruebas no inicializado.");
    private IContainer DynamoDb => _dynamoDb ?? throw new InvalidOperationException("DynamoDB Local de pruebas no inicializado.");

    public async Task InitializeAsync()
    {
        var token = _lifetimeCancellation.Token;
        var logger = new SensitiveTestcontainersLogger(ConsoleLogger.Instance,
            [_mongoPassword, _jwtKey, _rateLimitingKey, "fakeSecretAccessKey"]);
        _mongo = new MongoDbBuilder(MongoImage)
            .WithLogger(logger)
            .WithUsername("t015")
            .WithPassword(_mongoPassword)
            .WithReplicaSet(ReplicaSetName)
            .Build();
        _redis = new RedisBuilder(RedisImage)
            .WithLogger(logger)
            .WithCommand("redis-server", "--maxmemory-policy", "noeviction", "--maxmemory", "64mb")
            .Build();
        _dynamoDb = new ContainerBuilder(DynamoDbImage)
            .WithLogger(logger)
            .WithPortBinding(8000, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(8000))
            .Build();

        var startupPhase = "mongo-start";
        try
        {
            await Mongo.StartAsync(token);
            startupPhase = "redis-start";
            await Redis.StartAsync(token);
            startupPhase = "dynamodb-start";
            await DynamoDb.StartAsync(token);
            startupPhase = "dependency-verification";
            _initialized = true;
            await VerifyMongoAsync(token);
            await VerifyRedisAsync(token);
            await VerifyDynamoDbAsync(token);
            await CreateDynamoDbTableAsync(token);
        }
        catch (Exception exception)
        {
            // The module's cached discovery exception omits its exec result. Probe only
            // this failing container, before disposal; never retry startup or pass setup.
            var shellDiagnostic = exception is NotSupportedException &&
                exception.Message == "The mongosh or mongo binary could not be found."
                ? await ProbeMongoShellFailureAsync() : null;
            await ReportStartupFailureAsync(startupPhase);
            await DisposeContainersAsync();
            if (shellDiagnostic is not null)
            {
                // TRX ErrorInfo carries this whitelist. Arbitrary container logs and
                // environment values are deliberately excluded from CI artifacts.
                throw new InvalidOperationException(shellDiagnostic, exception);
            }
            throw;
        }
    }

    private async Task<string> ProbeMongoShellFailureAsync()
    {
        var metadata = $"Mongo shell discovery failed; context=fixture-container; containerState={Mongo.State}; hostPlatform={Environment.OSVersion.Platform}; hostArch={System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}; hostCwdIsRooted={Path.IsPathRooted(Environment.CurrentDirectory)}";
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            var discovery = await Mongo.ExecAsync(["/bin/sh", "-c", "command -v mongosh || command -v mongo"], timeout.Token);
            var resolved = discovery.Stdout.Trim() switch
            {
                "/usr/bin/mongosh" => "/usr/bin/mongosh",
                "/usr/bin/mongo" => "/usr/bin/mongo",
                _ => "UNEXPECTED_OR_EMPTY"
            };
            metadata += $"; postFailureDiscoveryExit={discovery.ExitCode?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "UNKNOWN"}; resolved={resolved}";
            var pathCheck = await Mongo.ExecAsync(["/bin/sh", "-c", "case \":$PATH:\" in *:/usr/bin:*) exit 0;; *) exit 1;; esac"], timeout.Token);
            metadata += $"; containerPathHasUsrBin={pathCheck.ExitCode switch { 0 => "True", 1 => "False", _ => "UNKNOWN" }}";
            var version = await Mongo.ExecAsync(["/usr/bin/mongosh", "--version"], timeout.Token);
            metadata += $"; explicitShellExit={version.ExitCode?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "UNKNOWN"}; pinnedVersion={version.Stdout.Trim() == "2.14.0"}";
        }
        catch
        {
            metadata += "; remainingMetadata=UNAVAILABLE";
        }
        return metadata + "; originalDiscoveryExit=NOT_EXPOSED_BY_MODULE; startup=FAILED";
    }

    private async Task ReportStartupFailureAsync(string phase)
    {
        // Solo categorías calculadas: nunca reenviar logs, argumentos, credenciales o excepciones originales.
        // Capturar antes del cleanup, que elimina la evidencia del contenedor fallido.
        if (_mongo is null || phase != "mongo-start") return;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            var (stdout, stderr) = await _mongo.GetLogsAsync(ct: timeout.Token);
            var combined = stdout + stderr;
            var state = _mongo.State;
            var exitCode = await _mongo.GetExitCodeAsync(timeout.Token).WaitAsync(timeout.Token);
            Console.WriteLine($"T015_SETUP_FAILURE PHASE={phase} RESOURCE=mongodb STATE={state} EXIT_CODE={exitCode} ADDRESS_IN_USE={combined.Contains("Address already in use", StringComparison.OrdinalIgnoreCase)} LISTEN_ERROR={combined.Contains("Error setting up listener", StringComparison.OrdinalIgnoreCase)} BOOTSTRAP_COMPLETE={combined.Contains("MongoDB init process complete", StringComparison.Ordinal)} PERMISSION_ERROR={combined.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)} NO_SPACE={combined.Contains("No space left on device", StringComparison.OrdinalIgnoreCase)}");
        }
        catch (Exception diagnosticFailure)
        {
            // El diagnóstico auxiliar no sustituye ni oculta el fallo original de setup.
            Console.WriteLine($"T015_SETUP_FAILURE PHASE={phase} RESOURCE=mongodb DIAGNOSTIC_UNAVAILABLE_TYPE={diagnosticFailure.GetType().FullName}");
        }
    }

    public IReadOnlyDictionary<string, string?> CreateConfiguration(IReadOnlyDictionary<string, string?>? overrides = null)
    {
        EnsureInitialized();
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["MongoDb:ConnectionString"] = MongoConnectionString,
            ["MongoDb:DatabaseName"] = MongoDatabase,
            ["MongoDb:UsersCollectionName"] = "users",
            ["MongoDb:OperationTimeoutMilliseconds"] = "500",
            ["MongoDb:ConnectionTimeoutMilliseconds"] = "500",
            ["MongoDb:WriteConcernCandidate"] = "majority",
            ["MongoDb:ReadConcernCandidate"] = "majority",
            ["MongoDb:ReadPreference"] = "primary",
            ["Redis:ConnectionString"] = RedisConnectionString,
            ["Redis:NamespacePrefix"] = RedisNamespace,
            ["Redis:RevocationPurpose"] = "auth:revocation",
            ["Redis:RateLimitingPurpose"] = "auth:ratelimit",
            ["Redis:OperationTimeoutMilliseconds"] = "250",
            ["Redis:ConnectionTimeoutMilliseconds"] = "250",
            ["Redis:RevocationOperationalMarginSeconds"] = "30",
            ["Redis:FailClosed"] = "true",
            ["Redis:RequiredRevocationEvictionPolicy"] = "noeviction",
            ["AuthenticationSecurityAuthority:TableName"] = DynamoDbTableName,
            ["AuthenticationSecurityAuthority:OperationTimeoutMilliseconds"] = "500",
            ["Jwt:Algorithm"] = "HS256",
            ["Jwt:Issuer"] = "enver-api",
            ["Jwt:Audience"] = "enver-mobile",
            ["Jwt:LifetimeSeconds"] = "900",
            ["Jwt:ClockSkewSeconds"] = "30",
            ["Jwt:ActiveKid"] = "t015-current",
            ["Jwt:Keys:0:Kid"] = "t015-current",
            ["Jwt:Keys:0:KeyBase64"] = _jwtKey,
            ["Authentication:InstitutionalDomain"] = "uttt.edu.mx",
            ["Authentication:FullNameMaxUnicodeScalars"] = "200",
            ["Authentication:EmailMaxAsciiBytes"] = "254",
            ["Authentication:EmailLocalPartMaxAsciiBytes"] = "64",
            ["Authentication:PasswordCreationMinUnicodeScalars"] = "12",
            ["Authentication:PasswordMaxUnicodeScalars"] = "128",
            ["Authentication:PasswordMaxUtf8Bytes"] = "512",
            ["Authentication:MaxRequestBodyBytes"] = "16384",
            ["Authentication:MaxJsonDepth"] = "4",
            ["Authentication:MaxJwtBytes"] = "4096",
            ["Authentication:RegisterBudgetMilliseconds"] = "1000",
            ["Authentication:LoginBudgetMilliseconds"] = "1000",
            ["Authentication:LogoutBudgetMilliseconds"] = "500",
            ["Authentication:HashingConcurrencyLimit"] = "1",
            ["RateLimiting:RegisterEmail:PermitLimit"] = "3",
            ["RateLimiting:RegisterEmail:WindowSeconds"] = "900",
            ["RateLimiting:RegisterOrigin:PermitLimit"] = "20",
            ["RateLimiting:RegisterOrigin:WindowSeconds"] = "900",
            ["RateLimiting:LoginOriginEmail:PermitLimit"] = "5",
            ["RateLimiting:LoginOriginEmail:WindowSeconds"] = "60",
            ["RateLimiting:LoginEmail:PermitLimit"] = "10",
            ["RateLimiting:LoginEmail:WindowSeconds"] = "60",
            ["RateLimiting:LoginOrigin:PermitLimit"] = "60",
            ["RateLimiting:LoginOrigin:WindowSeconds"] = "60",
            ["RateLimiting:LogoutAccount:PermitLimit"] = "30",
            ["RateLimiting:LogoutAccount:WindowSeconds"] = "60",
            ["RateLimiting:LogoutOrigin:PermitLimit"] = "300",
            ["RateLimiting:LogoutOrigin:WindowSeconds"] = "60",
            ["RateLimiting:PolicyRevision"] = "t015-synthetic",
            ["RateLimiting:PseudonymizationKeyBase64"] = _rateLimitingKey
        };

        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                values[key] = value;
            }
        }

        return values;
    }

    public async Task VerifyMongoAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var result = await Mongo.ExecScriptAsync($"var hello = db.adminCommand({{hello:1}}); if (hello.setName !== '{ReplicaSetName}' || hello.isWritablePrimary !== true) quit(71); var target = db.getSiblingDB('{MongoDatabase}'); var write = target.getCollection('t015_probes').updateOne({{_id:'fixture'}}, {{$set:{{value:'fixture'}}}}, {{upsert:true}}); if (write.acknowledged !== true || target.getCollection('t015_probes').findOne({{_id:'fixture'}}) === null) quit(72);", cancellationToken);
        EnsureSuccess(result.ExitCode, "La sonda MongoDB no confirmó replica set primario y lectura/escritura.");
        MarkDirty();
    }

    public async Task WriteMongoAsync(string marker, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var key = MongoKey(marker);
        var result = await Mongo.ExecScriptAsync($"var target = db.getSiblingDB('{MongoDatabase}'); var write = target.getCollection('t015_probes').updateOne({{_id:'{key}'}}, {{$set:{{value:'{key}'}}}}, {{upsert:true}}); quit(write.acknowledged === true ? 0 : 73);", cancellationToken);
        EnsureSuccess(result.ExitCode, "La escritura MongoDB sintética falló.");
        MarkDirty();
    }

    public async Task<bool> ContainsMongoAsync(string marker, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var key = MongoKey(marker);
        var result = await Mongo.ExecScriptAsync($"var target = db.getSiblingDB('{MongoDatabase}'); quit(target.getCollection('t015_probes').findOne({{_id:'{key}'}}) === null ? 1 : 0);", cancellationToken);
        return result.ExitCode == 0;
    }

    public async Task VerifyRedisAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var ping = await RedisCliAsync(cancellationToken, "PING");
        EnsureSuccess(ping.ExitCode, "Redis no respondió a PING.");
        if (!string.Equals(ping.Stdout.Trim(), "PONG", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Redis no devolvió PONG.");
        }

        var policy = await RedisCliAsync(cancellationToken, "CONFIG", "GET", "maxmemory-policy");
        EnsureSuccess(policy.ExitCode, "No se pudo consultar maxmemory-policy.");
        if (!string.Equals(ConfigValue(policy.Stdout, "maxmemory-policy"), "noeviction", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Redis no confirmó maxmemory-policy=noeviction.");
        }

        var memory = await RedisCliAsync(cancellationToken, "CONFIG", "GET", "maxmemory");
        EnsureSuccess(memory.ExitCode, "No se pudo consultar maxmemory.");
        if (!long.TryParse(ConfigValue(memory.Stdout, "maxmemory"), out var maxMemory) || maxMemory != RedisMaxMemoryBytes)
        {
            throw new InvalidOperationException("Redis no confirmó el maxmemory sintético esperado.");
        }

        await WriteRedisAsync("fixture", cancellationToken);
        if (!await ContainsRedisAsync("fixture", cancellationToken))
        {
            throw new InvalidOperationException("Redis no devolvió la clave sintética propia.");
        }
    }

    public async Task StopRedisAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await Redis.StopAsync(cancellationToken);
    }

    public async Task StartRedisAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await Redis.StartAsync(cancellationToken);
        await VerifyRedisAsync(cancellationToken);
    }

    /// <summary>Cliente exclusivo de DynamoDB Local con credenciales sintéticas no secretas.</summary>
    public IAmazonDynamoDB CreateDynamoDbClient()
    {
        EnsureInitialized();
        return new AmazonDynamoDBClient(new BasicAWSCredentials("fakeAccessKeyId", "fakeSecretAccessKey"), new AmazonDynamoDBConfig
        {
            ServiceURL = $"http://localhost:{DynamoDb.GetMappedPublicPort(8000)}",
            UseHttp = true,
            AuthenticationRegion = RegionEndpoint.USEast1.SystemName,
            MaxErrorRetry = 0
        });
    }

    public async Task DeleteAuthenticationSecurityAuthorityAsync(IAmazonDynamoDB client, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await client.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbTableName,
            Key = AuthenticationSecurityKey()
        }, cancellationToken);
    }

    /// <summary>Bootstrap explícito: ausencia no significa Open y el alta no sobrescribe una autoridad existente.</summary>
    public async Task BootstrapAuthenticationSecurityAuthorityAsync(IAmazonDynamoDB client, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await client.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbTableName,
            Item = new Dictionary<string, AttributeValue>(StringComparer.Ordinal)
            {
                ["Id"] = new() { S = "authentication" },
                ["State"] = new() { S = "Open" },
                ["Generation"] = new() { N = "0" },
                ["Revision"] = new() { N = "0" }
            },
            ConditionExpression = "attribute_not_exists(#id)",
            ExpressionAttributeNames = new Dictionary<string, string>(StringComparer.Ordinal) { ["#id"] = "Id" }
        }, cancellationToken);
    }

    public async Task WriteRedisAsync(string marker, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var key = RedisKey(marker);
        TrackRedisKey(key);
        var result = await RedisCliAsync(cancellationToken, "SET", key, marker);
        EnsureSuccess(result.ExitCode, "La escritura Redis sintética falló.");
        if (!string.Equals(result.Stdout.Trim(), "OK", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Redis no confirmó SET.");
        }

        MarkDirty();
    }

    public async Task<bool> ContainsRedisAsync(string marker, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var result = await RedisCliAsync(cancellationToken, "GET", RedisKey(marker));
        EnsureSuccess(result.ExitCode, "La lectura Redis sintética falló.");
        return string.Equals(result.Stdout.Trim(), marker, StringComparison.Ordinal);
    }

    public async Task CleanupOwnDataAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized || _cleaned)
        {
            return;
        }

        var mongoResult = await Mongo.ExecScriptAsync($"db.getSiblingDB('{MongoDatabase}').dropDatabase();", cancellationToken);
        EnsureSuccess(mongoResult.ExitCode, "No se pudo eliminar la base MongoDB propia.");

        if (_dynamoDb is not null)
        {
            using var dynamo = CreateDynamoDbClient();
            try
            {
                await dynamo.DeleteTableAsync(new DeleteTableRequest { TableName = DynamoDbTableName }, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                // Otra prueba de la misma colección ya limpió la tabla aislada.
            }
        }

        string[] keys;
        lock (_ownedRedisKeys)
        {
            keys = _ownedRedisKeys.ToArray();
        }

        foreach (var key in keys)
        {
            var redisResult = await RedisCliAsync(cancellationToken, "DEL", key);
            EnsureSuccess(redisResult.ExitCode, "No se pudo eliminar una clave Redis propia.");
        }

        _cleaned = true;
    }

    public async Task AssertOwnDataRemovedAsync(CancellationToken cancellationToken = default)
    {
        if (!_cleaned)
        {
            throw new InvalidOperationException("La limpieza propia no se ejecutó.");
        }

        var mongoResult = await Mongo.ExecScriptAsync($"var target = db.getSiblingDB('{MongoDatabase}'); quit(target.getCollectionNames().length === 0 ? 0 : 1);", cancellationToken);
        EnsureSuccess(mongoResult.ExitCode, "MongoDB conserva datos propios después de la limpieza.");

        string[] keys;
        lock (_ownedRedisKeys)
        {
            keys = _ownedRedisKeys.ToArray();
        }

        foreach (var key in keys)
        {
            var redisResult = await RedisCliAsync(cancellationToken, "EXISTS", key);
            EnsureSuccess(redisResult.ExitCode, "No se pudo comprobar una clave Redis propia.");
            if (!string.Equals(redisResult.Stdout.Trim(), "0", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Redis conserva una clave propia después de la limpieza.");
            }
        }
    }

    public bool ContainsSyntheticSecret(string value) => value.Contains(_mongoPassword, StringComparison.Ordinal) || value.Contains(_jwtKey, StringComparison.Ordinal) || value.Contains(_rateLimitingKey, StringComparison.Ordinal);

    public async Task DisposeAsync()
    {
        try
        {
            await CleanupOwnDataAsync();
        }
        finally
        {
            await DisposeContainersAsync();
            _lifetimeCancellation.Dispose();
        }
    }

    private async Task DisposeContainersAsync()
    {
        if (_redis is not null)
        {
            await _redis.DisposeAsync();
            _redis = null;
        }

        if (_dynamoDb is not null)
        {
            await _dynamoDb.DisposeAsync();
            _dynamoDb = null;
        }

        if (_mongo is not null)
        {
            await _mongo.DisposeAsync();
            _mongo = null;
        }
    }

    private async Task<DotNet.Testcontainers.Containers.ExecResult> RedisCliAsync(CancellationToken cancellationToken, params string[] arguments)
    {
        var command = new List<string> { "redis-cli", "--raw" };
        command.AddRange(arguments);
        return await Redis.ExecAsync(command, cancellationToken);
    }

    private async Task VerifyDynamoDbAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();
        using var client = CreateDynamoDbClient();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        _ = await client.ListTablesAsync(new ListTablesRequest(), timeout.Token);
    }

    private async Task CreateDynamoDbTableAsync(CancellationToken cancellationToken)
    {
        using var client = CreateDynamoDbClient();
        await client.CreateTableAsync(new CreateTableRequest
        {
            TableName = DynamoDbTableName,
            AttributeDefinitions = [new AttributeDefinition("Id", ScalarAttributeType.S)],
            KeySchema = [new KeySchemaElement("Id", KeyType.HASH)],
            ProvisionedThroughput = new ProvisionedThroughput(1, 1)
        }, cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        while (true)
        {
            var description = await client.DescribeTableAsync(new DescribeTableRequest { TableName = DynamoDbTableName }, timeout.Token);
            if (description.Table.TableStatus == TableStatus.ACTIVE)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50), timeout.Token);
        }
    }

    private static Dictionary<string, AttributeValue> AuthenticationSecurityKey() => new(StringComparer.Ordinal)
    {
        ["Id"] = new AttributeValue { S = "authentication" }
    };

    private static string ConfigValue(string output, string expectedName)
    {
        var values = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return values.Length == 2 && values[0] == expectedName ? values[1] : string.Empty;
    }

    private static void EnsureSuccess(long? exitCode, string message)
    {
        if (exitCode is not 0)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Las dependencias de pruebas aún no están disponibles.");
        }
    }

    private void TrackRedisKey(string key)
    {
        lock (_ownedRedisKeys)
        {
            _ownedRedisKeys.Add(key);
        }
    }

    private void MarkDirty() => _cleaned = false;

    private string MongoKey(string marker) => $"t015-{RunId}-{marker}";
    private string RedisKey(string marker) => $"{RedisNamespace}:probe:{marker}";
}

/// <summary>Frontera de diagnóstico exclusiva del harness; no cambia niveles ni el logger global.</summary>
internal sealed class SensitiveTestcontainersLogger : ILogger
{
    private const string Redacted = "[REDACTED]";
    private static readonly Regex ConnectionUri = new(
        @"\b(?:mongodb(?:\+srv)?|rediss?)://[^\s""'<>]+|\b[a-z][a-z0-9+.-]*://[^\s""'<>]*:[^\s""'<>]*@[^\s""'<>]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Regex CredentialPair = new(@"\b[^\s:@/]+:[^\s@/]+@",
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Regex SensitiveArgument = new(
        @"(?<label>(?:--(?:password|token|secret|jwt|hmac-key)|-p|[\w.:-]*(?:password|secret|token|jwt|hmacKey|keyBase64|connectionString))[""']?\s*(?:=|:|\s)\s*)(?:""[^""]*""|'[^']*'|[^\s,;}]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Regex Bearer = new(@"\bBearer\s+[^\s""',;}]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private readonly ILogger sink;
    private readonly string[] secrets;

    public SensitiveTestcontainersLogger(ILogger sink, IEnumerable<string> secrets)
    {
        this.sink = sink;
        this.secrets = secrets.Where(value => !string.IsNullOrEmpty(value))
            .SelectMany(value => new[] { value, Uri.EscapeDataString(value) }).Distinct()
            .OrderByDescending(value => value.Length).ToArray();
    }

    public bool IsEnabled(LogLevel logLevel) => sink.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var text = state is IEnumerable<KeyValuePair<string, object?>> values
            ? string.Join(" ", values.Select(pair => $"{pair.Key}={pair.Value}"))
            : state.ToString() ?? string.Empty;
        return sink.BeginScope(Redact(text));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (exception is not null)
        {
            // Conservar tipo, mensaje, inner exceptions y stack saneados; nunca reenviar el objeto original.
            message += Environment.NewLine + exception;
        }

        sink.Log(logLevel, eventId, Redact(message), null, static (text, _) => text);
    }

    private string Redact(string text)
    {
        foreach (var secret in secrets)
        {
            text = text.Replace(secret, Redacted, StringComparison.Ordinal);
        }

        text = ConnectionUri.Replace(text, Redacted);
        text = CredentialPair.Replace(text, Redacted);
        text = SensitiveArgument.Replace(text, "${label}" + Redacted);
        return Bearer.Replace(text, "Bearer " + Redacted);
    }
}

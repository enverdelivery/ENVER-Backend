using System.Globalization;
using System.Net;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Configuration;
using Enver.API.Http;
using Enver.API.Infrastructure.Aws;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using StackExchange.Redis;

namespace Enver.QuotaControl.Bootstrap;

/// <summary>
/// Composición real y acotada para initialize. No usa overrides de lifecycle ni
/// sustituye guards/authority. Solo se ejecuta cuando el operador invoca el CLI.
/// </summary>
internal sealed class RedisBootstrapBackend : IBootstrapBackend
{
    private readonly BootstrapArguments arguments;
    private readonly RedisConnections connections;
    private readonly IBootstrapKeyspaceReader keyspaceReader;
    private readonly RedisSecurityStateGuard localGuard;
    private readonly AuthenticationSecurityCoordinator globalState;
    private readonly RedisAuthenticationQuotaLifecycle lifecycle;
    private readonly RedisAuthenticationQuotaConfiguration configuration;
    private readonly IAmazonDynamoDB dynamo;
    private readonly RedisOptions redisOptions;
    private bool disposed;

    public RedisBootstrapBackend(BootstrapArguments arguments, RedisCredential credential)
    {
        this.arguments = arguments;
        ArgumentNullException.ThrowIfNull(credential);

        var connectionString = RedisConnectionStringFactory.Create(arguments.Endpoint, credential);
        redisOptions = new RedisOptions
        {
            ConnectionString = connectionString,
            NamespacePrefix = "enver:development",
            RevocationPurpose = "auth:revocation",
            RateLimitingPurpose = "auth:ratelimit",
            OperationTimeoutMilliseconds = BootstrapContract.OperationTimeoutMilliseconds,
            ConnectionTimeoutMilliseconds = BootstrapContract.ConnectionTimeoutMilliseconds,
            RevocationOperationalMarginSeconds = BootstrapContract.RevocationOperationalMarginSeconds,
            FailClosed = true,
            RequiredRevocationEvictionPolicy = "noeviction"
        };

        dynamo = CreateDynamo(arguments.Profile);
        connections = new RedisConnections(redisOptions);
        keyspaceReader = new RedisBootstrapKeyspaceReader(redisOptions, arguments.Endpoint);
        localGuard = new RedisSecurityStateGuard(connections, redisOptions);
        var authority = new DynamoDbAuthenticationSecurityAuthority(dynamo,
            new AuthenticationSecurityAuthorityOptions
            {
                TableName = BootstrapContract.AuthorityTable,
                OperationTimeoutMilliseconds = BootstrapContract.AuthorityTimeoutMilliseconds
            });
        globalState = new AuthenticationSecurityCoordinator(authority,
            new JwtOptions
            {
                LifetimeSeconds = BootstrapContract.JwtLifetimeSeconds,
                ClockSkewSeconds = BootstrapContract.JwtClockSkewSeconds
            },
            redisOptions);
        configuration = BootstrapContract.CreateConfiguration(arguments.Generation);
        lifecycle = new RedisAuthenticationQuotaLifecycle(connections, redisOptions, configuration, localGuard, globalState);
    }

    public ValueTask<PreflightResult> PreflightAsync(CancellationToken cancellationToken) =>
        PreflightAsync(arguments.TimeoutSeconds, localGuard, globalState,
            async token => (await connections.GetMultiplexerAsync(token).ConfigureAwait(false)).GetDatabase(0),
            keyspaceReader, configuration, Console.Error, cancellationToken);

    // Mismo flujo real; el punto interno permite probar fallos de E/S sin construir clientes ni conectarlos.
    internal static async ValueTask<PreflightResult> PreflightAsync(
        int timeoutSeconds,
        IAuthenticationSecurityState localGuard,
        IAuthenticationSecurityState globalState,
        Func<CancellationToken, ValueTask<IDatabase>> getDatabase,
        IBootstrapKeyspaceReader keyspaceReader,
        RedisAuthenticationQuotaConfiguration configuration,
        TextWriter diagnosticWriter,
        CancellationToken cancellationToken)
    {
        using var budget = new RequestBudget(timeoutSeconds * 1000, cancellationToken);
        var phase = "local-guard";
        try
        {
            var local = await localGuard.EvaluateAsync(budget).ConfigureAwait(false);
            if (!local.CanContinue)
            {
                return new PreflightResult(true, "unknown", "unknown", false, false);
            }

            phase = "authority-guard";
            var global = await globalState.EvaluateAsync(budget).ConfigureAwait(false);
            if (!global.CanContinue)
            {
                return new PreflightResult(true, "unknown", "unknown", true, false);
            }

            phase = "redis-database";
            var database = await getDatabase(budget.Token).ConfigureAwait(false);
            phase = "metadata-type";
            var metadataType = await TypeAsync(database, configuration.Keys.Metadata, budget.Token).ConfigureAwait(false);
            phase = "registry-type";
            var registryType = await TypeAsync(database, configuration.Keys.Registry, budget.Token).ConfigureAwait(false);
            phase = "keyspace";
            var (keyspaceTotal, _) = await KeyspaceAsync(keyspaceReader, budget.Token).ConfigureAwait(false);
            return new PreflightResult(true, metadataType, registryType, true, true, keyspaceTotal);
        }
        catch (Exception exception)
        {
            WritePreflightDiagnostic(diagnosticWriter, phase, exception, budget.Token.IsCancellationRequested);
            return PreflightResult.Unavailable();
        }
    }

    private static void WritePreflightDiagnostic(TextWriter writer, string phase, Exception exception, bool cancelled)
    {
        try
        {
            // Solo metadata de tipos y códigos de lista cerrada. Nunca Message, ToString, Data ni payloads.
            var innerTypes = new List<string>();
            var pending = new Queue<Exception>();
            EnqueueInner(exception);
            while (pending.Count > 0 && innerTypes.Count < 8)
            {
                var inner = pending.Dequeue();
                innerTypes.Add(inner.GetType().Name);
                EnqueueInner(inner);
            }
            if (pending.Count > 0) innerTypes.Add("truncated");

            var (category, code) = exception switch
            {
                AmazonServiceException aws => ("aws", aws.ErrorCode switch
                {
                    "ResourceNotFoundException" => "ResourceNotFoundException",
                    "AccessDeniedException" => "AccessDeniedException",
                    "UnrecognizedClientException" => "UnrecognizedClientException",
                    "ValidationException" => "ValidationException",
                    "ProvisionedThroughputExceededException" => "ProvisionedThroughputExceededException",
                    "ThrottlingException" => "ThrottlingException",
                    "RequestLimitExceeded" => "RequestLimitExceeded",
                    "ExpiredTokenException" => "ExpiredTokenException",
                    _ => "unclassified"
                }),
                RedisConnectionException redis => ("redis-connection", Enum.GetName(redis.FailureType) ?? "unclassified"),
                RedisTimeoutException redis => ("redis-timeout", Enum.GetName(redis.Commandstatus) ?? "unclassified"),
                RedisServerException redis => ("redis-server", Enum.GetName(redis.Kind) ?? "unclassified"),
                _ => ("none", "none")
            };
            writer.WriteLine($"PREFLIGHT DIAGNOSTIC: phase={phase} exception={exception.GetType().Name} " +
                $"inner={(innerTypes.Count == 0 ? "none" : string.Join(',', innerTypes))} " +
                $"cancelled={(cancelled ? "true" : "false")} category={category} code={code}");

            void EnqueueInner(Exception error)
            {
                if (error is AggregateException aggregate)
                {
                    foreach (var inner in aggregate.InnerExceptions.Take(8)) pending.Enqueue(inner);
                }
                else if (error.InnerException is { } inner) pending.Enqueue(inner);
            }
        }
        catch
        {
            // La observabilidad es best-effort: incluso un fallo del escritor conserva Unavailable().
        }
    }

    public async ValueTask<InitializeOutcome> InitializeAsync(CancellationToken cancellationToken)
    {
        using var budget = new RequestBudget(arguments.TimeoutSeconds * 1000, cancellationToken);
        try
        {
            var reply = await lifecycle.InitializeVerifiedNewEnvironmentAsync(budget).ConfigureAwait(false);
            return new InitializeOutcome(reply.Status, reply.Category, reply.IsSemantic);
        }
        catch
        {
            // Resultado desconocido u operación interrumpida: nunca inferir éxito ni repetir.
            return new InitializeOutcome("uncertain", "internal", false);
        }
    }

    public async ValueTask<ReadbackResult> ReadbackAsync(CancellationToken cancellationToken)
    {
        using var budget = new RequestBudget(arguments.TimeoutSeconds * 1000, cancellationToken);
        try
        {
            var multiplexer = await connections.GetMultiplexerAsync(budget.Token).ConfigureAwait(false);
            var database = multiplexer.GetDatabase(0);
            var metadataType = await TypeAsync(database, configuration.Keys.Metadata, budget.Token).ConfigureAwait(false);
            var registryType = await TypeAsync(database, configuration.Keys.Registry, budget.Token).ConfigureAwait(false);
            var metadataLength = await LongAsync(database, "HLEN", configuration.Keys.Metadata, budget.Token).ConfigureAwait(false);
            var metadataTtl = await LongAsync(database, "PTTL", configuration.Keys.Metadata, budget.Token).ConfigureAwait(false);
            var metadata = await HmgetAsync(database, configuration.Keys.Metadata, budget.Token).ConfigureAwait(false);
            var registryTtl = await LongAsync(database, "PTTL", configuration.Keys.Registry, budget.Token).ConfigureAwait(false);
            var (keyspaceTotal, db0Keys) = await KeyspaceAsync(keyspaceReader, budget.Token).ConfigureAwait(false);
            return ReadbackValidator.Validate(new QuotaReadbackInput(
                metadataType,
                registryType,
                metadataLength,
                metadataTtl,
                metadata,
                registryTtl,
                keyspaceTotal,
                db0Keys), arguments.Generation);
        }
        catch
        {
            return ReadbackResult.Failure("readback-unavailable");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        await connections.DisposeAsync().ConfigureAwait(false);
        dynamo.Dispose();
    }

    private async ValueTask<Dictionary<string, string?>> HmgetAsync(
        IDatabase database,
        RedisKey key,
        CancellationToken cancellationToken)
    {
        var fields = ReadbackValidator.ExpectedFields(arguments.Generation).Select(item => item.Field).ToArray();
        var args = new object[fields.Length + 1];
        args[0] = key.ToString();
        for (var index = 0; index < fields.Length; index++) args[index + 1] = fields[index];

        var result = await database.ExecuteAsync("HMGET", args, CommandFlags.DemandMaster)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        var values = (RedisResult[]?)result;
        var metadata = new Dictionary<string, string?>(StringComparer.Ordinal);
        if (values is null || values.Length != fields.Length) return metadata;
        for (var index = 0; index < fields.Length; index++)
        {
            metadata[fields[index]] = values[index].IsNull ? null : values[index].ToString();
        }

        return metadata;
    }

    private static async ValueTask<string> TypeAsync(IDatabase database, RedisKey key, CancellationToken cancellationToken)
    {
        var result = await database.ExecuteAsync("TYPE", new object[] { key.ToString() }, CommandFlags.DemandMaster)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        return result.ToString() ?? "unknown";
    }

    private static async ValueTask<long?> LongAsync(
        IDatabase database,
        string command,
        RedisKey key,
        CancellationToken cancellationToken)
    {
        var result = await database.ExecuteAsync(command, new object[] { key.ToString() }, CommandFlags.DemandMaster)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
        return long.TryParse(result.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    internal static async ValueTask<(long Total, long Db0)> KeyspaceAsync(
        IBootstrapKeyspaceReader keyspaceReader,
        CancellationToken cancellationToken)
    {
        var info = await keyspaceReader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return ParseKeyspace(info);
    }

    private static (long Total, long Db0) ParseKeyspace(string info)
    {
        long total = 0, db0 = 0;
        foreach (var rawLine in info.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            if (!line.StartsWith("db", StringComparison.Ordinal)) continue;
            var separator = line.IndexOf(':');
            if (separator <= 0) continue;
            var databaseName = line[..separator];
            long keys = 0;
            var found = false;
            foreach (var pair in line[(separator + 1)..].Split(','))
            {
                var equals = pair.IndexOf('=');
                if (equals <= 0 || !string.Equals(pair[..equals], "keys", StringComparison.Ordinal)) continue;
                if (!long.TryParse(pair[(equals + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out keys))
                {
                    return (0, 0);
                }

                found = true;
            }

            if (!found) continue;
            total += keys;
            if (string.Equals(databaseName, "db0", StringComparison.Ordinal)) db0 = keys;
        }

        return (total, db0);
    }

    private static IAmazonDynamoDB CreateDynamo(string? profile)
    {
        var configuration = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(BootstrapContract.Region),
            MaxErrorRetry = 0
        };

        if (profile is null) return new AmazonDynamoDBClient(configuration);
        var chain = new CredentialProfileStoreChain();
        if (!chain.TryGetAWSCredentials(profile, out var credentials))
        {
            throw new BootstrapValidationException("aws-profile-unavailable");
        }

        return new AmazonDynamoDBClient(credentials, configuration);
    }
}

internal static class RedisConnectionStringFactory
{
    public static string Create(string endpoint, RedisCredential credential)
    {
        if (credential.Username.Any(c => c is ',' or '"' or '\'') ||
            credential.Password.Any(c => c is ',' or '"' or '\'') ||
            !string.Equals(credential.Username, credential.Username.Trim(), StringComparison.Ordinal) ||
            !string.Equals(credential.Password, credential.Password.Trim(), StringComparison.Ordinal))
        {
            throw new BootstrapValidationException("redis-credential-not-representable");
        }

        var options = new ConfigurationOptions
        {
            AllowAdmin = false,
            AbortOnConnectFail = false,
            Ssl = true,
            SslHost = endpoint,
            DefaultDatabase = 0,
            User = credential.Username,
            Password = credential.Password,
            ConnectTimeout = BootstrapContract.ConnectionTimeoutMilliseconds,
            SyncTimeout = BootstrapContract.OperationTimeoutMilliseconds,
            AsyncTimeout = BootstrapContract.OperationTimeoutMilliseconds,
            BacklogPolicy = BacklogPolicy.FailFast,
            ReconnectRetryPolicy = new ExponentialRetry(BootstrapContract.ConnectionTimeoutMilliseconds),
            IncludeDetailInExceptions = false
        };
        options.EndPoints.Add(endpoint, BootstrapContract.Port);
        var text = options.ToString();
        var parsed = ConfigurationOptions.Parse(text);

        if (!parsed.Ssl ||
            parsed.DefaultDatabase != 0 ||
            !string.Equals(parsed.User, credential.Username, StringComparison.Ordinal) ||
            !string.Equals(parsed.Password, credential.Password, StringComparison.Ordinal) ||
            parsed.EndPoints.Count != 1 ||
            parsed.EndPoints[0] is not DnsEndPoint parsedEndpoint ||
            !string.Equals(parsedEndpoint.Host, endpoint, StringComparison.OrdinalIgnoreCase) ||
            parsedEndpoint.Port != BootstrapContract.Port ||
            (!string.IsNullOrEmpty(parsed.SslHost) &&
             !string.Equals(parsed.SslHost, endpoint, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BootstrapValidationException("redis-credential-roundtrip-mismatch");
        }

        return text;
    }
}

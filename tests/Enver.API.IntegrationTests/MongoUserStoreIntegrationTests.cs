using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoUserStoreIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task Confirmed_insert_is_readable_by_exact_email_and_id_with_effective_concerns() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine);
            await PrepareIndexAsync(scope, "confirmed-insert");
            using var budget = new RequestBudget(8000, default);
            try
            {
            scope.SetDiagnosticPhase("insert");
            var user = Mongo024IntegrationScope.User();
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(user, budget)).Status);
            scope.SetDiagnosticPhase("find/read/decode");
            var byEmail = await scope.Store.FindByNormalizedEmailAsync(user.NormalizedEmail, budget);
            var byId = await scope.Store.FindByIdAsync(user.Id, budget);
            Assert.Equal(UserReadStatus.Found, byEmail.Status);
            Assert.Equal(UserReadStatus.Found, byId.Status);
            Assert.Equal(user.Id, byEmail.User!.Id);
            Assert.Equal(user.Id, byId.User!.Id);
            Assert.Equal(UserReadStatus.NotFound, (await scope.Store.FindByIdAsync(Guid.NewGuid(), budget)).Status);
            Assert.Equal(UserReadStatus.NotFound, (await scope.Store.FindByNormalizedEmailAsync(user.NormalizedEmail.ToUpperInvariant(), budget)).Status);
            Assert.True(Assert.Single(scope.Writes));
            Assert.Equal(4, scope.Reads.Count);
            Assert.All(scope.Reads, valid => Assert.True(valid));
            Assert.Equal(ReadPreference.Primary, scope.Database.Settings.ReadPreference);
            Assert.False(scope.Client.Settings.RetryWrites);
            output.WriteLine("T024 INSERT=Confirmed LOOKUP=email+id NOT_FOUND=real CASE=binary WRITE=majority+j:true WTIMEOUT_MS=500 READ=majority PRIMARY=true RETRY_WRITES=false");
            }
            catch (Exception exception)
            {
                scope.RecordDiagnosticException("test-body", exception);
                throw;
            }
        });

    [Fact]
    public Task Real_email_duplicate_is_conflict_but_id_duplicate_is_not_email_conflict() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await PrepareIndexAsync(scope, "duplicates");
            using var budget = new RequestBudget(8000, default);
            var original = Mongo024IntegrationScope.User();
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, budget)).Status);
            var emailDuplicate = Mongo024IntegrationScope.User(email: original.NormalizedEmail);
            Assert.Equal(UserInsertStatus.Conflict, (await scope.Store.InsertAsync(emailDuplicate, budget)).Status);
            var idDuplicate = new User(original.Id, "Synthetic", "another@example.invalid", "synthetic-not-a-hash",
                UserStatus.Active, original.CreatedAt, original.UpdatedAt);
            Assert.Equal(UserInsertStatus.Unavailable, (await scope.Store.InsertAsync(idDuplicate, budget)).Status);
            Assert.Equal(1, await scope.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            Assert.Equal(3, scope.Writes.Count);
            output.WriteLine("T024 DUPLICATE_EMAIL=Conflict DUPLICATE_ID=Unavailable DOCUMENTS=1 INSERT_ATTEMPTS=3 RETRY=false COMPENSATION=false");
        });

    [Fact]
    public Task Disabled_is_returned_and_own_databases_and_cleanup_are_isolated() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var first = new Mongo024IntegrationScope(fixture);
            await using var second = new Mongo024IntegrationScope(fixture);
            await PrepareIndexAsync(first, "isolation-first");
            await PrepareIndexAsync(second, "isolation-second");
            using var budget = new RequestBudget(8000, default);
            var user = Mongo024IntegrationScope.User(UserStatus.Disabled);
            Assert.Equal(UserInsertStatus.Confirmed, (await first.Store.InsertAsync(user, budget)).Status);
            Assert.Equal(UserStatus.Disabled, (await first.Store.FindByIdAsync(user.Id, budget)).User!.Status);
            Assert.Equal(UserReadStatus.NotFound, (await second.Store.FindByIdAsync(user.Id, budget)).Status);
            Assert.Equal(UserInsertStatus.Confirmed, (await second.Store.InsertAsync(Mongo024IntegrationScope.User(), budget)).Status);
            await first.DropOwnDatabaseAsync();
            Assert.Equal(1, await second.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            output.WriteLine("T024 DISABLED=Found DATABASE_ISOLATION=true OWN_CLEANUP_ONLY=true OTHER_DATABASE_PRESERVED=true");
        });

    [Fact]
    public Task Read_error_on_real_boundary_is_unavailable_and_not_absence() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await PrepareIndexAsync(scope, "read-error");
            // Documento realmente incompatible: el modelo de 023 rechaza la lectura, no es una cuenta ausente.
            await scope.Database.GetCollection<BsonDocument>("users").InsertOneAsync(new BsonDocument
            {
                { "_id", new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard) },
                { "normalizedEmail", "malformed@example.invalid" }
            });
            using var budget = new RequestBudget(8000, default);
            Assert.Equal(UserReadStatus.Unavailable,
                (await scope.Store.FindByNormalizedEmailAsync("malformed@example.invalid", budget)).Status);
            Assert.Equal(1, await scope.Database.GetCollection<BsonDocument>("users").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
            output.WriteLine("T024 REAL_READ_DESERIALIZATION_FAILURE=Unavailable NOT_FOUND=false DATA_PRESERVED=true");
        });

    [Fact]
    public Task Real_insert_with_suppressed_ack_is_uncertain_without_retry_readback_or_delete() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await PrepareIndexAsync(scope, "suppressed-ack");
            var calls = 0;
            var reads = 0;
            var lostAckDatabase = Mongo024IntegrationProxy.Create<IMongoDatabase>((method, args) =>
            {
                if (method.Name == "RunCommandAsync")
                {
                    calls++;
                    return SuppressAckAsync((Task<BsonDocument>)Mongo024IntegrationProxy.Forward(method, scope.Database, args)!);
                }
                if (method.Name == "GetCollection") reads++;
                return Mongo024IntegrationProxy.Forward(method, scope.Database, args);
            });
            var adapter = new MongoUserStore(lostAckDatabase, scope.Options);
            var constructorCollections = reads;
            var user = Mongo024IntegrationScope.User();
            using var budget = new RequestBudget(8000, default);
            var result = await adapter.InsertAsync(user, budget);
            Assert.Equal(UserInsertStatus.Uncertain, result.Status);
            Assert.True(result.ResultUncertain);
            Assert.Equal(1, calls);
            Assert.Equal(constructorCollections, reads);
            Assert.Empty(scope.Reads);
            Assert.Equal(1, await scope.Users.CountDocumentsAsync(document => document.Id == user.Id));
            Assert.Equal(UserInsertStatus.Uncertain, result.Status); // observación no repara el resultado original.
            output.WriteLine("T024 REAL_WRITE=true ACK_DELIVERY_SUPPRESSED=adapter_boundary RESULT=Uncertain ATTEMPTS=1 AUTO_READBACK=false COMPENSATION=false TCP_CUT=false FAILOVER=false");
        });

    private async Task PrepareIndexAsync(Mongo024IntegrationScope scope, string host)
    {
        scope.SetDiagnosticPhase("setup/warm-up");
        await WarmUpAsync(scope.Client, host);
        // Precondición de infraestructura de 024, no operación del UserStore bajo prueba.
        // La creación/idempotencia dentro del presupuesto del initializer se acredita en 025.
        // Usar la definición productiva exacta, sin otra Options, cliente, timeout o retry.
        using var setupBudget = new RequestBudget(5000, default);
        scope.SetDiagnosticPhase("setup/create-index-precondition");
        await scope.Users.Indexes.CreateOneAsync(MongoUsersInitializer.CreateIndexDefinition(),
            cancellationToken: setupBudget.Token).WaitAsync(setupBudget.Token);
        scope.SetDiagnosticPhase("setup/verify-index-precondition");
        var initialized = await scope.Initializer.InitializeAsync(setupBudget);
        scope.RecordDiagnostic($"INDEX_PRECONDITION_RESULT={initialized}");
        Assert.Equal(MongoUsersInitializationStatus.Ready, initialized);
        output.WriteLine($"T024 INDEX_PRECONDITION_HOST={host} RESULT=Ready SETUP_ONLY=true SETUP_BUDGET_MS=5000 STORE_BUDGET_NOT_CREATED=true DEPENDENCY_TIMEOUT_MS=500 RETRY=false INDEX_DEFINITION=T025");
    }

    private async Task WarmUpAsync(IMongoClient client, string host)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var elapsed = Stopwatch.StartNew();
        var response = await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1), ReadPreference.Primary, timeout.Token).WaitAsync(timeout.Token);
        Assert.True(response.GetValue("ok", 0).ToDouble() == 1);
        Assert.Contains(client.Cluster.Description.Servers,
            server => server.Type == MongoDB.Driver.Core.Servers.ServerType.ReplicaSetPrimary);
        output.WriteLine($"T024 WARM_UP_HOST={host} OPERATION=ping PRIMARY_AVAILABLE=true ELAPSED_MS={elapsed.ElapsedMilliseconds} TEST_ONLY_TIMEOUT_MS=5000 OUTSIDE_REQUEST_BUDGET=true");
    }

    private static async Task<BsonDocument> SuppressAckAsync(Task<BsonDocument> realOperation)
    {
        var response = await realOperation;
        Assert.True(response.GetValue("n", 0).ToInt32() == 1 && !response.Contains("writeErrors") && !response.Contains("writeConcernError"));
        throw new TimeoutException("Entrega de confirmación suprimida por el decorador exclusivo de pruebas.");
    }
}

// Frontera test-only; no altera fixture, driver, comandos productivos ni transporte.
public class Mongo024IntegrationProxy : DispatchProxy
{
    public Func<MethodInfo, object?[], object?> Handler { get; set; } = null!;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args ?? []);
    public static T Create<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, Mongo024IntegrationProxy>();
        ((Mongo024IntegrationProxy)(object)proxy).Handler = handler;
        return proxy;
    }
    public static object? Forward(MethodInfo method, object target, object?[] args)
    {
        try { return method.Invoke(target, args); }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}

internal sealed class Mongo024IntegrationScope : IAsyncDisposable
{
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly Action<string>? diagnosticOutput;
    private readonly ConcurrentQueue<string> diagnosticLines = new();
    private readonly ConcurrentDictionary<int, string> diagnosticCommands = new();
    private string diagnosticPhase = "setup/construct-store";
    public Mongo024IntegrationScope(AuthenticationDependenciesFixture fixture, Action<string>? diagnosticOutput = null)
    {
        this.fixture = fixture;
        this.diagnosticOutput = diagnosticOutput;
        Options = new ConfigurationBuilder().AddInMemoryCollection(fixture.CreateConfiguration()).Build()
            .GetRequiredSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()!;
        Options.DatabaseName = $"{fixture.MongoDatabase}_t024025_{Guid.NewGuid().ToString("N")[..10]}";
        var databaseNameBytes = System.Text.Encoding.UTF8.GetByteCount(Options.DatabaseName);
        if (databaseNameBytes is 0 or > 63)
        {
            throw new InvalidOperationException($"Base propia de prueba: longitud UTF-8={databaseNameBytes}; límite=63 bytes; mínimo=1 byte.");
        }
        var settings = MongoClientFactory.CreateSettings(Options);
        settings.ClusterConfigurator = builder =>
        {
        builder.Subscribe<CommandStartedEvent>(command =>
        {
            if (command.DatabaseNamespace.DatabaseName != Options.DatabaseName) return;
            if (diagnosticOutput is not null)
            {
                var name = SafeCommand(command.CommandName);
                diagnosticCommands[command.RequestId] = name;
                RecordDiagnostic($"EVENT=CommandStarted COMMAND={name} PHASE={diagnosticPhase} DISPATCH=driver-started DATABASE=own-test-database COLLECTION={(name == "dropDatabase" ? "not-applicable" : "users")} DATABASE_UTF8_BYTES={System.Text.Encoding.UTF8.GetByteCount(Options.DatabaseName)}");
            }
            if (command.CommandName == "insert")
            {
                var concern = command.Command.GetValue("writeConcern", new BsonDocument()).AsBsonDocument;
                Writes.Enqueue(concern.GetValue("w", "").Equals(new BsonString("majority")) &&
                    concern.GetValue("j", false).Equals(BsonBoolean.True) && concern.GetValue("wtimeout", 0).ToInt64() == 500 &&
                    !command.Command.Contains("txnNumber"));
            }
            if (command.CommandName == "find")
            {
                var concern = command.Command.GetValue("readConcern", new BsonDocument()).AsBsonDocument;
                Reads.Enqueue(concern.GetValue("level", "").Equals(new BsonString("majority")) &&
                    command.Command.GetValue("collation", new BsonDocument()).AsBsonDocument.GetValue("locale", "").AsString == "simple");
            }
        });
        if (diagnosticOutput is not null)
        {
            builder.Subscribe<CommandFailedEvent>(command =>
            {
                if (!diagnosticCommands.TryRemove(command.RequestId, out var name)) return;
                RecordDiagnostic($"EVENT=CommandFailed COMMAND={name} PHASE={diagnosticPhase} DISPATCH=after-driver-started SERVER_RESPONDED={command.Failure is MongoCommandException}");
                RecordDiagnosticException(name, command.Failure);
                RecordDiagnosticTopology();
            });
            builder.Subscribe<CommandSucceededEvent>(command =>
            {
                if (diagnosticCommands.TryRemove(command.RequestId, out var name))
                    RecordDiagnostic($"EVENT=CommandSucceeded COMMAND={name} PHASE={diagnosticPhase} SERVER_RESPONDED=true");
            });
        }
        };
        Client = new MongoClient(settings);
        Database = Client.GetDatabase(Options.DatabaseName);
        Users = Database.GetCollection<UserDocument>("users");
        Store = new MongoUserStore(Database, Options);
        Initializer = new MongoUsersInitializer(Database, Options);
        RecordDiagnostic($"SETTINGS READ_CONCERN={(Database.Settings.ReadConcern.Equals(ReadConcern.Majority) ? "majority" : "other")} READ_PREFERENCE={(Database.Settings.ReadPreference.Equals(ReadPreference.Primary) ? "primary" : "other")} WRITE_CONCERN={(Database.Settings.WriteConcern.W.ToString() == "majority" ? "majority" : "other")} JOURNAL={Database.Settings.WriteConcern.Journal} WTIMEOUT_MS={Database.Settings.WriteConcern.WTimeout?.TotalMilliseconds} RETRY_WRITES={Client.Settings.RetryWrites} OPERATION_TIMEOUT_MS={Options.OperationTimeoutMilliseconds}");
    }

    public MongoDbOptions Options { get; }
    public MongoClient Client { get; }
    public IMongoDatabase Database { get; }
    public IMongoCollection<UserDocument> Users { get; }
    public MongoUserStore Store { get; }
    public MongoUsersInitializer Initializer { get; }
    public ConcurrentQueue<bool> Writes { get; } = new();
    public ConcurrentQueue<bool> Reads { get; } = new();
    public void SetDiagnosticPhase(string phase)
    {
        diagnosticPhase = phase;
        RecordDiagnostic($"PHASE={phase}");
    }
    public void RecordDiagnostic(string line)
    {
        if (diagnosticOutput is not null) diagnosticLines.Enqueue("T024_DIAG " + line);
    }
    public void RecordDiagnosticException(string operation, Exception exception)
    {
        if (diagnosticOutput is null) return;
        var command = exception as MongoCommandException;
        var labels = command is null ? "not-applicable" : string.Join(",", command.ErrorLabels.Select(SafeToken));
        RecordDiagnostic($"EXCEPTION OPERATION={operation} PHASE={diagnosticPhase} TYPE={exception.GetType().FullName} CODE={command?.Code.ToString() ?? "not-applicable"} CODE_NAME={(command is null ? "not-applicable" : SafeToken(command.CodeName))} ERROR_LABELS=[{labels}] INNER_TYPE={exception.InnerException?.GetType().FullName ?? "none"}");
    }
    private void RecordDiagnosticTopology()
    {
        foreach (var server in Client.Cluster.Description.Servers)
        {
            var local = server.EndPoint is System.Net.IPEndPoint ip && System.Net.IPAddress.IsLoopback(ip.Address);
            var setName = server.ReplicaSetConfig?.Name == "rs0" ? "rs0" : "other-or-unknown";
            RecordDiagnostic($"TOPOLOGY SERVER={(local ? "loopback-test-node" : "test-node-endpoint-withheld")} ROLE={server.Type} REPLICA_SET={setName} PRIMARY_AVAILABLE={Client.Cluster.Description.Servers.Any(candidate => candidate.Type == MongoDB.Driver.Core.Servers.ServerType.ReplicaSetPrimary)}");
        }
    }
    private static string SafeCommand(string name) => name is "listIndexes" or "createIndexes" or "insert" or "find" or "dropDatabase" ? name : "other";
    private static string SafeToken(string? value) => value is null ? "none" :
        System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Za-z][A-Za-z0-9_]{0,79}$") ? value : "noncanonical-withheld";
    public static User User(UserStatus status = UserStatus.Active, string email = "synthetic@example.invalid") =>
        new(Guid.NewGuid(), "Synthetic", email, "synthetic-not-a-hash", status, DateTime.UtcNow, DateTime.UtcNow);
    public async Task DropOwnDatabaseAsync()
    {
        SetDiagnosticPhase("cleanup/drop-database");
        Assert.StartsWith($"{fixture.MongoDatabase}_t024025_", Options.DatabaseName, StringComparison.Ordinal);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await Client.DropDatabaseAsync(Options.DatabaseName, timeout.Token);
    }
    public async ValueTask DisposeAsync()
    {
        try { await DropOwnDatabaseAsync(); }
        catch (Exception exception)
        {
            RecordDiagnosticException("cleanup", exception);
            throw;
        }
        finally
        {
            Client.Dispose();
            while (diagnosticLines.TryDequeue(out var line)) diagnosticOutput?.Invoke(line);
        }
    }
    public static async Task SafelyAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception exception)
        {
            // Conservar categoría de fallo sin publicar MongoException, keyValue, documentos o secretos.
            throw new XunitException($"T-AUTH-024/025: evidencia no acreditada; tipo={exception.GetType().Name}; detalles sensibles suprimidos.");
        }
    }
}

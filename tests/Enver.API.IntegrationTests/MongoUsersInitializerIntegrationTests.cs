using System.Collections.Concurrent;
using System.Diagnostics;
using Enver.API.Configuration;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoUsersInitializerIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task Creation_effective_inspection_second_initialization_and_duplicate_rejection() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine);
            await WarmUpAsync(scope.Client, "creation");
            var audit = new Mongo025Diagnostic("creation", output);
            var initializer = scope.Initializer;
            using var budget = new RequestBudget(8000, default);
            var phase = "first-initialization";
            var assertion = "Assert.Equal(Ready, firstInitialization)";
            try
            {
                scope.SetDiagnosticPhase(phase);
                var first = await initializer.InitializeAsync(budget);
                audit.Write($"FIRST_RESULT={first} REQUEST_CAUSE={budget.Cause} REQUEST_REMAINING_MS={budget.Remaining.TotalMilliseconds:F0}");
                audit.Write($"ASSERT={assertion} EXPECTED=Ready ACTUAL={first}");
                Assert.Equal(MongoUsersInitializationStatus.Ready, first);
                phase = "inspection-after-first";
                var before = await IndexesAsync(scope);
                audit.ReportIndexes(before);
                assertion = "Assert.Single(index-with-conventional-name)";
                var index = Assert.Single(before, index => index.GetValue("name", "") == "normalizedEmail_1");
                assertion = "Assert.True(IsCompatible(index))";
                audit.Write($"ASSERT={assertion} EXPECTED=True ACTUAL={MongoUsersInitializer.IsCompatible(index)}");
                Assert.True(MongoUsersInitializer.IsCompatible(index));
                assertion = "Assert.Equal(2, before.Count)";
                audit.Write($"ASSERT={assertion} EXPECTED=2 ACTUAL={before.Count}");
                Assert.Equal(2, before.Count); // Expectativa original, sin modificación.
                phase = "second-initialization";
                scope.SetDiagnosticPhase(phase);
                var second = await initializer.InitializeAsync(budget);
                assertion = "Assert.Equal(Ready, secondInitialization)";
                audit.Write($"SECOND_RESULT={second} ASSERT={assertion} EXPECTED=Ready ACTUAL={second}");
                Assert.Equal(MongoUsersInitializationStatus.Ready, second);
                var after = await IndexesAsync(scope);
                audit.ReportIndexes(after);
                assertion = "Assert.True(before.SequenceEqual(after))";
                audit.Write($"ASSERT={assertion} EXPECTED=True ACTUAL={before.SequenceEqual(after)}");
                Assert.True(before.SequenceEqual(after));
                phase = "duplicate-rejection";
                scope.SetDiagnosticPhase(phase);
                audit.Write("DUPLICATE_PHASE_REACHED=true");
                await scope.Users.InsertOneAsync(UserDocument.FromUser(Mongo024IntegrationScope.User()));
                var rejection = await Record.ExceptionAsync(() => scope.Users.InsertOneAsync(UserDocument.FromUser(Mongo024IntegrationScope.User())));
                if (rejection is not null) audit.ReportException("insert-duplicate", rejection);
                assertion = "Assert.True(MongoWriteException.DuplicateKey)";
                Assert.True(rejection is MongoWriteException error && error.WriteError.Category == ServerErrorCategory.DuplicateKey);
                var count = await scope.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty);
                assertion = "Assert.Equal(1, documentCount)";
                audit.Write($"ASSERT={assertion} EXPECTED=1 ACTUAL={count}");
                Assert.Equal(1, count);
                output.WriteLine("T025 INDEX_CREATED=true EFFECTIVE_OPTIONS_VERIFIED=true INDEX_COUNT=2 IDEMPOTENT=true DUPLICATE_REJECTED_BY_MONGODB=true");
            }
            catch (Exception exception)
            {
                audit.Write($"FAILING_PHASE={phase} FAILING_ASSERT={assertion} DUPLICATE_PHASE_REACHED={phase == "duplicate-rejection"}");
                audit.ReportException(phase, exception);
                await audit.InspectFinalAsync(scope.Database);
                throw;
            }
            finally { audit.Flush(); }
        });

    [Fact]
    public Task Equivalent_index_with_different_name_is_accepted_without_creation_or_rename() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await WarmUpAsync(scope.Client, "equivalent-index");
            var definition = MongoUsersInitializer.CreateIndexDefinition();
            definition.Options.Name = "equivalent_other_name";
            await scope.Users.Indexes.CreateOneAsync(definition);
            var before = await IndexesAsync(scope);
            using var budget = new RequestBudget(8000, default);
            Assert.Equal(MongoUsersInitializationStatus.Ready, await scope.Initializer.InitializeAsync(budget));
            Assert.True(before.SequenceEqual(await IndexesAsync(scope)));
            output.WriteLine("T025 EQUIVALENT_OTHER_NAME=Ready RENAME=false RECREATE=false");
        });

    [Fact]
    public Task Two_independent_logical_hosts_initialize_same_database_concurrently() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine);
            using var secondOwner = new MongoClientFactory(scope.Options);
            var auditA = new Mongo025Diagnostic("HostA", output);
            var auditB = new Mongo025Diagnostic("HostB", output);
            var observerA = new Mongo025ConcurrentObservation(scope.Database);
            var observerB = new Mongo025ConcurrentObservation(secondOwner.Client.GetDatabase(scope.Options.DatabaseName));
            var hostA = new MongoUsersInitializer(observerA.Database, scope.Options);
            var hostB = new MongoUsersInitializer(observerB.Database, scope.Options);
            Assert.NotSame(scope.Client, secondOwner.Client);
            await Task.WhenAll(WarmUpAsync(scope.Client, "HostA"), WarmUpAsync(secondOwner.Client, "HostB"));
            using var budgetA = new RequestBudget(8000, default);
            using var budgetB = new RequestBudget(8000, default);
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<Mongo025ConcurrentResult> StartAsync(MongoUsersInitializer initializer, RequestBudget budget,
                Mongo025ConcurrentObservation observer)
            {
                await gate.Task;
                var result = await initializer.InitializeAsync(budget);
                // Congelar evidencia al retornar, ANTES de cualquier inspección posterior.
                return observer.Snapshot(result);
            }
            var a = StartAsync(hostA, budgetA, observerA);
            var b = StartAsync(hostB, budgetB, observerB);
            gate.SetResult();
            var results = await Task.WhenAll(a, b);
            var originalResults = results.Select(result => result.Status).ToArray();
            auditA.Write($"INITIALIZER_RESULT={results[0].Status} DEPENDENCY_CANCELLED_AT_RETURN={results[0].DependencyCancelled} VERIFIED_INSIDE_BUDGET={results[0].VerifiedInsideBudget} CREATE_CALLS={results[0].CreateCalls}");
            auditB.Write($"INITIALIZER_RESULT={results[1].Status} DEPENDENCY_CANCELLED_AT_RETURN={results[1].DependencyCancelled} VERIFIED_INSIDE_BUDGET={results[1].VerifiedInsideBudget} CREATE_CALLS={results[1].CreateCalls}");
            try
            {
                Assert.All(results, (result, index) =>
                {
                    try
                    {
                        Assert.True(result.Status is MongoUsersInitializationStatus.Ready or MongoUsersInitializationStatus.Unavailable);
                        Assert.True(result.TokenObserved);
                        Assert.True(result.SameDependencyToken);
                        Assert.InRange(result.CreateCalls, 0, 1); // Sin retry de creación.
                        Assert.InRange(result.InspectionCalls, 1, 2); // Inspección y, si procede, reinspección acotada.
                        Assert.Equal(0, result.ForbiddenCalls);
                        if (result.Status == MongoUsersInitializationStatus.Ready)
                        {
                            Assert.True(result.VerifiedInsideBudget);
                        }
                        else
                        {
                            // No aceptar fallos arbitrarios como alternativa a Ready.
                            Assert.True(result.DependencyCancelled);
                            Assert.NotEqual(MongoUsersInitializationStatus.Ready, result.Status);
                        }
                    }
                    catch (Exception exception)
                    {
                        var audit = index == 0 ? auditA : auditB;
                        audit.Write($"ALL_INNER_INDEX={index} ASSERT=bounded-verification-or-fail-closed ACTUAL={result.Status}");
                        audit.ReportException("result-assertion", exception);
                        throw;
                    }
                });
                // Evidencia independiente del estado compartido: no reintenta inicializadores ni cambia snapshots.
                var indexes = await ObserveFinalIndexesAsync(scope);
                auditA.ReportIndexes(indexes);
                Assert.Equal(2, indexes.Count);
                var finalIndex = Assert.Single(indexes, MongoUsersInitializer.IsCompatible);
                Assert.True(finalIndex["key"].AsBsonDocument.Equals(new BsonDocument("normalizedEmail", 1)));
                Assert.True(finalIndex.GetValue("unique", false).Equals(BsonBoolean.True));
                Assert.True(finalIndex.GetValue("sparse", false).Equals(BsonBoolean.False));
                Assert.True(finalIndex.GetValue("hidden", false).Equals(BsonBoolean.False));
                Assert.False(finalIndex.Contains("partialFilterExpression"));
                Assert.False(finalIndex.Contains("expireAfterSeconds"));
                Assert.True(!finalIndex.Contains("collation") || finalIndex["collation"].AsBsonDocument.Equals(new BsonDocument("locale", "simple")));
                Assert.Equal(originalResults, results.Select(result => result.Status).ToArray());
                output.WriteLine($"T025 LOGICAL_HOSTS=2 INDEPENDENT_CLIENTS=true CONCURRENT_INITIALIZATION=true HOST_A={results[0].Status} HOST_B={results[1].Status} FINAL_COMPATIBLE_INDEXES=1 RETROACTIVE_READY=false APPLICATION_RETRY=false AUTOMATIC_REPAIR=false LOCAL_LOCK_AUTHORITY=false PHYSICAL_HOSTS=false");
            }
            catch (Exception exception)
            {
                auditA.ReportException("aggregate-assertion", exception);
                throw;
            }
            finally { auditA.Flush(); auditB.Flush(); }
        });

    [Theory]
    [InlineData("nonunique")]
    [InlineData("sparse")]
    [InlineData("partial")]
    [InlineData("collation")]
    [InlineData("hidden")]
    [InlineData("wrong-key")]
    [InlineData("descending")]
    public Task Incompatible_real_index_is_detected_without_changing_data_or_indexes(string scenario) =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine);
            try
            {
            scope.SetDiagnosticPhase("setup/warm-up");
            await WarmUpAsync(scope.Client, "incompatible-index");
            var options = new CreateIndexOptions<UserDocument>
            {
                Name = MongoUsersInitializer.IndexName, Unique = true, Collation = Collation.Simple
            };
            var keys = Builders<UserDocument>.IndexKeys.Ascending(user => user.NormalizedEmail);
            switch (scenario)
            {
                case "nonunique": options.Unique = false; break;
                case "sparse": options.Sparse = true; break;
                case "partial": options.PartialFilterExpression = Builders<UserDocument>.Filter.Eq(user => user.Status, Enver.API.Features.Authentication.Core.UserStatus.Active); break;
                case "collation": options.Collation = new Collation("en", strength: CollationStrength.Secondary); break;
                case "hidden": options.Hidden = true; break;
                case "wrong-key": keys = Builders<UserDocument>.IndexKeys.Ascending(user => user.FullName); break;
                case "descending": keys = Builders<UserDocument>.IndexKeys.Descending(user => user.NormalizedEmail); break;
            }
            scope.SetDiagnosticPhase("setup/create-incompatible-index");
            var definition = new CreateIndexModel<UserDocument>(keys, options);
            if (scenario is "hidden" or "collation")
                await CreateScenarioIndexAsync(scope, definition);
            else
                await scope.Users.Indexes.CreateOneAsync(definition);
            scope.SetDiagnosticPhase("setup/insert-synthetic-document");
            await scope.Users.InsertOneAsync(UserDocument.FromUser(Mongo024IntegrationScope.User()));
            scope.SetDiagnosticPhase("setup/capture-state");
            var indexesBefore = await IndexesAsync(scope);
            var documentsBefore = await scope.Database.GetCollection<BsonDocument>("users").Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            using var budget = new RequestBudget(8000, default);
            scope.SetDiagnosticPhase("initializer/inspect");
            Assert.Equal(MongoUsersInitializationStatus.Incompatible, await scope.Initializer.InitializeAsync(budget));
            scope.SetDiagnosticPhase("verify/preservation");
            Assert.True(indexesBefore.SequenceEqual(await IndexesAsync(scope)));
            var documentsAfter = await scope.Database.GetCollection<BsonDocument>("users").Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            Assert.True(documentsBefore.SequenceEqual(documentsAfter));
            output.WriteLine($"T025 INCOMPATIBILITY={scenario} RESULT=Incompatible INDEXES_PRESERVED=true DOCUMENTS_PRESERVED=true AUTO_REPAIR=false");
            }
            catch (Exception exception)
            {
                ReportScenarioFailure(scope, exception);
                throw;
            }
        });

    [Fact]
    public Task Existing_duplicate_data_blocks_readiness_and_is_not_reconciled() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            // Diagnóstico: no eliminar documentos/índices/base durante las observaciones.
            // La fixture conserva su disposición normal del contenedor efímero al finalizar.
            var scope = new Mongo024IntegrationScope(fixture, output.WriteLine);
            using var audit = new Mongo025DuplicateIndexAudit(scope.Options, output);
            try
            {
            scope.SetDiagnosticPhase("setup/warm-up");
            await WarmUpAsync(scope.Client, "existing-duplicates");
            await WarmUpAsync(audit.Client, "existing-duplicates-command-observer");
            scope.SetDiagnosticPhase("setup/seed-two-duplicates");
            await scope.Users.InsertManyAsync(new[] { UserDocument.FromUser(Mongo024IntegrationScope.User()), UserDocument.FromUser(Mongo024IntegrationScope.User()) });
            var before = await IndexesAsync(scope);
            var docs = await scope.Database.GetCollection<BsonDocument>("users").Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            audit.ReportIndexes("before", before);
            audit.Mark($"DOCUMENTS_BEFORE={docs.Count} EXACT_DUPLICATES={docs.Count == 2 && docs[0]["normalizedEmail"].Equals(docs[1]["normalizedEmail"])}");
            var observer = new Mongo025ConcurrentObservation(audit.Database, audit.ObserveDependency);
            var initializer = new MongoUsersInitializer(observer.Database, scope.Options);
            using var budget = new RequestBudget(8000, default, audit.Clock);
            scope.SetDiagnosticPhase("initializer/create-index-over-duplicates");
            audit.BeginInitializer();
            var result = await initializer.InitializeAsync(budget);
            var observed = observer.Snapshot(result);
            audit.Mark($"INITIALIZER_RETURN={result} DEPENDENCY_CANCELLED={observed.DependencyCancelled} REQUEST_REMAINING_MS={budget.Remaining.TotalMilliseconds:F3}");
            var rejectionCode = observer.CreateRejectionCode;
            var rejectionEvidence = rejectionCode == 0 ? "not-observed" : rejectionCode.ToString(System.Globalization.CultureInfo.InvariantCulture);
            scope.RecordDiagnostic($"DUPLICATES_ASSERT=conclusive-Incompatible-or-cancelled-Unavailable EXPECTED=Incompatible-or-budget-cancelled-Unavailable ACTUAL={result} DEPENDENCY_CANCELLED_AT_RETURN={observed.DependencyCancelled} CREATE_CALLS={observed.CreateCalls} SERVER_REJECTION_CODE_AT_RETURN={rejectionEvidence}");
            scope.SetDiagnosticPhase("verify/preservation");
            var indexesAfter = await IndexesAsync(scope);
            audit.ReportIndexes("immediately-after", indexesAfter);
            var docsAfter = await scope.Database.GetCollection<BsonDocument>("users").Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            audit.Mark($"DOCUMENTS_AFTER={docsAfter.Count} ORIGINAL_DOCUMENTS_PRESERVED={docs.SequenceEqual(docsAfter)}");
            await audit.ProbeUsabilityAsync("immediately-after");
            await audit.ObserveServerBuildAsync("immediately-after");
            // Una sola pausa finita de observación, sin retry de Initialize/CreateIndexes.
            using (var observation = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                await Task.Delay(300, observation.Token);
            var indexesLater = await IndexesAsync(scope);
            audit.ReportIndexes("after-300ms", indexesLater);
            audit.Mark($"INDEX_LIST_STABLE={indexesAfter.SequenceEqual(indexesLater)} ORIGINAL_INDEXES_PRESERVED_LATER={before.SequenceEqual(indexesLater)}");
            await audit.ProbeUsabilityAsync("after-300ms");
            await audit.ObserveServerBuildAsync("after-300ms");
            await audit.ProbeThirdDuplicateAsync(docs);
            audit.Mark($"FROZEN_INITIALIZER_RESULT={result} INITIALIZER_RETRY=false REPAIR=false DELETE=false");
            scope.RecordDiagnostic($"DUPLICATE_DOCUMENT_COUNT={docs.Count} INDEXES_BEFORE={before.Count} INDEXES_AFTER={indexesAfter.Count} COMPATIBLE_INDEXES={indexesAfter.Count(MongoUsersInitializer.IsCompatible)} INDEXES_PRESERVED={before.SequenceEqual(indexesAfter)} DOCUMENTS_PRESERVED={docs.SequenceEqual(docsAfter)}");
            // AUTH-009, plan §§6–7 y T-AUTH-025 exigen no admisión; no una clasificación
            // Incompatible obligatoria si vence el presupuesto antes de terminar la reinspección.
            Assert.True(result is MongoUsersInitializationStatus.Incompatible or MongoUsersInitializationStatus.Unavailable);
            if (result == MongoUsersInitializationStatus.Unavailable)
            {
                Assert.True(observed.TokenObserved);
                Assert.True(observed.SameDependencyToken);
                Assert.True(observed.DependencyCancelled);
                // La cancelación no inventa respuesta del servidor ni cambia la clasificación.
            }
            else
            {
                // En este escenario el índice falta inicialmente: la evidencia concluyente
                // observada es el rechazo estructurado de createIndexes por duplicados.
                Assert.Equal(11000, rejectionCode);
            }
            Assert.NotEqual(MongoUsersInitializationStatus.Ready, result);
            Assert.Equal(1, observed.CreateCalls);
            Assert.Equal(0, observed.ForbiddenCalls);
            Assert.Equal(2, docs.Count);
            Assert.Equal(0, indexesAfter.Count(MongoUsersInitializer.IsCompatible));
            Assert.True(before.SequenceEqual(indexesAfter));
            Assert.True(docs.SequenceEqual(docsAfter));
            Assert.Equal(observed.Status, result); // La inspección posterior solo acredita preservación.
            output.WriteLine($"T025 EXISTING_DUPLICATES=2 RESULT={result} FAIL_CLOSED=true SERVER_REJECTION_CODE={rejectionEvidence} DATA_PRESERVED=true INDEXES_PRESERVED=true RECONCILE=false RETROACTIVE_READY=false");
            }
            catch (Exception exception)
            {
                ReportScenarioFailure(scope, exception);
                throw;
            }
            finally
            {
                audit.Flush();
                scope.Client.Dispose();
            }
        });

    private static void ReportScenarioFailure(Mongo024IntegrationScope scope, Exception exception)
    {
        scope.RecordDiagnosticException("scenario-body", exception);
        foreach (var server in scope.Client.Cluster.Description.Servers)
            scope.RecordDiagnostic($"SERVER_ROLE={server.Type} SERVER_STATE={server.State} PRIMARY_AVAILABLE={scope.Client.Cluster.Description.Servers.Any(candidate => candidate.Type == MongoDB.Driver.Core.Servers.ServerType.ReplicaSetPrimary)} HEARTBEAT_EXCEPTION_TYPE={server.HeartbeatException?.GetType().FullName ?? "none"}");
    }

    private async Task CreateScenarioIndexAsync(Mongo024IntegrationScope scope, CreateIndexModel<UserDocument> definition)
    {
        // Solo prepara el estado incompatible. No se entrega este cliente al initializer.
        // Conserva concerns/wtimeout=500, RetryWrites=false y todos los plazos del cliente SUT.
        var settings = scope.Client.Settings.Clone();
        settings.SocketTimeout = TimeSpan.FromSeconds(5);
        using var setupClient = new MongoClient(settings);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await WarmUpAsync(setupClient, "incompatible-index-setup-only");
        await setupClient.GetDatabase(scope.Options.DatabaseName).GetCollection<UserDocument>("users")
            .Indexes.CreateOneAsync(definition, cancellationToken: timeout.Token).WaitAsync(timeout.Token);
        scope.RecordDiagnostic("SETUP_INDEX_CREATED=true SETUP_SOCKET_TIMEOUT_MS=5000 SUT_OPERATION_TIMEOUT_MS=500 SUT_SOCKET_TIMEOUT_MS=500 CONCERNS_CHANGED=false RETRY_WRITES=false INITIALIZER_CALLED=false");
    }

    private async Task<List<BsonDocument>> ObserveFinalIndexesAsync(Mongo024IntegrationScope scope)
    {
        // Una escritura cancelada puede seguir en curso en el servidor. Muestreo solo de metadatos,
        // con plazo propio: nunca vuelve a invocar InitializeAsync/CreateOneAsync ni cambia resultados.
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var samples = 0;
        try
        {
            while (true)
            {
                samples++;
                List<BsonDocument> indexes;
                try
                {
                    using var cursor = await scope.Users.Indexes.ListAsync(timeout.Token).WaitAsync(timeout.Token);
                    indexes = await cursor.ToListAsync(timeout.Token).WaitAsync(timeout.Token);
                }
                catch (MongoCommandException exception) when (exception.Code == 26)
                {
                    indexes = [];
                }
                if (indexes.Count == 2 && indexes.Count(MongoUsersInitializer.IsCompatible) == 1)
                {
                    output.WriteLine($"T025 POST_RACE_METADATA_SAMPLES={samples} OBSERVATION_TIMEOUT_MS=5000 INITIALIZER_RETRY=false MUTATION=false RETROACTIVE_READY=false");
                    return indexes;
                }
                await Task.Delay(25, timeout.Token);
            }
        }
        catch (OperationCanceledException)
        {
            throw new Xunit.Sdk.XunitException("Estado compartido no verificado dentro del plazo de observación test-only; no se alteran resultados individuales.");
        }
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
        output.WriteLine($"T025 WARM_UP_HOST={host} OPERATION=ping PRIMARY_AVAILABLE=true ELAPSED_MS={elapsed.ElapsedMilliseconds} TEST_ONLY_TIMEOUT_MS=5000 OUTSIDE_REQUEST_BUDGET=true");
    }

    private static async Task<List<BsonDocument>> IndexesAsync(Mongo024IntegrationScope scope)
    {
        using var cursor = await scope.Users.Indexes.ListAsync();
        return await cursor.ToListAsync();
    }
}

// Observación pasiva del token y del cursor real, sin modificar plazos, respuestas ni comandos.
internal sealed class Mongo025ConcurrentObservation
{
    private CancellationToken dependencyToken;
    private bool tokenObserved;
    private bool sameDependencyToken = true;
    private int inspections;
    private int creations;
    private int forbidden;
    private int verifiedInsideBudget;
    private int createRejectionCode;

    public Mongo025ConcurrentObservation(IMongoDatabase database, Action<CancellationToken>? observeDependency = null)
    {
        var users = database.GetCollection<UserDocument>("users");
        var indexes = Mongo024IntegrationProxy.Create<IMongoIndexManager<UserDocument>>((method, args) =>
        {
            if (method.Name is "ListAsync" or "CreateOneAsync")
            {
                var token = (CancellationToken)args[^1]!;
                observeDependency?.Invoke(token);
                if (tokenObserved) sameDependencyToken &= dependencyToken == token;
                else { dependencyToken = token; tokenObserved = token.CanBeCanceled; }
                if (method.Name == "CreateOneAsync")
                {
                    Interlocked.Increment(ref creations);
                    return ObserveCreateAsync((Task<string>)Mongo024IntegrationProxy.Forward(method, users.Indexes, args)!);
                }
                Interlocked.Increment(ref inspections);
                return ObserveListAsync((Task<IAsyncCursor<BsonDocument>>)Mongo024IntegrationProxy.Forward(method, users.Indexes, args)!, token);
            }
            Interlocked.Increment(ref forbidden);
            throw new InvalidOperationException("Operación de índice no autorizada en la prueba de concurrencia.");
        });
        var collection = Mongo024IntegrationProxy.Create<IMongoCollection<UserDocument>>((method, args) =>
            method.Name == "get_Indexes" ? indexes : Mongo024IntegrationProxy.Forward(method, users, args));
        Database = Mongo024IntegrationProxy.Create<IMongoDatabase>((method, args) =>
            method.Name == "GetCollection" ? collection : Mongo024IntegrationProxy.Forward(method, database, args));
    }

    public IMongoDatabase Database { get; }
    public int CreateRejectionCode => Volatile.Read(ref createRejectionCode);
    public Mongo025ConcurrentResult Snapshot(MongoUsersInitializationStatus status) => new(status, tokenObserved,
        sameDependencyToken, dependencyToken.IsCancellationRequested, Volatile.Read(ref verifiedInsideBudget) != 0,
        Volatile.Read(ref inspections), Volatile.Read(ref creations), Volatile.Read(ref forbidden));

    private async Task<IAsyncCursor<BsonDocument>> ObserveListAsync(Task<IAsyncCursor<BsonDocument>> operation, CancellationToken token) =>
        new VerificationCursor(await operation.ConfigureAwait(false), token, () => Interlocked.Exchange(ref verifiedInsideBudget, 1));

    private async Task<string> ObserveCreateAsync(Task<string> operation)
    {
        try { return await operation.ConfigureAwait(false); }
        catch (MongoCommandException exception)
        {
            Interlocked.Exchange(ref createRejectionCode, exception.Code);
            throw;
        }
    }

    private sealed class VerificationCursor(IAsyncCursor<BsonDocument> inner, CancellationToken token, Action verified) : IAsyncCursor<BsonDocument>
    {
        private readonly List<BsonDocument> definitions = [];
        public IEnumerable<BsonDocument> Current => inner.Current;
        private bool Observe(bool moved)
        {
            if (moved) definitions.AddRange(inner.Current);
            else if (!token.IsCancellationRequested && definitions.Count(MongoUsersInitializer.IsCompatible) == 1 &&
                definitions.Where(index => index.GetValue("key", new BsonDocument()).AsBsonDocument.Contains("normalizedEmail") ||
                    index.GetValue("name", "").Equals(new BsonString(MongoUsersInitializer.IndexName)))
                    .All(MongoUsersInitializer.IsCompatible))
            {
                verified();
            }
            return moved;
        }
        public bool MoveNext(CancellationToken cancellationToken = default) => Observe(inner.MoveNext(cancellationToken));
        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken = default) =>
            Observe(await inner.MoveNextAsync(cancellationToken).ConfigureAwait(false));
        public void Dispose() => inner.Dispose();
    }
}

// Diagnóstico test-only del build duplicado: mismo settings/concerns/timeout/retries del SUT.
// No imprime payloads, documentos, namespaces, keyValue ni mensajes originales de excepciones.
internal sealed class Mongo025DuplicateIndexAudit : IDisposable
{
    private readonly Mongo025Diagnostic log;
    private readonly Stopwatch elapsed = Stopwatch.StartNew();
    private readonly ConcurrentDictionary<int, long> createCommands = new();
    private CancellationToken dependencyToken;
    private CancellationTokenRegistration registration;
    private bool tokenObserved;
    private bool initializerStarted;
    private int timers;
    private long dependencyStarted;

    public Mongo025DuplicateIndexAudit(MongoDbOptions options, ITestOutputHelper output)
    {
        log = new Mongo025Diagnostic("duplicates-index-audit", output);
        Clock = new ObservedSystemClock(this);
        var settings = MongoClientFactory.CreateSettings(options);
        settings.ClusterConfigurator = builder =>
        {
            builder.Subscribe<CommandStartedEvent>(command =>
            {
                if (command.DatabaseNamespace.DatabaseName != options.DatabaseName || command.CommandName != "createIndexes") return;
                createCommands[command.RequestId] = Stopwatch.GetTimestamp();
                Mark("COMMAND=createIndexes EVENT=Started");
            });
            builder.Subscribe<CommandSucceededEvent>(command =>
            {
                if (createCommands.TryRemove(command.RequestId, out _))
                    Mark($"COMMAND=createIndexes EVENT=Succeeded DRIVER_DURATION_MS={command.Duration.TotalMilliseconds:F3}");
            });
            builder.Subscribe<CommandFailedEvent>(command =>
            {
                if (!createCommands.TryRemove(command.RequestId, out _)) return;
                Mark($"COMMAND=createIndexes EVENT=Failed DRIVER_DURATION_MS={command.Duration.TotalMilliseconds:F3}");
                log.ReportException("createIndexes", command.Failure);
            });
        };
        Client = new MongoClient(settings);
        Database = Client.GetDatabase(options.DatabaseName);
        Mark($"SETTINGS OPERATION_TIMEOUT_MS={options.OperationTimeoutMilliseconds} SOCKET_TIMEOUT_MS={settings.SocketTimeout.TotalMilliseconds:F0} RETRY_WRITES={settings.RetryWrites}");
    }

    public MongoClient Client { get; }
    public IMongoDatabase Database { get; }
    public TimeProvider Clock { get; }

    public void BeginInitializer()
    {
        initializerStarted = true;
        Mark("INITIALIZER_CALL=true");
    }

    public void Mark(string text)
    {
        var dependencyElapsed = dependencyStarted == 0 ? -1 : Stopwatch.GetElapsedTime(dependencyStarted).TotalMilliseconds;
        log.Write($"AUDIT_MS={elapsed.Elapsed.TotalMilliseconds:F3} DEPENDENCY_ELAPSED_MS={dependencyElapsed:F3} DEPENDENCY_TOKEN_CANCELLED={dependencyToken.IsCancellationRequested} {text}");
    }

    public void ObserveDependency(CancellationToken token)
    {
        if (tokenObserved) return;
        tokenObserved = true;
        dependencyToken = token;
        Mark("DEPENDENCY_TOKEN_FIRST_OBSERVED=true");
        registration = token.Register(() => Mark("DEPENDENCY_TOKEN_CANCELLED_CALLBACK=true"));
    }

    public void ReportIndexes(string phase, IReadOnlyList<BsonDocument> indexes)
    {
        Mark($"INDEX_SNAPSHOT={phase}");
        log.ReportIndexes(indexes);
        foreach (var index in indexes)
        {
            var ttl = index.GetValue("expireAfterSeconds", BsonNull.Value);
            Mark($"INDEX_SNAPSHOT={phase} INDEX_IS_NORMALIZED_EMAIL_1={index.GetValue("name", "").Equals(new BsonString(MongoUsersInitializer.IndexName))} TTL_SECONDS={(ttl.IsNumeric ? ttl.ToDouble().ToString(System.Globalization.CultureInfo.InvariantCulture) : "absent-or-nonnumeric")} METADATA_ONLY_NOT_PROOF_OF_READY=true");
        }
    }

    public async Task ProbeUsabilityAsync(string phase)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            var response = await Database.RunCommandAsync<BsonDocument>(new BsonDocument
            {
                { "explain", new BsonDocument
                    {
                        { "find", "users" },
                        { "filter", new BsonDocument("normalizedEmail", "synthetic@example.invalid") },
                        { "hint", MongoUsersInitializer.IndexName }
                    }
                },
                { "verbosity", "queryPlanner" }
            }, ReadPreference.Primary, timeout.Token).WaitAsync(timeout.Token);
            Mark($"USABILITY_PHASE={phase} HINT_ACCEPTED=true NAMED_INDEX_IN_PLAN={ContainsNamedIndex(response)}");
        }
        catch (Exception exception)
        {
            Mark($"USABILITY_PHASE={phase} HINT_ACCEPTED=false");
            log.ReportException("explain-hinted-index", exception);
        }
    }

    public async Task ObserveServerBuildAsync(string phase)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            var response = await Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument
            {
                { "aggregate", 1 },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$currentOp", new BsonDocument { { "allUsers", true }, { "localOps", true } }),
                        new BsonDocument("$match", new BsonDocument
                            {
                                { "command.createIndexes", "users" },
                                { "command.$db", Database.DatabaseNamespace.DatabaseName }
                            }),
                        new BsonDocument("$project", new BsonDocument { { "_id", 0 }, { "active", 1 } })
                    }
                },
                { "cursor", new BsonDocument("batchSize", 100) }
            }, ReadPreference.Primary, timeout.Token).WaitAsync(timeout.Token);
            var activeBuilds = response["cursor"]["firstBatch"].AsBsonArray.Count;
            Mark($"SERVER_BUILD_PHASE={phase} MATCHING_CREATE_INDEX_OPERATIONS={activeBuilds} CURSOR_COMPLETE={response["cursor"]["id"].ToInt64() == 0}");
        }
        catch (Exception exception) { log.ReportException("currentOp-own-index-build", exception); }
    }

    public async Task ProbeThirdDuplicateAsync(IReadOnlyList<BsonDocument> originals)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await Database.GetCollection<UserDocument>("users").InsertOneAsync(
                UserDocument.FromUser(Mongo024IntegrationScope.User()), cancellationToken: timeout.Token).WaitAsync(timeout.Token);
            Mark("THIRD_DUPLICATE=Accepted UNIQUE_ENFORCEMENT_OBSERVED=false");
        }
        catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey && exception.WriteConcernError is null)
        {
            Mark("THIRD_DUPLICATE=Rejected UNIQUE_ENFORCEMENT_OBSERVED=true");
            log.ReportException("third-duplicate-insert", exception);
        }
        catch (Exception exception)
        {
            Mark("THIRD_DUPLICATE=Unconfirmed UNIQUE_ENFORCEMENT_OBSERVED=unknown");
            log.ReportException("third-duplicate-insert", exception);
        }
        var documents = await Database.GetCollection<BsonDocument>("users")
            .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(timeout.Token).WaitAsync(timeout.Token);
        var preserved = originals.All(original => documents.Any(document => document.Equals(original)));
        var equalEmailCount = documents.Count(document => document.GetValue("normalizedEmail", BsonNull.Value).Equals(originals[0]["normalizedEmail"]));
        Mark($"DOCUMENTS_AFTER_PROBE={documents.Count} SAME_EMAIL_COUNT={equalEmailCount} ORIGINAL_TWO_PRESERVED={preserved} DELETE=false");
    }

    private static bool ContainsNamedIndex(BsonValue value)
    {
        if (value.IsBsonArray) return value.AsBsonArray.Any(ContainsNamedIndex);
        if (!value.IsBsonDocument) return false;
        var document = value.AsBsonDocument;
        return document.GetValue("indexName", BsonNull.Value).Equals(new BsonString(MongoUsersInitializer.IndexName)) ||
            document.Elements.Any(element => ContainsNamedIndex(element.Value));
    }

    public void Flush() => log.Flush();
    public void Dispose()
    {
        registration.Dispose();
        Client.Dispose();
    }

    private sealed class ObservedSystemClock(Mongo025DuplicateIndexAudit owner) : TimeProvider
    {
        public override long TimestampFrequency => TimeProvider.System.TimestampFrequency;
        public override DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
        public override TimeZoneInfo LocalTimeZone => TimeProvider.System.LocalTimeZone;
        public override long GetTimestamp()
        {
            var timestamp = TimeProvider.System.GetTimestamp();
            if (owner.initializerStarted) Interlocked.CompareExchange(ref owner.dependencyStarted, timestamp, 0);
            return timestamp;
        }
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var dependency = Interlocked.Increment(ref owner.timers) == 2;
            owner.Mark($"TIMER={(dependency ? "dependency" : "request")} ASSIGNED_MS={dueTime.TotalMilliseconds:F3}");
            return TimeProvider.System.CreateTimer(value =>
            {
                owner.Mark($"TIMER={(dependency ? "dependency" : "request")} CALLBACK=begin");
                try { callback(value); }
                finally { owner.Mark($"TIMER={(dependency ? "dependency" : "request")} CALLBACK=end"); }
            }, state, dueTime, period);
        }
    }
}

internal sealed record Mongo025ConcurrentResult(MongoUsersInitializationStatus Status, bool TokenObserved,
    bool SameDependencyToken, bool DependencyCancelled, bool VerifiedInsideBudget, int InspectionCalls, int CreateCalls, int ForbiddenCalls);

// Diagnóstico posterior a la operación: no envuelve las APIs dentro del presupuesto bajo prueba.
internal sealed class Mongo025Diagnostic
{
    private readonly string host;
    private readonly ITestOutputHelper output;
    private readonly ConcurrentQueue<string> lines = new();
    private readonly Stopwatch elapsed = Stopwatch.StartNew();

    public Mongo025Diagnostic(string host, ITestOutputHelper output)
    {
        this.host = host;
        this.output = output;
    }

    public void Write(string text) => lines.Enqueue($"T025_DIAG HOST={host} ELAPSED_MS={elapsed.ElapsedMilliseconds} {text}");

    public void ReportException(string operation, Exception exception)
    {
        var command = exception as MongoCommandException;
        var write = exception as MongoWriteException;
        var code = command?.Code ?? write?.WriteError?.Code;
        var labels = exception is MongoException mongo ? string.Join(",", mongo.ErrorLabels.Select(SafeToken)) : "not-applicable";
        Write($"EXCEPTION OPERATION={operation} TYPE={exception.GetType().FullName} INNER_TYPE={exception.InnerException?.GetType().FullName ?? "none"} CODE={code?.ToString() ?? "not-applicable"} CODE_NAME={(command is null ? "not-applicable" : SafeToken(command.CodeName))} ERROR_LABELS=[{labels}] SERVER_RESPONDED={(command is not null || write is not null ? "true" : "not-established")}");
    }

    public async Task InspectFinalAsync(IMongoDatabase database)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var cursor = await database.GetCollection<UserDocument>("users").Indexes.ListAsync(timeout.Token);
            ReportIndexes(await cursor.ToListAsync(timeout.Token));
        }
        catch (Exception exception) { ReportException("diagnostic-final-inspection", exception); }
    }

    public void ReportIndexes(IReadOnlyList<BsonDocument> indexes)
    {
        Write($"INDEX_COUNT={indexes.Count} COMPATIBLE_COUNT={indexes.Count(MongoUsersInitializer.IsCompatible)}");
        foreach (var index in indexes)
        {
            var name = index.GetValue("name", "").AsString;
            var key = index.GetValue("key", new BsonDocument()).AsBsonDocument;
            var keyText = string.Join(",", key.Select(element =>
                $"{(element.Name is "normalizedEmail" or "_id" ? element.Name : "other-field")}:{(element.Value.IsNumeric ? element.Value.ToDouble().ToString(System.Globalization.CultureInfo.InvariantCulture) : "non-numeric")}"));
            var collation = index.GetValue("collation", BsonNull.Value);
            var simple = collation.IsBsonNull || collation.IsBsonDocument && collation.AsBsonDocument.GetValue("locale", "").AsString == "simple";
            Write($"INDEX_NAME={(name is "normalizedEmail_1" or "_id_" or "equivalent_other_name" ? name : "other-name")} KEY={{{keyText}}} UNIQUE={BoolOption(index, "unique")} COLLATION_PRESENT={index.Contains("collation")} EFFECTIVE_COLLATION={(simple ? "simple" : "other")} COLLATION_FIELD_COUNT={(collation.IsBsonDocument ? collation.AsBsonDocument.ElementCount : 0)} SPARSE={BoolOption(index, "sparse")} PARTIAL_PRESENT={index.Contains("partialFilterExpression")} HIDDEN={BoolOption(index, "hidden")} TTL_PRESENT={index.Contains("expireAfterSeconds")} BUILD_UUID_PRESENT={index.Contains("buildUUID")} COMPATIBLE={MongoUsersInitializer.IsCompatible(index)}");
        }
    }

    private static string BoolOption(BsonDocument index, string name) => !index.TryGetValue(name, out var value)
        ? "absent/effective-false" : value.IsBoolean ? $"present/{value.AsBoolean}" : "present/nonboolean";
    private static string SafeToken(string? value) => value is not null &&
        System.Text.RegularExpressions.Regex.IsMatch(value, "^[A-Za-z][A-Za-z0-9_]{0,79}$") ? value : "withheld";
    public void Flush()
    {
        Write("DIAGNOSTIC_LOGGING=post-operation");
        while (lines.TryDequeue(out var line)) output.WriteLine(line);
    }
}

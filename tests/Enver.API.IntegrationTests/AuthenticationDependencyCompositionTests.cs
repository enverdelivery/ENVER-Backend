using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.API.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationDependencyCompositionTests(AuthenticationDependenciesFixture fixture)
{
    private Dictionary<string, string?> Configuration() => new()
    {
        ["MongoDb:DatabaseName"] = fixture.MongoDatabase + "_t057_" + Guid.NewGuid().ToString("N")[..8],
        ["Authentication:RegisterBudgetMilliseconds"] = "8000", ["Authentication:HashingConcurrencyLimit"] = "2"
    };

    [Fact]
    public async Task Real_host_resolves_singletons_real_adapters_and_active_JWT_without_bootstrapping()
    {
        await using var authority = await OwnedCompositionAuthority.CreateAsync(fixture, open: false);
        var config = Configuration();
        config["AuthenticationSecurityAuthority:TableName"] = authority.TableName;
        await using var factory = new AuthenticationApiFactory(fixture, config);
        using var client = factory.CreateClient();
        var services = factory.Services;
        using var scope = services.CreateScope();
        Assert.Same(services.GetRequiredService<IMongoClient>(), scope.ServiceProvider.GetRequiredService<IMongoClient>());
        Assert.Same(services.GetRequiredService<RedisConnections>(), scope.ServiceProvider.GetRequiredService<RedisConnections>());
        Assert.IsType<MongoUserStore>(services.GetRequiredService<IUserStore>());
        Assert.IsType<RedisAuthenticationQuotaStore>(services.GetRequiredService<IAuthenticationQuotaStore>());
        Assert.IsType<HmacAuthenticationScopePseudonymizer>(services.GetRequiredService<IAuthenticationScopePseudonymizer>());
        Assert.IsType<PasswordService>(services.GetRequiredService<IPasswordService>());
        Assert.NotNull(services.GetRequiredService<AuthenticationInputValidator>());
        Assert.Equal(2, services.GetRequiredService<HashingConcurrencyGate>().Limit);
        Assert.True(services.GetRequiredService<DummyPasswordHash>().IsInitialized);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        await AssertActiveAuthenticationPipelineAsync(services, client);
        using var register = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.PostAsync("/api/v1/auth/register", register)).StatusCode);
        using var budget = new RequestBudget(8000, default);
        Assert.False((await services.GetRequiredService<IAuthenticationSecurityState>().EvaluateAsync(budget)).CanContinue);
        await services.GetRequiredService<IMongoClient>().DropDatabaseAsync(config["MongoDb:DatabaseName"]!);
    }

    internal static async Task AssertActiveAuthenticationPipelineAsync(IServiceProvider services, HttpClient client)
    {
        var endpoints = services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>().Endpoints
            .OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>().ToArray();
        var login = Assert.Single(endpoints, e => e.RoutePattern.RawText == "api/v1/auth/login");
        Assert.NotNull(login.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>());
        Assert.Equal(AuthenticationOperation.Login, login.Metadata.GetMetadata<AuthenticationRequestMetadata>()!.Operation);
        Assert.Equal(new[] { "POST" }, login.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()!.HttpMethods);
        Assert.NotNull(services.GetRequiredService<ILoginControls>());
        Assert.NotNull(services.GetRequiredService<ILoginTokenIssuer>());
        var logout = Assert.Single(endpoints, e => e.RoutePattern.RawText == "api/v1/auth/logout");
        Assert.Equal(AuthenticationPolicies.Logout, Assert.Single(logout.Metadata.GetOrderedMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>()).Policy);
        Assert.Equal(new[] { "POST" }, logout.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()!.HttpMethods);
        Assert.Equal(AuthenticationOperation.Logout, logout.Metadata.GetMetadata<AuthenticationRequestMetadata>()!.Operation);
        var schemes = services.GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        Assert.Equal(AuthenticationPolicies.Scheme, Assert.Single(await schemes.GetAllSchemesAsync()).Name);
        var policies = services.GetRequiredService<IOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions>>().Value;
        Assert.NotNull(policies.FallbackPolicy); Assert.Same(policies.DefaultPolicy, policies.FallbackPolicy);
        Assert.Null(client.DefaultRequestHeaders.Authorization);
        using var missingLogin = await client.PostAsync("/api/v1/auth/login", null);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, missingLogin.StatusCode);
        using var missingBearer = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, missingBearer.StatusCode);
        Assert.Equal("Bearer", Assert.Single(missingBearer.Headers.GetValues("WWW-Authenticate")));
        Assert.Equal("no-store", missingBearer.Headers.CacheControl?.ToString());
        Assert.Equal("application/problem+json", missingBearer.Content.Headers.ContentType?.MediaType);
        using var problem = System.Text.Json.JsonDocument.Parse(await missingBearer.Content.ReadAsStringAsync());
        Assert.Equal(401, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("authentication_failed", problem.RootElement.GetProperty("code").GetString());
        Assert.Equal("No fue posible autenticar la solicitud.", problem.RootElement.GetProperty("detail").GetString());
        Assert.Equal(missingBearer.Headers.GetValues("X-Correlation-ID").Single(), problem.RootElement.GetProperty("traceId").GetString());
    }

    [Theory]
    [InlineData("AuthenticationComposition:QuotaGeneration", "")]
    [InlineData("AuthenticationComposition:QuotaIdentityReference", "")]
    [InlineData("RateLimiting:MaxActivePartitions", "4999")]
    public void Missing_control_configuration_is_not_replaced_with_defaults(string key, string value)
    {
        using var factory = new AuthenticationApiFactory(fixture, new Dictionary<string, string?> { [key] = value });
        var error = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.True(error is OptionsValidationException || error is AggregateException aggregate &&
            aggregate.InnerExceptions.All(e => e is OptionsValidationException));
        Assert.Contains("AuthenticationComposition:", error.ToString());
        Assert.False(fixture.ContainsSyntheticSecret(error.ToString()));
    }

    [Fact]
    public async Task Incompatible_index_blocks_dependent_test_operation_without_repair()
    {
        var config = Configuration();
        using var setup = new MongoClient(fixture.MongoConnectionString);
        var database = setup.GetDatabase(config["MongoDb:DatabaseName"]!);
        try
        {
            await database.GetCollection<BsonDocument>("users").Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("normalizedEmail"), new CreateIndexOptions { Name = "normalizedEmail_1", Unique = false }));
            await using var factory = new AuthenticationApiFactory(fixture, config);
            using var host = WithProbe(factory);
            using var client = host.CreateClient();
            await WarmMongoAsync(host.Services);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/__test/admission/register")).StatusCode);
            Assert.Equal(MongoUsersInitializationStatus.Incompatible, host.Services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
            using var cursor = await database.GetCollection<BsonDocument>("users").Indexes.ListAsync();
            Assert.False((await cursor.ToListAsync()).Single(i => i["name"] == "normalizedEmail_1").GetValue("unique", false).AsBoolean);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        }
        finally { await setup.DropDatabaseAsync(database.DatabaseNamespace.DatabaseName); }
    }

    [Fact]
    public async Task Temporary_mongo_failure_keeps_liveness_and_does_not_couple_logout_admission_to_users()
    {
        await using var authority = await OwnedCompositionAuthority.CreateAsync(fixture, open: true);
            var config = Configuration(); config["MongoDb:ConnectionString"] = "mongodb://127.0.0.1:1/?directConnection=true";
            config["AuthenticationSecurityAuthority:TableName"] = authority.TableName;
            await using var factory = new AuthenticationApiFactory(fixture, config);
            using var host = WithProbe(factory); using var client = host.CreateClient();
            Assert.Equal(MongoUsersInitializationStatus.Unavailable, host.Services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/__test/admission/register")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync("/__test/admission/logout")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    [Fact]
    public async Task Ready_index_real_quota_DI_and_global_closed_state_are_preserved()
    {
        await using var authority = await OwnedCompositionAuthority.CreateAsync(fixture, open: true);
        var config = Configuration();
        config["AuthenticationSecurityAuthority:TableName"] = authority.TableName;
        await using var factory = new AuthenticationApiFactory(fixture, config);
        using var host = WithProbe(factory); using var client = host.CreateClient();
        var services = host.Services;
        var captured = services.GetRequiredService<RedisAuthenticationQuotaConfiguration>();
        var connection = services.GetRequiredService<RedisConnections>();
        var db = (await connection.GetMultiplexerAsync()).GetDatabase();
        var owned = new List<RedisKey> { captured.Keys.Metadata, captured.Keys.Registry };
        try
        {
            await WarmMongoAsync(services);
            Assert.False(await db.KeyExistsAsync(captured.Keys.Metadata)); // El host NO inicializa cuotas.
            Assert.Equal(HttpStatusCode.NoContent, (await client.GetAsync("/__test/admission/register")).StatusCode);
            Assert.Equal(MongoUsersInitializationStatus.Ready, services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
            var pseudonyms = services.GetRequiredService<IAuthenticationScopePseudonymizer>();
            var scopes = AuthenticationQuotaPolicyDescriptor.RequiredScopes(AuthenticationOperation.Register).Select(type =>
                new AuthenticationQuotaScope(type, pseudonyms.Pseudonymize(type == AuthenticationQuotaScopeType.Email
                    ? AuthenticationScopeIdentity.Email(AuthenticationOperation.Register, AuthenticationCanonicalEmail.FromValidatedEmail("synthetic@uttt.edu.mx"))
                    : AuthenticationScopeIdentity.Origin(AuthenticationOperation.Register, AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback))),
                    captured.Snapshot.Policy.GetDefinition(AuthenticationOperation.Register, type))).ToArray();
            var store = services.GetRequiredService<IAuthenticationQuotaStore>();
            AuthenticationQuotaConsumeRequest Request() => new(AuthenticationOperation.Register, Guid.NewGuid().ToString("N"), captured.Snapshot.Policy.Revision, scopes);
            using var before = new RequestBudget(8000, default);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await store.EvaluateAndConsumeAsync(Request(), before)).Status);
            var lifecycle = new RedisAuthenticationQuotaLifecycle(connection, services.GetRequiredService<IOptions<RedisOptions>>().Value, captured,
                services.GetRequiredService<RedisSecurityStateGuard>(), services.GetRequiredService<AuthenticationDependencyAdmission>());
            using var initialize = new RequestBudget(8000, default);
            Assert.Equal("initialized", (await lifecycle.InitializeVerifiedNewEnvironmentAsync(initialize)).Status);
            foreach (var s in scopes) owned.Add(captured.Keys.Bucket(AuthenticationOperation.Register, s.ScopeType, s.OpaqueId));
            for (var i = 0; i < 4; i++)
            {
                using var attempt = new RequestBudget(8000, default);
                Assert.Equal(i < 3 ? AuthenticationQuotaStoreStatus.Admitted : AuthenticationQuotaStoreStatus.Rejected,
                    (await store.EvaluateAndConsumeAsync(Request(), attempt)).Status);
            }
            using var close = new RequestBudget(8000, default);
            await services.GetRequiredService<AuthenticationSecurityCoordinator>().CloseAsync(close);
            var closed = await client.GetAsync("/__test/admission/register");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, closed.StatusCode);
            var body = await closed.Content.ReadAsStringAsync();
            Assert.False(fixture.ContainsSyntheticSecret(body));
            Assert.DoesNotContain("Mongo", body); Assert.DoesNotContain("Redis", body);
        }
        finally
        {
            try { await db.KeyDeleteAsync(owned.ToArray()); }
            finally
            {
                await services.GetRequiredService<IMongoClient>().DropDatabaseAsync(config["MongoDb:DatabaseName"]!);
            }
        }
    }

    private static Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<AuthenticationOptions> WithProbe(AuthenticationApiFactory factory) =>
        factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(DependencyAdmissionProbeController).Assembly)));

    private static async Task WarmMongoAsync(IServiceProvider services)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await services.GetRequiredService<IMongoClient>().GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: timeout.Token);
    }
}

/// <summary>Tabla exclusiva de cada caso 057; el cleanup de la fixture nunca es propietario de ella.</summary>
internal sealed class OwnedCompositionAuthority : IAsyncDisposable
{
    private readonly IAmazonDynamoDB client;
    private OwnedCompositionAuthority(AuthenticationDependenciesFixture fixture) => client = fixture.CreateDynamoDbClient();
    internal string TableName { get; } = "enver_t057_" + Guid.NewGuid().ToString("N");

    internal static async Task<OwnedCompositionAuthority> CreateAsync(AuthenticationDependenciesFixture fixture, bool open)
    {
        var scope = new OwnedCompositionAuthority(fixture);
        var created = false;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await scope.client.CreateTableAsync(new CreateTableRequest
            {
                TableName = scope.TableName,
                AttributeDefinitions = [new AttributeDefinition("Id", ScalarAttributeType.S)],
                KeySchema = [new KeySchemaElement("Id", KeyType.HASH)],
                ProvisionedThroughput = new ProvisionedThroughput(1, 1)
            }, timeout.Token);
            created = true;
            while ((await scope.client.DescribeTableAsync(new DescribeTableRequest { TableName = scope.TableName }, timeout.Token)).Table.TableStatus != TableStatus.ACTIVE)
                await Task.Delay(TimeSpan.FromMilliseconds(50), timeout.Token);
            if (open)
            {
                // Mismo formato de 020; alta explícita solo del escenario sintético, nunca desde producto.
                await scope.client.PutItemAsync(new PutItemRequest
                {
                    TableName = scope.TableName,
                    Item = new Dictionary<string, AttributeValue>(StringComparer.Ordinal)
                    {
                        ["Id"] = new() { S = "authentication" }, ["State"] = new() { S = "Open" },
                        ["Generation"] = new() { N = "0" }, ["Revision"] = new() { N = "0" }
                    },
                    ConditionExpression = "attribute_not_exists(#id)",
                    ExpressionAttributeNames = new Dictionary<string, string> { ["#id"] = "Id" }
                }, timeout.Token);
            }
            return scope;
        }
        catch
        {
            if (created) await scope.DisposeAsync();
            else scope.client.Dispose();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await client.DeleteTableAsync(new DeleteTableRequest { TableName = TableName }, timeout.Token);
        }
        finally { client.Dispose(); }
    }
}

// Superficie únicamente del assembly IT y solo incorporada por WithProbe; no implementa Register/Logout.
// 058: prueba admisión de dependencias, no identidad; la excepción pública es solo del probe IT.
[Microsoft.AspNetCore.Authorization.AllowAnonymous]
[ApiController]
[Route("__test/admission")]
public sealed class DependencyAdmissionProbeController(AuthenticationDependencyAdmission admission) : ControllerBase
{
    [HttpGet("register")]
    public Task RegisterAdmission() => Evaluate(AuthenticationOperation.Register);
    [HttpGet("logout")]
    public Task LogoutAdmission() => Evaluate(AuthenticationOperation.Logout);
    private async Task Evaluate(AuthenticationOperation operation)
    {
        using var budget = new RequestBudget(8000, HttpContext.RequestAborted);
        if (!(await admission.EvaluateOperationAsync(operation, budget)).CanContinue) await ApiProblemDetails.TryWriteAsync(HttpContext, 503);
        else Response.StatusCode = 204;
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Security;
using Enver.API.Http;
using Enver.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RegistrationTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public Task Confirmed_201_has_exact_projection_real_seven_field_document_and_real_hash_without_auto_login() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        s.Client.DefaultRequestHeaders.Authorization = new("Bearer", "not-a-jwt");
        var before = DateTime.UtcNow;
        using var response = await s.SendAsync(" A.B+Tag@UTTT.EDU.MX ");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString()); Assert.Null(response.Headers.Location);
        Assert.False(response.Headers.Contains("Set-Cookie"));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(new[] { "email", "fullName", "id" }, body.RootElement.EnumerateObject().Select(p => p.Name).Order());
        var id = body.RootElement.GetProperty("id").GetString()!;
        Assert.True(Guid.TryParseExact(id, "D", out var guid)); Assert.Equal(4, guid.Version);
        Assert.Equal(guid.ToString("D"), id); Assert.Equal("a.b+tag@uttt.edu.mx", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("Ána  María", body.RootElement.GetProperty("fullName").GetString());
        var bson = Assert.Single(await s.Documents.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync());
        Assert.Equal(new[] { "_id", "createdAt", "fullName", "normalizedEmail", "passwordHash", "status", "updatedAt" }, bson.Names.Order());
        var user = Assert.Single(await s.Database.GetCollection<UserDocument>("users").Find(FilterDefinition<UserDocument>.Empty).ToListAsync()).ToUser();
        Assert.Equal(guid, user.Id); Assert.Equal("Active", bson["status"].AsString);
        Assert.Equal(user.CreatedAt, user.UpdatedAt); Assert.InRange(user.CreatedAt, before.AddMilliseconds(-1), DateTime.UtcNow);
        var verified = s.Services.GetRequiredService<IPasswordHasher<Enver.API.Features.Authentication.Core.User>>().VerifyHashedPassword(user, user.PasswordHash, "synthetic-secret");
        Assert.True(verified is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded);
        var publicText = body.RootElement.GetRawText();
        Assert.True(!publicText.Contains(user.PasswordHash, StringComparison.Ordinal));
        Assert.True(!publicText.Contains("synthetic-secret", StringComparison.Ordinal));
        Assert.Equal(1, s.Quotas.Calls); Assert.Equal(1, s.Store.Calls);
        AssertLogsSafe(s, user.PasswordHash);
        // Retirar el Bearer sintético inválido usado en Register; comprobar Logout sin autenticación.
        s.Client.DefaultRequestHeaders.Authorization = null;
        await AuthenticationDependencyCompositionTests.AssertPreparedLogoutWithoutProductiveJwtAsync(s.Services, s.Client);
    });

    [Fact]
    public Task Equivalent_email_returns_generic_409_and_never_refunds_quota() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        Assert.Equal(HttpStatusCode.Created, (await s.SendAsync()).StatusCode);
        await s.AssertProblemAsync(await s.SendAsync(" SYNTHETIC@UTTT.EDU.MX "), 409, "registration_conflict");
        await s.AssertProblemAsync(await s.SendAsync(), 409, "registration_conflict");
        await s.AssertProblemAsync(await s.SendAsync(), 429, "rate_limited");
        Assert.Equal(4, s.Quotas.Calls); Assert.Equal(3, s.Store.Calls);
        Assert.Equal(1, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        AssertLogsSafe(s);
    });

    [Theory]
    [InlineData(UserInsertStatus.Unavailable, 0)]
    [InlineData(UserInsertStatus.Uncertain, 1)]
    public Task Persistence_outcomes_map_to_503_without_retry_or_compensating_delete(UserInsertStatus outcome, int documents) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        s.Store.Outcome = outcome;
        await s.AssertProblemAsync(await s.SendAsync(), 503, "temporarily_unavailable");
        Assert.Equal(1, s.Quotas.Calls); Assert.Equal(1, s.Store.Calls);
        Assert.Equal(documents, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        AssertLogsSafe(s);
    });

    [Fact]
    public Task Real_unavailable_mongo_closes_admission_and_preserves_liveness() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        var bad = s.AddHost(new Dictionary<string, string?> { ["MongoDb:ConnectionString"] = "mongodb://127.0.0.1:1/?directConnection=true" });
        await s.AssertProblemAsync(await s.SendAsync(client: bad), 503, "temporarily_unavailable");
        Assert.Equal(HttpStatusCode.OK, (await bad.GetAsync("/health")).StatusCode);
        Assert.Equal(0, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    });

    [Fact]
    public Task Two_independent_hosts_race_equivalent_email_with_one_201_one_409_and_one_account() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        var other = s.AddHost();
        var responses = await Task.WhenAll(s.SendAsync("synthetic@uttt.edu.mx"), s.SendAsync(" SYNTHETIC@UTTT.EDU.MX ", client: other));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(1, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        foreach (var response in responses) Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        AssertLogsSafe(s);
    });

    [Fact]
    public Task Secondary_validation_consumes_but_never_hashes_or_writes_and_later_quota_precedes_400() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        for (var i = 0; i < 3; i++) await s.AssertProblemAsync(await s.SendAsync(name: "", password: "short"), 400, "validation_failed");
        await s.AssertProblemAsync(await s.SendAsync(name: "", password: "short"), 429, "rate_limited");
        Assert.Equal(0, s.Store.Calls); Assert.Equal(4, s.Quotas.Calls);
        Assert.Equal(2, await s.PartitionCountAsync());
        Assert.Equal(0, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    });

    [Fact]
    public Task Hashing_saturation_is_503_and_does_not_refund_the_real_joint_admission() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        using var budget = new RequestBudget(8000, default);
        var gate = s.Services.GetRequiredService<HashingConcurrencyGate>();
        using var a = gate.TryAcquire(budget); using var b = gate.TryAcquire(budget);
        Assert.NotNull(a); Assert.NotNull(b);
        for (var i = 0; i < 3; i++) await s.AssertProblemAsync(await s.SendAsync(), 503, "temporarily_unavailable");
        await s.AssertProblemAsync(await s.SendAsync(), 429, "rate_limited");
        Assert.Equal(4, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls);
    });

    [Fact]
    public Task Origin_20_per_900_joint_with_email_3_per_900_has_one_charge_and_conservative_retry_after() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        // Inválidos secundarios evitan coste de hash sin evadir el contador de intentos admitidos.
        for (var i = 0; i < 20; i++) await s.AssertProblemAsync(await s.SendAsync($"synthetic{i}@uttt.edu.mx", name: ""), 400, "validation_failed");
        using var response = await s.SendAsync("synthetic20@uttt.edu.mx", name: "");
        await s.AssertProblemAsync(response, 429, "rate_limited");
        Assert.InRange((int)response.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 900);
        Assert.Equal(21, s.Quotas.Calls);
        var partitions = await s.PartitionCountAsync();
        Assert.Equal(21, partitions); // 20 emails + origen, no bucket parcial del rechazado.
        var definitions = s.Quotas.Last!.Scopes.ToDictionary(q => q.ScopeType, q => q.Definition);
        Assert.Equal(3, definitions[Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaScopeType.Email].PermitLimit);
        Assert.Equal(20, definitions[Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaScopeType.Origin].PermitLimit);
        Assert.All(definitions.Values, q =>
        {
            Assert.Equal(TimeSpan.FromSeconds(900), q.Window);
        });
        Assert.Equal(0, s.Store.Calls);
    });

    [Theory]
    [InlineData(Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Unavailable)]
    [InlineData(Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Uncertain)]
    public Task Quota_operational_outcomes_are_503_before_secondary_defects_and_not_retried(Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus outcome) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        s.Quotas.Outcome = outcome;
        await s.AssertProblemAsync(await s.SendAsync(name: ""), 503, "temporarily_unavailable");
        Assert.Equal(1, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls);
        Assert.Equal(outcome == Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Uncertain ? 2 : 0, await s.PartitionCountAsync());
    });

    [Fact]
    public Task Missing_quota_metadata_and_closed_authority_stay_fail_closed_without_bootstrap() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using (var s = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false))
        {
            await s.AssertProblemAsync(await s.SendAsync(), 503, "temporarily_unavailable");
            Assert.Equal(0, s.Store.Calls); Assert.Equal(0, await s.PartitionCountAsync());
        }
        await using (var s = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, openAuthority: false))
        {
            using var body = new StringContent("{", Encoding.UTF8, "application/json");
            await s.AssertProblemAsync(await s.Client.PostAsync("/api/v1/auth/register", body), 503, "temporarily_unavailable");
            Assert.Equal(0, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls);
        }
    });

    [Theory]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\",\"status\":\"Active\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\",\"_id\":\"client\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"a@uttt.edu.mx\",\"email\":\"b@uttt.edu.mx\",\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"FullName\":\"Synthetic\",\"Email\":\"synthetic@uttt.edu.mx\",\"Password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":{\"$ne\":null},\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":[],\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":null,\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":123,\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"a@sub.uttt.edu.mx\",\"password\":\"synthetic-secret\"}")]
    [InlineData("[]")]
    [InlineData("{")]
    public Task Malformed_identity_or_mass_assignment_never_writes_or_creates_arbitrary_buckets(string json) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        await s.AssertProblemAsync(await s.Client.PostAsync("/api/v1/auth/register", body), 400, "validation_failed");
        Assert.Equal(0, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls); Assert.Equal(0, await s.PartitionCountAsync());
        Assert.Equal(0, await s.Documents.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        AssertLogsSafe(s);
    });

    [Theory]
    [InlineData("{\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":null,\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":[],\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"synthetic@uttt.edu.mx\",\"password\":null}")]
    public Task Required_secondary_fields_are_400_after_single_joint_consumption(string json) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        await s.AssertProblemAsync(await s.Client.PostAsync("/api/v1/auth/register", body), 400, "validation_failed");
        Assert.Equal(1, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls); Assert.Equal(2, await s.PartitionCountAsync());
    });

    [Theory]
    [InlineData("text/plain", null)]
    [InlineData("application/json; charset=utf-16", null)]
    [InlineData("application/json; profile=unexpected", null)]
    [InlineData("application/json", "gzip")]
    public Task Unsupported_media_charset_or_encoding_is_terminal_415_without_quota(string type, string? encoding) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        using var body = RegistrationTestScope.Body();
        body.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(type);
        if (encoding is not null) body.Headers.ContentEncoding.Add(encoding);
        await s.AssertProblemAsync(await s.Client.PostAsync("/api/v1/auth/register", body), 415, "unsupported_media_type");
        Assert.Equal(0, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls);
    });

    [Fact]
    public Task Oversized_body_is_terminal_413_without_quota_hashing_or_store() => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var s = await RegistrationTestScope.CreateAsync(fixture);
        using var body = new StringContent(new string('x', 16385), Encoding.UTF8, "application/json");
        await s.AssertProblemAsync(await s.Client.PostAsync("/api/v1/auth/register", body), 413, "payload_too_large");
        Assert.Equal(0, s.Quotas.Calls); Assert.Equal(0, s.Store.Calls); Assert.Equal(0, await s.PartitionCountAsync());
    });

    private static void AssertLogsSafe(RegistrationTestScope s, string? hash = null)
    {
        var logs = string.Join('\n', s.Logs.Messages);
        foreach (var secret in new[] { "synthetic-secret", "synthetic@uttt.edu.mx", "a.b+tag@uttt.edu.mx", "Ána", hash }.Where(x => !string.IsNullOrEmpty(x)))
            Assert.True(!logs.Contains(secret!, StringComparison.OrdinalIgnoreCase), "Sensitive value in logs.");
        Assert.All(s.Logs.EventKeys, keys => Assert.Equal(new[] { "Kind", "Operation", "Outcome", "{OriginalFormat}" }.Order(), keys));
    }
}

using System.Net;
using System.Text.Json;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Security;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class LoginTests(AuthenticationDependenciesFixture fixture)
{
    // Solo A/B: KDF observado ~300 ms más CAS, reverificación y autoridad. 4 s separan concurrencia
    // del perfil de 1 s; no cambian defaults, timeouts de dependencias ni las pruebas de agotamiento.
    private const int CredentialConcurrencyTestBudgetMilliseconds = 4000;
    [Fact, Trait("Task", "051")]
    public async Task Login_200_is_exact_validable_and_independent_sessions_survive_Logout_of_A_cross_host()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var a = scope.First; var b = await scope.AddHostAsync();
        var user = await a.SeedAsync();
        var tokenA = await TokenAsync(await a.LoginAsync(" LOGIN@UTTT.EDU.MX ")); var tokenB = await TokenAsync(await b.LoginAsync());
        Assert.True(tokenA != tokenB); await scope.TrackAsync(tokenA); await scope.TrackAsync(tokenB);
        foreach (var (host, token) in new[] { (a, tokenA), (b, tokenB) })
        {
            var validated = await host.Services.GetRequiredService<JwtClaimsValidator>().ValidateAsync(token); Assert.True(validated.IsValid);
            using var payload = JsonDocument.Parse(Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(token.Split('.')[1]));
            var p = payload.RootElement; Assert.Equal(new[] { "aud", "exp", "iat", "iss", "jti", "sub" }, p.EnumerateObject().Select(x => x.Name).Order());
            Assert.True(user.Id.ToString("D") == p.GetProperty("sub").GetString());
            Assert.Equal("enver-api", p.GetProperty("iss").GetString()); Assert.Equal("enver-mobile", p.GetProperty("aud").GetString());
            Assert.Equal(900, p.GetProperty("exp").GetInt64() - p.GetProperty("iat").GetInt64());
            using var header = JsonDocument.Parse(Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(token.Split('.')[0]));
            Assert.Equal("HS256", header.RootElement.GetProperty("alg").GetString()); Assert.False(string.IsNullOrEmpty(header.RootElement.GetProperty("kid").GetString()));
            Assert.DoesNotContain(user.NormalizedEmail, payload.RootElement.GetRawText());
        }
        using (var before = await b.AuthorizedAsync(tokenA)) Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        using (var logout = await a.AuthorizedAsync(tokenA, "/api/v1/auth/logout", logout: true)) Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        await ProblemAsync(await b.AuthorizedAsync(tokenA), 401, "authentication_failed");
        using (var other = await b.AuthorizedAsync(tokenB)) Assert.Equal(HttpStatusCode.OK, other.StatusCode);
        Assert.Equal(0, a.Users.Writes + b.Users.Writes + a.Passwords.Rehashes + b.Passwords.Rehashes);
        Assert.Equal(2, a.Issuer.Calls + b.Issuer.Calls);
        var logs = string.Join('\n', scope.Logs.Messages);
        foreach (var secret in new[] { tokenA, tokenB, user.NormalizedEmail, user.Id.ToString("D"), user.PasswordHash, "Synthetic Password 123!", "127.0.0.1" }) Assert.DoesNotContain(secret, logs);
        Assert.Equal(2, scope.Logs.Messages.Count(m => m == "Authentication login success"));
    }

    [Theory, Trait("Task", "051")]
    [InlineData("missing")] [InlineData("wrong")] [InlineData("disabled")] [InlineData("disabled-legacy")]
    public async Task Functional_rejections_use_exactly_one_real_KDF_and_generic_401_never_rehash_or_issue(string kind)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        User? user = kind == "missing" ? null : await h.SeedAsync(status: kind.StartsWith("disabled") ? UserStatus.Disabled : UserStatus.Active, legacy: kind.EndsWith("legacy"));
        var hashes = h.Kdf.Hashes; var work = h.Kdf.Verifications;
        await ProblemAsync(await h.LoginAsync(password: kind == "wrong" ? "Wrong Password 123!" : "Synthetic Password 123!"), 401, "authentication_failed");
        Assert.Equal(hashes, h.Kdf.Hashes); Assert.Equal(work + 1, h.Kdf.Verifications);
        Assert.Equal(kind == "missing" ? 1 : 0, h.Passwords.Dummies); Assert.Equal(kind == "missing" ? 0 : 1, h.Passwords.Verifies);
        Assert.Equal(1, h.Users.Lookups); Assert.Equal(0, h.Users.Reads + h.Users.Writes + h.Passwords.Rehashes + h.Issuer.Calls);
        if (user is not null)
        {
            using var budget = new RequestBudget(8000, default); var saved = (await h.Users.Inner.FindByIdAsync(user.Id, budget)).User!;
            Assert.True(user.PasswordHash == saved.PasswordHash); Assert.Equal(user.UpdatedAt, saved.UpdatedAt);
        }
        Assert.Equal("Authentication login rejected", Assert.Single(scope.Logs.Messages));
    }

    [Fact, Trait("Task", "051")]
    public async Task Legacy_short_secret_updates_only_hash_and_backend_UTC_then_issues_once()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; var user = await h.SeedAsync(secret: "short", legacy: true);
        var before = DateTime.UtcNow.AddMilliseconds(-1); _ = await TokenAsync(await h.LoginAsync(password: "short")); var after = DateTime.UtcNow;
        using var budget = new RequestBudget(8000, default); var saved = (await h.Users.Inner.FindByIdAsync(user.Id, budget)).User!;
        Assert.True(saved.PasswordHash != user.PasswordHash); Assert.InRange(saved.UpdatedAt, before, after);
        Assert.Equal(DateTimeKind.Utc, saved.UpdatedAt.Kind); Assert.Equal(0, saved.UpdatedAt.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.Equal(user.Id, saved.Id); Assert.Equal(user.FullName, saved.FullName); Assert.True(user.NormalizedEmail == saved.NormalizedEmail);
        Assert.Equal(user.Status, saved.Status); Assert.Equal(user.CreatedAt, saved.CreatedAt);
        Assert.Equal(1, h.Users.Writes); Assert.Equal(0, h.Users.Reads); Assert.Equal(1, h.Passwords.Verifies); Assert.Equal(1, h.Passwords.Rehashes); Assert.Equal(1, h.Issuer.Calls);
        Assert.Contains("Authentication rehash rehash_updated", scope.Logs.Messages); Assert.Contains("Authentication login success", scope.Logs.Messages);
    }

    [Fact, Trait("Task", "051")]
    public async Task Concurrent_Login_two_real_hosts_one_CAS_winner_loser_rereads_once_and_both_issue_once()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture, testOnlyLoginBudgetMilliseconds: CredentialConcurrencyTestBudgetMilliseconds);
        var a = scope.First; var b = await scope.AddHostAsync();
        var original = await a.SeedAsync(secret: "short", legacy: true);
        var barrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var arrived = 0;
        async Task Wait() { if (Interlocked.Increment(ref arrived) == 2) barrier.SetResult(); await barrier.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        a.Users.BeforeWrite = Wait; b.Users.BeforeWrite = Wait;
        var results = await Task.WhenAll(a.LoginAsync(password: "short"), b.LoginAsync(password: "short"));
        var firstToken = await TokenAsync(results[0]); var secondToken = await TokenAsync(results[1]);
        Assert.True(firstToken != secondToken);
        Assert.Equal(2, a.Users.Lookups + b.Users.Lookups); Assert.Equal(2, a.Users.Writes + b.Users.Writes); Assert.Equal(1, a.Users.Reads + b.Users.Reads);
        Assert.Equal(3, a.Passwords.Verifies + b.Passwords.Verifies); Assert.Equal(2, a.Issuer.Calls + b.Issuer.Calls);
        Assert.Single(scope.Logs.Messages, m => m == "Authentication rehash rehash_updated"); Assert.Single(scope.Logs.Messages, m => m == "Authentication rehash rehash_race_lost");
        var winner = Assert.Single(new[] { a, b }, h => h.Users.LastCredentialUpdateStatus == UserCredentialUpdateStatus.Updated);
        var loser = Assert.Single(new[] { a, b }, h => h.Users.LastCredentialUpdateStatus == UserCredentialUpdateStatus.Conflict);
        Assert.Equal(0, winner.Users.Reads); Assert.Equal(1, loser.Users.Reads);
        Assert.Equal(UserReadStatus.Found, loser.Users.LastReadByIdStatus); Assert.Equal(UserStatus.Active, loser.Users.LastReadUserStatus);
        Assert.Equal(PasswordVerificationStatus.Success, loser.Passwords.LastVerificationStatus);
        using var inspect = new RequestBudget(8000, default); var saved = (await winner.Users.Inner.FindByIdAsync(original.Id, inspect)).User!;
        Assert.True(saved.PasswordHash == winner.Users.LastReplacementHash); Assert.True(saved.PasswordHash != original.PasswordHash);
        Assert.All(new[] { a, b }, h => Assert.Equal(CredentialConcurrencyTestBudgetMilliseconds,
            h.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Enver.API.Configuration.AuthenticationOptions>>().Value.LoginBudgetMilliseconds));
    }

    [Theory, Trait("Task", "051")]
    [InlineData("disabled", 401)] [InlineData("changed", 401)] [InlineData("legacy", 503)] [InlineData("removed", 401)]
    public async Task Concurrent_credential_changes_are_not_overwritten_and_never_emit_a_token(string change, int status)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture,
            testOnlyLoginBudgetMilliseconds: change == "changed" ? CredentialConcurrencyTestBudgetMilliseconds : null);
        var h = scope.First; var user = await h.SeedAsync(secret: "short", legacy: true);
        var database = h.Services.GetRequiredService<IMongoDatabase>(); var collection = database.GetCollection<Enver.API.Infrastructure.MongoDB.UserDocument>("users");
        string? changedHash = null;
        if (change is "changed" or "legacy")
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>(Microsoft.Extensions.Options.Options.Create(new Microsoft.AspNetCore.Identity.PasswordHasherOptions
            { CompatibilityMode = change == "legacy" ? Microsoft.AspNetCore.Identity.PasswordHasherCompatibilityMode.IdentityV2 : Microsoft.AspNetCore.Identity.PasswordHasherCompatibilityMode.IdentityV3 }));
            changedHash = hasher.HashPassword(user, change == "legacy" ? "short" : "changed");
        }
        h.Users.BeforeWrite = async () =>
        {
            if (change == "removed") await collection.DeleteOneAsync(x => x.Id == user.Id);
            else if (change == "disabled") await collection.UpdateOneAsync(x => x.Id == user.Id, Builders<Enver.API.Infrastructure.MongoDB.UserDocument>.Update.Set(x => x.Status, UserStatus.Disabled));
            else
            {
                await collection.UpdateOneAsync(x => x.Id == user.Id, Builders<Enver.API.Infrastructure.MongoDB.UserDocument>.Update.Set(x => x.PasswordHash, changedHash!));
            }
        };
        await ProblemAsync(await h.LoginAsync(password: "short"), status, status == 401 ? "authentication_failed" : "temporarily_unavailable");
        Assert.Equal(1, h.Users.Writes); Assert.Equal(1, h.Users.Reads); Assert.Equal(0, h.Issuer.Calls);
        Assert.Equal(change == "removed" ? 1 : 2, h.Passwords.Verifies);
        if (changedHash is not null) { var saved = await collection.Find(x => x.Id == user.Id).SingleAsync(); Assert.True(changedHash == saved.PasswordHash); }
        if (change == "changed")
        {
            Assert.Equal(UserCredentialUpdateStatus.Conflict, h.Users.LastCredentialUpdateStatus);
            Assert.Equal(UserReadStatus.Found, h.Users.LastReadByIdStatus); Assert.Equal(UserStatus.Active, h.Users.LastReadUserStatus);
            Assert.Equal(PasswordVerificationStatus.Failed, h.Passwords.LastVerificationStatus);
        }
    }

    [Theory, Trait("Task", "051")]
    [InlineData("lookup")] [InlineData("cas-unavailable")] [InlineData("cas-uncertain")] [InlineData("authority")] [InlineData("authority-uncertain")] [InlineData("post-sign")]
    public async Task Dependency_nonconfirmation_is_503_without_retry_readback_or_reissuance(string fault)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; var legacy = fault.StartsWith("cas");
        await h.SeedAsync(legacy: legacy);
        if (fault == "lookup") h.Users.ReadOutcome = UserReadStatus.Unavailable;
        if (legacy) h.Users.WriteOutcome = fault.EndsWith("uncertain") ? UserCredentialUpdateStatus.Uncertain : UserCredentialUpdateStatus.Unavailable;
        if (fault.StartsWith("authority")) h.Authority.ReadOutcome = fault.EndsWith("uncertain") ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable;
        if (fault == "post-sign") h.Issuer.AfterIssue = () => h.Authority.ReadOutcome = AuthenticationSecurityAuthorityResultStatus.Unavailable;
        await ProblemAsync(await h.LoginAsync(), 503, "temporarily_unavailable");
        Assert.Equal(fault.StartsWith("authority") ? 0 : 1, h.Users.Lookups);
        Assert.Equal(legacy ? 1 : 0, h.Users.Writes); Assert.Equal(0, h.Users.Reads); Assert.Equal(fault == "post-sign" ? 1 : 0, h.Issuer.Calls);
        Assert.Single(scope.Logs.Messages, m => m.StartsWith("Authentication login "));
    }

    [Fact, Trait("Task", "051")]
    public async Task Real_Redis_shutdown_denies_before_lookup_and_never_retries_the_request()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; await h.SeedAsync();
        try { await fixture.StopRedisAsync(); await ProblemAsync(await h.LoginAsync(), 503, "temporarily_unavailable"); }
        finally { await fixture.StartRedisAsync(); }
        Assert.Equal(0, h.Users.Lookups + h.Users.Writes + h.Kdf.Verifications + h.Issuer.Calls);
    }

    internal static async Task<string> TokenAsync(HttpResponseMessage response)
    {
        using (response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType); Assert.False(response.Headers.Contains("WWW-Authenticate"));
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var root = json.RootElement;
            Assert.Equal(new[] { "accessToken", "expiresIn", "tokenType" }, root.EnumerateObject().Select(x => x.Name).Order());
            Assert.Equal("Bearer", root.GetProperty("tokenType").GetString()); Assert.Equal(900, root.GetProperty("expiresIn").GetInt32());
            return root.GetProperty("accessToken").GetString()!;
        }
    }

    [Theory, Trait("Task", "050")]
    [InlineData("joint", 5)] [InlineData("email", 10)] [InlineData("origin", 60)]
    public async Task Joint_real_quota_has_exact_limits_shared_across_hosts_without_partial_charge_or_refund(string limit, int permits)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var a = scope.First; var b = await scope.AddHostAsync();
        for (var n = 0; n < permits; n++)
        {
            using var response = await (n % 2 == 0 ? a : b).LoginAsync(limit == "origin" ? $"login{n}@uttt.edu.mx" : "login@uttt.edu.mx", "",
                limit == "email" ? $"10.0.0.{n + 1}" : "127.0.0.1");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        var host = permits % 2 == 0 ? b : a; var db = await DatabaseAsync(host);
        var blocking = Assert.Single(host.Quotas.Last!.Scopes, s => s.ScopeType == (limit == "joint" ? AuthenticationQuotaScopeType.OriginEmail : limit == "email" ? AuthenticationQuotaScopeType.Email : AuthenticationQuotaScopeType.Origin));
        Assert.Equal(permits, await db.SortedSetLengthAsync(Bucket(host, blocking)));
        using var rejected = await a.LoginAsync(limit == "origin" ? "fresh@uttt.edu.mx" : "login@uttt.edu.mx", "", limit == "email" ? "10.0.0.200" : "127.0.0.1");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode); Assert.InRange(rejected.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 60);
        var scopes = a.Quotas.Last!.Scopes; Assert.Equal(3, scopes.Count);
        Assert.Equal(permits, await db.SortedSetLengthAsync(Bucket(host, blocking)));
        foreach (var s in scopes)
        {
            Assert.Equal(s.ScopeType == AuthenticationQuotaScopeType.OriginEmail ? 5 : s.ScopeType == AuthenticationQuotaScopeType.Email ? 10 : 60, s.Definition.PermitLimit);
            Assert.Equal(TimeSpan.FromSeconds(60), s.Definition.Window);
            Assert.Null(await db.SortedSetScoreAsync(Bucket(a, s), a.Quotas.Last.InternalAttemptId));
            Assert.DoesNotContain("login@", Bucket(a, s)); Assert.DoesNotContain("127.0.0.1", Bucket(a, s));
        }
        Assert.Equal(permits + 1, a.Quotas.Calls + b.Quotas.Calls);
        Assert.Equal(0, a.Users.Lookups + b.Users.Lookups + a.Kdf.Verifications + b.Kdf.Verifications + a.Issuer.Calls + b.Issuer.Calls);
        var events = scope.Logs.Messages.Where(m => m.StartsWith("Authentication login ")).ToArray();
        Assert.Equal(permits + 1, events.Length); Assert.Equal(permits, events.Count(m => m == "Authentication login rejected"));
        Assert.Single(events, m => m == "Authentication login rate_limited");
    }

    [Theory, Trait("Task", "050")]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable)] [InlineData(AuthenticationQuotaStoreStatus.Uncertain)]
    public async Task Unavailable_or_hidden_quota_admission_is_503_no_lookup_or_hash_and_no_refund(AuthenticationQuotaStoreStatus outcome)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; h.Quotas.Outcome = outcome;
        await ProblemAsync(await h.LoginAsync(password: ""), 503, "temporarily_unavailable");
        Assert.Equal(1, h.Quotas.Calls); Assert.Equal(0, h.Users.Lookups + h.Kdf.Verifications + h.Issuer.Calls);
        foreach (var s in h.Quotas.Last!.Scopes) Assert.Equal(outcome == AuthenticationQuotaStoreStatus.Uncertain ? 1 : 0, await (await DatabaseAsync(h)).SortedSetLengthAsync(Bucket(h, s)));
        Assert.Single(scope.Logs.Messages, m => m == "Authentication login " + (outcome == AuthenticationQuotaStoreStatus.Uncertain ? "uncertain" : "dependency_unavailable"));
    }

    private static async Task<IDatabase> DatabaseAsync(LoginTestHost h) => (await h.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase();
    [Fact, Trait("Task", "050")]
    public async Task Two_blocking_scopes_return_the_maximum_conservative_RetryAfter_without_any_partial_charge()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; var db = await DatabaseAsync(h);
        for (var i = 0; i < 10; i++)
        { using var r = await h.LoginAsync(password: "", origin: i < 5 ? "10.0.0.1" : "10.0.0.2"); Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode); }
        async Task<double> Now() { var t = (RedisResult[])(await db.ExecuteAsync("TIME"))!; return (long)t[0] * 1_000_000d + (long)t[1]; }
        var before = await Now(); using var blocked = await h.LoginAsync(password: "", origin: "10.0.0.1"); var after = await Now();
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode); var releases = new List<double>();
        foreach (var s in h.Quotas.Last!.Scopes)
        {
            var entries = await db.SortedSetRangeByRankWithScoresAsync(Bucket(h, s));
            Assert.Equal(s.ScopeType == AuthenticationQuotaScopeType.Email ? 10 : 5, entries.Length);
            Assert.Null(await db.SortedSetScoreAsync(Bucket(h, s), h.Quotas.Last.InternalAttemptId));
            if (entries.Length == s.Definition.PermitLimit) releases.Add(entries[0].Score + 60_000_000);
        }
        Assert.Equal(2, releases.Count); var retry = blocked.Headers.RetryAfter!.Delta!.Value.TotalSeconds;
        Assert.InRange(retry, Math.Ceiling((releases.Max() - after) / 1_000_000), Math.Ceiling((releases.Max() - before) / 1_000_000));
    }

    [Fact, Trait("Task", "051")]
    public async Task Real_unreachable_Mongo_closes_Login_preflight_without_a_credential_read_or_KDF()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture, mongoUnavailable: true); var h = scope.First;
        await ProblemAsync(await h.LoginAsync(), 503, "temporarily_unavailable");
        Assert.Equal(0, h.Quotas.Calls + h.Users.Lookups + h.Kdf.Verifications + h.Issuer.Calls);
    }

    private static string Bucket(LoginTestHost h, AuthenticationQuotaScope s) => h.Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Bucket(AuthenticationOperation.Login, s.ScopeType, s.OpaqueId);
    internal static async Task ProblemAsync(HttpResponseMessage response, int status, string code)
    {
        using (response)
        {
            Assert.Equal(status, (int)response.StatusCode); Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
            Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32());
            if (status == 401) Assert.Equal("Bearer", Assert.Single(response.Headers.GetValues("WWW-Authenticate")));
            else Assert.False(response.Headers.Contains("WWW-Authenticate"));
        }
    }
}

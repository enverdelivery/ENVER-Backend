using System.Net;
using System.Text.Json;
using Enver.API.Composition;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Enver.API.Security;
using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class LogoutTests(AuthenticationDependenciesFixture fixture)
{
    [Fact, Trait("Task", "046")]
    public async Task Two_hosts_revoke_only_A_repeat_and_concurrent_HTTP_confirmations_preserve_absolute_expiry()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var a = scope.First; var b = await scope.AddHostAsync();
        Assert.NotSame(a.Services.GetRequiredService<RedisConnections>(), b.Services.GetRequiredService<RedisConnections>());
        var subject = Guid.NewGuid(); var tokenA = a.Issuer.Issue(subject).AccessToken!; var tokenB = a.Issuer.Issue(subject).AccessToken!;
        var inputA = await a.InputAsync(tokenA); var inputB = await b.InputAsync(tokenB); Assert.NotEqual(inputA.Jti, inputB.Jti);
        var key = scope.MarkKey(inputA); var db = await DatabaseAsync(a);
        using (var before = await b.SendAsync(tokenA, "/_logout/protected", HttpMethod.Get)) Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        using (var first = await a.SendAsync(tokenA)) await EmptySuccessAsync(first);
        var expiry = (long)await db.ExecuteAsync("PEXPIRETIME", key);
        Assert.True(expiry >= inputA.ExpiresAtUtc.AddSeconds(90).ToUnixTimeMilliseconds());
        await ProblemAsync(await b.SendAsync(tokenA, "/_logout/protected", HttpMethod.Get), 401, "authentication_failed");
        using (var unaffected = await b.SendAsync(tokenB, "/_logout/protected", HttpMethod.Get)) Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode);
        Assert.False(await db.KeyExistsAsync(scope.MarkKey(inputB)));
        using (var repeated = await b.SendAsync(tokenA)) await EmptySuccessAsync(repeated);
        Assert.Equal(expiry, (long)await db.ExecuteAsync("PEXPIRETIME", key));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task Send(LogoutTestHost h) { await start.Task; using var response = await h.SendAsync(tokenA); await EmptySuccessAsync(response); }
        var x = Send(a); var y = Send(b); start.SetResult(); await Task.WhenAll(x, y);
        Assert.Equal(4, a.Revocations.Writes + b.Revocations.Writes); Assert.Equal(4, a.Quotas.Calls + b.Quotas.Calls);
        Assert.Equal("1", (string?)await db.StringGetAsync(key)); Assert.Equal(expiry, (long)await db.ExecuteAsync("PEXPIRETIME", key));
        foreach (var path in new[] { "/_logout/copied", "/api/v1/auth/logout" })
            await ProblemAsync(await b.SendAsync(tokenA, path, path.EndsWith("copied") ? HttpMethod.Post : HttpMethod.Get), 401, "authentication_failed");
        using var stillB = await b.SendAsync(tokenB, "/_logout/protected", HttpMethod.Get); Assert.Equal(HttpStatusCode.OK, stillB.StatusCode);
    }

    [Theory, Trait("Task", "046")]
    [InlineData(TokenRevocationWriteStatus.NoOperation)] [InlineData(TokenRevocationWriteStatus.Unavailable)] [InlineData(TokenRevocationWriteStatus.Uncertain)]
    public async Task HTTP_nonconfirmation_is_503_and_only_a_separate_confirmed_attempt_can_succeed(TokenRevocationWriteStatus failure)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; var key = scope.MarkKey(await host.InputAsync(token));
        host.Revocations.WriteOutcome = failure;
        await ProblemAsync(await host.SendAsync(token), 503, "temporarily_unavailable");
        Assert.Equal(1, host.Revocations.Writes); Assert.Equal(1, host.Revocations.Reads); Assert.Equal(1, host.Quotas.Calls);
        Assert.Equal(failure == TokenRevocationWriteStatus.Uncertain, await (await DatabaseAsync(host)).KeyExistsAsync(key));
        host.Revocations.WriteOutcome = null;
        using var independent = await host.SendAsync(token); await EmptySuccessAsync(independent);
        Assert.Equal(2, host.Revocations.Writes); Assert.Equal(2, host.Revocations.Reads); Assert.Equal(2, host.Quotas.Calls);
    }

    [Theory, Trait("Task", "046")]
    [InlineData("closed")] [InlineData("missing")] [InlineData("unavailable")] [InlineData("uncertain")]
    public async Task Authority_failure_is_503_and_PING_cannot_reopen_a_closed_latch(string failure)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        if (failure == "missing") await scope.RemoveAuthorityAsync();
        else if (failure == "closed")
        {
            using var budget = new RequestBudget(4000, default);
            var result = await host.Services.GetRequiredService<Enver.API.Infrastructure.Security.AuthenticationSecurityCoordinator>().CloseAsync(budget);
            Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, result.Status);
        }
        else host.Authority.ReadOutcome = failure == "uncertain" ? AuthenticationSecurityAuthorityResultStatus.Uncertain : AuthenticationSecurityAuthorityResultStatus.Unavailable;
        await ProblemAsync(await host.SendAsync(token), 503, "temporarily_unavailable");
        host.Authority.ReadOutcome = null; await (await DatabaseAsync(host)).PingAsync();
        await ProblemAsync(await host.SendAsync(token), 503, "temporarily_unavailable");
        Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Revocations.Writes); Assert.Equal(0, host.Quotas.Calls);
    }

    [Fact, Trait("Task", "046")]
    public async Task Stopped_real_Redis_blocks_HTTP_logout_and_uses_current_endpoint_only_for_cleanup()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; var container = fixture.RedisContainerId;
        using (var before = await host.SendAsync(token, "/_logout/protected", HttpMethod.Get)) Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        try
        {
            await fixture.StopRedisAsync();
            await ProblemAsync(await host.SendAsync(token), 503, "temporarily_unavailable");
            Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
        }
        finally { await fixture.StartRedisAsync(); }
        Assert.Equal(container, fixture.RedisContainerId);
    }

    [Fact, Trait("Task", "046")]
    public async Task Mongo_inaccessible_does_not_prevent_controller_JWT_policy_quota_and_confirmed_logout()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture, mongoUnavailable: true); var host = scope.First;
        Assert.NotEqual(Enver.API.Infrastructure.MongoDB.MongoUsersInitializationStatus.Ready, host.Services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; scope.Track(await host.InputAsync(token));
        using var response = await host.SendAsync(token); await EmptySuccessAsync(response);
        Assert.Equal(1, host.Revocations.Reads); Assert.Equal(1, host.Quotas.Calls); Assert.Equal(1, host.Revocations.Writes);
    }

    private static async Task EmptySuccessAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.Null(response.Content.Headers.ContentType); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.False(response.Headers.Contains("WWW-Authenticate"));
    }

    [Fact, Trait("Task", "045")]
    public async Task Account_30_per_60_is_joint_one_charge_and_rejection_changes_neither_scope_nor_expiry()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var subject = Guid.NewGuid(); var token = host.Issuer.Issue(subject).AccessToken!; scope.Track(await host.InputAsync(token));
        var db = await DatabaseAsync(host);
        for (var i = 1; i <= 30; i++)
        {
            // Cambiar jti no crea otra cuota de cuenta.
            if (i == 2) { token = host.Issuer.Issue(subject).AccessToken!; scope.Track(await host.InputAsync(token)); }
            using var response = await host.SendAsync(token); Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(i, host.Quotas.Calls); Assert.Equal(i, host.Revocations.Writes);
            var request = host.Quotas.Last!;
            Assert.Equal(2, request.Scopes.Count);
            foreach (var s in request.Scopes)
            {
                Assert.Equal(s.ScopeType == AuthenticationQuotaScopeType.Account ? 30 : 300, s.Definition.PermitLimit);
                Assert.Equal(TimeSpan.FromSeconds(60), s.Definition.Window);
                Assert.Equal(i, await db.SortedSetLengthAsync(Bucket(host, s)));
                Assert.NotNull(await db.SortedSetScoreAsync(Bucket(host, s), request.InternalAttemptId));
                Assert.DoesNotContain(subject.ToString("D"), Bucket(host, s)); Assert.DoesNotContain("127.0.0.1", Bucket(host, s));
            }
        }
        var before = await QuotaSnapshotAsync(host);
        using var rejected = await host.SendAsync(token);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.InRange(rejected.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 60);
        Assert.Equal(31, host.Quotas.Calls); Assert.Equal(30, host.Revocations.Writes);
        Assert.Equal(before, await QuotaSnapshotAsync(host));
        Assert.Contains("Authentication logout rate_limited", scope.Logs.Messages);
        Assert.All(scope.Logs.Messages, m => Assert.Contains(m, SafeLogoutEvents));
    }

    [Fact, Trait("Task", "045")]
    public async Task Origin_300_per_60_and_301st_has_no_partial_charge_for_a_fresh_account()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var db = await DatabaseAsync(host); var accounts = new HashSet<string>(); string? firstToken = null;
        for (var account = 0; account < 10; account++)
        {
            var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; scope.Track(await host.InputAsync(token));
            firstToken ??= token;
            for (var i = 0; i < 30; i++)
            {
                using var response = await host.SendAsync(token); Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
            var s = Assert.Single(host.Quotas.Last!.Scopes, x => x.ScopeType == AuthenticationQuotaScopeType.Account);
            accounts.Add(Bucket(host, s)); Assert.Equal(30, await db.SortedSetLengthAsync(Bucket(host, s)));
        }
        Assert.Equal(10, accounts.Count);
        var origin = Assert.Single(host.Quotas.Last!.Scopes, x => x.ScopeType == AuthenticationQuotaScopeType.Origin);
        Assert.Equal(300, await db.SortedSetLengthAsync(Bucket(host, origin)));
        var before = await QuotaSnapshotAsync(host);
        using var rejected = await host.SendAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode); Assert.InRange(rejected.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 60);
        Assert.Equal(301, host.Quotas.Calls); Assert.Equal(300, host.Revocations.Writes);
        var fresh = Assert.Single(host.Quotas.Last!.Scopes, x => x.ScopeType == AuthenticationQuotaScopeType.Account);
        Assert.False(await db.KeyExistsAsync(Bucket(host, fresh))); Assert.Equal(before, await QuotaSnapshotAsync(host));
        // Ambos scopes bloquean: el Retry-After HTTP coincide con el máximo real, usando Redis TIME.
        var timeBefore = await RedisMicrosecondsAsync(db);
        using var both = await host.SendAsync(firstToken!);
        var timeAfter = await RedisMicrosecondsAsync(db);
        Assert.Equal(HttpStatusCode.TooManyRequests, both.StatusCode);
        var releases = new List<double>();
        foreach (var s in host.Quotas.Last!.Scopes)
        {
            var entries = await db.SortedSetRangeByRankWithScoresAsync(Bucket(host, s));
            Assert.Equal(s.Definition.PermitLimit, entries.Length);
            releases.Add(entries[0].Score + 60_000_000);
        }
        var retry = both.Headers.RetryAfter!.Delta!.Value.TotalSeconds;
        Assert.InRange(retry, Math.Ceiling((releases.Max() - timeAfter) / 1_000_000), Math.Ceiling((releases.Max() - timeBefore) / 1_000_000));
        Assert.Equal(302, host.Quotas.Calls); Assert.Equal(300, host.Revocations.Writes); Assert.Equal(before, await QuotaSnapshotAsync(host));
    }

    [Theory, Trait("Task", "045")]
    [InlineData("body")] [InlineData("NoOperation")] [InlineData("Unavailable")] [InlineData("Uncertain")]
    public async Task Post_quota_failures_keep_one_charge_in_both_buckets_and_emit_only_safe_events(string failure)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; var input = await host.InputAsync(token); scope.Track(input);
        if (failure != "body") host.Revocations.WriteOutcome = Enum.Parse<TokenRevocationWriteStatus>(failure);
        await ProblemAsync(await host.SendAsync(token, body: failure == "body" ? new StringContent("{}") : null),
            failure == "body" ? 400 : 503, failure == "body" ? "validation_failed" : "temporarily_unavailable");
        Assert.Equal(1, host.Quotas.Calls); Assert.Equal(failure == "body" ? 0 : 1, host.Revocations.Writes);
        var db = await DatabaseAsync(host);
        foreach (var s in host.Quotas.Last!.Scopes) Assert.Equal(1, await db.SortedSetLengthAsync(Bucket(host, s)));
        Assert.Contains("Authentication logout " + (failure == "body" ? "rejected" : failure == "Uncertain" ? "uncertain" : "dependency_unavailable"), scope.Logs.Messages);
        Assert.All(scope.Logs.Messages, m => Assert.Contains(m, SafeLogoutEvents));
        var logs = string.Join('\n', scope.Logs.Messages);
        foreach (var secret in new[] { token, input.Jti, "127.0.0.1", scope.MarkKey(input), input.Account.ToString()! })
            if (secret != nameof(AuthenticationAuthenticatedAccount)) Assert.DoesNotContain(secret, logs);
    }

    [Theory, Trait("Task", "045")]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable)] [InlineData(AuthenticationQuotaStoreStatus.Uncertain)]
    public async Task Quota_failure_is_503_without_revocation_and_never_refunds_a_hidden_admission(AuthenticationQuotaStoreStatus outcome)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First; host.Quotas.Outcome = outcome;
        await ProblemAsync(await host.SendAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!, body: new StringContent("{}")), 503, "temporarily_unavailable");
        Assert.Equal(1, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
        var db = await DatabaseAsync(host);
        foreach (var s in host.Quotas.Last!.Scopes) Assert.Equal(outcome == AuthenticationQuotaStoreStatus.Uncertain ? 1 : 0, await db.SortedSetLengthAsync(Bucket(host, s)));
        Assert.Contains("Authentication logout " + (outcome == AuthenticationQuotaStoreStatus.Uncertain ? "uncertain" : "dependency_unavailable"), scope.Logs.Messages);
    }

    private static readonly string[] SafeLogoutEvents = ["Authentication logout confirmed", "Authentication logout rejected",
        "Authentication logout rate_limited", "Authentication logout dependency_unavailable", "Authentication logout uncertain"];
    private static async Task<IDatabase> DatabaseAsync(LogoutTestHost host) =>
        (await host.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase();
    private static async Task<double> RedisMicrosecondsAsync(IDatabase db)
    {
        var time = (RedisResult[])(await db.ExecuteAsync("TIME"))!;
        return (long)time[0] * 1_000_000d + (long)time[1];
    }
    private static string Bucket(LogoutTestHost host, AuthenticationQuotaScope s) =>
        host.Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Bucket(AuthenticationOperation.Logout, s.ScopeType, s.OpaqueId);
    private static async Task<string[]> QuotaSnapshotAsync(LogoutTestHost host)
    {
        var db = await DatabaseAsync(host); var configuration = host.Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>();
        var values = new List<string>();
        foreach (var entry in await db.SortedSetRangeByRankWithScoresAsync(configuration.Keys.Registry))
        {
            var key = configuration.Keys.CandidateBucket(entry.Element.ToString());
            values.Add(key + ":" + entry.Score.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + (long)await db.ExecuteAsync("PEXPIRETIME", key));
            foreach (var e in await db.SortedSetRangeByRankWithScoresAsync(key)) values.Add(e.Element + ":" + e.Score.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        return values.ToArray();
    }

    [Fact, Trait("Fix", "JwtChallenge401")]
    public async Task Configured_scheme_challenge_on_MVC_metadata_keeps_one_Bearer_and_safe_problem()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        using var services = host.Services.CreateScope();
        var http = new Microsoft.AspNetCore.Http.DefaultHttpContext { RequestServices = services.ServiceProvider };
        http.Request.Method = "POST"; http.Request.Path = "/api/v1/auth/logout"; http.Response.Body = new MemoryStream();
        var endpoint = Assert.Single(host.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>().Endpoints
            .OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>(), e => e.RoutePattern.RawText == "api/v1/auth/logout");
        Microsoft.AspNetCore.Http.EndpointHttpContextExtensions.SetEndpoint(http, endpoint);
        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>>().Get(AuthenticationPolicies.Scheme);
        Assert.False(options.IncludeErrorDetails);
        await new RequestContextMiddleware(c => Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.ChallengeAsync(c, AuthenticationPolicies.Scheme)).InvokeAsync(http);
        Assert.Equal(401, http.Response.StatusCode); Assert.Equal("Bearer", Assert.Single(http.Response.Headers.WWWAuthenticate));
        Assert.Equal("no-store", http.Response.Headers.CacheControl);
        http.Response.Body.Position = 0; using var json = await JsonDocument.ParseAsync(http.Response.Body);
        Assert.Equal("authentication_failed", json.RootElement.GetProperty("code").GetString());
        Assert.Equal(http.Response.Headers[RequestContextMiddleware.CorrelationHeader].ToString(), json.RootElement.GetProperty("traceId").GetString());
        Assert.DoesNotContain("error_description", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, host.Revocations.Writes); Assert.Equal(0, host.Quotas.Calls);
    }

    [Theory, Trait("Fix", "Challenge401")]
    [InlineData(400, "validation_failed")] [InlineData(409, "registration_conflict")]
    [InlineData(429, "rate_limited")] [InlineData(503, "temporarily_unavailable")]
    public Task Register_shared_errors_keep_their_contract_without_Bearer(int status, string code) => RegistrationTestScope.SafelyAsync(async () =>
    {
        await using var scope = await RegistrationTestScope.CreateAsync(fixture);
        HttpResponseMessage response;
        if (status == 409)
        {
            using var created = await scope.SendAsync(); Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            response = await scope.SendAsync();
        }
        else if (status == 429)
        {
            for (var i = 0; i < 3; i++)
            { using var admitted = await scope.SendAsync(name: " "); Assert.Equal(HttpStatusCode.BadRequest, admitted.StatusCode); }
            response = await scope.SendAsync(name: " ");
        }
        else
        {
            if (status == 503) scope.Quotas.Outcome = Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Unavailable;
            response = await scope.SendAsync(name: " ");
        }
        using (response)
        {
            await scope.AssertProblemAsync(response, status, code);
            Assert.False(response.Headers.Contains("WWW-Authenticate"));
        }
    });

    [Theory, Trait("Task", "044")]
    [InlineData(null)] [InlineData("application/json")] [InlineData("text/plain")]
    public async Task Empty_body_is_204_without_response_content_type(string? contentType)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; scope.Track(await host.InputAsync(token));
        using var body = new ByteArrayContent([]);
        if (contentType is not null) body.Headers.ContentType = new(contentType);
        using var response = await host.SendAsync(token, body: body);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        Assert.Null(response.Content.Headers.ContentType); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal(1, host.Quotas.Calls); Assert.Equal(1, host.Revocations.Writes);
    }

    [Theory, Trait("Task", "044")]
    [InlineData("{}")] [InlineData("   ")] [InlineData("{\"jti\":\"untrusted\"}")]
    public async Task Any_nonempty_body_is_400_after_one_quota_and_never_writes(string text)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        await ProblemAsync(await host.SendAsync(token, body: new StringContent(text)), 400, "validation_failed");
        Assert.Equal(1, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
    }

    [Theory, Trait("Task", "044")]
    [InlineData("missing")] [InlineData("malformed")] [InlineData("expired")]
    public async Task Missing_malformed_or_expired_is_401_without_control_work(string defect)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = defect == "missing" ? null : defect == "malformed" ? "malformed" :
            new JwtTokenIssuer(host.Services.GetRequiredService<JwtKeyRing>(), new FixedClock(DateTimeOffset.UtcNow.AddSeconds(-1000))).Issue(Guid.NewGuid()).AccessToken;
        await ProblemAsync(await host.SendAsync(token), 401, "authentication_failed");
        Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Revocations.Writes);
    }

    [Fact, Trait("Task", "044")]
    public async Task Excess_body_is_413_before_identity_and_quota()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        await ProblemAsync(await host.SendAsync(null, body: new ByteArrayContent(new byte[16385])), 413, "payload_too_large");
        Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Revocations.Writes);
    }

    [Theory, Trait("Task", "044")]
    [InlineData(TokenRevocationReadStatus.Unavailable)] [InlineData(TokenRevocationReadStatus.Uncertain)]
    public async Task Unverifiable_policy_read_is_503_not_401(TokenRevocationReadStatus status)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        host.Revocations.ReadOutcome = status;
        await ProblemAsync(await host.SendAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!), 503, "temporarily_unavailable");
        Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
    }

    [Fact, Trait("Task", "044")]
    public async Task Revoked_token_only_uses_exact_POST_logout_exception()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!; scope.Track(await host.InputAsync(token));
        using var first = await host.SendAsync(token); Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        await ProblemAsync(await host.SendAsync(token, "/_logout/protected", HttpMethod.Get), 401, "authentication_failed");
        await ProblemAsync(await host.SendAsync(token, method: HttpMethod.Get), 401, "authentication_failed");
        await ProblemAsync(await host.SendAsync(token, "/_logout/copied"), 401, "authentication_failed");
        using var second = await host.SendAsync(token); Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(2, host.Revocations.Writes);
    }

    [Fact, Trait("Task", "043")]
    public async Task Real_retention_repeat_and_concurrent_attempts_each_confirm_without_readback()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture);
        var a = scope.First; var b = await scope.AddHostAsync();
        var token = a.Issuer.Issue(Guid.NewGuid()).AccessToken!; var input = await a.InputAsync(token); var key = scope.MarkKey(input);
        Assert.Equal(LogoutStatus.Confirmed, await ApplyAsync(a, input));
        var db = (await a.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase();
        Assert.Equal("1", (string?)await db.StringGetAsync(key));
        var first = (long)await db.ExecuteAsync("PEXPIRETIME", key);
        Assert.True(first >= input.ExpiresAtUtc.AddSeconds(90).ToUnixTimeMilliseconds());
        Assert.True(await db.KeyTimeToLiveAsync(key) > TimeSpan.Zero);
        Assert.Equal(LogoutStatus.Confirmed, await ApplyAsync(b, input));
        Assert.Equal(first, (long)await db.ExecuteAsync("PEXPIRETIME", key));
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<LogoutStatus> Run(LogoutTestHost h) { await start.Task; return await ApplyAsync(h, input); }
        var x = Run(a); var y = Run(b); start.SetResult();
        Assert.All(await Task.WhenAll(x, y), status => Assert.Equal(LogoutStatus.Confirmed, status));
        Assert.Equal(4, a.Revocations.Writes + b.Revocations.Writes); Assert.Equal(0, a.Revocations.Reads + b.Revocations.Reads);
        Assert.Equal(4, a.Quotas.Calls + b.Quotas.Calls); Assert.Equal(first, (long)await db.ExecuteAsync("PEXPIRETIME", key));
    }

    [Theory, Trait("Task", "043")]
    [InlineData(TokenRevocationWriteStatus.Unavailable)] [InlineData(TokenRevocationWriteStatus.NoOperation)]
    [InlineData(TokenRevocationWriteStatus.Uncertain)]
    public async Task Nonconfirmation_is_not_success_and_lost_response_can_be_confirmed_by_a_new_attempt(TokenRevocationWriteStatus status)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var input = await host.InputAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!); var key = scope.MarkKey(input);
        host.Revocations.WriteOutcome = status;
        Assert.Equal(LogoutStatus.Unavailable, await ApplyAsync(host, input));
        Assert.Equal(1, host.Revocations.Writes); Assert.Equal(0, host.Revocations.Reads); Assert.Equal(1, host.Quotas.Calls);
        var db = (await host.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase();
        Assert.Equal(status == TokenRevocationWriteStatus.Uncertain, await db.KeyExistsAsync(key));
        host.Revocations.WriteOutcome = null;
        Assert.Equal(LogoutStatus.Confirmed, await ApplyAsync(host, input));
        Assert.Equal(2, host.Revocations.Writes); Assert.Equal(0, host.Revocations.Reads);
    }

    [Fact, Trait("Task", "043")]
    public async Task Mongo_unavailable_does_not_block_real_authority_quota_and_revocation()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture, mongoUnavailable: true); var host = scope.First;
        Assert.NotEqual(Enver.API.Infrastructure.MongoDB.MongoUsersInitializationStatus.Ready,
            host.Services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
        var input = await host.InputAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!); scope.Track(input);
        Assert.Equal(LogoutStatus.Confirmed, await ApplyAsync(host, input)); Assert.Equal(1, host.Revocations.Writes);
    }

    [Fact, Trait("Task", "043")]
    public async Task Canceled_or_exhausted_budget_does_not_dispatch_and_cancel_after_ACK_does_not_succeed()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var input = await host.InputAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!); scope.Track(input);
        using var cancel = new CancellationTokenSource(); cancel.Cancel();
        using var canceled = new RequestBudget(4000, cancel.Token);
        Assert.Equal(LogoutStatus.Unavailable, (await host.Services.GetRequiredService<Enver.API.Features.Authentication.Application.AuthenticationService>()
            .LogoutAsync(input, Origin(), true, canceled)).Status);
        var clock = new BudgetClock(); using var exhausted = new RequestBudget(4000, default, clock); clock.Stamp += 4 * clock.TimestampFrequency;
        Assert.Equal(LogoutStatus.Unavailable, (await host.Services.GetRequiredService<Enver.API.Features.Authentication.Application.AuthenticationService>()
            .LogoutAsync(input, Origin(), true, exhausted)).Status);
        Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
        using var late = new CancellationTokenSource(); host.Revocations.AfterWrite = late.Cancel;
        using var budget = new RequestBudget(4000, late.Token);
        Assert.Equal(LogoutStatus.Unavailable, (await host.Services.GetRequiredService<Enver.API.Features.Authentication.Application.AuthenticationService>()
            .LogoutAsync(input, Origin(), true, budget)).Status);
        Assert.Equal(1, host.Revocations.Writes); Assert.Equal(0, host.Revocations.Reads);
    }

    private static Enver.API.Features.Authentication.Application.Quotas.AuthenticationCanonicalOrigin Origin() =>
        Enver.API.Features.Authentication.Application.Quotas.AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback);
    private static async Task<LogoutStatus> ApplyAsync(LogoutTestHost host, LogoutInput input)
    {
        using var budget = new RequestBudget(4000, default);
        return (await host.Services.GetRequiredService<Enver.API.Features.Authentication.Application.AuthenticationService>()
            .LogoutAsync(input, Origin(), true, budget)).Status;
    }
    private sealed class BudgetClock : TimeProvider { public long Stamp; public override long GetTimestamp() => Stamp; }
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }
    private static async Task ProblemAsync(HttpResponseMessage response, int status, string code)
    {
        using (response)
        {
            Assert.Equal(status, (int)response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(status == 401 ? "Bearer" : null, response.Headers.WwwAuthenticate.SingleOrDefault()?.ToString());
            if (status == 401) Assert.Equal("Bearer", Assert.Single(response.Headers.GetValues("WWW-Authenticate")));
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32()); Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
            Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), json.RootElement.GetProperty("traceId").GetString());
            if (status == 401)
            {
                Assert.Equal(new[] { "code", "detail", "status", "title", "traceId", "type" }, json.RootElement.EnumerateObject().Select(p => p.Name).Order());
                Assert.Equal("about:blank", json.RootElement.GetProperty("type").GetString());
                Assert.Equal("Unauthorized", json.RootElement.GetProperty("title").GetString());
                Assert.Equal("No fue posible autenticar la solicitud.", json.RootElement.GetProperty("detail").GetString());
                Assert.DoesNotContain("error_description", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

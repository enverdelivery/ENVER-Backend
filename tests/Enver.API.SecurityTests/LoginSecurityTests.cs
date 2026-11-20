using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Observability;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.Testing;
using Enver.API.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Enver.API.Configuration;
using Enver.API.Composition;
using Xunit.Abstractions;


namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class LoginSecurityTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact, Trait("Task", "051")]
    public async Task Issued_JWT_has_only_approved_claims_header_and_no_PII_or_secret_in_events()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; var user = await h.SeedAsync();
        using var response = await h.LoginAsync(); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var token = body.RootElement.GetProperty("accessToken").GetString()!;
        Assert.Equal(new[] { "accessToken", "expiresIn", "tokenType" }, body.RootElement.EnumerateObject().Select(p => p.Name).Order());
        Assert.True((await h.Services.GetRequiredService<JwtClaimsValidator>().ValidateAsync(token)).IsValid);
        using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.Split('.')[1])); using var header = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.Split('.')[0]));
        Assert.Equal(new[] { "aud", "exp", "iat", "iss", "jti", "sub" }, payload.RootElement.EnumerateObject().Select(p => p.Name).Order());
        Assert.All(header.RootElement.EnumerateObject(), p => Assert.Contains(p.Name, new[] { "alg", "kid", "typ" }));
        Assert.Equal("HS256", header.RootElement.GetProperty("alg").GetString()); Assert.False(string.IsNullOrWhiteSpace(header.RootElement.GetProperty("kid").GetString()));
        Assert.Equal(900, payload.RootElement.GetProperty("exp").GetInt64() - payload.RootElement.GetProperty("iat").GetInt64());
        var logs = string.Join('\n', scope.Logs.Messages);
        foreach (var forbidden in new[] { token, payload.RootElement.GetProperty("jti").GetString()!, user.Id.ToString("D"), user.FullName,
                     user.NormalizedEmail, user.PasswordHash, "Synthetic Password 123!", "127.0.0.1" }) Assert.DoesNotContain(forbidden, logs);
        Assert.DoesNotContain(user.NormalizedEmail, payload.RootElement.GetRawText()); Assert.DoesNotContain(user.FullName, payload.RootElement.GetRawText());
    }

    [Theory, Trait("Task", "051")]
    [InlineData("quota")] [InlineData("rehash")] [InlineData("authority")]
    public async Task Nonconfirmations_expose_only_503_and_safe_closed_events_never_a_token(string fault)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; await h.SeedAsync(legacy: fault == "rehash");
        if (fault == "quota") h.Quotas.Outcome = AuthenticationQuotaStoreStatus.Uncertain;
        if (fault == "rehash") h.Users.WriteOutcome = UserCredentialUpdateStatus.Uncertain;
        if (fault == "authority") h.Authority.ReadOutcome = AuthenticationSecurityAuthorityResultStatus.Uncertain;
        await ProblemAsync(await h.LoginAsync(), 503, "temporarily_unavailable"); Assert.Equal(0, h.Issuer.Calls + h.Users.Reads);
        Assert.Single(scope.Logs.Messages, m => m == "Authentication login uncertain");
    }

    [Fact, Trait("Task", "051")]
    public async Task Security_rehash_race_has_one_winner_no_loop_and_only_closed_observability()
    {
        // Igual que el IT A/B: verifica el CAS concurrente, no el agotamiento del perfil sintético de 1 s.
        // La reproducción acreditó RequestBudget en la reverificación y emisión; no cambia producción.
        const int raceBudgetMilliseconds = 4000;
        await using var scope = await LoginTestScope.CreateAsync(fixture,
            testOnlyLoginBudgetMilliseconds: raceBudgetMilliseconds, captureRaceDiagnostics: true);
        var a = scope.First; var b = await scope.AddHostAsync();
        Assert.Equal("1000", fixture.CreateConfiguration()["Authentication:LoginBudgetMilliseconds"]);
        Assert.All(new[] { a, b }, host =>
        {
            Assert.Equal(raceBudgetMilliseconds, host.Services.GetRequiredService<IOptions<AuthenticationOptions>>().Value.LoginBudgetMilliseconds);
            Assert.Equal(500, host.Services.GetRequiredService<IOptions<MongoDbOptions>>().Value.OperationTimeoutMilliseconds);
            Assert.Equal(250, host.Services.GetRequiredService<IOptions<RedisOptions>>().Value.OperationTimeoutMilliseconds);
            Assert.Equal(1, host.Services.GetRequiredService<HashingConcurrencyGate>().Limit);
        });
        await a.SeedAsync(secret: "short", legacy: true); var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var arrived = 0;
        async Task Barrier() { if (Interlocked.Increment(ref arrived) == 2) ready.SetResult(); await ready.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        a.Users.BeforeWrite = Barrier; b.Users.BeforeWrite = Barrier;
        var results = await Task.WhenAll(a.LoginAsync(password: "short"), b.LoginAsync(password: "short"));
        for (var i = 0; i < results.Length; i++)
        {
            var host = i == 0 ? a : b;
            string? code = null;
            if (results[i].StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                using var problem = JsonDocument.Parse(await results[i].Content.ReadAsStringAsync());
                code = problem.RootElement.GetProperty("code").GetString() == "temporarily_unavailable" ? "temporarily_unavailable" : "other";
            }
            output.WriteLine("LOGIN_RACE_SAFE " + JsonSerializer.Serialize(new
            {
                Host = i == 0 ? "A" : "B", Status = (int)results[i].StatusCode, Code = code,
                LoginBudgetMilliseconds = host.Services.GetRequiredService<IOptions<AuthenticationOptions>>().Value.LoginBudgetMilliseconds,
                Index = host.Services.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus.ToString(),
                DummyReady = host.Services.GetRequiredService<DummyPasswordHash>().IsInitialized,
                HashingLimit = host.Services.GetRequiredService<HashingConcurrencyGate>().Limit,
                host.Users.Lookups, host.Users.Writes, host.Users.Reads, host.Kdf.Hashes, host.Kdf.Verifications,
                Issuances = host.Issuer.Calls, Steps = host.RaceDiagnostics!.Snapshot
            }));
        }
        foreach (var r in results) { using (r) Assert.Equal(HttpStatusCode.OK, r.StatusCode); }
        Assert.Equal(2, a.Users.Writes + b.Users.Writes); Assert.Equal(1, a.Users.Reads + b.Users.Reads); Assert.Equal(2, a.Issuer.Calls + b.Issuer.Calls);
        Assert.Equal(2, scope.Logs.Messages.Count(m => m == "Authentication login success"));
        Assert.Single(scope.Logs.Messages, m => m == "Authentication rehash rehash_updated"); Assert.Single(scope.Logs.Messages, m => m == "Authentication rehash rehash_race_lost");
        Assert.All(scope.Logs.Messages, m => Assert.Contains(m, new[] { "Authentication login success", "Authentication rehash rehash_updated", "Authentication rehash rehash_race_lost" }));
    }

    public static TheoryData<string, bool> InvalidJson => new()
    {
        { "", false }, { "   ", false }, { "null", false }, { "[]", false }, { "1", false }, { "{", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"x\",}", false },
        { "{/*comment*/\"email\":\"login@uttt.edu.mx\",\"password\":\"x\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"x\",\"password\":\"y\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"x\",\"pass\\u0077ord\":\"y\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"em\\u0061il\":\"other@uttt.edu.mx\",\"password\":\"x\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":null}", true },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":12}", true },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":true}", true },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":[]}", true },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":{\"$ne\":null}}", true },
        { "{\"email\":{\"$ne\":null},\"password\":\"x\"}", false },
        { "{\"email\":null,\"password\":\"x\"}", false },
        { "{\"Email\":\"login@uttt.edu.mx\",\"password\":\"x\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"Password\":\"x\"}", true },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"x\",\"admin\":true}", true },
        { "{\"email\":\"login@uttt.edu.mx\"}", true },
        { "{\"password\":\"x\"}", false },
        // Un surrogate aislado impide materializar JSON Unicode válido: frontera ilegible, no defecto secundario.
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"\\uD800\"}", false },
        { "{\"email\":\"login@uttt.edu.mx\",\"password\":\"x\",\"nested\":{\"a\":{\"b\":{\"c\":{}}}}}", false }
    };

    [Theory, MemberData(nameof(InvalidJson)), Trait("Task", "051")]
    public async Task HTTP_strict_JSON_never_reaches_lookup_or_KDF_and_secondary_defects_keep_joint_charge(string json, bool charge)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        await ProblemAsync(await h.SendAsync(new StringContent(json, Encoding.UTF8, "application/json")), 400, "validation_failed");
        Assert.Equal(charge ? 1 : 0, h.Quotas.Calls); Assert.Equal(0, h.Users.Lookups + h.Kdf.Verifications + h.Issuer.Calls);
        Assert.Single(scope.Logs.Messages, m => m == "Authentication login rejected");
    }

    [Theory, Trait("Task", "051")]
    [InlineData("text/plain", null)] [InlineData("application/json; charset=utf-16", null)]
    [InlineData("application/json; charset=iso-8859-1", null)] [InlineData("application/json; other=x", null)]
    [InlineData("application/json; charset=utf-8; charset=utf-8", null)]
    [InlineData("application/json", "gzip")] [InlineData("application/json", "br")]
    public async Task Unsupported_media_or_encoding_is_415_before_preflight_and_quota(string media, string? encoding)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        h.Authority.ReadOutcome = AuthenticationSecurityAuthorityResultStatus.Unavailable;
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes("{}")); content.Headers.TryAddWithoutValidation("Content-Type", media);
        if (encoding is not null) content.Headers.ContentEncoding.Add(encoding);
        await ProblemAsync(await h.SendAsync(content), 415, "unsupported_media_type"); Assert.Equal(0, h.Quotas.Calls + h.Users.Lookups);
        Assert.Equal("Authentication login rejected", Assert.Single(scope.Logs.Messages));
    }

    [Theory, Trait("Task", "051")]
    [InlineData(16384, false, 200)] [InlineData(16385, false, 413)]
    [InlineData(16384, true, 200)] [InlineData(16385, true, 413)]
    public async Task Real_byte_limit_applies_to_known_and_fragmented_unknown_length_streams(int length, bool fragmented, int status)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; await h.SeedAsync(secret: "short");
        var json = Encoding.UTF8.GetBytes("{\"email\":\"login@uttt.edu.mx\",\"password\":\"short\"}");
        var bytes = Enumerable.Repeat((byte)' ', length).ToArray(); json.CopyTo(bytes, 0);
        HttpContent content = fragmented ? new FragmentedContent(bytes) : new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        using var response = await h.SendAsync(content); Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal(status == 200 ? 1 : 0, h.Quotas.Calls); Assert.Equal(status == 200 ? 1 : 0, h.Issuer.Calls);
        Assert.Equal(status == 200 ? "Authentication login success" : "Authentication login rejected", Assert.Single(scope.Logs.Messages));
    }

    [Theory, Trait("Task", "051")]
    [InlineData("malformed", 503)] [InlineData("invalid-email", 503)] [InlineData("secondary", 503)]
    [InlineData("oversized", 413)] [InlineData("media", 415)]
    public async Task D012_preflight_failure_does_not_lose_transport_body_and_media_precedence(string kind, int status)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        h.Authority.ReadOutcome = AuthenticationSecurityAuthorityResultStatus.Unavailable;
        var json = kind switch { "malformed" => "{", "invalid-email" => "{\"email\":\"bad\",\"password\":\"x\"}", "oversized" => new string('x', 16385), _ => "{\"email\":\"login@uttt.edu.mx\",\"password\":null}" };
        using var response = await h.SendAsync(new StringContent(json, Encoding.UTF8, kind == "media" ? "text/plain" : "application/json"));
        Assert.Equal(status, (int)response.StatusCode); Assert.Equal(0, h.Quotas.Calls + h.Users.Lookups + h.Kdf.Verifications);
    }

    [Theory, Trait("Task", "051")]
    [InlineData("login@uttt.edu.mx.evil")] [InlineData("login@uttt.edu.mx\u200b")] [InlineData("login@uttt．edu.mx")]
    [InlineData("lógin@uttt.edu.mx")] [InlineData("a@@uttt.edu.mx")] [InlineData("{$ne:null}")]
    public async Task Noncanonical_or_injection_email_never_creates_arbitrary_buckets(string email)
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        await ProblemAsync(await h.LoginAsync(email), 400, "validation_failed");
        Assert.Equal(0, h.Quotas.Calls + h.Users.Lookups + h.Kdf.Verifications);
    }

    [Theory, Trait("Task", "051")]
    [InlineData("short")] [InlineData("spaces")] [InlineData("unicode")] [InlineData("128")] [InlineData("512")]
    public async Task Existing_credentials_allow_short_spaces_and_Unicode_without_mutating_the_secret(string kind)
    {
        var secret = kind switch { "short" => "x", "spaces" => "  x  ", "unicode" => "é", "128" => new string('x', 128), _ => string.Concat(Enumerable.Repeat("\U0001F600", 128)) };
        var altered = kind switch { "spaces" => secret.Trim(), "unicode" => "e\u0301", _ => secret.ToUpperInvariant() + "z" };
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First; await h.SeedAsync(secret: secret);
        using (var success = await h.LoginAsync(password: secret)) Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        using var rejected = await h.LoginAsync(password: altered); Assert.NotEqual(HttpStatusCode.OK, rejected.StatusCode);
        Assert.Equal(1, h.Issuer.Calls);
    }

    [Theory, Trait("Task", "051")]
    [InlineData("empty")] [InlineData("129")] [InlineData("513")]
    public async Task Out_of_bounds_password_keeps_quota_but_never_enters_KDF(string kind)
    {
        var secret = kind == "empty" ? "" : kind == "129" ? new string('x', 129) : string.Concat(Enumerable.Repeat("\U0001F600", 128)) + "a";
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        await ProblemAsync(await h.LoginAsync(password: secret), 400, "validation_failed");
        Assert.Equal(1, h.Quotas.Calls); Assert.Equal(0, h.Kdf.Verifications + h.Users.Lookups + h.Issuer.Calls);
    }

    [Fact, Trait("Task", "051")]
    public async Task Enumeration_and_Disabled_have_same_public_error_and_no_zero_KDF_shortcut_not_a_constant_time_claim()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        await h.SeedAsync(); await h.SeedAsync("disabled@uttt.edu.mx", status: UserStatus.Disabled);
        var signatures = new List<string>(); var hashes = h.Kdf.Hashes;
        foreach (var email in new[] { "missing@uttt.edu.mx", "login@uttt.edu.mx", "disabled@uttt.edu.mx" })
        {
            var work = h.Kdf.Verifications;
            using var response = await h.LoginAsync(email, "Wrong Password 123!"); Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var root = json.RootElement;
            signatures.Add(string.Join('|', new[] { "type", "title", "status", "detail", "code" }.Select(p => root.GetProperty(p).ToString())));
            Assert.Equal("Bearer", Assert.Single(response.Headers.GetValues("WWW-Authenticate")));
            Assert.Equal(work + 1, h.Kdf.Verifications);
        }
        Assert.Single(signatures.Distinct()); Assert.Equal(hashes, h.Kdf.Hashes); Assert.Equal(0, h.Issuer.Calls + h.Users.Writes);
        Assert.Equal(3, scope.Logs.Messages.Count); Assert.All(scope.Logs.Messages, m => Assert.Equal("Authentication login rejected", m));
        // Conteo de trabajo real: deliberadamente sin umbrales temporales ni afirmación constant-time.
    }

    [Fact, Trait("Task", "051")]
    public async Task Invalid_UTF8_is_not_replaced_or_coerced()
    {
        await using var scope = await LoginTestScope.CreateAsync(fixture); var h = scope.First;
        var content = new ByteArrayContent([0xc0, 0xaf]); content.Headers.ContentType = new("application/json");
        await ProblemAsync(await h.SendAsync(content), 400, "validation_failed"); Assert.Equal(0, h.Quotas.Calls + h.Kdf.Verifications);
    }
    private static async Task ProblemAsync(HttpResponseMessage response, int status, string code)
    {
        using (response) { Assert.Equal(status, (int)response.StatusCode); using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(code, json.RootElement.GetProperty("code").GetString()); Assert.Equal("no-store", response.Headers.CacheControl?.ToString()); }
    }
    private sealed class FragmentedContent(byte[] bytes) : HttpContent
    {
        protected override bool TryComputeLength(out long length) { length = 0; return false; }
        protected override async Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
        { for (var i = 0; i < bytes.Length; i += 7) { await stream.WriteAsync(bytes.AsMemory(i, Math.Min(7, bytes.Length - i))); await stream.FlushAsync(); } }
    }
}

public sealed class LoginEventSchemaTests
{
    [Theory, Trait("Task", "050")]
    [InlineData(AuthenticationEventKind.Login, AuthenticationEventOutcome.Success, "success")]
    [InlineData(AuthenticationEventKind.Login, AuthenticationEventOutcome.Rejected, "rejected")]
    [InlineData(AuthenticationEventKind.Login, AuthenticationEventOutcome.RateLimited, "rate_limited")]
    [InlineData(AuthenticationEventKind.Login, AuthenticationEventOutcome.Unavailable, "dependency_unavailable")]
    [InlineData(AuthenticationEventKind.Login, AuthenticationEventOutcome.Uncertain, "uncertain")]
    [InlineData(AuthenticationEventKind.Rehash, AuthenticationEventOutcome.RehashUpdated, "rehash_updated")]
    [InlineData(AuthenticationEventKind.Rehash, AuthenticationEventOutcome.RehashRaceLost, "rehash_race_lost")]
    public void Closed_schema_emits_no_request_labels(AuthenticationEventKind kind, AuthenticationEventOutcome outcome, string label)
    {
        var log = new Capture(); var events = new AuthenticationEvents(log); events.Record(kind, outcome);
        var fields = Assert.Single(log.Fields); Assert.Equal(new[] { "Operation", "Outcome", "{OriginalFormat}" }.Order(), fields.Keys.Order());
        Assert.Equal(kind == AuthenticationEventKind.Login ? "login" : "rehash", fields["Operation"]);
        Assert.Equal(label, fields["Outcome"]);
    }
    [Theory, Trait("Task", "050")]
    [InlineData(AuthenticationEventKind.Login)] [InlineData(AuthenticationEventKind.Rehash)]
    public void Other_defined_and_unknown_outcomes_fail_without_an_event(AuthenticationEventKind kind)
    {
        var log = new Capture(); var events = new AuthenticationEvents(log);
        var allowed = kind == AuthenticationEventKind.Login ? new[] { AuthenticationEventOutcome.Success, AuthenticationEventOutcome.Rejected, AuthenticationEventOutcome.RateLimited, AuthenticationEventOutcome.Unavailable, AuthenticationEventOutcome.Uncertain }
            : new[] { AuthenticationEventOutcome.RehashUpdated, AuthenticationEventOutcome.RehashRaceLost };
        foreach (var outcome in Enum.GetValues<AuthenticationEventOutcome>().Where(o => !allowed.Contains(o)).Append((AuthenticationEventOutcome)999))
            Assert.Throws<ArgumentOutOfRangeException>(() => events.Record(kind, outcome));
        Assert.Empty(log.Fields);
    }
    private sealed class Capture : ILogger<AuthenticationEvents>
    {
        public List<Dictionary<string, object?>> Fields { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter) =>
            Fields.Add(((IEnumerable<KeyValuePair<string, object?>>)state!).ToDictionary());
    }
}

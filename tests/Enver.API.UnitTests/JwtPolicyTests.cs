using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using AuthenticationOptions = Enver.API.Configuration.AuthenticationOptions;

namespace Enver.API.UnitTests;

public sealed class JwtPolicyTests
{
    [Fact, Trait("Task", "038")]
    public async Task Issuer_emits_exact_claims_canonical_ids_and_one_floored_clock_read()
    {
        var f = new Fixture(); var sub = Guid.NewGuid();
        var first = f.Issuer.Issue(sub); Assert.Equal(1, f.Clock.Reads);
        var second = f.Issuer.Issue(sub); Assert.Equal(2, f.Clock.Reads);
        Assert.Equal(AccessTokenIssueStatus.Issued, first.Status);
        Assert.Equal(nameof(AccessTokenIssueResult), first.ToString());
        using var p = Payload(first.AccessToken!); using var q = Payload(second.AccessToken!);
        Assert.Equal(new[] { "aud", "exp", "iat", "iss", "jti", "sub" }, p.RootElement.EnumerateObject().Select(x => x.Name).Order());
        Assert.Equal(sub.ToString("D"), p.RootElement.GetProperty("sub").GetString());
        Assert.Equal(f.Clock.Now.ToUnixTimeSeconds(), p.RootElement.GetProperty("iat").GetInt64());
        Assert.Equal(900, p.RootElement.GetProperty("exp").GetInt64() - p.RootElement.GetProperty("iat").GetInt64());
        var jti = p.RootElement.GetProperty("jti").GetString()!;
        Assert.Equal(43, jti.Length); Assert.Equal(32, Base64UrlEncoder.DecodeBytes(jti).Length);
        Assert.True(JwtClaimsValidator.IsCanonicalJti(jti)); Assert.NotEqual(jti, q.RootElement.GetProperty("jti").GetString());
        Assert.Equal(0, first.IssuedAtUtc!.Value.Ticks % TimeSpan.TicksPerSecond);
        Assert.True((await f.Validator.ValidateAsync(first.AccessToken!)).IsValid);
    }

    [Theory, Trait("Task", "038")]
    [InlineData("algorithm")][InlineData("issuer")][InlineData("audience")][InlineData("life")]
    [InlineData("skew-low")][InlineData("skew-high")][InlineData("skew-null")]
    [InlineData("active")][InlineData("duplicate")][InlineData("empty-kid")]
    [InlineData("short")][InlineData("base64")][InlineData("noncanonical")][InlineData("hmac")]
    public void Invalid_configuration_blocks_snapshot_without_disclosing_material(string defect)
    {
        var f = new Fixture(); var o = f.Options(); var rate = f.Rate();
        switch (defect)
        {
            case "algorithm": o.Algorithm = "HS384"; break;
            case "issuer": o.Issuer = "other"; break;
            case "audience": o.Audience = "other"; break;
            case "life": o.LifetimeSeconds = 901; break;
            case "skew-low": o.ClockSkewSeconds = -1; break;
            case "skew-high": o.ClockSkewSeconds = 31; break;
            case "skew-null": o.ClockSkewSeconds = null; break;
            case "active": o.ActiveKid = "missing"; break;
            case "duplicate": o.Keys.Add(o.Keys[0]); break;
            case "empty-kid": o.Keys[0]!.Kid = " "; break;
            case "short": o.Keys[0]!.KeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(31)); break;
            case "base64": o.Keys[0]!.KeyBase64 = "invalid"; break;
            case "noncanonical": o.Keys[0]!.KeyBase64 += "\n"; break;
            case "hmac": rate.PseudonymizationKeyBase64 = o.Keys[0]!.KeyBase64; break;
        }
        var error = Assert.Throws<OptionsValidationException>(() => new JwtKeyRing(o, rate));
        Assert.DoesNotContain(Convert.ToBase64String(f.Key), error.ToString());
    }

    [Fact, Trait("Task", "038")]
    public async Task Snapshot_does_not_follow_mutated_options_and_invalid_subject_is_unavailable()
    {
        var f = new Fixture(); var options = f.Options(); var ring = new JwtKeyRing(options, f.Rate());
        options.Keys.Clear(); options.ActiveKid = "missing"; options.ClockSkewSeconds = 0;
        var issued = new JwtTokenIssuer(ring, f.Clock).Issue(Guid.NewGuid());
        Assert.True((await new JwtClaimsValidator(ring, f.Clock).ValidateAsync(issued.AccessToken!)).IsValid);
        var failed = f.Issuer.Issue(Guid.Empty);
        Assert.Equal(AccessTokenIssueStatus.Unavailable, failed.Status); Assert.Null(failed.AccessToken); Assert.Null(failed.ExpiresAtUtc);
    }

    [Theory, Trait("Task", "039")]
    [InlineData(30, 8999999999L, true)]
    [InlineData(30, 9000000000L, true)]
    [InlineData(30, 9299999999L, true)]
    [InlineData(30, 9300000000L, false)]
    [InlineData(30, 9300000001L, false)]
    [InlineData(0, 8999999999L, true)]
    [InlineData(0, 9000000000L, false)]
    public async Task Expiry_has_exclusive_precise_boundary(int skew, long ticks, bool expected)
    {
        var f = new Fixture(skew); var token = f.Token();
        f.Clock.Now = DateTimeOffset.FromUnixTimeSeconds(Fixture.Epoch).AddTicks(ticks);
        Assert.Equal(expected, (await f.Validator.ValidateAsync(token)).IsValid);
    }

    [Theory, Trait("Task", "039")]
    [InlineData("iat", 30, true)][InlineData("iat", 31, false)]
    [InlineData("nbf", 30, true)][InlineData("nbf", 31, false)]
    public async Task Future_claims_allow_only_configured_skew(string claim, int seconds, bool expected)
    {
        var f = new Fixture(); f.Clock.Now = DateTimeOffset.FromUnixTimeSeconds(Fixture.Epoch);
        var p = f.Claims(); p[claim] = Fixture.Epoch + seconds;
        if (claim == "iat") p["exp"] = Fixture.Epoch + seconds + 900;
        Assert.Equal(expected, (await f.Validator.ValidateAsync(f.Sign(JsonSerializer.Serialize(p)))).IsValid);
    }

    [Theory, Trait("Task", "039")]
    [InlineData("malformed")][InlineData("size")][InlineData("missing-sub")][InlineData("missing-jti")]
    [InlineData("missing-iat")][InlineData("missing-exp")][InlineData("extra")][InlineData("dup-sub")]
    [InlineData("dup-jti")][InlineData("dup-escaped")][InlineData("iat-string")][InlineData("exp-fraction")]
    [InlineData("aud-array")][InlineData("aud-duplicate")][InlineData("iss-duplicate")]
    [InlineData("issuer")][InlineData("audience")][InlineData("sub-upper")][InlineData("sub-braces")]
    [InlineData("sub-nov4")][InlineData("jti-padding")][InlineData("jti-short")][InlineData("jti-noncanonical")]
    [InlineData("exp-equal")][InlineData("exp-short")][InlineData("nbf-after-exp")][InlineData("nbf-string")]
    [InlineData("kid-missing")][InlineData("kid-unknown")][InlineData("kid-duplicate")][InlineData("kid-number")]
    [InlineData("alg-none")][InlineData("alg-other")][InlineData("jku")][InlineData("x5u")][InlineData("crit")]
    [InlineData("json")][InlineData("utf8")][InlineData("signature")]
    public async Task Strict_profile_rejects_adversarial_tokens(string defect)
    {
        var f = new Fixture(); var p = f.Claims(); var header = "{\"alg\":\"HS256\",\"kid\":\"A\"}";
        string? raw = null;
        switch (defect)
        {
            case "malformed": raw = "a.b.c"; break;
            case "size": raw = new string('a', 4097); break;
            case "missing-sub": p.Remove("sub"); break;
            case "missing-jti": p.Remove("jti"); break;
            case "missing-iat": p.Remove("iat"); break;
            case "missing-exp": p.Remove("exp"); break;
            case "extra": p["roles"] = "admin"; break;
            case "iat-string": p["iat"] = Fixture.Epoch.ToString(); break;
            case "exp-fraction": p["exp"] = Fixture.Epoch + 900.5; break;
            case "aud-array": p["aud"] = new[] { "enver-mobile" }; break;
            case "issuer": p["iss"] = "other"; break;
            case "audience": p["aud"] = "other"; break;
            case "sub-upper": p["sub"] = p["sub"].ToString()!.ToUpperInvariant(); break;
            case "sub-braces": p["sub"] = "{" + p["sub"] + "}"; break;
            case "sub-nov4": p["sub"] = "00000000-0000-1000-8000-000000000000"; break;
            case "jti-padding": p["jti"] += "="; break;
            case "jti-short": p["jti"] = "short"; break;
            case "jti-noncanonical": p["jti"] = new string('A', 42) + "B"; break;
            case "exp-equal": p["exp"] = p["iat"]; break;
            case "exp-short": p["exp"] = Fixture.Epoch + 899; break;
            case "nbf-after-exp": p["nbf"] = Fixture.Epoch + 901; break;
            case "nbf-string": p["nbf"] = "wrong"; break;
            case "kid-missing": header = "{\"alg\":\"HS256\"}"; break;
            case "kid-unknown": header = "{\"alg\":\"HS256\",\"kid\":\"a\"}"; break;
            case "kid-duplicate": header = "{\"alg\":\"HS256\",\"kid\":\"A\",\"kid\":\"A\"}"; break;
            case "kid-number": header = "{\"alg\":\"HS256\",\"kid\":1}"; break;
            case "alg-none": header = "{\"alg\":\"none\",\"kid\":\"A\"}"; break;
            case "alg-other": header = "{\"alg\":\"HS384\",\"kid\":\"A\"}"; break;
            case "jku": header = header[..^1] + ",\"jku\":\"https://invalid.example\"}"; break;
            case "x5u": header = header[..^1] + ",\"x5u\":\"https://invalid.example\"}"; break;
            case "crit": header = header[..^1] + ",\"crit\":[\"extension\"]}"; break;
            case "json": raw = f.Sign("{"); break;
            case "utf8": raw = f.SignBytes([0xff, 0xfe]); break;
        }
        var payload = JsonSerializer.Serialize(p);
        var duplicate = defect switch
        { "dup-sub" => "sub", "dup-jti" => "jti", "dup-escaped" => "s\\u0075b", "aud-duplicate" => "aud", "iss-duplicate" => "iss", _ => null };
        if (duplicate is not null) payload = payload[..^1] + ",\"" + duplicate + "\":\"duplicate\"}";
        var token = raw ?? f.Sign(payload, header);
        if (defect == "signature") token = token[..(token.LastIndexOf('.') + 1)] + Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var result = await f.Validator.ValidateAsync(token);
        Assert.False(result.IsValid); Assert.Null(result.SecurityToken);
        Assert.DoesNotContain(token, result.Exception.ToString());
    }

    [Fact, Trait("Task", "039")]
    public void Bearer_configuration_has_no_remote_or_legacy_route()
    {
        var f = new Fixture(); var options = new JwtBearerOptions(); new JwtBearerConfiguration(f.Validator).Configure(options);
        Assert.False(options.MapInboundClaims); Assert.False(options.SaveToken); Assert.False(options.IncludeErrorDetails);
        Assert.False(options.UseSecurityTokenValidators); Assert.False(options.RefreshOnIssuerKeyNotFound);
        Assert.Null(options.Authority); Assert.Null(options.MetadataAddress); Assert.Null(options.ConfigurationManager);
        Assert.Single(options.TokenHandlers); Assert.IsAssignableFrom<JsonWebTokenHandler>(options.TokenHandlers[0]);
        Assert.Equal(4096, options.TokenHandlers[0].MaximumTokenSizeInBytes);
    }

    [Theory, Trait("Task", "040")]
    [InlineData(TokenRevocationReadStatus.NotRevoked, 200)]
    [InlineData(TokenRevocationReadStatus.Revoked, 401)]
    [InlineData(TokenRevocationReadStatus.Unavailable, 503)]
    [InlineData(TokenRevocationReadStatus.Uncertain, 503)]
    public async Task Policy_distinguishes_identity_revocation_and_operational_failure(TokenRevocationReadStatus status, int expected)
    {
        var f = new Fixture(); using var h = new PolicyHarness(f, status);
        var http = await h.AuthenticateAsync(); await h.AuthorizeAsync(http);
        Assert.Equal(expected, http.Response.StatusCode); Assert.Equal("no-store", http.Response.Headers.CacheControl);
        if (expected == 401) Assert.Equal("Bearer", http.Response.Headers.WWWAuthenticate);
    }

    [Theory, Trait("Task", "040")]
    [InlineData(AuthenticationSecurityAdmission.Unavailable)][InlineData(AuthenticationSecurityAdmission.Uncertain)]
    public async Task Unverifiable_security_state_is_503_without_a_store_read(AuthenticationSecurityAdmission admission)
    {
        var f = new Fixture(); using var h = new PolicyHarness(f, TokenRevocationReadStatus.NotRevoked, admission);
        var http = await h.AuthenticateAsync(); await h.AuthorizeAsync(http);
        Assert.Equal(503, http.Response.StatusCode); Assert.Equal(0, h.Store.Reads);
    }

    [Theory, Trait("Task", "040")]
    [InlineData("POST", "/api/v1/auth/logout", true, true, 200)]
    [InlineData("GET", "/api/v1/auth/logout", true, true, 401)]
    [InlineData("POST", "/other", true, true, 401)]
    [InlineData("POST", "/api/v1/auth/logout", false, true, 401)]
    [InlineData("POST", "/api/v1/auth/logout", true, false, 401)]
    public async Task Logout_exception_requires_policy_metadata_method_and_exact_path(string method, string path, bool metadata, bool policy, int expected)
    {
        var f = new Fixture(); using var h = new PolicyHarness(f, TokenRevocationReadStatus.Revoked);
        var http = await h.AuthenticateAsync(); http.Request.Method = method; http.Request.Path = path;
        if (metadata) http.SetEndpoint(new Endpoint(_ => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthorizeAttribute(AuthenticationPolicies.Logout)), "test-context-only"));
        await h.AuthorizeAsync(http, policy ? AuthenticationPolicies.Logout : AuthenticationPolicies.Normal);
        Assert.Equal(expected, http.Response.StatusCode); Assert.Equal(0, h.Store.Writes);
    }

    [Fact, Trait("Task", "040")]
    public async Task Default_fallback_and_forged_identity_cannot_bypass_validated_feature()
    {
        var f = new Fixture(); using var h = new PolicyHarness(f, TokenRevocationReadStatus.NotRevoked);
        var options = h.Services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        Assert.Same(options.DefaultPolicy, options.FallbackPolicy);
        var http = h.Context(); http.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("jti", "forged")], "Bearer"));
        await h.AuthorizeAsync(http); Assert.Equal(401, http.Response.StatusCode); Assert.Equal(0, h.Store.Reads);
    }

    [Fact, Trait("Task", "041")]
    public async Task Rotation_distributes_before_activation_coexists_then_retires_at_930()
    {
        var f = new Fixture(); var original = f.Token();
        var b = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var both = f.Options(); both.Keys.Add(new() { Kid = "B", KeyBase64 = b });
        var coexist = new JwtClaimsValidator(new JwtKeyRing(both, f.Rate()), f.Clock);
        Assert.True((await coexist.ValidateAsync(original)).IsValid);
        both.ActiveKid = "B"; var ringB = new JwtKeyRing(both, f.Rate());
        var tokenB = new JwtTokenIssuer(ringB, f.Clock).Issue(Guid.NewGuid()).AccessToken!;
        var next = new JwtClaimsValidator(ringB, f.Clock);
        Assert.True((await coexist.ValidateAsync(tokenB)).IsValid); Assert.True((await next.ValidateAsync(original)).IsValid);
        var ceased = DateTimeOffset.FromUnixTimeSeconds(Fixture.Epoch);
        f.Clock.Now = ceased.AddSeconds(930).AddTicks(-1);
        Assert.False(JwtKeyRing.CanRetirePreviousKey(ceased, f.Clock.Now)); Assert.True((await next.ValidateAsync(original)).IsValid);
        f.Clock.Now = ceased.AddSeconds(930);
        Assert.True(JwtKeyRing.CanRetirePreviousKey(ceased, f.Clock.Now)); Assert.False((await next.ValidateAsync(original)).IsValid);
        both.Keys.RemoveAt(0); var retired = new JwtClaimsValidator(new JwtKeyRing(both, f.Rate()), f.Clock);
        // Con un token B nuevo la retirada no impide la nueva emisión/validación.
        var freshB = new JwtTokenIssuer(new JwtKeyRing(both, f.Rate()), f.Clock).Issue(Guid.NewGuid()).AccessToken!;
        Assert.True((await retired.ValidateAsync(freshB)).IsValid); Assert.False((await retired.ValidateAsync(original)).IsValid);
    }

    private static JsonDocument Payload(string token) => JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.Split('.')[1]));
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(Fixture.Epoch).AddTicks(1234567);
        public int Reads;
        public override DateTimeOffset GetUtcNow() { Reads++; return Now; }
    }
    private sealed class Fixture
    {
        public const long Epoch = 1800000000;
        public readonly byte[] Key = RandomNumberGenerator.GetBytes(32);
        private readonly string hmac = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        private readonly int skew;
        public Clock Clock { get; } = new();
        public JwtTokenIssuer Issuer { get; }
        public JwtClaimsValidator Validator { get; }
        public Fixture(int skew = 30)
        { this.skew = skew; var ring = new JwtKeyRing(Options(), Rate()); Issuer = new(ring, Clock); Validator = new(ring, Clock); }
        public JwtOptions Options() => new() { Algorithm = "HS256", Issuer = "enver-api", Audience = "enver-mobile", LifetimeSeconds = 900,
            ClockSkewSeconds = skew, ActiveKid = "A", Keys = [new() { Kid = "A", KeyBase64 = Convert.ToBase64String(Key) }] };
        public RateLimitingOptions Rate() => new() { PseudonymizationKeyBase64 = hmac };
        public Dictionary<string, object> Claims() => new() { ["sub"] = Guid.NewGuid().ToString("D"),
            ["jti"] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)), ["iss"] = "enver-api", ["aud"] = "enver-mobile", ["iat"] = Epoch, ["exp"] = Epoch + 900 };
        public string Token() => Issuer.Issue(Guid.NewGuid()).AccessToken!;
        public string Sign(string payload, string header = "{\"alg\":\"HS256\",\"kid\":\"A\"}") => SignBytes(Encoding.UTF8.GetBytes(payload), header);
        // Solo fixtures adversariales: preserva JSON malicioso sin que un serializer JWT lo normalice.
        public string SignBytes(byte[] payload, string header = "{\"alg\":\"HS256\",\"kid\":\"A\"}")
        {
            var compact = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(header)) + "." + Base64UrlEncoder.Encode(payload);
            return compact + "." + Base64UrlEncoder.Encode(HMACSHA256.HashData(Key, Encoding.ASCII.GetBytes(compact)));
        }
    }
    private sealed class PolicyHarness : IDisposable
    {
        private readonly Fixture f;
        public ServiceProvider Services { get; }
        public Store Store { get; }
        public PolicyHarness(Fixture f, TokenRevocationReadStatus status, AuthenticationSecurityAdmission admission = AuthenticationSecurityAdmission.Verifiable)
        {
            this.f = f; Store = new(status);
            var services = new ServiceCollection(); services.AddLogging(); ApiProblemDetails.AddSafeApiErrors(services);
            services.Configure<AuthenticationOptions>(o => o.LogoutBudgetMilliseconds = 1000);
            services.AddAuthorization(AuthenticationPolicies.Configure);
            services.AddSingleton<ITokenRevocationStore>(Store); services.AddSingleton<IAuthenticationSecurityState>(new State(admission));
            services.AddSingleton<IAuthorizationHandler, RevocationAuthorizationHandler>(); Services = services.BuildServiceProvider();
        }
        public DefaultHttpContext Context() { var http = new DefaultHttpContext { RequestServices = Services }; http.Response.Body = new MemoryStream(); return http; }
        public async Task<DefaultHttpContext> AuthenticateAsync()
        {
            var token = f.Token(); var validated = await f.Validator.ValidateAsync(token); Assert.True(validated.IsValid);
            var http = Context(); http.User = new ClaimsPrincipal(validated.ClaimsIdentity);
            var options = new JwtBearerOptions(); new JwtBearerConfiguration(f.Validator).Configure(options);
            var context = new TokenValidatedContext(http, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options)
            { Principal = http.User, SecurityToken = validated.SecurityToken };
            await options.Events.TokenValidated(context); Assert.Null(context.Result?.Failure); return http;
        }
        public async Task AuthorizeAsync(DefaultHttpContext http, string name = AuthenticationPolicies.Normal)
        {
            var result = await Services.GetRequiredService<IAuthorizationService>().AuthorizeAsync(http.User, http, name);
            var policy = (await Services.GetRequiredService<IAuthorizationPolicyProvider>().GetPolicyAsync(name))!;
            await new AuthenticationAuthorizationResultHandler().HandleAsync(_ => Task.CompletedTask, http, policy,
                result.Succeeded ? PolicyAuthorizationResult.Success() : PolicyAuthorizationResult.Forbid(result.Failure!));
        }
        public void Dispose() => Services.Dispose();
    }
    private sealed class State(AuthenticationSecurityAdmission admission) : IAuthenticationSecurityState
    { public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget budget) => ValueTask.FromResult(new AuthenticationSecurityDecision(admission)); }
    private sealed class Store(TokenRevocationReadStatus status) : ITokenRevocationStore
    {
        public int Reads; public int Writes;
        public ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget)
        { Reads++; return ValueTask.FromResult(new TokenRevocationReadResult(status)); }
        public ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget)
        { Writes++; throw new InvalidOperationException("No se permite escribir desde una política."); }
    }
}

using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ApplicationAuthenticationService = Enver.API.Features.Authentication.Application.AuthenticationService;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.UnitTests;

public sealed class LogoutTests
{
    [Fact, Trait("Task", "045")]
    public void RetryAfter_is_the_conservative_maximum_of_both_blocking_scopes()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1800000000);
        var account = AuthenticationQuotaRetryAfter.ForScope(now, new(30, TimeSpan.FromSeconds(60)), Enumerable.Repeat(now.AddSeconds(-40.1), 30));
        var origin = AuthenticationQuotaRetryAfter.ForScope(now, new(300, TimeSpan.FromSeconds(60)), Enumerable.Repeat(now.AddSeconds(-10.1), 300));
        Assert.Equal(TimeSpan.FromSeconds(20), account); Assert.Equal(TimeSpan.FromSeconds(50), origin);
        Assert.Equal(TimeSpan.FromSeconds(50), AuthenticationQuotaRetryAfter.Maximum([account, origin]));
        Assert.Equal(TimeSpan.FromSeconds(50), AuthenticationQuotaRetryAfter.Maximum([null, origin]));
    }

    [Theory, Trait("Task", "045")]
    [InlineData(AuthenticationQuotaStoreStatus.Rejected, LogoutStatus.RateLimited)]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable, LogoutStatus.Unavailable)]
    [InlineData(AuthenticationQuotaStoreStatus.Uncertain, LogoutStatus.Unavailable)]
    public async Task One_joint_quota_failure_precedes_body_and_never_dispatches_a_write(AuthenticationQuotaStoreStatus quota, LogoutStatus expected)
    {
        var s = new Scenario { Quota = quota }; using var budget = new RequestBudget(4000, default);
        var result = await s.Service.LogoutAsync(await InputAsync(), Origin(), false, budget);
        Assert.Equal(expected, result.Status); Assert.Equal(new[] { "admission", "quota" }, s.Calls);
        Assert.Equal(quota == AuthenticationQuotaStoreStatus.Rejected ? TimeSpan.FromSeconds(7) : (TimeSpan?)null, result.RetryAfter);
    }

    [Theory, Trait("Fix", "Challenge401")]
    [InlineData("absent")] [InlineData("Bearer")] [InlineData("duplicate-lines")]
    [InlineData("Bearer, Bearer")] [InlineData("Bearer error_description=private")] [InlineData("Basic")]
    public async Task Authentication_failed_has_one_parameterless_Bearer_and_one_body(string previous)
    {
        var services = new ServiceCollection(); services.AddLogging(); ApiProblemDetails.AddSafeApiErrors(services);
        using var provider = services.BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = provider }; http.Response.Body = new MemoryStream();
        if (previous == "duplicate-lines") http.Response.Headers.WWWAuthenticate = new Microsoft.Extensions.Primitives.StringValues(new[] { "Bearer", "Bearer" });
        else if (previous != "absent") http.Response.Headers.WWWAuthenticate = previous;
        Assert.True(await ApiProblemDetails.TryWriteAsync(http, 401));
        Assert.Equal("Bearer", Assert.Single(http.Response.Headers.WWWAuthenticate));
        var length = http.Response.Body.Length;
        Assert.False(await ApiProblemDetails.TryWriteAsync(http, 401));
        Assert.Equal(length, http.Response.Body.Length); Assert.Equal("Bearer", Assert.Single(http.Response.Headers.WWWAuthenticate));
        http.Response.Body.Position = 0; using var json = await System.Text.Json.JsonDocument.ParseAsync(http.Response.Body);
        Assert.Equal("authentication_failed", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("No fue posible autenticar la solicitud.", json.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("error_description", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("no-store", http.Response.Headers.CacheControl);
    }

    [Theory, Trait("Fix", "Challenge401")]
    [InlineData(400)] [InlineData(409)] [InlineData(413)] [InlineData(415)]
    [InlineData(429)] [InlineData(500)] [InlineData(503)]
    public async Task Other_problem_statuses_do_not_acquire_a_Bearer_header(int status)
    {
        var services = new ServiceCollection(); services.AddLogging(); ApiProblemDetails.AddSafeApiErrors(services);
        using var provider = services.BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = provider }; http.Response.Body = new MemoryStream();
        Assert.True(await ApiProblemDetails.TryWriteAsync(http, status));
        Assert.Equal(status, http.Response.StatusCode); Assert.False(http.Response.Headers.ContainsKey("WWW-Authenticate"));
        http.Response.Body.Position = 0; using var json = await System.Text.Json.JsonDocument.ParseAsync(http.Response.Body);
        Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32());
        Assert.NotEqual("authentication_failed", json.RootElement.GetProperty("code").GetString());
    }

    [Theory, Trait("Fix", "Challenge401")]
    [InlineData(403)] [InlineData(404)]
    public void Unrelated_statuses_are_outside_the_problem_writer_contract(int status)
    {
        var http = new DefaultHttpContext(); http.Response.StatusCode = status;
        Assert.False(new ApiProblemDetails().CanWrite(new ProblemDetailsContext { HttpContext = http }));
        Assert.False(http.Response.Headers.ContainsKey("WWW-Authenticate"));
    }

    [Fact, Trait("Task", "044")]
    public async Task Authenticated_exp_is_a_unique_integer_UTC_and_another_principal_is_not_accepted()
    {
        var http = await AuthenticatedAsync(); var exp = long.Parse(http.User.FindFirst("exp")!.Value, System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(LogoutInput.TryFromAuthenticatedContext(http, out var input));
        Assert.Equal(TimeSpan.Zero, input!.ExpiresAtUtc.Offset); Assert.Equal(exp, input.ExpiresAtUtc.ToUnixTimeSeconds());
        http.User = new ClaimsPrincipal(new ClaimsIdentity(http.User.Claims, "Bearer"));
        Assert.False(LogoutInput.TryFromAuthenticatedContext(http, out _));
    }

    [Theory, Trait("Task", "044")]
    [InlineData("missing")] [InlineData("duplicate")] [InlineData("1.5")] [InlineData("1e9")]
    [InlineData("+1")] [InlineData(" 1")] [InlineData("9999999999999999999999")]
    public async Task Invalid_exp_representation_cannot_create_internal_input(string defect)
    {
        var http = await AuthenticatedAsync(); var identity = (ClaimsIdentity)http.User.Identity!;
        var original = identity.FindFirst("exp")!;
        if (defect == "duplicate") identity.AddClaim(new Claim("exp", original.Value));
        else { identity.RemoveClaim(original); if (defect != "missing") identity.AddClaim(new Claim("exp", defect)); }
        Assert.False(LogoutInput.TryFromAuthenticatedContext(http, out _));
    }

    [Theory, Trait("Task", "043")]
    [InlineData(TokenRevocationWriteStatus.Confirmed, LogoutStatus.Confirmed)]
    [InlineData(TokenRevocationWriteStatus.Unavailable, LogoutStatus.Unavailable)]
    [InlineData(TokenRevocationWriteStatus.Uncertain, LogoutStatus.Unavailable)]
    [InlineData(TokenRevocationWriteStatus.NoOperation, LogoutStatus.Unavailable)]
    public async Task Exactly_one_write_and_only_confirmed_succeeds(TokenRevocationWriteStatus write, LogoutStatus expected)
    {
        var s = new Scenario { Write = write }; var input = await InputAsync();
        using var budget = new RequestBudget(4000, default);
        var result = await s.Service.LogoutAsync(input, Origin(), true, budget);
        Assert.Equal(expected, result.Status); Assert.Equal(new[] { "admission", "quota", "write" }, s.Calls);
        Assert.Equal(input.Jti, s.Last!.Value.Jti); Assert.Equal(input.ExpiresAtUtc, s.Last.Value.ExpiresAtUtc);
        Assert.Equal("enver-api", s.Last.Value.Issuer); Assert.Equal(TimeSpan.FromSeconds(30), s.Last.Value.EffectiveClockSkew);
        Assert.Equal(TimeSpan.FromSeconds(60), s.Last.Value.OperationalMargin); Assert.Same(budget, s.Budget);
        Assert.Equal(nameof(LogoutInput), input.ToString()); Assert.DoesNotContain(input.Jti, result.ToString());
    }

    [Fact, Trait("Task", "043")]
    public async Task Repeat_and_concurrent_calls_each_require_their_own_confirmation()
    {
        var s = new Scenario(); var input = await InputAsync();
        async Task<LogoutStatus> Send() { using var b = new RequestBudget(4000, default); return (await s.Service.LogoutAsync(input, Origin(), true, b)).Status; }
        Assert.Equal(LogoutStatus.Confirmed, await Send()); Assert.Equal(LogoutStatus.Confirmed, await Send());
        Assert.All(await Task.WhenAll(Send(), Send()), status => Assert.Equal(LogoutStatus.Confirmed, status));
        Assert.Equal(4, s.Calls.Count(x => x == "write"));
    }

    [Theory, Trait("Task", "043")]
    [InlineData(AuthenticationSecurityAdmission.Unavailable)] [InlineData(AuthenticationSecurityAdmission.Uncertain)]
    public async Task Closed_admission_does_not_consume_or_write(AuthenticationSecurityAdmission state)
    {
        var s = new Scenario { Admission = state }; using var budget = new RequestBudget(4000, default);
        Assert.Equal(LogoutStatus.Unavailable, (await s.Service.LogoutAsync(await InputAsync(), Origin(), true, budget)).Status);
        Assert.Equal(new[] { "admission" }, s.Calls);
    }

    [Theory, Trait("Task", "043")]
    [InlineData(false)] [InlineData(true)]
    public async Task Canceled_or_expired_budget_never_starts_controls(bool client)
    {
        using var cancel = new CancellationTokenSource(); var clock = new BudgetClock();
        using var budget = new RequestBudget(4000, cancel.Token, clock);
        if (client) cancel.Cancel(); else clock.Timestamp += 4 * clock.TimestampFrequency;
        var s = new Scenario(); Assert.Equal(LogoutStatus.Unavailable, (await s.Service.LogoutAsync(await InputAsync(), Origin(), true, budget)).Status);
        Assert.Empty(s.Calls);
    }

    [Fact, Trait("Task", "043")]
    public async Task Cancellation_after_dispatch_cannot_become_success()
    {
        using var cancel = new CancellationTokenSource(); var s = new Scenario { AfterWrite = cancel.Cancel };
        using var budget = new RequestBudget(4000, cancel.Token);
        Assert.Equal(LogoutStatus.Unavailable, (await s.Service.LogoutAsync(await InputAsync(), Origin(), true, budget)).Status);
        Assert.Single(s.Calls.Where(x => x == "write"));
    }

    internal static AuthenticationCanonicalOrigin Origin() => AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback);
    internal static async Task<LogoutInput> InputAsync()
    {
        var http = await AuthenticatedAsync(); Assert.True(LogoutInput.TryFromAuthenticatedContext(http, out var input)); return input!;
    }
    internal static async Task<DefaultHttpContext> AuthenticatedAsync()
    {
        var options = new JwtOptions { Algorithm = "HS256", Issuer = "enver-api", Audience = "enver-mobile", LifetimeSeconds = 900,
            ClockSkewSeconds = 30, ActiveKid = "test", Keys = [new() { Kid = "test", KeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }] };
        var ring = new JwtKeyRing(options, new RateLimitingOptions { PseudonymizationKeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) });
        var validator = new JwtClaimsValidator(ring, TimeProvider.System);
        var result = await validator.ValidateAsync(new JwtTokenIssuer(ring, TimeProvider.System).Issue(Guid.NewGuid()).AccessToken!);
        Assert.True(result.IsValid); var http = new DefaultHttpContext { User = new ClaimsPrincipal(result.ClaimsIdentity) };
        var bearer = new JwtBearerOptions(); new JwtBearerConfiguration(validator).Configure(bearer);
        var context = new TokenValidatedContext(http, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), bearer)
        { Principal = http.User, SecurityToken = result.SecurityToken };
        await bearer.Events.TokenValidated(context); Assert.Null(context.Result?.Failure); return http;
    }

    private sealed class BudgetClock : TimeProvider
    { public long Timestamp; public override long GetTimestamp() => Timestamp; }

    private sealed class Scenario : IRegisterAdmission, IRegisterQuota, ILogoutControls, ITokenRevocationStore, IUserStore, IAuthenticationEvents
    {
        public List<string> Calls { get; } = [];
        public TokenRevocationWriteStatus Write = TokenRevocationWriteStatus.Confirmed;
        public AuthenticationSecurityAdmission Admission = AuthenticationSecurityAdmission.Verifiable;
        public AuthenticationQuotaStoreStatus Quota = AuthenticationQuotaStoreStatus.Admitted;
        public Action? AfterWrite;
        public TokenRevocationWriteRequest? Last;
        public RequestBudget? Budget;
        public ApplicationAuthenticationService Service { get; }
        public Scenario()
        {
            var options = Options.Create(PasswordPolicyTests.PolicyOptions());
            Service = new(this, this, new(options), new(new(options), new(options), options), null!, this, this, this, this, null!, null!);
        }
        public TimeSpan EffectiveClockSkew => TimeSpan.FromSeconds(30);
        public TimeSpan OperationalMargin => TimeSpan.FromSeconds(60);
        public ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget)
        { Calls.Add("admission"); return ValueTask.FromResult(new AuthenticationSecurityDecision(Admission)); }
        public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(AuthenticationAuthenticatedAccount account, AuthenticationCanonicalOrigin origin, RequestBudget budget)
        { Calls.Add("quota"); return ValueTask.FromResult(new AuthenticationQuotaConsumeResult(Quota, Quota == AuthenticationQuotaStoreStatus.Rejected ? TimeSpan.FromSeconds(7) : null)); }
        public ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget)
        { Calls.Add("write"); Last = request; Budget = budget; AfterWrite?.Invoke(); return ValueTask.FromResult(new TokenRevocationWriteResult(Write)); }
        public ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget) => throw new InvalidOperationException("No readback.");
        public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string email, AuthenticationCanonicalOrigin origin, RequestBudget budget) => throw new InvalidOperationException("No Register quota.");
        public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget) => throw new InvalidOperationException("No MongoDB.");
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new InvalidOperationException("No MongoDB.");
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) => throw new InvalidOperationException("No MongoDB.");
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string expected, UserStatus status, string hash, DateTime at, RequestBudget budget) => throw new InvalidOperationException("No MongoDB.");
        public void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome) { }
    }
}

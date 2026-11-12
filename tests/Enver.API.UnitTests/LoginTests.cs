using System.Net;
using System.Security.Cryptography;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using System.Text.Json;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Contracts;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class LoginTests
{
    [Theory, Trait("Task", "047")]
    [InlineData("{}")] [InlineData("[]")] [InlineData("null")]
    [InlineData("{\"email\":\"a@uttt.edu.mx\",\"password\":null}")]
    [InlineData("{\"Email\":\"a@uttt.edu.mx\",\"password\":\"x\"}")]
    [InlineData("{\"email\":\"a@uttt.edu.mx\",\"password\":\"x\",\"role\":\"admin\"}")]
    [InlineData("{\"email\":\"a@uttt.edu.mx\",\"password\":\"x\",\"pass\\u0077ord\":\"y\"}")]
    public void Strict_Login_contract_has_no_coercion_or_extra_fields(string json)
    {
        var o = new JsonSerializerOptions(); AuthenticationRequestLimitsMiddleware.ConfigureSerializer(o);
        if (json == "null") Assert.Null(JsonSerializer.Deserialize<LoginRequest>(json, o));
        else Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LoginRequest>(json, o));
    }

    [Fact, Trait("Task", "047")]
    public void Response_and_ToString_never_expand_the_public_contract()
    {
        var request = new LoginRequest { Email = "synthetic@uttt.edu.mx", Password = "sentinel" };
        Assert.Equal(nameof(LoginRequest), request.ToString()); Assert.Equal(nameof(LoginInput), new LoginInput(request.Email, request.Password).ToString());
        var response = new LoginResponse("token-sentinel"); Assert.Equal(nameof(LoginResponse), response.ToString());
        var json = JsonSerializer.SerializeToElement(response);
        Assert.Equal(new[] { "accessToken", "expiresIn", "tokenType" }, json.EnumerateObject().Select(x => x.Name).Order());
        Assert.Equal(900, json.GetProperty("expiresIn").GetInt32()); Assert.Equal("Bearer", json.GetProperty("tokenType").GetString());
    }

    [Theory, Trait("Task", "047")]
    [InlineData("preflight", LoginStatus.Unavailable, "admission")]
    [InlineData("json", LoginStatus.ValidationFailed, "admission")]
    [InlineData("email", LoginStatus.ValidationFailed, "admission")]
    [InlineData("quota", LoginStatus.RateLimited, "admission,quota")]
    [InlineData("uncertain", LoginStatus.Unavailable, "admission,quota")]
    [InlineData("password", LoginStatus.ValidationFailed, "admission,quota")]
    [InlineData("read", LoginStatus.Unavailable, "admission,quota,lookup")]
    public async Task D012_precedes_lookup_and_crypto(string defect, LoginStatus expected, string calls)
    {
        var s = new Scenario();
        if (defect == "preflight") s.Admission = AuthenticationSecurityAdmission.Unavailable;
        if (defect == "quota") s.Quota = AuthenticationQuotaStoreStatus.Rejected;
        if (defect == "uncertain") s.Quota = AuthenticationQuotaStoreStatus.Uncertain;
        if (defect == "read") s.Read = UserReadStatus.Unavailable;
        using var b = new RequestBudget(8000, default);
        var result = await s.Service.LoginAsync(new(defect == "email" ? "a@invalid" : " A@UTTT.EDU.MX ", defect is "password" or "preflight" ? "" : "short", defect is not ("json" or "preflight")), Origin(), b);
        Assert.Equal(expected, result.Status); Assert.Equal(calls, string.Join(',', s.Calls));
        Assert.Equal(0, s.Passwords.Verifies + s.Passwords.Dummies); Assert.Equal(0, s.Writes);
    }

    [Theory, Trait("Task", "048")]
    [InlineData(UserReadStatus.NotFound, UserStatus.Active, PasswordVerificationStatus.Failed, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.NotFound, UserStatus.Active, PasswordVerificationStatus.Success, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.NotFound, UserStatus.Active, PasswordVerificationStatus.SuccessRehashNeeded, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.NotFound, UserStatus.Active, PasswordVerificationStatus.Unavailable, LoginStatus.Unavailable, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.Success, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.Failed, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.SuccessRehashNeeded, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Failed, LoginStatus.AuthenticationFailed, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Unavailable, LoginStatus.Unavailable, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Success, LoginStatus.Issued, 0)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.SuccessRehashNeeded, LoginStatus.Issued, 1)]
    public async Task Credentials_use_real_or_dummy_work_before_deciding_without_disabled_rehash(UserReadStatus read, UserStatus status,
        PasswordVerificationStatus verify, LoginStatus expected, int writes)
    {
        var s = new Scenario { Read = read }; s.User = new(s.User.Id, s.User.FullName, s.User.NormalizedEmail, s.User.PasswordHash, status, s.User.CreatedAt, s.User.UpdatedAt);
        s.Passwords.Result = verify; using var budget = new RequestBudget(8000, default);
        var result = await s.Service.LoginAsync(new("a@uttt.edu.mx", "short"), Origin(), budget);
        Assert.Equal(expected, result.Status); Assert.Equal(read == UserReadStatus.NotFound ? 1 : 0, s.Passwords.Dummies);
        Assert.Equal(read == UserReadStatus.Found ? 1 : 0, s.Passwords.Verifies); Assert.Equal(writes, s.Writes);
        Assert.Equal(nameof(LoginResult), result.ToString());
    }

    private static AuthenticationCanonicalOrigin Origin() => AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback);
    private sealed class Scenario : ILoginControls, IUserStore, IAuthenticationEvents, ILoginTokenIssuer
    {
        public readonly List<string> Calls = [];
        public readonly SpyPasswords Passwords = new();
        public User User = User.Create("Synthetic", "a@uttt.edu.mx", "old", DateTime.UtcNow);
        public UserReadStatus Read = UserReadStatus.Found;
        public AuthenticationSecurityAdmission Admission = AuthenticationSecurityAdmission.Verifiable;
        public AuthenticationQuotaStoreStatus Quota = AuthenticationQuotaStoreStatus.Admitted;
        public int Writes;
        public AuthenticationService Service
        {
            get { var o = Options.Create(PasswordPolicyTests.PolicyOptions()); return new(null!, null!, new(o), new(new(o), new(o), o), Passwords, this, this, null!, null!, this, this); }
        }
        public ValueTask<AccessTokenIssueResult?> IssueAsync(Guid subject, RequestBudget budget) => ValueTask.FromResult<AccessTokenIssueResult?>(RealIssuer().Issue(subject));
        public ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget) { Calls.Add("admission"); return ValueTask.FromResult(new AuthenticationSecurityDecision(Admission)); }
        public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string email, AuthenticationCanonicalOrigin origin, RequestBudget budget)
        { Calls.Add("quota"); Assert.True(email == "a@uttt.edu.mx"); return ValueTask.FromResult(new AuthenticationQuotaConsumeResult(Quota, Quota == AuthenticationQuotaStoreStatus.Rejected ? TimeSpan.FromSeconds(5) : null)); }
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget)
        { Calls.Add("lookup"); Assert.True(email == "a@uttt.edu.mx"); return ValueTask.FromResult(new UserReadResult(Read, Read == UserReadStatus.Found ? User : null)); }
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) => throw new InvalidOperationException("Unexpected reread.");
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string old, UserStatus status, string hash, DateTime at, RequestBudget budget)
        { Writes++; return ValueTask.FromResult(new UserCredentialUpdateResult(UserCredentialUpdateStatus.Updated)); }
        public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget) => throw new InvalidOperationException("No insert.");
        public void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome) { }
    }

    private static JwtTokenIssuer RealIssuer()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return new(new(new JwtOptions { Algorithm = "HS256", Issuer = "enver-api", Audience = "enver-mobile", LifetimeSeconds = 900,
            ClockSkewSeconds = 30, ActiveKid = "A", Keys = [new() { Kid = "A", KeyBase64 = key }] },
            new RateLimitingOptions { PseudonymizationKeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }), TimeProvider.System);
    }

    [Theory, Trait("Task", "049")]
    [InlineData("ok", 1, true)] [InlineData("preflight", 0, false)] [InlineData("lease", 0, false)]
    [InlineData("postcheck", 1, false)] [InlineData("cancel", 1, false)] [InlineData("throw", 1, false)]
    public async Task Protected_emission_calls_existing_issuer_once_and_never_compensates_horizon(string phase, int calls, bool success)
    {
        var authority = new AuthenticationSecurityAuthorityFake(new(AuthenticationSecurityGlobalState.Open, 0, 0, null, null, null, null, null));
        var coordinator = new AuthenticationSecurityCoordinator(authority, new JwtOptions { LifetimeSeconds = 900, ClockSkewSeconds = 30 }, new RedisOptions { RevocationOperationalMarginSeconds = 60 });
        using var cancel = new CancellationTokenSource(); using var budget = new RequestBudget(8000, cancel.Token);
        if (phase == "lease") authority.EnqueuePotentialIssuanceOutcome(AuthenticationSecurityAuthorityResultStatus.Uncertain);
        var state = new EmissionState(phase); var issuer = new CountingIssuer(RealIssuer(), () => { if (phase == "cancel") cancel.Cancel(); if (phase == "throw") throw new InvalidOperationException("Safe test fault."); });
        var adapter = new LoginTokenIssuer(state, coordinator, issuer);
        if (phase == "throw") await Assert.ThrowsAsync<InvalidOperationException>(async () => await adapter.IssueAsync(Guid.NewGuid(), budget));
        else Assert.Equal(success, (await adapter.IssueAsync(Guid.NewGuid(), budget)) is not null);
        Assert.Equal(calls, issuer.Calls);
        if (calls == 1) Assert.NotNull(authority.State!.LastPossibleIssuanceUtc);
    }
    private sealed class EmissionState(string phase) : IAuthenticationSecurityState
    {
        private int calls;
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget budget) => ValueTask.FromResult(new AuthenticationSecurityDecision(
            phase == "preflight" || phase == "postcheck" && ++calls == 2 ? AuthenticationSecurityAdmission.Unavailable : AuthenticationSecurityAdmission.Verifiable));
    }
    private sealed class CountingIssuer(IAccessTokenIssuer inner, Action after) : IAccessTokenIssuer
    {
        public int Calls;
        public AccessTokenIssueResult Issue(Guid subject) { Calls++; var result = inner.Issue(subject); after(); return result; }
    }
    private sealed class SpyPasswords : IPasswordService
    {
        public int Verifies, Dummies;
        public PasswordVerificationStatus Result = PasswordVerificationStatus.Success;
        public PasswordVerificationStatus Verify(User user, string secret, RequestBudget budget) { Verifies++; return Result; }
        public PasswordVerificationStatus VerifyDummy(string secret, RequestBudget budget) { Dummies++; return Result; }
        public PasswordHashResult Hash(User user, string secret, RequestBudget budget) => throw new InvalidOperationException("No Register hash.");
        public PasswordHashResult Rehash(User user, string secret, RequestBudget budget)
        {
            var p = PasswordPolicyTests.PolicyOptions(); p.HashingConcurrencyLimit = 2; var o = Options.Create(p); var h = new PasswordHasher<User>();
            return new PasswordService(h, new(o), new(h), new(o)).Rehash(user, secret, budget);
        }
    }
}

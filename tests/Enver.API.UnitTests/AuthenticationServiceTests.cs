using System.Net;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task Confirmed_registration_has_server_identity_and_exact_projection_after_ordered_controls()
    {
        var s = new Scenario();
        var before = DateTime.UtcNow;
        var result = await s.Send(new("  Ána  María  ", " A.B+Tag@UTTT.EDU.MX ", "synthetic-secret"));
        Assert.Equal(RegistrationStatus.Confirmed, result.Status);
        Assert.Equal(new[] { "admission", "quota", "hash", "insert" }, s.Calls);
        var user = Assert.Single(s.Written);
        Assert.Equal(4, user.Id.Version); Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(user.CreatedAt, user.UpdatedAt); Assert.Equal(DateTimeKind.Utc, user.CreatedAt.Kind);
        Assert.InRange(user.CreatedAt, before.AddMilliseconds(-1), DateTime.UtcNow);
        Assert.Equal("Ána  María", user.FullName); Assert.Equal("a.b+tag@uttt.edu.mx", user.NormalizedEmail);
        Assert.True(user.PasswordHash == "synthetic-hash-not-plaintext");
        Assert.Equal(user.Id, result.Identity!.Id); Assert.Equal(user.NormalizedEmail, result.Identity.Email);
        Assert.Equal("a.b+tag@uttt.edu.mx", s.QuotaEmail);
        Assert.Equal(new[] { "Email", "FullName", "Id" }, typeof(RegisteredIdentity).GetProperties().Select(p => p.Name).Order());
    }

    [Theory]
    [InlineData(false, "bad", RegistrationStatus.Unavailable, "admission")]
    [InlineData(true, "bad", RegistrationStatus.ValidationFailed, "admission")]
    [InlineData(true, "a@uttt.edu.mx", RegistrationStatus.ValidationFailed, "admission,quota")]
    public async Task Preflight_identity_and_secondary_defects_have_D012_order(bool ready, string email, RegistrationStatus expected, string calls)
    {
        var s = new Scenario { Ready = ready };
        var result = await s.Send(new("", email, "short"));
        Assert.Equal(expected, result.Status); Assert.Equal(calls.Split(','), s.Calls);
        Assert.Empty(s.Written);
    }

    [Theory]
    [InlineData(AuthenticationQuotaStoreStatus.Rejected, RegistrationStatus.RateLimited)]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable, RegistrationStatus.Unavailable)]
    [InlineData(AuthenticationQuotaStoreStatus.Uncertain, RegistrationStatus.Unavailable)]
    public async Task Quota_failures_precede_invalid_secondary_fields_and_never_hash(AuthenticationQuotaStoreStatus quota, RegistrationStatus expected)
    {
        var s = new Scenario { QuotaStatus = quota };
        var result = await s.Send(new("", "a@uttt.edu.mx", "short"));
        Assert.Equal(expected, result.Status); Assert.Equal(new[] { "admission", "quota" }, s.Calls);
        Assert.Empty(s.Written);
        Assert.Equal(quota == AuthenticationQuotaStoreStatus.Rejected, result.RetryAfter is not null);
    }

    [Theory]
    [InlineData(UserInsertStatus.Confirmed, RegistrationStatus.Confirmed)]
    [InlineData(UserInsertStatus.Conflict, RegistrationStatus.Conflict)]
    [InlineData(UserInsertStatus.Unavailable, RegistrationStatus.Unavailable)]
    [InlineData(UserInsertStatus.Uncertain, RegistrationStatus.Unavailable)]
    public async Task Single_insert_maps_results_without_retry_read_or_compensation(UserInsertStatus insert, RegistrationStatus expected)
    {
        var s = new Scenario { InsertStatus = insert };
        var result = await s.Send(new("Synthetic", "a@uttt.edu.mx", "synthetic-secret"));
        Assert.Equal(expected, result.Status); Assert.Single(s.Written);
        Assert.Equal(new[] { "admission", "quota", "hash", "insert" }, s.Calls);
        Assert.Equal(insert == UserInsertStatus.Confirmed, result.Identity is not null);
    }

    [Fact]
    public async Task Malformed_body_and_unverifiable_origin_never_consume()
    {
        var s = new Scenario();
        Assert.Equal(RegistrationStatus.ValidationFailed, (await s.Send(new(null, null, null, false))).Status);
        Assert.Equal(new[] { "admission" }, s.Calls);
        s.Calls.Clear();
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(RegistrationStatus.Unavailable, (await s.Service.RegisterAsync(new("Name", "a@uttt.edu.mx", "synthetic-secret"), null, budget)).Status);
        Assert.Equal(new[] { "admission" }, s.Calls);
    }

    [Fact]
    public async Task Saturated_hashing_keeps_the_admitted_charge_and_never_writes()
    {
        var s = new Scenario();
        using var budget = new RequestBudget(8000, default);
        using var first = s.Gate.TryAcquire(budget); using var second = s.Gate.TryAcquire(budget);
        var result = await s.Send(new("Synthetic", "a@uttt.edu.mx", "synthetic-secret"));
        Assert.Equal(RegistrationStatus.Unavailable, result.Status);
        Assert.Equal(new[] { "admission", "quota" }, s.Calls); Assert.Empty(s.Written);
    }

    private sealed class Scenario : IRegisterAdmission, IRegisterQuota, IUserStore, IAuthenticationEvents, IPasswordHasher<User>, ILogoutControls, ITokenRevocationStore
    {
        public List<string> Calls { get; } = [];
        public List<User> Written { get; } = [];
        public bool Ready = true;
        public AuthenticationQuotaStoreStatus QuotaStatus = AuthenticationQuotaStoreStatus.Admitted;
        public UserInsertStatus InsertStatus = UserInsertStatus.Confirmed;
        public string? QuotaEmail;
        public HashingConcurrencyGate Gate { get; }
        public AuthenticationService Service { get; }
        public Scenario()
        {
            var policy = PasswordPolicyTests.PolicyOptions(); policy.HashingConcurrencyLimit = 2;
            var options = Options.Create(policy);
            Gate = new(options);
            Service = new(this, this, new(options), new(new(options), new(options), options),
                new PasswordService(this, new(options), new(this), Gate), this, this, this, this);
        }
        public async Task<RegistrationResult> Send(RegisterInput input)
        {
            using var budget = new RequestBudget(8000, default);
            return await Service.RegisterAsync(input, AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback), budget);
        }
        public ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget)
        { Calls.Add("admission"); return ValueTask.FromResult(new AuthenticationSecurityDecision(Ready ? AuthenticationSecurityAdmission.Verifiable : AuthenticationSecurityAdmission.Unavailable)); }
        public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string normalizedEmail, AuthenticationCanonicalOrigin origin, RequestBudget budget)
        { Calls.Add("quota"); QuotaEmail = normalizedEmail; return ValueTask.FromResult(new AuthenticationQuotaConsumeResult(QuotaStatus, QuotaStatus == AuthenticationQuotaStoreStatus.Rejected ? TimeSpan.FromSeconds(1) : null)); }
        public string HashPassword(User user, string password) { Calls.Add("hash"); return "synthetic-hash-not-plaintext"; }
        public PasswordVerificationResult VerifyHashedPassword(User user, string hash, string provided) => throw new NotSupportedException();
        public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget)
        { Calls.Add("insert"); Written.Add(user); return ValueTask.FromResult(new UserInsertResult(InsertStatus)); }
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new InvalidOperationException("Read-before-write forbidden.");
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) => throw new InvalidOperationException("Read-before-write forbidden.");
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string expected, UserStatus status, string next, DateTime at, RequestBudget budget) => throw new InvalidOperationException("CAS forbidden.");
        public void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome) { }
        public TimeSpan EffectiveClockSkew => TimeSpan.FromSeconds(30);
        public TimeSpan OperationalMargin => TimeSpan.FromSeconds(60);
        public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(AuthenticationAuthenticatedAccount account, AuthenticationCanonicalOrigin origin, RequestBudget budget) => throw new InvalidOperationException("Register cannot consume Logout quota.");
        public ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget) => throw new InvalidOperationException("Register cannot revoke.");
        public ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget) => throw new InvalidOperationException("Register cannot check revocation.");
    }
}

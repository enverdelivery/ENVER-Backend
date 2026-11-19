using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class PasswordRehashTests
{
    [Theory]
    [InlineData(UserCredentialUpdateStatus.Updated, PasswordRehashStatus.Verified)]
    [InlineData(UserCredentialUpdateStatus.Unavailable, PasswordRehashStatus.Unavailable)]
    [InlineData(UserCredentialUpdateStatus.Uncertain, PasswordRehashStatus.Unavailable)]
    public async Task A_single_CAS_requires_Active_old_hash_same_budget_and_no_readback(UserCredentialUpdateStatus write, PasswordRehashStatus expected)
    {
        var s = new Scenario { Write = write }; using var b = new RequestBudget(8000, default);
        var result = await s.Coordinator.ResolveAsync(s.Snapshot, PasswordVerificationStatus.SuccessRehashNeeded, "short", b);
        Assert.Equal(expected, result.Status); Assert.Equal(1, s.Writes); Assert.Equal(0, s.Reads + s.Verifies);
        Assert.Equal(1, s.Hashes); Assert.Same(b, s.Budget);
        Assert.Equal(write == UserCredentialUpdateStatus.Updated, result.Updated);
        Assert.Equal(write == UserCredentialUpdateStatus.Uncertain, result.Uncertain);
        Assert.Equal(nameof(PasswordRehashResult), result.ToString());
    }

    [Theory]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Success, PasswordRehashStatus.Verified)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Failed, PasswordRehashStatus.Rejected)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.SuccessRehashNeeded, PasswordRehashStatus.Unavailable)]
    [InlineData(UserReadStatus.Found, UserStatus.Active, PasswordVerificationStatus.Unavailable, PasswordRehashStatus.Unavailable)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.Success, PasswordRehashStatus.Rejected)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.SuccessRehashNeeded, PasswordRehashStatus.Rejected)]
    [InlineData(UserReadStatus.Found, UserStatus.Disabled, PasswordVerificationStatus.Failed, PasswordRehashStatus.Rejected)]
    [InlineData(UserReadStatus.Unavailable, UserStatus.Active, PasswordVerificationStatus.Success, PasswordRehashStatus.Unavailable)]
    [InlineData(UserReadStatus.NotFound, UserStatus.Active, PasswordVerificationStatus.Success, PasswordRehashStatus.Rejected)]
    public async Task Conflict_allows_only_one_reread_and_at_most_one_reverification(UserReadStatus read, UserStatus status,
        PasswordVerificationStatus verify, PasswordRehashStatus expected)
    {
        var s = new Scenario { Write = UserCredentialUpdateStatus.Conflict, Read = read, CurrentStatus = status, VerifyResult = verify };
        using var b = new RequestBudget(8000, default);
        var result = await s.Coordinator.ResolveAsync(s.Snapshot, PasswordVerificationStatus.SuccessRehashNeeded, "short", b);
        Assert.Equal(expected, result.Status); Assert.True(result.RaceLost); Assert.False(result.Updated);
        Assert.Equal(1, s.Writes); Assert.Equal(1, s.Reads); Assert.Equal(read == UserReadStatus.Found ? 1 : 0, s.Verifies);
    }

    [Theory]
    [InlineData("before")] [InlineData("hash")] [InlineData("write")] [InlineData("read")]
    public async Task Cancellation_never_produces_verified_credentials_or_further_work(string phase)
    {
        using var cancel = new CancellationTokenSource();
        var s = new Scenario { Write = UserCredentialUpdateStatus.Conflict, Cancel = cancel, CancelPhase = phase };
        using var b = new RequestBudget(8000, cancel.Token); if (phase == "before") cancel.Cancel();
        Assert.Equal(PasswordRehashStatus.Unavailable, (await s.Coordinator.ResolveAsync(s.Snapshot, PasswordVerificationStatus.SuccessRehashNeeded, "short", b)).Status);
        Assert.Equal(phase is "before" or "hash" ? 0 : 1, s.Writes);
        Assert.Equal(phase == "read" ? 1 : 0, s.Reads); Assert.Equal(0, s.Verifies);
    }

    [Fact]
    public async Task Exhausted_budget_does_not_hash_or_dispatch()
    {
        var clock = new BudgetClock(); using var b = new RequestBudget(8000, default, clock); clock.Now += 8 * clock.TimestampFrequency;
        var s = new Scenario();
        Assert.Equal(PasswordRehashStatus.Unavailable, (await s.Coordinator.ResolveAsync(s.Snapshot, PasswordVerificationStatus.SuccessRehashNeeded, "short", b)).Status);
        Assert.Equal(0, s.Hashes + s.Writes + s.Reads + s.Verifies);
    }

    [Fact]
    public void Real_legacy_short_secret_rehashes_without_changing_Register_creation_policy()
    {
        var user = User.Create("Synthetic", "synthetic@uttt.edu.mx", "", DateTime.UtcNow);
        var legacy = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
        var old = new User(user.Id, user.FullName, user.NormalizedEmail, legacy.HashPassword(user, "short"), user.Status, user.CreatedAt, user.UpdatedAt);
        var hasher = new PasswordHasher<User>(); var policy = PasswordPolicyTests.PolicyOptions(); policy.HashingConcurrencyLimit = 2;
        var options = Options.Create(policy); var service = new PasswordService(hasher, new(options), new(hasher), new(options));
        using var b = new RequestBudget(8000, default);
        Assert.Equal(PasswordVerificationStatus.SuccessRehashNeeded, service.Verify(old, "short", b));
        Assert.Equal(PasswordHashStatus.InvalidInput, service.Hash(old, "short", b).Status);
        var updated = service.Rehash(old, "short", b); Assert.Equal(PasswordHashStatus.Completed, updated.Status);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(old, updated.Hash!, "short"));
        Assert.Equal(PasswordHashStatus.InvalidInput, service.Rehash(old, new string('x', 129), b).Status);
    }

    private sealed class BudgetClock : TimeProvider { public long Now; public override long GetTimestamp() => Now; }
    private sealed class Scenario : IUserStore, IPasswordService
    {
        public User Snapshot = User.Create("Synthetic", "synthetic@uttt.edu.mx", "old", new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc));
        public UserCredentialUpdateStatus Write = UserCredentialUpdateStatus.Updated;
        public UserReadStatus Read = UserReadStatus.Found;
        public UserStatus CurrentStatus = UserStatus.Active;
        public PasswordVerificationStatus VerifyResult = PasswordVerificationStatus.Success;
        public int Writes, Reads, Verifies, Hashes; public RequestBudget? Budget;
        public CancellationTokenSource? Cancel; public string? CancelPhase;
        public PasswordRehashCoordinator Coordinator => new(this, this);
        public PasswordHashResult Rehash(User user, string secret, RequestBudget budget)
        {
            Hashes++; if (CancelPhase == "hash") Cancel!.Cancel();
            // Constructor del resultado pertenece al adapter: usar un hasher real para producirlo.
            var options = PasswordPolicyTests.PolicyOptions(); options.HashingConcurrencyLimit = 2;
            var hasher = new PasswordHasher<User>(); var o = Options.Create(options);
            return new PasswordService(hasher, new(o), new(hasher), new(o)).Rehash(user, secret, budget);
        }
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string old, UserStatus status, string hash, DateTime at, RequestBudget budget)
        {
            Writes++; Budget = budget; Assert.Equal(Snapshot.Id, id); Assert.True(old == Snapshot.PasswordHash);
            Assert.Equal(UserStatus.Active, status); Assert.Equal(DateTimeKind.Utc, at.Kind); Assert.True(hash != old);
            if (CancelPhase == "write") Cancel!.Cancel(); return ValueTask.FromResult(new UserCredentialUpdateResult(Write));
        }
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget)
        {
            Reads++; Assert.Equal(Snapshot.Id, id); if (CancelPhase == "read") Cancel!.Cancel();
            return ValueTask.FromResult(new UserReadResult(Read, Read == UserReadStatus.Found ?
                new(Snapshot.Id, Snapshot.FullName, Snapshot.NormalizedEmail, "winner", CurrentStatus, Snapshot.CreatedAt, Snapshot.UpdatedAt) : null));
        }
        public PasswordVerificationStatus Verify(User user, string secret, RequestBudget budget) { Verifies++; return VerifyResult; }
        public PasswordHashResult Hash(User user, string secret, RequestBudget budget) => throw new InvalidOperationException("No creation hash.");
        public PasswordVerificationStatus VerifyDummy(string secret, RequestBudget budget) => throw new InvalidOperationException("No dummy in rehash.");
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new InvalidOperationException("No initial lookup.");
        public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget) => throw new InvalidOperationException("No insertion.");
    }
}

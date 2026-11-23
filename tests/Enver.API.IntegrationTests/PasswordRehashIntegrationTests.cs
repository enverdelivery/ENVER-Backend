using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class PasswordRehashIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    private const string Secret = "short";
    private static readonly DateTime Later = new(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("updated", PasswordRehashStatus.Verified)]
    [InlineData("success", PasswordRehashStatus.Verified)]
    [InlineData("failed", PasswordRehashStatus.Rejected)]
    [InlineData("disabled", PasswordRehashStatus.Rejected)]
    [InlineData("missing", PasswordRehashStatus.Rejected)]
    [InlineData("legacy", PasswordRehashStatus.Unavailable)]
    [InlineData("read-unavailable", PasswordRehashStatus.Unavailable)]
    [InlineData("unavailable", PasswordRehashStatus.Unavailable)]
    [InlineData("uncertain", PasswordRehashStatus.Unavailable)]
    public Task Real_store_preserves_CAS_contract_and_bounded_conflict_resolution(string scenario, PasswordRehashStatus expected) =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine); await Mongo026Evidence.PrepareAsync(scope);
            var original = Legacy(); using var setup = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, setup)).Status);
            var passwords = Passwords(); var store = new Probe(scope.Store);
            if (scenario is "unavailable" or "uncertain") store.Outcome = scenario == "unavailable" ? UserCredentialUpdateStatus.Unavailable : UserCredentialUpdateStatus.Uncertain;
            else if (scenario != "updated") store.BeforeWrite = async () =>
            {
                if (scenario == "missing") await scope.Users.DeleteOneAsync(x => x.Id == original.Id);
                else if (scenario == "disabled") await scope.Users.UpdateOneAsync(x => x.Id == original.Id, Builders<Enver.API.Infrastructure.MongoDB.UserDocument>.Update.Set(x => x.Status, UserStatus.Disabled));
                else
                {
                    var hasher = scenario == "legacy" ? new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 })) : new PasswordHasher<User>();
                    var next = hasher.HashPassword(original, scenario == "failed" ? "changed" : Secret);
                    await scope.Users.UpdateOneAsync(x => x.Id == original.Id, Builders<Enver.API.Infrastructure.MongoDB.UserDocument>.Update.Set(x => x.PasswordHash, next));
                }
            };
            store.ReadUnavailable = scenario == "read-unavailable";
            using var budget = new RequestBudget(8000, default);
            var verified = passwords.Verify(original, Secret, budget); Assert.Equal(PasswordVerificationStatus.SuccessRehashNeeded, verified);
            var result = await new PasswordRehashCoordinator(store, passwords, new FixedClock()).ResolveAsync(original, verified, Secret, budget);
            Assert.Equal(expected, result.Status); Assert.Equal(1, store.Writes);
            Assert.Equal(scenario is "updated" or "unavailable" or "uncertain" ? 0 : 1, store.Reads);
            if (scenario is "updated" or "uncertain")
            {
                var saved = await scope.Users.Find(x => x.Id == original.Id).SingleAsync();
                Assert.True(saved.PasswordHash != original.PasswordHash);
                Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>().VerifyHashedPassword(original, saved.PasswordHash, Secret));
                Mongo026Evidence.AssertPreserved(original, saved, saved.PasswordHash, Later);
            }
            Assert.Equal(scenario == "uncertain", result.Uncertain);
        });

    [Fact]
    public Task Independent_clients_rehash_same_legacy_secret_one_winner_then_reverify_without_overwrite() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture, output.WriteLine); await Mongo026Evidence.PrepareAsync(scope);
            var original = Legacy(); using var setup = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, setup)).Status);
            using var actorA = new Mongo026Actor(scope); using var actorB = new Mongo026Actor(scope);
            await Task.WhenAll(Mongo026Evidence.WarmAsync(actorA.Client), Mongo026Evidence.WarmAsync(actorB.Client));
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var arrived = 0;
            async Task Barrier() { if (Interlocked.Increment(ref arrived) == 2) ready.SetResult(); await ready.Task; }
            var a = new Probe(actorA.Store) { BeforeWrite = Barrier }; var b = new Probe(actorB.Store) { BeforeWrite = Barrier };
            using var ba = new RequestBudget(8000, default); using var bb = new RequestBudget(8000, default);
            var results = await Task.WhenAll(new PasswordRehashCoordinator(a, Passwords(), new FixedClock()).ResolveAsync(original, PasswordVerificationStatus.SuccessRehashNeeded, Secret, ba).AsTask(),
                new PasswordRehashCoordinator(b, Passwords(), new FixedClock()).ResolveAsync(original, PasswordVerificationStatus.SuccessRehashNeeded, Secret, bb).AsTask());
            Assert.All(results, r => Assert.Equal(PasswordRehashStatus.Verified, r.Status));
            Assert.Single(results, r => r.Updated); Assert.Single(results, r => r.RaceLost);
            Assert.Equal(2, a.Writes + b.Writes); Assert.Equal(1, a.Reads + b.Reads);
            var saved = await scope.Users.Find(x => x.Id == original.Id).SingleAsync();
            Mongo026Evidence.AssertPreserved(original, saved, saved.PasswordHash, Later);
            Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>().VerifyHashedPassword(original, saved.PasswordHash, Secret));
        });

    private static User Legacy()
    {
        var user = User.Create("Synthetic", "rehash@uttt.edu.mx", "", Mongo026Evidence.Created);
        var legacy = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
        return new(user.Id, user.FullName, user.NormalizedEmail, legacy.HashPassword(user, Secret), user.Status, user.CreatedAt, user.UpdatedAt);
    }
    private static PasswordService Passwords()
    {
        var o = Options.Create(new AuthenticationOptions { PasswordCreationMinUnicodeScalars = 12, PasswordMaxUnicodeScalars = 128, PasswordMaxUtf8Bytes = 512, HashingConcurrencyLimit = 2 });
        var hasher = new PasswordHasher<User>(); return new(hasher, new(o), new(hasher), new(o));
    }
    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(Later); }
    private sealed class Probe(IUserStore inner) : IUserStore
    {
        public int Writes, Reads; public Func<Task>? BeforeWrite; public UserCredentialUpdateStatus? Outcome; public bool ReadUnavailable;
        public async ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string old, UserStatus status, string hash, DateTime at, RequestBudget budget)
        {
            Writes++; if (BeforeWrite is not null) await BeforeWrite();
            if (Outcome == UserCredentialUpdateStatus.Unavailable) return new(Outcome.Value);
            var result = await inner.CompareExchangePasswordHashAsync(id, old, status, hash, at, budget);
            return Outcome == UserCredentialUpdateStatus.Uncertain && result.Status == UserCredentialUpdateStatus.Updated ? new(UserCredentialUpdateStatus.Uncertain) : result;
        }
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget)
        { Reads++; return ReadUnavailable ? ValueTask.FromResult(new UserReadResult(UserReadStatus.Unavailable)) : inner.FindByIdAsync(id, budget); }
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new InvalidOperationException("No initial lookup.");
        public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget) => throw new InvalidOperationException("No insert.");
    }
}

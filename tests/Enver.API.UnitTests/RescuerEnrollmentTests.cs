using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Core;
using Enver.API.Features.Users.Validation;
using Enver.API.Http;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class RescuerEnrollmentTests
{
    private const string Rules = "test.rules_1";

    [Theory]
    [InlineData("""{"acceptedRules":true,"rulesVersion":"test.rules_1"}""", true)]
    [InlineData("""{"rulesVersion":"a","acceptedRules":true}""", true)]
    [InlineData("""{"rulesVersion":"X","acceptedRules":false}""", false)]
    [InlineData("""{"rulesVersion":"X","acceptedRules":"true"}""", false)]
    [InlineData("""{"acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":"X"}""", false)]
    [InlineData("""{"rulesVersion":" X","acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":"-X","acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":"ñ","acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":null,"acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":"X","acceptedRules":true,"permissions":[]}""", false)]
    [InlineData("""{"rulesVersion":"X","acceptedRules":true,"acceptedRules":true}""", false)]
    [InlineData("""{"rulesVersion":"X","rulesVersion":"X","acceptedRules":true}""", false)]
    public void Parses_strictly(string payload, bool valid)
    {
        using var json = JsonDocument.Parse(payload);
        Assert.Equal(valid, RescuerEnrollmentInput.Parse(json.RootElement) is not null);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(64, true)]
    [InlineData(65, false)]
    public void Bounds_version(int length, bool valid)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(new { acceptedRules = true, rulesVersion = new string('a', length) }));
        Assert.Equal(valid, RescuerEnrollmentInput.Parse(json.RootElement) is not null);
    }

    [Theory]
    [InlineData("inactive", EnrollmentOutcome.Forbidden)]
    [InlineData("email", EnrollmentOutcome.Forbidden)]
    [InlineData("missing-phone", EnrollmentOutcome.PhoneRequired)]
    [InlineData("invalid-phone", EnrollmentOutcome.PhoneRequired)]
    [InlineData("profile-down", EnrollmentOutcome.Unavailable)]
    [InlineData("policy-down", EnrollmentOutcome.Unavailable)]
    [InlineData("version", EnrollmentOutcome.VersionConflict)]
    [InlineData("revoked", EnrollmentOutcome.Forbidden)]
    [InlineData("store-down", EnrollmentOutcome.Unavailable)]
    [InlineData("quota-down", EnrollmentOutcome.Unavailable)]
    [InlineData("limited", EnrollmentOutcome.RateLimited)]
    public async Task Rejects_without_writing(string scenario, EnrollmentOutcome outcome)
    {
        var fixture = new Store(scenario);
        using var budget = new RequestBudget(10000, default);
        var actual = await fixture.Service.ExecuteAsync(fixture.Id, scenario == "version" ? "test.other" : Rules, budget);
        Assert.Equal(outcome, actual.Outcome);
        Assert.Equal(0, fixture.InsertCount);
        if (scenario == "limited") Assert.Equal(TimeSpan.FromSeconds(3), actual.RetryAfter);
    }

    [Fact]
    public async Task Replays_a_historical_acceptance_without_revalidation_or_writing()
    {
        var fixture = new Store("historical");
        using var budget = new RequestBudget(10000, default);
        Assert.Equal(EnrollmentOutcome.Enabled, (await fixture.Service.ExecuteAsync(fixture.Id, Rules, budget)).Outcome);
        Assert.Equal(EnrollmentOutcome.VersionConflict,
            (await fixture.Service.ExecuteAsync(fixture.Id, "test.next", budget)).Outcome);
        Assert.Equal(0, fixture.ProfileReads);
        Assert.Equal(0, fixture.InsertCount);
    }

    [Theory]
    [InlineData("normal", EnrollmentOutcome.Enabled)]
    [InlineData("write-uncertain", EnrollmentOutcome.Unavailable)]
    [InlineData("concurrent-revocation", EnrollmentOutcome.Forbidden)]
    public async Task Separates_confirmed_uncertain_and_concurrent_write_outcomes(string scenario, EnrollmentOutcome outcome)
    {
        var fixture = new Store(scenario);
        using var budget = new RequestBudget(10000, default);
        Assert.Equal(outcome, (await fixture.Service.ExecuteAsync(fixture.Id, Rules, budget)).Outcome);
        Assert.Equal(1, fixture.InsertCount);
        Assert.Equal(fixture.Id, fixture.Decision!.UserId);
        Assert.Equal(Rules, fixture.Decision.RulesVersion);
        Assert.Equal("test.policy", fixture.Decision.PolicyVersion);
    }

    private sealed class Store(string scenario) : IUserStore, IUserProfileStore, IRescuerEnrollmentStore, IRescuerEnrollmentQuota
    {
        internal Guid Id { get; } = Guid.NewGuid();
        internal EnrollmentDecision? Decision { get; private set; }
        internal int InsertCount { get; private set; }
        internal int ProfileReads { get; private set; }
        internal RescuerEnrollmentService Service => new(this, this, this,
            new InstitutionalEmailNormalizer(Options.Create(new AuthenticationOptions
                { InstitutionalDomain = "uttt.edu.mx", EmailMaxAsciiBytes = 254, EmailLocalPartMaxAsciiBytes = 64 })),
            this, new(scenario == "historical" ? "test.next" : Rules, scenario == "policy-down" ? "" : "test.policy"),
            TimeProvider.System);

        ValueTask<EnrollmentQuotaResult> IRescuerEnrollmentQuota.ConsumeAsync(Guid id, RequestBudget budget) => ValueTask.FromResult(
            scenario switch
            {
                "quota-down" => new EnrollmentQuotaResult(EnrollmentQuotaStatus.Unavailable),
                "limited" => new(EnrollmentQuotaStatus.Limited, TimeSpan.FromSeconds(3)),
                _ => new(EnrollmentQuotaStatus.Allowed)
            });
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget)
        {
            var now = DateTime.UtcNow;
            return ValueTask.FromResult(new UserReadResult(UserReadStatus.Found, new(Id, "Sintético",
                scenario == "email" ? "sintetico@example.invalid" : "sintetico@uttt.edu.mx",
                "", scenario == "inactive" ? UserStatus.Disabled : UserStatus.Active, now, now)));
        }
        Task<ProfileStoreResult> IUserProfileStore.ReadAsync(Guid id, RequestBudget budget)
        {
            ProfileReads++;
            return Task.FromResult(scenario switch
            {
                "missing-phone" => new ProfileStoreResult(ProfileStoreStatus.Missing),
                "profile-down" => new(ProfileStoreStatus.Unavailable),
                _ => new(ProfileStoreStatus.Confirmed, new(null, scenario == "invalid-phone" ? "123" : "+525512345678",
                    ActiveRole.Solicitante))
            });
        }
        Task<GrantRead> IRescuerEnrollmentStore.ReadAsync(Guid id, RequestBudget budget)
        {
            var now = DateTime.UtcNow;
            return Task.FromResult(scenario switch
            {
                "historical" => new GrantRead(GrantReadStatus.Confirmed, new(Id, GrantStatus.Enabled, 1, now, Rules, now)),
                "revoked" or "concurrent-revocation" when scenario == "revoked" || InsertCount != 0 =>
                    new(GrantReadStatus.Confirmed, new(Id, GrantStatus.Revoked, 2, now, Rules, now)),
                "store-down" => new(GrantReadStatus.Unavailable),
                _ => new(GrantReadStatus.Missing)
            });
        }
        public Task<GrantWriteStatus> InsertAsync(EnrollmentDecision decision, RequestBudget budget)
        {
            Decision = decision;
            InsertCount++;
            return Task.FromResult(scenario switch
            {
                "write-uncertain" => GrantWriteStatus.Unavailable,
                "concurrent-revocation" => GrantWriteStatus.Reconciled,
                _ => GrantWriteStatus.Confirmed
            });
        }
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new NotSupportedException();
        public ValueTask<UserInsertResult> InsertAsync(User value, RequestBudget budget) => throw new NotSupportedException();
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string expectedPasswordHash,
            UserStatus expectedStatus, string newPasswordHash, DateTime updatedAtUtc, RequestBudget budget) => throw new NotSupportedException();
        public Task<ProfileStoreResult> UpdateAsync(Guid id, ProfileChange change, RequestBudget budget) => throw new NotSupportedException();
    }
}

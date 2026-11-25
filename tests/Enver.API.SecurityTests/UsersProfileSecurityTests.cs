using System.Net;
using System.Text;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Controllers;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Core;
using Enver.API.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Fact]
    public async Task Users_profile_uses_normal_policy_and_never_grants_eligibility_from_mode()
    {
        var account = User.Create("Perfil base", "profile-fixture@uttt.edu.mx", Harness.SensitiveSentinel, DateTime.UtcNow);
        var store = new ProfileFixture();
        using var host = new Harness(services =>
        {
            // Use the production controller in the isolated Bearer test host.
            services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);
            services.Configure<AuthenticationOptions>(o => o.FullNameMaxUnicodeScalars = 200);
            services.AddSingleton<InstitutionalEmailNormalizer>();
            services.AddSingleton<PasswordPolicy>();
            services.AddSingleton<AuthenticationInputValidator>();
            services.AddSingleton<IUserStore>(new AccountFixture(account));
            services.AddSingleton<IUserProfileStore>(store);
            services.AddSingleton<IRescuerEligibility>(store);
            services.AddScoped<UserProfileService>();
        });
        const string path = "/api/v1/users/me";
        var token = host.Issuer.Issue(account.Id).AccessToken!;
        using (var missing = await host.SendAsync(null, path)) await host.AssertProblemAsync(missing, 401, "");
        using (var invalid = await host.SendAsync("invalid", path)) await host.AssertProblemAsync(invalid, 401, "invalid");
        using (var unrelated = await host.SendAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken, path))
            Assert.Equal(HttpStatusCode.Forbidden, unrelated.StatusCode);
        using (var initial = await host.SendAsync(token, path))
        using (var json = await Read(initial))
        {
            Assert.Equal(account.Id, json.RootElement.GetProperty("id").GetGuid());
            Assert.Equal("Perfil base", json.RootElement.GetProperty("fullName").GetString());
            Assert.Equal("Solicitante", json.RootElement.GetProperty("activeRole").GetString());
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("phone").ValueKind);
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("rating").ValueKind);
            Assert.Equal(8, json.RootElement.EnumerateObject().Count());
            Assert.Equal(0, store.Writes);
        }
        using (var patch = await Send(HttpMethod.Patch, "", "{\"fullName\":\" Perfil visible \",\"phone\":\"+527731234567\"}"))
        using (var json = await Read(patch)) Assert.Equal("Perfil visible", json.RootElement.GetProperty("fullName").GetString());
        foreach (var input in new[] { "{\"phone\":null,\"rescuerEnabled\":true}", "{}", "{\"fullName\":null}",
                     "{\"phone\":7731234567}", "{\"phone\":null,\"phone\":null}" })
        {
            using var response = await Send(HttpMethod.Patch, "", input);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        using (var badRole = await Send(HttpMethod.Put, "/active-role", "{\"activeRole\":\"rescatista\"}"))
            Assert.Equal(HttpStatusCode.BadRequest, badRole.StatusCode);
        Assert.Equal(1, store.Writes);

        foreach (var role in new[] { "Rescatista", "Solicitante" })
        {
            using var response = await Send(HttpMethod.Put, "/active-role", JsonSerializer.Serialize(new { activeRole = role }));
            using var json = await Read(response);
            Assert.Equal(role, json.RootElement.GetProperty("activeRole").GetString());
            Assert.False(json.RootElement.GetProperty("rescuerEnabled").GetBoolean());
            Assert.Equal("+527731234567", json.RootElement.GetProperty("phone").GetString());
        }
        using (var clear = await Send(HttpMethod.Patch, "", "{\"phone\":null}"))
        using (var json = await Read(clear)) Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("phone").ValueKind);
        using (var status = await host.SendAsync(token, path + "/rescuer-status"))
        using (var json = await Read(status)) Assert.False(json.RootElement.GetProperty("rescuerEnabled").GetBoolean());
        store.Eligibility = new(true, "Test");
        using (var eligibleChange = await Send(HttpMethod.Put, "/active-role", "{\"activeRole\":\"Rescatista\"}"))
        using (var json = await Read(eligibleChange)) Assert.True(json.RootElement.GetProperty("rescuerEnabled").GetBoolean());
        using (var status = await host.SendAsync(token, path + "/rescuer-status"))
        using (var json = await Read(status)) Assert.True(json.RootElement.GetProperty("rescuerEnabled").GetBoolean());
        store.Eligibility = null;
        using (var unavailable = await host.SendAsync(token, path + "/rescuer-status"))
            await host.AssertProblemAsync(unavailable, 503, token);
        host.Store.Status = TokenRevocationReadStatus.Revoked;
        using (var revoked = await Send(HttpMethod.Patch, "", "{\"phone\":null}")) await host.AssertProblemAsync(revoked, 401, token);
        Assert.Equal(5, store.Writes);

        Task<HttpResponseMessage> Send(HttpMethod method, string suffix, string json) =>
            host.SendAsync(token, path + suffix, method, new StringContent(json, Encoding.UTF8, "application/json"));
        static async Task<JsonDocument> Read(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            var payload = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain(Harness.SensitiveSentinel, payload);
            return JsonDocument.Parse(payload);
        }
    }

    private sealed class ProfileFixture : IUserProfileStore, IRescuerEligibility
    {
        private UserProfile? profile;
        public int Writes { get; private set; }
        public RescuerEligibility? Eligibility { get; set; } = new(false, null);
        public Task<ProfileStoreResult> ReadAsync(Guid id, RequestBudget budget) => Task.FromResult(profile is null
            ? new ProfileStoreResult(ProfileStoreStatus.Missing) : new(ProfileStoreStatus.Confirmed, profile));
        public Task<ProfileStoreResult> UpdateAsync(Guid id, ProfileChange change, RequestBudget budget)
        {
            Writes++;
            profile = new(change.FullName ?? profile?.FullName, change.HasPhone ? change.Phone : profile?.Phone,
                change.ActiveRole ?? profile?.ActiveRole ?? ActiveRole.Solicitante);
            return ReadAsync(id, budget);
        }
        ValueTask<RescuerEligibility?> IRescuerEligibility.ReadAsync(Guid id, RequestBudget budget) => ValueTask.FromResult(Eligibility);
    }

    private sealed class AccountFixture(User user) : IUserStore
    {
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) => ValueTask.FromResult(id == user.Id
            ? new UserReadResult(UserReadStatus.Found, user) : new(UserReadStatus.NotFound));
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new NotSupportedException();
        public ValueTask<UserInsertResult> InsertAsync(User value, RequestBudget budget) => throw new NotSupportedException();
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string expectedPasswordHash,
            UserStatus expectedStatus, string newPasswordHash, DateTime updatedAtUtc, RequestBudget budget) => throw new NotSupportedException();
    }
}

using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class PasswordServiceTests
{
    [Fact]
    public void Uninitialized_dummy_and_saturated_real_KDF_deny_without_creating_per_request_dummy_or_rehash()
    {
        var hasher = new PasswordHasher<User>(); var options = PasswordPolicyTests.PolicyOptions(); options.HashingConcurrencyLimit = 1;
        var o = Options.Create(options); var gate = new HashingConcurrencyGate(o); var dummy = new DummyPasswordHash(hasher);
        var service = new PasswordService(hasher, new(o), dummy, gate); using var budget = new RequestBudget(8000, default);
        Assert.Equal(PasswordVerificationStatus.Unavailable, service.VerifyDummy("short", budget)); Assert.False(dummy.IsInitialized);
        dummy.Initialize(); using var held = gate.TryAcquire(budget); Assert.NotNull(held);
        Assert.Equal(PasswordVerificationStatus.Unavailable, service.VerifyDummy("short", budget));
        Assert.Equal(PasswordHashStatus.Unavailable, service.Rehash(Account(), "short", budget).Status);
    }

    private static User Account(string hash = "") => User.Create("Synthetic", "synthetic@uttt.edu.mx", hash, DateTime.UtcNow);
    private static PasswordService Service(IPasswordHasher<User>? hasher = null)
    {
        hasher ??= new PasswordHasher<User>();
        var options = PasswordPolicyTests.PolicyOptions();
        options.HashingConcurrencyLimit = 2;
        return new(hasher, new(Options.Create(options)), new(hasher), new(Options.Create(options)));
    }

    [Fact]
    public void Framework_hashes_verify_with_distinct_salts_and_do_not_expose_values_via_ToString()
    {
        var service = Service();
        using var budget = new RequestBudget(8000, default);
        var first = service.Hash(Account(), "synthetic-secret", budget);
        var second = service.Hash(Account(), "synthetic-secret", budget);
        Assert.Equal(PasswordHashStatus.Completed, first.Status);
        Assert.True(first.Hash != second.Hash);
        Assert.True(!first.ToString().Contains(first.Hash!, StringComparison.Ordinal));
        Assert.Equal(PasswordVerificationStatus.Success, service.Verify(Account(first.Hash!), "synthetic-secret", budget));
        Assert.Equal(PasswordVerificationStatus.Failed, service.Verify(Account(first.Hash!), "different-secret", budget));
    }

    [Theory]
    [InlineData("  Aaé𝄞 contraseña  ", "Aaé𝄞 contraseña")]
    [InlineData("Aaaaaaaaaaaa", "aaaaaaaaaaaa")]
    [InlineData("e\u0301aaaaaaaaaaaa", "éaaaaaaaaaaaa")]
    public void Exact_plaintext_is_not_transformed(string original, string changed)
    {
        var service = Service();
        using var budget = new RequestBudget(8000, default);
        var result = service.Hash(Account(), original, budget);
        Assert.Equal(PasswordHashStatus.Completed, result.Status);
        Assert.Equal(PasswordVerificationStatus.Success, service.Verify(Account(result.Hash!), original, budget));
        Assert.Equal(PasswordVerificationStatus.Failed, service.Verify(Account(result.Hash!), changed, budget));
    }

    [Fact]
    public void Legacy_framework_format_requests_rehash_without_writing_or_regenerating_it()
    {
        // IdentityV2 lo genera el framework solo como vector de prueba; no fija iteraciones manuales.
        var legacy = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2 }));
        var hash = legacy.HashPassword(Account(), "synthetic-secret");
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(PasswordVerificationStatus.SuccessRehashNeeded, Service().Verify(Account(hash), "synthetic-secret", budget));
    }

    [Fact]
    public void Dummy_is_initialized_once_with_same_hasher_and_never_rehashed_per_request()
    {
        var hasher = new CountingHasher();
        var dummy = new DummyPasswordHash(hasher);
        Assert.False(dummy.IsInitialized);
        dummy.Initialize(); dummy.Initialize();
        var options = PasswordPolicyTests.PolicyOptions(); options.HashingConcurrencyLimit = 2;
        var service = new PasswordService(hasher, new(Options.Create(options)), dummy, new(Options.Create(options)));
        using var budget = new RequestBudget(8000, default);
        service.VerifyDummy("wrong", budget); service.VerifyDummy("wrong", budget);
        Assert.True(dummy.IsInitialized);
        Assert.Equal(1, hasher.HashCalls);
        Assert.Equal(2, hasher.VerifyCalls);
        Assert.Equal(nameof(DummyPasswordHash), dummy.ToString());
    }

    [Fact]
    public void Invalid_input_does_not_invoke_hasher()
    {
        var hasher = new CountingHasher();
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(PasswordHashStatus.InvalidInput, Service(hasher).Hash(Account(), "short", budget).Status);
        Assert.Equal(0, hasher.HashCalls);
    }

    private sealed class CountingHasher : IPasswordHasher<User>
    {
        private readonly PasswordHasher<User> framework = new();
        public int HashCalls { get; private set; }
        public int VerifyCalls { get; private set; }
        public string HashPassword(User user, string password) { HashCalls++; return framework.HashPassword(user, password); }
        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
        { VerifyCalls++; return framework.VerifyHashedPassword(user, hashedPassword, providedPassword); }
    }
}

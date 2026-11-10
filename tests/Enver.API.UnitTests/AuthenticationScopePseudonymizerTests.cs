using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Security;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class AuthenticationScopePseudonymizerTests
{
    private static readonly byte[] SyntheticKey = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
    private const string AccountId = "6e4240ea-5b73-4a87-b911-b180a4bbcd21";

    [Fact]
    public void Fixed_vector_has_exact_length_charset_full_digest_and_no_padding()
    {
        using var pseudonymizer = Create();
        var identity = AuthenticationScopeIdentity.Email(AuthenticationOperation.Login, AuthenticationCanonicalEmail.FromValidatedEmail(" A.B+Tag@UTTT.EDU.MX "));
        var opaque = pseudonymizer.Pseudonymize(identity);
        Assert.Equal("knrVeguSRUkPfUPYDwz4Kngm7Y0J1-kNQH-QjbcxqOw", opaque);
        Assert.Equal(43, opaque.Length);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", opaque);
        Assert.DoesNotContain("=", opaque);
        Assert.Equal(32, Convert.FromBase64String(opaque.Replace('-', '+').Replace('_', '/') + "=").Length);
    }

    [Fact]
    public void Independent_instances_with_same_snapshot_produce_identical_results_and_different_key_changes_vector()
    {
        var identity = Email("a.b+tag@uttt.edu.mx");
        using var first = Create();
        using var second = Create();
        using var changedKey = Create(key: Convert.ToBase64String(Enumerable.Repeat((byte)99, 32).ToArray()));
        Assert.Equal(first.Pseudonymize(identity), second.Pseudonymize(identity));
        Assert.Equal(first.Pseudonymize(identity), first.Pseudonymize(identity));
        Assert.NotEqual(first.Pseudonymize(identity), changedKey.Pseudonymize(identity));
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    [InlineData("es-MX")]
    public void Canonicalization_is_invariant_and_preserves_dots_and_plus_tags(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            using var pseudonymizer = Create(environment: "TESTING");
            var actual = pseudonymizer.Pseudonymize(Email(" \tI.Test+Tag@UTTT.EDU.MX\r\n"));
            Assert.Equal(Reference("testing", "login", "email", "i.test+tag@uttt.edu.mx"), actual);
            Assert.NotEqual(actual, pseudonymizer.Pseudonymize(Email("itest+tag@uttt.edu.mx")));
            Assert.NotEqual(actual, pseudonymizer.Pseudonymize(Email("i.test@uttt.edu.mx")));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("usér@uttt.edu.mx")]
    [InlineData("\u212A@uttt.edu.mx")]
    [InlineData("u\u0301@uttt.edu.mx")]
    [InlineData("user\t@uttt.edu.mx")]
    [InlineData("")]
    [InlineData("user")]
    [InlineData("@uttt.edu.mx")]
    [InlineData("user@")]
    [InlineData("a@b@uttt.edu.mx")]
    public void Incompatible_email_representation_cannot_create_identity(string value)
    {
        var error = Assert.Throws<ArgumentException>(() => AuthenticationCanonicalEmail.FromValidatedEmail(value));
        if (value.Length > 0) Assert.DoesNotContain(value, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("192.0.2.1", "ipv4:c0000201")]
    [InlineData("2001:db8::1", "ipv6:20010db8000000000000000000000001")]
    [InlineData("2001:0DB8:0000:0000:0000:0000:0000:0001", "ipv6:20010db8000000000000000000000001")]
    [InlineData("::ffff:192.0.2.1", "ipv4:c0000201")]
    public void Address_bytes_have_explicit_family_and_stable_lowercase_hex(string address, string canonical)
    {
        using var pseudonymizer = Create();
        var identity = AuthenticationScopeIdentity.Origin(AuthenticationOperation.Login, AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse(address)));
        Assert.Equal(Reference("testing", "login", "origin", canonical), pseudonymizer.Pseudonymize(identity));
    }

    [Theory]
    [InlineData("fe80::1%7")]
    [InlineData("::ffff:192.0.2.1%7")]
    public void Zone_is_rejected_before_any_mapped_address_conversion(string address) =>
        Assert.Throws<ArgumentException>(() => AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse(address)));

    [Fact]
    public void IPv6_addresses_in_same_prefix_are_not_silently_grouped()
    {
        using var pseudonymizer = Create();
        Assert.NotEqual(pseudonymizer.Pseudonymize(Origin("2001:db8::1")), pseudonymizer.Pseudonymize(Origin("2001:db8::2")));
    }

    [Fact]
    public void Domains_include_environment_operation_scope_and_utf8_byte_lengths()
    {
        var email = AuthenticationCanonicalEmail.FromValidatedEmail("a.b+tag@uttt.edu.mx");
        var origin = AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse("192.0.2.1"));
        using var testing = Create();
        using var staging = Create(environment: "Staging");
        using var utf8Environment = Create(environment: "TÉST");
        var login = testing.Pseudonymize(AuthenticationScopeIdentity.Email(AuthenticationOperation.Login, email));
        Assert.NotEqual(login, testing.Pseudonymize(AuthenticationScopeIdentity.Email(AuthenticationOperation.Register, email)));
        Assert.NotEqual(login, staging.Pseudonymize(AuthenticationScopeIdentity.Email(AuthenticationOperation.Login, email)));
        var originOnly = testing.Pseudonymize(AuthenticationScopeIdentity.Origin(AuthenticationOperation.Login, origin));
        var pair = testing.Pseudonymize(AuthenticationScopeIdentity.OriginEmail(origin, email));
        Assert.NotEqual(login, originOnly);
        Assert.NotEqual(originOnly, pair);
        Assert.Equal(Reference("tést", "login", "email", "a.b+tag@uttt.edu.mx"),
            utf8Environment.Pseudonymize(AuthenticationScopeIdentity.Email(AuthenticationOperation.Login, email)));
    }

    [Fact]
    public void Composite_has_fixed_order_and_separately_length_prefixed_components()
    {
        using var pseudonymizer = Create();
        var origin = AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse("192.0.2.1"));
        var pair = AuthenticationScopeIdentity.OriginEmail(origin, AuthenticationCanonicalEmail.FromValidatedEmail("a.b+tag@uttt.edu.mx"));
        var actual = pseudonymizer.Pseudonymize(pair);
        Assert.Equal(Reference("testing", "login", "origin-email", "ipv4:c0000201", "a.b+tag@uttt.edu.mx"), actual);
        Assert.NotEqual(Reference("testing", "login", "origin-email", "ipv4:c0000201a.b+tag@uttt.edu.mx"), actual);
        Assert.NotEqual(Reference("testing", "login", "origin-email", "a.b+tag@uttt.edu.mx", "ipv4:c0000201"), actual);
        Assert.NotEqual(Reference("testing", "login", "origin-email", "ipv4:c000020", "1a.b+tag@uttt.edu.mx"), actual);
    }

    [Fact]
    public void Authenticated_account_is_uuid_D_lowercase_under_separate_domain()
    {
        using var pseudonymizer = Create();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", AccountId.ToUpperInvariant())], "synthetic-validated"));
        var account = AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(principal);
        var identity = AuthenticationScopeIdentity.Account(account);
        Assert.Equal(AuthenticationOperation.Logout, identity.Operation);
        Assert.Equal(AuthenticationQuotaScopeType.Account, identity.ScopeType);
        Assert.Equal(Reference("testing", "logout", "account", AccountId), pseudonymizer.Pseudonymize(identity));
        Assert.NotEqual(Reference("testing", "logout", "origin", AccountId), pseudonymizer.Pseudonymize(identity));
        Assert.DoesNotContain(AccountId, account.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("not-a-uuid")]
    [InlineData("6e4240ea5b734a87b911b180a4bbcd21")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Account_rejects_noncanonical_shape_or_empty_identifier(string value)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", value)], "synthetic-validated"));
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(principal));
    }

    [Fact]
    public void Account_requires_one_authenticated_identity_and_one_verified_subject()
    {
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(
            new(new ClaimsIdentity([new Claim("sub", AccountId)]))));
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(
            new(new ClaimsIdentity([new Claim("sub", AccountId), new Claim("sub", AccountId)], "synthetic-validated"))));
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(
            new(new[] { new ClaimsIdentity([new Claim("sub", AccountId)], "first"), new ClaimsIdentity([new Claim("sub", AccountId)], "second") })));
        var anonymousSubject = new ClaimsIdentity([new Claim("sub", AccountId)]);
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(
            new(new[] { new ClaimsIdentity([], "authenticated-without-sub"), anonymousSubject })));
    }

    [Fact]
    public void Scope_operation_mismatch_cannot_create_identity()
    {
        Assert.Throws<ArgumentException>(() => AuthenticationScopeIdentity.Email(AuthenticationOperation.Logout,
            AuthenticationCanonicalEmail.FromValidatedEmail("user@uttt.edu.mx")));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuthenticationScopeIdentity.Origin((AuthenticationOperation)999,
            AuthenticationCanonicalOrigin.FromAddress(IPAddress.Loopback)));
    }

    [Fact]
    public void Snapshot_does_not_follow_uncoordinated_key_or_revision_changes_and_output_fits_052()
    {
        var options = new RateLimitingOptions { PseudonymizationKeyBase64 = Convert.ToBase64String(SyntheticKey), PolicyRevision = "academic-initial" };
        using var pseudonymizer = new HmacAuthenticationScopePseudonymizer(Options.Create(options), Options.Create(new JwtOptions()), new TestEnvironment("Testing"));
        var opaque = pseudonymizer.Pseudonymize(Email("a.b+tag@uttt.edu.mx"));
        options.PolicyRevision = "new-revision";
        options.PseudonymizationKeyBase64 = Convert.ToBase64String(Enumerable.Repeat((byte)99, 32).ToArray());
        Assert.Equal(opaque, pseudonymizer.Pseudonymize(Email("a.b+tag@uttt.edu.mx")));
        var definition = AuthenticationQuotaPolicyDescriptor.AcademicInitial.GetDefinition(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email);
        var scope = new AuthenticationQuotaScope(AuthenticationQuotaScopeType.Email, opaque, definition);
        Assert.Equal(opaque, scope.OpaqueId);
        Assert.DoesNotContain(typeof(AuthenticationScopeIdentity).GetProperties(), property => property.Name is "PolicyRevision" or "InternalAttemptId" or "RedisKey");
    }

    [Fact]
    public async Task Concurrent_calls_are_deterministic_and_disposal_does_not_create_fallback()
    {
        var pseudonymizer = Create();
        var identity = Email("user@uttt.edu.mx");
        var expected = pseudonymizer.Pseudonymize(identity);
        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => pseudonymizer.Pseudonymize(identity))));
        Assert.All(results, result => Assert.Equal(expected, result));
        pseudonymizer.Dispose();
        pseudonymizer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => pseudonymizer.Pseudonymize(identity));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("testing invalid")]
    [InlineData("testing\n")]
    public void Environment_must_be_supplied_by_valid_host_context(string? value) =>
        Assert.Throws<ArgumentException>(() => Create(environment: value!));

    internal static HmacAuthenticationScopePseudonymizer Create(string? key = null, string environment = "Testing", JwtOptions? jwt = null) =>
        new(Options.Create(new RateLimitingOptions { PseudonymizationKeyBase64 = key ?? Convert.ToBase64String(SyntheticKey) }),
            Options.Create(jwt ?? new JwtOptions()), new TestEnvironment(environment));

    private static AuthenticationScopeIdentity Email(string value) => AuthenticationScopeIdentity.Email(AuthenticationOperation.Login,
        AuthenticationCanonicalEmail.FromValidatedEmail(value));

    private static AuthenticationScopeIdentity Origin(string value) => AuthenticationScopeIdentity.Origin(AuthenticationOperation.Login,
        AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse(value)));

    private static string Reference(string environment, string operation, string scope, params string[] components)
    {
        using var stream = new MemoryStream();
        foreach (var field in new[] { "enver.auth.quota", "1", environment, operation, scope }.Concat(components))
        {
            var bytes = Encoding.UTF8.GetBytes(field);
            var size = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(size, (uint)bytes.Length);
            stream.Write(size);
            stream.Write(bytes);
        }
        return Convert.ToBase64String(HMACSHA256.HashData(SyntheticKey, stream.ToArray())).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private sealed class TestEnvironment(string environment) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environment;
        public string ApplicationName { get; set; } = nameof(AuthenticationScopePseudonymizerTests);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

using System.Net;
using System.Security.Claims;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.API.SecurityTests;

// Sin Collection, IClassFixture, factory ni dependencia de AuthenticationDependenciesFixture/Testcontainers.
public sealed class AuthenticationScopePseudonymizationSecurityTests
{
    [Theory]
    [InlineData("X-Forwarded-For")]
    [InlineData("Forwarded")]
    [InlineData("X-Original-For")]
    public void Spoofed_header_never_establishes_trust(string header)
    {
        var context = Context("192.0.2.1");
        context.Request.Headers[header] = "for=198.51.100.99";
        var result = new AuthenticationOriginProvider().Resolve(context);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, result.Status);
        Assert.Null(result.Origin);
    }

    [Fact]
    public void Missing_origin_is_unavailable_even_when_direct_connection_was_approved()
    {
        var context = new DefaultHttpContext();
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        Assert.Equal(AuthenticationOriginStatus.Unavailable, new AuthenticationOriginProvider().Resolve(context).Status);
    }

    [Fact]
    public void Direct_mode_ignores_arbitrary_forwarding_and_preserves_actual_peer_partition()
    {
        var context = Context("192.0.2.1");
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        var provider = new AuthenticationOriginProvider();
        using var pseudonymizer = Pseudonymizer();
        var original = Pseudonymize(pseudonymizer, provider.Resolve(context));
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.1, invalid-value";
        context.Request.Headers["Forwarded"] = "for=198.51.100.2;proto=https";
        context.Request.Headers["X-Original-For"] = "198.51.100.3";
        Assert.Equal(original, Pseudonymize(pseudonymizer, provider.Resolve(context)));
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.2");
        Assert.NotEqual(original, Pseudonymize(pseudonymizer, provider.Resolve(context)));
    }

    [Fact]
    public void Required_but_unverified_forwarding_never_falls_back_to_proxy_peer_or_unknown()
    {
        var context = Context("192.0.2.10");
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.2";
        context.Features.Set(AuthenticationOriginTrustContext.RequireVerifiedForwarding(null));
        var provider = new AuthenticationOriginProvider();
        Assert.Same(AuthenticationOriginResult.Unavailable, provider.Resolve(context));
        Assert.Same(AuthenticationOriginResult.Unavailable, provider.Resolve(context));
        Assert.Null(provider.Resolve(context).Origin);
    }

    [Fact]
    public void Verified_forwarding_uses_only_server_supplied_resolution_not_headers_or_peer()
    {
        var context = Context("192.0.2.10");
        var canonical = AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse("198.51.100.2"));
        context.Features.Set(AuthenticationOriginTrustContext.RequireVerifiedForwarding(canonical));
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.99";
        var result = new AuthenticationOriginProvider().Resolve(context);
        Assert.Equal(AuthenticationOriginStatus.Available, result.Status);
        Assert.Same(canonical, result.Origin);
        // Acredita consumo de la feature de servidor, NO validación real de una cadena de proxies.
    }

    [Theory]
    [InlineData("192.0.2.1", "::ffff:192.0.2.1")]
    [InlineData("2001:db8::1", "2001:0DB8:0000:0000:0000:0000:0000:0001")]
    public void Equivalent_IP_representations_cannot_split_partitions(string first, string second)
    {
        using var pseudonymizer = Pseudonymizer();
        var provider = new AuthenticationOriginProvider();
        var context = Context(first);
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        var original = Pseudonymize(pseudonymizer, provider.Resolve(context));
        context.Connection.RemoteIpAddress = IPAddress.Parse(second);
        Assert.Equal(original, Pseudonymize(pseudonymizer, provider.Resolve(context)));
    }

    [Fact]
    public void Zoned_peer_is_unavailable_without_silent_zone_removal()
    {
        var context = Context("fe80::1%7");
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        Assert.Equal(AuthenticationOriginStatus.Unavailable, new AuthenticationOriginProvider().Resolve(context).Status);
    }

    [Fact]
    public void Outputs_and_diagnostics_do_not_expose_raw_values_or_key()
    {
        const string email = "sensitive.synthetic+tag@uttt.edu.mx";
        const string ip = "192.0.2.123";
        const string accountId = "b783fa80-a126-4f0a-beb7-c08b632a5d17";
        var key = SyntheticKey();
        var canonicalEmail = AuthenticationCanonicalEmail.FromValidatedEmail(email);
        var origin = AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse(ip));
        var account = AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", accountId)], "synthetic-validated")));
        AuthenticationScopeIdentity[] identities =
        [
            AuthenticationScopeIdentity.Email(AuthenticationOperation.Login, canonicalEmail),
            AuthenticationScopeIdentity.Origin(AuthenticationOperation.Login, origin),
            AuthenticationScopeIdentity.OriginEmail(origin, canonicalEmail),
            AuthenticationScopeIdentity.Account(account)
        ];
        using var pseudonymizer = Pseudonymizer();
        foreach (var identity in identities)
        {
            var opaque = pseudonymizer.Pseudonymize(identity);
            Assert.Matches("^[A-Za-z0-9_-]{43}$", opaque);
            foreach (var raw in new[] { email, ip, accountId, key })
            {
                Assert.DoesNotContain(raw, opaque, StringComparison.Ordinal);
                Assert.DoesNotContain(raw, identity.ToString(), StringComparison.Ordinal);
                Assert.DoesNotContain(raw, pseudonymizer.ToString(), StringComparison.Ordinal);
            }
        }
        Assert.DoesNotContain(email, canonicalEmail.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(ip, origin.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(accountId, account.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(typeof(AuthenticationScopeIdentity).GetProperties(), property =>
            property.Name is "Components" or "Email" or "Address" or "Account" or "Key");
    }

    [Fact]
    public void Unauthenticated_or_ambiguous_account_claims_cannot_create_account_scope()
    {
        var context = Context("192.0.2.1");
        const string id = "b783fa80-a126-4f0a-beb7-c08b632a5d17";
        context.Request.Headers["sub"] = id;
        context.Request.QueryString = new("?account=" + id);
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", id)]));
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(context.User));
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", id), new Claim("sub", id)], "synthetic-validated"));
        Assert.Throws<ArgumentException>(() => AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(context.User));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-secret-but-invalid-base64")]
    public void Invalid_configuration_fails_safely_without_random_key_or_echo(string? key)
    {
        var options = Options.Create(new RateLimitingOptions { PseudonymizationKeyBase64 = key! });
        var error = Assert.Throws<ArgumentException>(() => new HmacAuthenticationScopePseudonymizer(options,
            Options.Create(new JwtOptions()), new TestEnvironment()));
        if (!string.IsNullOrEmpty(key)) Assert.DoesNotContain(key, error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Short_or_reused_key_is_rejected_even_without_registered_options_validation()
    {
        var shortKey = Convert.ToBase64String(Enumerable.Repeat((byte)3, 31).ToArray());
        Assert.Throws<ArgumentException>(() => Pseudonymizer(shortKey));
        var key = SyntheticKey();
        var jwt = new JwtOptions { Keys = [new() { Kid = "synthetic", KeyBase64 = key }] };
        var error = Assert.Throws<ArgumentException>(() => new HmacAuthenticationScopePseudonymizer(
            Options.Create(new RateLimitingOptions { PseudonymizationKeyBase64 = key }), Options.Create(jwt), new TestEnvironment()));
        Assert.DoesNotContain(key, error.ToString(), StringComparison.Ordinal);
    }

    private static DefaultHttpContext Context(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }

    private static string Pseudonymize(HmacAuthenticationScopePseudonymizer pseudonymizer, AuthenticationOriginResult result)
    {
        Assert.Equal(AuthenticationOriginStatus.Available, result.Status);
        return pseudonymizer.Pseudonymize(AuthenticationScopeIdentity.Origin(AuthenticationOperation.Login, result.Origin!));
    }

    private static string SyntheticKey() => Convert.ToBase64String(Enumerable.Repeat((byte)53, 32).ToArray());
    private static HmacAuthenticationScopePseudonymizer Pseudonymizer(string? key = null) => new(
        Options.Create(new RateLimitingOptions { PseudonymizationKeyBase64 = key ?? SyntheticKey() }),
        Options.Create(new JwtOptions()), new TestEnvironment());

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = nameof(AuthenticationScopePseudonymizationSecurityTests);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

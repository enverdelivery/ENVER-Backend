using System.Net;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.SecurityTests;

// ST de 055: fronteras de seguridad enlazadas a un Redis REAL y aislado por fixture de test.
// No demuestra forwarding productivo, proxies, NAT reales, AWS ni capacidad de red externa.
[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaSecurityTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    private const string Email = "sensitive.synthetic+tag@uttt.edu.mx";
    private const string PeerIp = "203.0.113.10";
    private const string ForwardedIp = "198.51.100.99";

    [Fact]
    public async Task Spoofed_forwarding_headers_never_create_or_share_a_real_partition()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        var context = Context(PeerIp);
        context.Request.Headers["X-Forwarded-For"] = ForwardedIp;
        context.Request.Headers["Forwarded"] = $"for={ForwardedIp};proto=https";
        context.Request.Headers["X-Original-For"] = ForwardedIp;
        Assert.Equal(AuthenticationOriginStatus.Unavailable, new AuthenticationOriginProvider().Resolve(context).Status);
        Assert.Equal(0, await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry));
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        var resolved = new AuthenticationOriginProvider().Resolve(context);
        Assert.Equal(AuthenticationOriginStatus.Available, resolved.Status);
        // Igualdad de identidad opaca de cuota: AuthenticationCanonicalOrigin no implementa igualdad por valor.
        Assert.Equal(
            scope.OriginOpaque(AuthenticationCanonicalOrigin.FromAddress(IPAddress.Parse(PeerIp))),
            scope.OriginOpaque(resolved.Origin!));
        var request = scope.RegisterRequest(Email, resolved.Origin!);
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await scope.ConsumeAsync(request)).Status);
        var originKey = scope.OriginKey(resolved.Origin!);
        Assert.True(await scope.Database.KeyExistsAsync(originKey));
        foreach (var forbidden in new[] { PeerIp, ForwardedIp, Email, scope.KeyBase64 })
        {
            Assert.DoesNotContain(forbidden, originKey.ToString(), StringComparison.Ordinal);
            foreach (var key in scope.OwnedKeys) Assert.DoesNotContain(forbidden, key.ToString(), StringComparison.Ordinal);
        }
        foreach (var member in await scope.Database.SortedSetRangeByRankAsync(scope.Configuration.Keys.Registry))
            foreach (var forbidden in new[] { PeerIp, ForwardedIp, Email, scope.KeyBase64 })
                Assert.DoesNotContain(forbidden, member.ToString(), StringComparison.Ordinal);
        var registryAfter = await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry);
        output.WriteLine($"SPOOFING_RESULT SPOOFED_HEADERS=Unavailable TRUSTED_DIRECT=Available ORIGIN_OPAQUE_MATCH=true REGISTRY={registryAfter} DISPATCHES={scope.Dispatches} DIRTY={(await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "dirty")).ToString()}");
    }

    [Fact]
    public async Task NAT_and_equivalent_address_forms_share_one_physical_origin_partition()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        var natOrigin = ResolveOrigin(PeerIp);
        var first = scope.RegisterRequest(Email, natOrigin);
        var second = scope.RegisterRequest("second.synthetic@uttt.edu.mx", natOrigin);
        var third = scope.RegisterRequest("third.synthetic@uttt.edu.mx", natOrigin);
        foreach (var request in new[] { first, second, third })
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await scope.ConsumeAsync(request)).Status);
        var natKey = scope.OriginKey(natOrigin);
        Assert.Equal(3, await scope.Database.SortedSetLengthAsync(natKey));
        // La representación IPv4-mapped de la MISMA dirección no abre otra partición de origen.
        var mapped = ResolveOrigin("::ffff:" + PeerIp);
        Assert.Equal(scope.OriginOpaque(natOrigin), scope.OriginOpaque(mapped));
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted,
            (await scope.ConsumeAsync(scope.RegisterRequest("mapped.synthetic@uttt.edu.mx", mapped))).Status);
        Assert.Equal(4, await scope.Database.SortedSetLengthAsync(natKey));
        // IPv6 equivalente comparte partición; una dirección distinta no.
        var ipv6 = ResolveOrigin("2001:db8::1");
        var ipv6Expanded = ResolveOrigin("2001:0DB8:0000:0000:0000:0000:0000:0001");
        Assert.Equal(scope.OriginOpaque(ipv6), scope.OriginOpaque(ipv6Expanded));
        Assert.NotEqual(scope.OriginOpaque(ipv6), scope.OriginOpaque(ResolveOrigin("2001:db8::2")));
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted,
            (await scope.ConsumeAsync(scope.RegisterRequest("v6.synthetic@uttt.edu.mx", ipv6))).Status);
        Assert.Equal(1, await scope.Database.SortedSetLengthAsync(scope.OriginKey(ipv6)));
        Assert.False(await scope.Database.KeyExistsAsync(scope.OriginKey(ResolveOrigin("2001:db8::2"))));
        // Distintos correos detrás del mismo NAT comparten el contador de origen y conservan el propio.
        Assert.Equal(1, await scope.Database.SortedSetLengthAsync(
            scope.EmailKey(AuthenticationCanonicalEmail.FromValidatedEmail(Email))));
    }

    [Fact]
    public async Task Untrusted_origin_never_dispatches_and_creates_no_partition()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, new AuthenticationOriginProvider().Resolve(Context(PeerIp)).Status);
        var forged = Context(PeerIp);
        forged.Request.Headers["X-Forwarded-For"] = ForwardedIp;
        forged.Features.Set(AuthenticationOriginTrustContext.RequireVerifiedForwarding(null));
        Assert.Equal(AuthenticationOriginStatus.Unavailable, new AuthenticationOriginProvider().Resolve(forged).Status);
        Assert.Equal(0, await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry));
        Assert.Equal(0, scope.Dispatches);
        Assert.Equal("ready", (await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "state")).ToString());
        Assert.Equal("0", (await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "count")).ToString());
    }

    [Fact]
    public async Task Keys_members_and_diagnostics_never_expose_raw_identity_or_key_material()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        var origin = ResolveOrigin(PeerIp);
        var canonicalEmail = AuthenticationCanonicalEmail.FromValidatedEmail(Email);
        var request = scope.RegisterRequest(Email, origin);
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await scope.ConsumeAsync(request)).Status);
        var diagnostics = new[]
        {
            request.ToString()!, request.Scopes[0].ToString()!, scope.Store.ToString(), scope.Pseudonymizer.ToString()!,
            AuthenticationScopeIdentity.Email(AuthenticationOperation.Register, canonicalEmail).ToString()!,
            AuthenticationScopeIdentity.Origin(AuthenticationOperation.Register, origin).ToString()!
        };
        foreach (var value in diagnostics)
            foreach (var forbidden in new[] { Email, PeerIp, scope.KeyBase64 })
                Assert.DoesNotContain(forbidden, value, StringComparison.Ordinal);
        foreach (var key in scope.OwnedKeys)
            foreach (var forbidden in new[] { Email, PeerIp, scope.KeyBase64 })
                Assert.DoesNotContain(forbidden, key.ToString(), StringComparison.Ordinal);
        var opaqueIds = string.Join(",", request.Scopes.Select(scope2 => scope2.OpaqueId));
        foreach (var forbidden in new[] { Email, PeerIp, scope.KeyBase64 })
            Assert.DoesNotContain(forbidden, opaqueIds, StringComparison.Ordinal);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", request.Scopes[0].OpaqueId);
    }

    [Fact]
    public async Task Invalid_revision_and_divergent_snapshot_fail_closed_without_partitions()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        var forged = scope.RegisterRequest(Email, ResolveOrigin(PeerIp), revision: "forged-revision");
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await scope.ConsumeAsync(forged)).Status);
        Assert.Equal(0, scope.Dispatches);
        output.WriteLine($"INVALID_REVISION=Unavailable DISPATCHES={scope.Dispatches} DIRTY={(await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "dirty")).ToString()} REGISTRY={await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry)}");
        var divergent = new RedisAuthenticationQuotaConfiguration(new(
            scope.Configuration.Snapshot.Environment, "config-divergent", "identity-053",
            AuthenticationQuotaPolicyDescriptor.AcademicInitial,
            AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), scope.Configuration.Generation);
        var request = scope.RegisterRequest("other.synthetic@uttt.edu.mx", ResolveOrigin("203.0.113.77"));
        // El snapshot divergente debe llegar realmente al store: se enlaza la configuración divergente al ejecutor.
        var divergentStore = scope.NewStore(scope.OpenState, scope.OpenState, divergent);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            (await scope.ConsumeAsync(request, store: divergentStore)).Status);
        Assert.Equal(1, scope.Dispatches);
        foreach (var scopeType in new[] { AuthenticationQuotaScopeType.Email, AuthenticationQuotaScopeType.Origin })
            Assert.False(await scope.Database.KeyExistsAsync(
                scope.Configuration.Keys.Bucket(request.Operation, scopeType, request.Scopes.Single(scope2 => scope2.ScopeType == scopeType).OpaqueId)));
        Assert.Equal(0, await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry));
        Assert.Equal("0", (await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "dirty")).ToString());
        output.WriteLine($"DIVERGENT_SNAPSHOT=Unavailable DISPATCHES={scope.Dispatches} DIRTY={(await scope.Database.HashGetAsync(scope.Configuration.Keys.Metadata, "dirty")).ToString()} REGISTRY={await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry)}");
    }

    [Fact]
    public async Task Closed_local_or_global_security_state_never_dispatches()
    {
        await using var scope = await SecurityScope.OpenAsync(fixture);
        var request = scope.RegisterRequest(Email, ResolveOrigin(PeerIp));
        var guard = new RedisSecurityStateGuard(scope.Connections, scope.Options);
        guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
        var blockedLocally = scope.NewStore(guard, scope.OpenState);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await scope.ConsumeAsync(request, store: blockedLocally)).Status);
        Assert.Equal(0, scope.Dispatches);
        var blockedGlobally = scope.NewStore(scope.OpenState, new FixedState(AuthenticationSecurityAdmission.Unavailable));
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await scope.ConsumeAsync(request, store: blockedGlobally)).Status);
        Assert.Equal(0, scope.Dispatches);
        var uncertainGlobally = scope.NewStore(scope.OpenState, new FixedState(AuthenticationSecurityAdmission.Uncertain));
        var uncertain = await scope.ConsumeAsync(request, store: uncertainGlobally);
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, uncertain.Status);
        Assert.True(uncertain.ResultUncertain);
        Assert.Equal(0, scope.Dispatches);
        Assert.Equal(0, await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry));
    }

    private static DefaultHttpContext Context(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }

    private static AuthenticationCanonicalOrigin ResolveOrigin(string ip)
    {
        var context = Context(ip);
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        var result = new AuthenticationOriginProvider().Resolve(context);
        Assert.Equal(AuthenticationOriginStatus.Available, result.Status);
        return result.Origin!;
    }

    private sealed class SecurityScope : IAsyncDisposable
    {
        private readonly AuthenticationDependenciesFixture fixture;
        private readonly List<RedisKey> owned = [];
        internal RedisOptions Options { get; }
        internal RedisConnections Connections { get; }
        internal IDatabase Database { get; private set; } = null!;
        internal RedisAuthenticationQuotaConfiguration Configuration { get; }
        internal RedisAuthenticationQuotaStore Store { get; private set; } = null!;
        internal HmacAuthenticationScopePseudonymizer Pseudonymizer { get; }
        internal VerifiableState OpenState { get; } = new();
        internal string KeyBase64 { get; } = Convert.ToBase64String(Enumerable.Repeat((byte)55, 32).ToArray());
        internal long Dispatches;
        internal IEnumerable<RedisKey> OwnedKeys => owned;

        private SecurityScope(AuthenticationDependenciesFixture fixture)
        {
            this.fixture = fixture;
            var environment = "st055_" + Guid.NewGuid().ToString("N")[..8];
            Configuration = new(new(environment, "config-055-st", "identity-053",
                AuthenticationQuotaPolicyDescriptor.AcademicInitial,
                AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), Guid.NewGuid().ToString("N"));
            Options = new RedisOptions
            {
                ConnectionString = fixture.RedisConnectionString + ",allowAdmin=true",
                NamespacePrefix = "enver:" + environment,
                OperationTimeoutMilliseconds = 2000,
                ConnectionTimeoutMilliseconds = 2000
            };
            Connections = new RedisConnections(Options);
            Pseudonymizer = new HmacAuthenticationScopePseudonymizer(
                Microsoft.Extensions.Options.Options.Create(new RateLimitingOptions { PseudonymizationKeyBase64 = KeyBase64 }),
                Microsoft.Extensions.Options.Options.Create(new JwtOptions()), new TestEnvironment());
            Track(Configuration.Keys.Metadata);
            Track(Configuration.Keys.Registry);
        }

        internal static async Task<SecurityScope> OpenAsync(AuthenticationDependenciesFixture fixture)
        {
            var scope = new SecurityScope(fixture);
            try
            {
                scope.Database = (await scope.Connections.GetMultiplexerAsync()).GetDatabase();
                var lifecycle = new RedisAuthenticationQuotaLifecycle(scope.Connections, scope.Options, scope.Configuration,
                    scope.OpenState, scope.OpenState);
                using var budget = Budget();
                Assert.Equal("initialized", (await lifecycle.InitializeVerifiedNewEnvironmentAsync(budget)).Status);
                scope.Store = scope.NewStore(scope.OpenState, scope.OpenState);
                return scope;
            }
            catch
            {
                await scope.DisposeAsync();
                throw;
            }
        }

        internal RedisAuthenticationQuotaStore NewStore(IAuthenticationSecurityState local, IAuthenticationSecurityState global,
            RedisAuthenticationQuotaConfiguration? configuration = null) =>
            new(Connections, Options, configuration ?? Configuration, local, global,
                async (dependency, invocation) =>
                {
                    Interlocked.Increment(ref Dispatches);
                    var database = (await Connections.GetMultiplexerAsync(dependency.Token)).GetDatabase();
                    return await database.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
                        invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster).WaitAsync(dependency.Token);
                },
                () => ValueTask.FromResult(Array.Empty<string>()));

        internal AuthenticationQuotaConsumeRequest RegisterRequest(string email, AuthenticationCanonicalOrigin origin, string? revision = null) =>
            RegisterRequest(email, origin, revision, Configuration);

        internal AuthenticationQuotaConsumeRequest RegisterRequest(string email, AuthenticationCanonicalOrigin origin,
            string? revision, RedisAuthenticationQuotaConfiguration configuration)
        {
            var canonicalEmail = AuthenticationCanonicalEmail.FromValidatedEmail(email);
            var identities = AuthenticationQuotaPolicyDescriptor.RequiredScopes(AuthenticationOperation.Register)
                .Select(type => type switch
                {
                    AuthenticationQuotaScopeType.Email =>
                        AuthenticationScopeIdentity.Email(AuthenticationOperation.Register, canonicalEmail),
                    AuthenticationQuotaScopeType.Origin =>
                        AuthenticationScopeIdentity.Origin(AuthenticationOperation.Register, origin),
                    _ => throw new InvalidOperationException("Ámbito no usado por el registro.")
                });
            var request = new AuthenticationQuotaConsumeRequest(AuthenticationOperation.Register,
                Guid.NewGuid().ToString("N"), revision ?? configuration.Snapshot.Policy.Revision,
                identities.Select(identity => new AuthenticationQuotaScope(identity.ScopeType,
                    Pseudonymizer.Pseudonymize(identity),
                    configuration.Snapshot.Policy.GetDefinition(AuthenticationOperation.Register, identity.ScopeType))));
            foreach (var scope in request.Scopes)
                Track(configuration.Keys.Bucket(request.Operation, scope.ScopeType, scope.OpaqueId));
            return request;
        }

        internal async Task<AuthenticationQuotaConsumeResult> ConsumeAsync(AuthenticationQuotaConsumeRequest request,
            RedisAuthenticationQuotaConfiguration? configuration = null, RedisAuthenticationQuotaStore? store = null)
        {
            using var budget = Budget();
            var target = store ?? (configuration is null ? Store : NewStore(OpenState, OpenState));
            return await target.EvaluateAndConsumeAsync(request, budget);
        }

        internal RedisKey OriginKey(AuthenticationCanonicalOrigin origin) =>
            Configuration.Keys.Bucket(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin, OriginOpaque(origin));

        internal string OriginOpaque(AuthenticationCanonicalOrigin origin) =>
            Pseudonymizer.Pseudonymize(AuthenticationScopeIdentity.Origin(AuthenticationOperation.Register, origin));

        internal RedisKey EmailKey(AuthenticationCanonicalEmail email) =>
            Configuration.Keys.Bucket(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email,
                Pseudonymizer.Pseudonymize(AuthenticationScopeIdentity.Email(AuthenticationOperation.Register, email)));

        private void Track(RedisKey key) { lock (owned) owned.Add(key); }

        private static RequestBudget Budget() => new(10000, CancellationToken.None);

        public async ValueTask DisposeAsync()
        {
            IReadOnlyList<RedisKey> keys;
            lock (owned) keys = owned.ToArray();
            if (Database is not null)
                foreach (var batch in keys.Chunk(128)) await Database.KeyDeleteAsync(batch, CommandFlags.DemandMaster);
            Pseudonymizer.Dispose();
            await Connections.DisposeAsync();
        }

        internal sealed class VerifiableState : IAuthenticationSecurityState
        {
            public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) =>
                ValueTask.FromResult(new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Verifiable));
        }
    }

    private sealed class FixedState(AuthenticationSecurityAdmission admission) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) =>
            ValueTask.FromResult(new AuthenticationSecurityDecision(admission));
    }

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = nameof(DistributedQuotaSecurityTests);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

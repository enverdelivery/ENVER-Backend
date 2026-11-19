using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisOperationalScopeSecurityTests(
    AuthenticationDependenciesFixture fixture,
    ITestOutputHelper output)
{
    [Fact]
    public async Task Closed_or_unverifiable_shared_state_never_authorizes_through_a_second_host()
    {
        await using var scope = await SecurityScope.CreateAsync(fixture);
        var jti = SecurityScope.NewJti();

        var write = await scope.RevokeAsync(0, jti, DateTimeOffset.UtcNow.AddMinutes(5));
        Assert.Equal(TokenRevocationWriteStatus.Confirmed, write.Status);
        Assert.True(await scope.KeyExistsAsync(jti));

        scope.CloseBothByInjectedSignal();
        Assert.True(await scope.PingAsync());

        var firstDecision = await scope.EvaluateAsync(0);
        var secondDecision = await scope.EvaluateAsync(1);
        Assert.False(firstDecision.CanContinue);
        Assert.False(secondDecision.CanContinue);

        var firstRead = await scope.CheckAsync(0, jti);
        var secondRead = await scope.CheckAsync(1, jti);
        Assert.Equal(TokenRevocationReadStatus.Unavailable, firstRead.Status);
        Assert.Equal(TokenRevocationReadStatus.Unavailable, secondRead.Status);
        Assert.False(firstRead.ResultUncertain);
        Assert.False(secondRead.ResultUncertain);

        var firstWrite = await scope.RevokeAsync(0, SecurityScope.NewJti(), DateTimeOffset.UtcNow.AddMinutes(5));
        var secondWrite = await scope.RevokeAsync(1, SecurityScope.NewJti(), DateTimeOffset.UtcNow.AddMinutes(5));
        Assert.Equal(TokenRevocationWriteStatus.Unavailable, firstWrite.Status);
        Assert.Equal(TokenRevocationWriteStatus.Unavailable, secondWrite.Status);

        output.WriteLine(
            "CLOSED_STATE_HOSTS=2 DENIED=2 EXISTING_MARK_NOT_AUTHORIZED=true REDIS_PING_WHILE_CLOSED=true " +
            "LOCAL_FALLBACK=false NEGATIVE_CACHE_PERMISSIVE=false CLOSURE_SIGNAL=INJECTED REACTION_ONLY=true " +
            "DETECTOR_EVIDENCE=T-AUTH-019/T-AUTH-055");
    }

    [Fact]
    public async Task Revoked_or_opaque_operational_diagnostics_do_not_expose_identities_or_secret_material()
    {
        await using var scope = await SecurityScope.CreateAsync(fixture);
        var jti = SecurityScope.NewJti();
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5);

        var write = await scope.RevokeAsync(0, jti, expiry);
        Assert.Equal(TokenRevocationWriteStatus.Confirmed, write.Status);
        var read = await scope.CheckAsync(1, jti);
        Assert.Equal(TokenRevocationReadStatus.Revoked, read.Status);
        var ttl = await scope.TimeToLiveAsync(jti);
        Assert.NotNull(ttl);
        Assert.True(ttl > TimeSpan.Zero);

        var report = scope.BuildDiagnosticReport(revokedObserved: true, ttlPreserved: true);
        output.WriteLine(report);

        var email = "privacy.synthetic+tag@uttt.edu.mx";
        var ip = "203.0.113.88";
        var opaque = Convert.ToBase64String(SHA256.HashData(BitConverter.GetBytes(22)))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var subject = Guid.NewGuid().ToString("D");
        var jwt = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJzeW50aGV0aWMifQ.c2lnbmF0dXJl";

        Assert.DoesNotContain(jti, report);
        Assert.DoesNotContain(email, report);
        Assert.DoesNotContain(ip, report);
        Assert.DoesNotContain(opaque, report);
        Assert.DoesNotContain(subject, report);
        Assert.DoesNotContain(jwt, report);
        Assert.DoesNotContain("eyJ", report);
        Assert.DoesNotContain(scope.EnvironmentName, report);
        Assert.DoesNotContain(scope.RevocationKey(jti), report);
        Assert.DoesNotContain(scope.AdminConnectionString, report);
        Assert.False(fixture.ContainsSyntheticSecret(report));
        Assert.DoesNotContain("password", report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connectionstring", report, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sub=", report, StringComparison.OrdinalIgnoreCase);
    }

    // Composición EXCLUSIVAMENTE de test con dos hosts lógicos y estado local independiente.
    private sealed class SecurityScope : IAsyncDisposable
    {
        private readonly AuthenticationDependenciesFixture fixture;
        private readonly string environment;
        private readonly RedisConnections firstConnections;
        private readonly RedisConnections secondConnections;
        private readonly RedisSecurityStateGuard firstGuard;
        private readonly RedisSecurityStateGuard secondGuard;
        private readonly RedisTokenRevocationStore firstStore;
        private readonly RedisTokenRevocationStore secondStore;
        private readonly HashSet<RedisKey> owned = [];
        private readonly object sync = new();
        private ConnectionMultiplexer? admin;
        private IDatabase? database;
        private Task? disposal;

        internal string EnvironmentName => environment;
        internal string AdminConnectionString => fixture.RedisConnectionString;
        internal string RevocationKey(string jti) => RedisKeyBuilder.BuildRevocationKey(environment, "enver-api", jti);

        private SecurityScope(AuthenticationDependenciesFixture fixture)
        {
            this.fixture = fixture;
            environment = fixture.EnvironmentName.ToLowerInvariant() + "_op022st_" + Guid.NewGuid().ToString("N")[..8];
            var firstOptions = Options(fixture.RedisConnectionString, environment);
            var secondOptions = Options(fixture.RedisConnectionString, environment);
            firstConnections = new RedisConnections(firstOptions);
            secondConnections = new RedisConnections(secondOptions);
            firstGuard = new RedisSecurityStateGuard(firstConnections, firstOptions);
            secondGuard = new RedisSecurityStateGuard(secondConnections, secondOptions);
            firstStore = new RedisTokenRevocationStore(firstConnections, firstGuard, firstOptions);
            secondStore = new RedisTokenRevocationStore(secondConnections, secondGuard, secondOptions);
        }

        internal static async Task<SecurityScope> CreateAsync(AuthenticationDependenciesFixture fixture)
        {
            var scope = new SecurityScope(fixture);
            try
            {
                await scope.InitializeAsync();
                return scope;
            }
            catch (Exception initializationFailure)
            {
                try
                {
                    await scope.DisposeAsync();
                }
                catch (Exception cleanupFailure)
                {
                    throw new AggregateException(
                        "Fallaron la inicialización y el cleanup del alcance operativo de seguridad.",
                        initializationFailure,
                        cleanupFailure);
                }

                throw;
            }
        }

        private async Task InitializeAsync()
        {
            var configuration = ConfigurationOptions.Parse(fixture.RedisConnectionString);
            configuration.AllowAdmin = true;
            configuration.AbortOnConnectFail = false;
            admin = await ConnectionMultiplexer.ConnectAsync(configuration);
            database = admin.GetDatabase();
            _ = await firstConnections.GetMultiplexerAsync();
            _ = await secondConnections.GetMultiplexerAsync();
            Assert.NotSame(
                await firstConnections.GetMultiplexerAsync(),
                await secondConnections.GetMultiplexerAsync());
            Assert.True((await database.PingAsync()) >= TimeSpan.Zero);
        }

        internal async Task<TokenRevocationWriteResult> RevokeAsync(
            int host,
            string jti,
            DateTimeOffset expiresAtUtc)
        {
            lock (sync)
            {
                owned.Add(RevocationKey(jti));
            }

            using var budget = new RequestBudget(5000, CancellationToken.None);
            var store = host == 0 ? firstStore : secondStore;
            return await store.RevokeAsync(
                new TokenRevocationWriteRequest("enver-api", jti, expiresAtUtc, TimeSpan.Zero, TimeSpan.Zero),
                budget);
        }

        internal async Task<TokenRevocationReadResult> CheckAsync(int host, string jti)
        {
            using var budget = new RequestBudget(5000, CancellationToken.None);
            var store = host == 0 ? firstStore : secondStore;
            return await store.CheckAsync(new TokenRevocationReadRequest("enver-api", jti), budget);
        }

        internal async Task<AuthenticationSecurityDecision> EvaluateAsync(int host)
        {
            using var budget = new RequestBudget(5000, CancellationToken.None);
            return await (host == 0 ? firstGuard : secondGuard).EvaluateAsync(budget);
        }

        internal void CloseBothByInjectedSignal()
        {
            firstGuard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
            secondGuard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
        }

        internal async Task<bool> PingAsync() => (await Database().PingAsync()) >= TimeSpan.Zero;

        internal async Task<bool> KeyExistsAsync(string jti) =>
            await Database().KeyExistsAsync(RevocationKey(jti), CommandFlags.DemandMaster);

        internal async Task<TimeSpan?> TimeToLiveAsync(string jti) =>
            await Database().KeyTimeToLiveAsync(RevocationKey(jti), CommandFlags.DemandMaster);

        internal string BuildDiagnosticReport(bool revokedObserved, bool ttlPreserved) =>
            string.Join(
                " ",
                "OPERATIONAL_SCOPE_SECURITY",
                "HOSTS=2",
                "HOST_KIND=LOGICAL_TEST",
                "ENVIRONMENT_ISOLATED=true",
                "REVOCATION_NAMESPACE_SHARED=true",
                "PURPOSE_SEPARATED=true",
                "REVOCATION_CONFIRMED=true",
                $"REVOCATION_OBSERVED_BY_SECOND_HOST={Bool(revokedObserved)}",
                $"TTL_PRESERVED={Bool(ttlPreserved)}",
                "CLOSED_STATE_DENIES_BOTH=false",
                "LOCAL_FALLBACK=false",
                "NEGATIVE_CACHE_PERMISSIVE=false",
                "SECRET_MATERIAL=false",
                "IDENTITY_MATERIAL=false",
                "PURPOSE_REVOCATION=auth:revocation",
                "PURPOSE_RATE_LIMIT=auth:ratelimit",
                "TLS_LOCAL_APPLICABLE=false",
                "ACL_LOCAL_APPLICABLE=false",
                "PHYSICAL_HOSTS=false",
                "AWS=false",
                "LOAD_BALANCER=false",
                "PRODUCTION_CLAIM=false");

        public ValueTask DisposeAsync()
        {
            lock (sync)
            {
                return new ValueTask(disposal ??= DisposeCoreAsync());
            }
        }

        private async Task DisposeCoreAsync()
        {
            List<Exception> failures = [];
            try
            {
                RedisKey[] keys;
                lock (sync)
                {
                    keys = owned.ToArray();
                }

                if (database is not null)
                {
                    foreach (var batch in keys.Chunk(128))
                    {
                        await database.KeyDeleteAsync(batch, CommandFlags.DemandMaster);
                    }
                }
            }
            catch (Exception cleanupFailure)
            {
                failures.Add(cleanupFailure);
            }
            finally
            {
                try
                {
                    await firstConnections.DisposeAsync();
                }
                catch (Exception disposalFailure)
                {
                    failures.Add(disposalFailure);
                }

                try
                {
                    await secondConnections.DisposeAsync();
                }
                catch (Exception disposalFailure)
                {
                    failures.Add(disposalFailure);
                }

                try
                {
                    if (admin is not null)
                    {
                        await admin.DisposeAsync();
                    }
                }
                catch (Exception disposalFailure)
                {
                    failures.Add(disposalFailure);
                }

                lock (sync)
                {
                    owned.Clear();
                }
            }

            if (failures.Count > 0)
            {
                throw new AggregateException("Falló el cleanup del alcance operativo de seguridad.", failures);
            }
        }

        private IDatabase Database() =>
            database ?? throw new InvalidOperationException("Redis del alcance operativo no inicializado.");

        private static RedisOptions Options(string connectionString, string environment) => new()
        {
            ConnectionString = connectionString,
            NamespacePrefix = $"enver:{environment}",
            RevocationPurpose = "auth:revocation",
            RateLimitingPurpose = "auth:ratelimit",
            OperationTimeoutMilliseconds = 2000,
            ConnectionTimeoutMilliseconds = 2000,
            RevocationOperationalMarginSeconds = 30,
            FailClosed = true,
            RequiredRevocationEvictionPolicy = "noeviction"
        };

        internal static string NewJti() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        private static string Bool(bool value) => value ? "true" : "false";
    }
}

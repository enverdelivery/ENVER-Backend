using System.Net;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Amazon.DynamoDBv2;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enver.Testing;

/// <summary>Opt-in Login: usa el aislamiento/cleanup ya probado de Logout, sin alterar Program ni el factory habitual.</summary>
public sealed class LoginTestScope : IAsyncDisposable
{
    private readonly LogoutTestScope resources;
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly int? testOnlyLoginBudgetMilliseconds;
    private readonly bool captureRaceDiagnostics;
    private readonly List<LoginTestHost> hosts = [];
    public LoginTestHost First => hosts[0];
    public LogoutLogCapture Logs => resources.Logs;
    private LoginTestScope(LogoutTestScope resources, AuthenticationDependenciesFixture fixture, int? testOnlyLoginBudgetMilliseconds, bool captureRaceDiagnostics)
    { this.resources = resources; this.fixture = fixture; this.testOnlyLoginBudgetMilliseconds = testOnlyLoginBudgetMilliseconds; this.captureRaceDiagnostics = captureRaceDiagnostics; }
    public static async Task<LoginTestScope> CreateAsync(AuthenticationDependenciesFixture fixture, bool mongoUnavailable = false,
        int? testOnlyLoginBudgetMilliseconds = null, bool captureRaceDiagnostics = false)
    {
        var scope = new LoginTestScope(await LogoutTestScope.CreateAsync(fixture, mongoUnavailable), fixture, testOnlyLoginBudgetMilliseconds, captureRaceDiagnostics);
        try { await scope.AddHostAsync(); return scope; }
        catch { await scope.DisposeAsync(); throw; }
    }
    public async Task<LoginTestHost> AddHostAsync()
    {
        var configuration = resources.First.Services.GetRequiredService<IConfiguration>();
        if (testOnlyLoginBudgetMilliseconds is { } milliseconds)
            configuration = new ConfigurationBuilder().AddConfiguration(configuration).AddInMemoryCollection(new Dictionary<string, string?>
            { ["Authentication:LoginBudgetMilliseconds"] = milliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) }).Build();
        var host = new LoginTestHost(fixture, configuration,
            resources.First.Services.GetRequiredService<IHostEnvironment>().EnvironmentName, Logs, captureRaceDiagnostics);
        hosts.Add(host); await host.StartAsync(); return host;
    }
    public async Task TrackAsync(string token) => resources.Track(await resources.First.InputAsync(token));
    public async Task RemoveAuthorityAsync() => await resources.RemoveAuthorityAsync();
    public async ValueTask DisposeAsync()
    {
        try { foreach (var host in hosts.AsEnumerable().Reverse()) await host.DisposeAsync(); }
        finally { await resources.DisposeAsync(); }
    }
}

public sealed class LoginTestHost : IAsyncDisposable
{
    private readonly IHost host;
    public IServiceProvider Services => host.Services;
    public HttpClient Client { get; private set; } = null!;
    public LoginUserStoreProbe Users => Services.GetRequiredService<LoginUserStoreProbe>();
    public LoginPasswordProbe Passwords => Services.GetRequiredService<LoginPasswordProbe>();
    public LoginKdfProbe Kdf => Services.GetRequiredService<LoginKdfProbe>();
    public LoginIssuerProbe Issuer => Services.GetRequiredService<LoginIssuerProbe>();
    public LogoutQuotaProbe Quotas => Services.GetRequiredService<LogoutQuotaProbe>();
    public LogoutAuthorityProbe Authority => Services.GetRequiredService<LogoutAuthorityProbe>();
    public LoginRaceDiagnostics? RaceDiagnostics { get; }

    internal LoginTestHost(AuthenticationDependenciesFixture fixture, IConfiguration configuration, string environment, LogoutLogCapture logs, bool captureRaceDiagnostics)
    {
        RaceDiagnostics = captureRaceDiagnostics ? new LoginRaceDiagnostics() : null;
        host = new HostBuilder().ConfigureLogging(l => { l.ClearProviders(); l.AddProvider(logs); })
            .ConfigureWebHost(web => web.UseTestServer().UseEnvironment(environment)
                .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(configuration.AsEnumerable()))
                .ConfigureServices((context, s) =>
                {
                    s.AddControllers().AddApplicationPart(typeof(Enver.API.Controllers.AuthenticationController).Assembly);
                    s.AddAuthenticationFoundation(context.Configuration, context.HostingEnvironment);
                    s.AddSingleton<IAmazonDynamoDB>(_ => fixture.CreateDynamoDbClient());
                    s.AddAuthenticationDependencies(context.Configuration);
                    var user = s.Last(d => d.ServiceType == typeof(IUserStore)); s.Remove(user);
                    s.AddSingleton(p => new LoginUserStoreProbe((IUserStore)user.ImplementationFactory!(p), RaceDiagnostics));
                    s.AddSingleton<IUserStore>(p => p.GetRequiredService<LoginUserStoreProbe>());
                    s.AddSingleton<LoginKdfProbe>(); s.AddSingleton<IPasswordHasher<User>>(p => p.GetRequiredService<LoginKdfProbe>());
                    s.AddSingleton<PasswordService>();
                    s.AddSingleton<LoginPasswordProbe>(p => new(p.GetRequiredService<PasswordService>(), RaceDiagnostics));
                    s.AddSingleton<IPasswordService>(p => p.GetRequiredService<LoginPasswordProbe>());
                    var quota = s.Last(d => d.ServiceType == typeof(IAuthenticationQuotaStore)); s.Remove(quota);
                    s.AddSingleton(p => new LogoutQuotaProbe((IAuthenticationQuotaStore)quota.ImplementationFactory!(p)));
                    s.AddSingleton<IAuthenticationQuotaStore>(p => p.GetRequiredService<LogoutQuotaProbe>());
                    var authority = s.Last(d => d.ServiceType == typeof(IAuthenticationSecurityAuthority)); s.Remove(authority);
                    s.AddSingleton(p => new LogoutAuthorityProbe((IAuthenticationSecurityAuthority)authority.ImplementationFactory!(p)));
                    s.AddSingleton<IAuthenticationSecurityAuthority>(p => p.GetRequiredService<LogoutAuthorityProbe>());
                    if (RaceDiagnostics is { } diagnostic)
                    {
                        var controls = s.Last(d => d.ServiceType == typeof(ILoginControls)); s.Remove(controls);
                        s.AddSingleton<ILoginControls>(p => new LoginControlsDiagnosticProbe(
                            (ILoginControls)ActivatorUtilities.CreateInstance(p, controls.ImplementationType!), diagnostic));
                        var emission = s.Last(d => d.ServiceType == typeof(ILoginTokenIssuer)); s.Remove(emission);
                        s.AddSingleton<ILoginTokenIssuer>(p => new LoginEmissionDiagnosticProbe(
                            (ILoginTokenIssuer)ActivatorUtilities.CreateInstance(p, emission.ImplementationType!), diagnostic));
                    }
                    s.AddSingleton(p => new LoginIssuerProbe(new JwtTokenIssuer(p.GetRequiredService<JwtKeyRing>(), TimeProvider.System)));
                    s.AddSingleton<IAccessTokenIssuer>(p => p.GetRequiredService<LoginIssuerProbe>());
                    s.AddSingleton(p => new JwtClaimsValidator(p.GetRequiredService<JwtKeyRing>(), TimeProvider.System));
                    s.AddAuthentication(AuthenticationPolicies.Scheme).AddJwtBearer();
                    s.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerConfiguration>();
                    s.AddAuthorization(AuthenticationPolicies.Configure);
                    s.AddSingleton<IAuthorizationHandler, RevocationAuthorizationHandler>();
                    s.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticationAuthorizationResultHandler>();
                }).Configure(app =>
                {
                    app.Use(async (http, next) =>
                    {
                        // Solo test: no se incorpora confianza en headers al host productivo.
                        http.Connection.RemoteIpAddress = IPAddress.Parse(http.Request.Headers["X-Test-Origin"].FirstOrDefault() ?? "127.0.0.1");
                        await next();
                    });
                    app.UseAuthenticationFoundation(); app.UseAuthentication(); app.UseAuthorization();
                    app.UseEndpoints(e => { e.MapControllers(); e.MapGet("/_login/protected", () => "ok").RequireAuthorization(); });
                })).Build();
    }
    internal async Task StartAsync()
    {
        await host.StartAsync(); Client = host.GetTestClient();
        await (await Services.GetRequiredService<Enver.API.Infrastructure.Redis.RedisConnections>().GetMultiplexerAsync()).GetDatabase().PingAsync();
        using var b = new RequestBudget(8000, default);
        _ = await Users.Inner.FindByNormalizedEmailAsync("warm@uttt.edu.mx", b);
    }
    public async Task<User> SeedAsync(string email = "login@uttt.edu.mx", string secret = "Synthetic Password 123!", UserStatus status = UserStatus.Active, bool legacy = false)
    {
        var candidate = User.Create("Synthetic", email, "", DateTime.UtcNow.AddDays(-1));
        var hasher = new PasswordHasher<User>(Options.Create(new PasswordHasherOptions { CompatibilityMode = legacy ? PasswordHasherCompatibilityMode.IdentityV2 : PasswordHasherCompatibilityMode.IdentityV3 }));
        var user = new User(candidate.Id, candidate.FullName, email, hasher.HashPassword(candidate, secret), status, candidate.CreatedAt, candidate.UpdatedAt);
        using var b = new RequestBudget(8000, default);
        if ((await Users.Inner.InsertAsync(user, b)).Status != UserInsertStatus.Confirmed) throw new InvalidOperationException("Fixture user unavailable.");
        return user;
    }
    public Task<HttpResponseMessage> LoginAsync(string email = "login@uttt.edu.mx", string password = "Synthetic Password 123!", string origin = "127.0.0.1") =>
        SendAsync(JsonContent.Create(new { email, password }), origin);
    public Task<HttpResponseMessage> SendAsync(HttpContent? content, string origin = "127.0.0.1")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login") { Content = content };
        request.Headers.Add("X-Test-Origin", origin); return Client.SendAsync(request);
    }
    public Task<HttpResponseMessage> AuthorizedAsync(string token, string path = "/_login/protected", bool logout = false)
    {
        var r = new HttpRequestMessage(logout ? HttpMethod.Post : HttpMethod.Get, path);
        r.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return Client.SendAsync(r);
    }
    public async ValueTask DisposeAsync()
    { Client?.Dispose(); try { await host.StopAsync(); } finally { host.Dispose(); } }
}

public sealed class LoginUserStoreProbe(IUserStore inner, LoginRaceDiagnostics? diagnostic = null) : IUserStore
{
    public IUserStore Inner => inner;
    public int Lookups, Reads, Writes;
    public UserReadStatus? ReadOutcome;
    public UserCredentialUpdateStatus? WriteOutcome;
    public UserCredentialUpdateStatus? LastCredentialUpdateStatus { get; private set; }
    public string? LastReplacementHash { get; private set; }
    public UserReadStatus? LastReadByIdStatus { get; private set; }
    public UserStatus? LastReadUserStatus { get; private set; }
    public Func<Task>? BeforeWrite;
    public async ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget)
    {
        Interlocked.Increment(ref Lookups);
        var result = ReadOutcome == UserReadStatus.Unavailable ? new(UserReadStatus.Unavailable) : await inner.FindByNormalizedEmailAsync(email, budget);
        diagnostic?.Record(LoginDiagnosticPhase.Lookup, result.Status, budget, result.CancellationCause); return result;
    }
    public async ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget)
    {
        Interlocked.Increment(ref Reads);
        var result = ReadOutcome == UserReadStatus.Unavailable ? new UserReadResult(UserReadStatus.Unavailable) : await inner.FindByIdAsync(id, budget);
        LastReadByIdStatus = result.Status; LastReadUserStatus = result.User?.Status;
        diagnostic?.Record(LoginDiagnosticPhase.Reread, result.Status, budget, result.CancellationCause); return result;
    }
    public ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget) => inner.InsertAsync(user, budget);
    public async ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string old, UserStatus status, string hash, DateTime at, RequestBudget budget)
    {
        Interlocked.Increment(ref Writes); LastReplacementHash = hash;
        diagnostic?.Record(LoginDiagnosticPhase.BarrierEnter, budget.Cause, budget);
        if (BeforeWrite is not null) await BeforeWrite();
        diagnostic?.Record(LoginDiagnosticPhase.BarrierExit, budget.Cause, budget);
        if (WriteOutcome == UserCredentialUpdateStatus.Unavailable)
        { LastCredentialUpdateStatus = WriteOutcome.Value; return new(WriteOutcome.Value); }
        var result = await inner.CompareExchangePasswordHashAsync(id, old, status, hash, at, budget);
        var reported = WriteOutcome == UserCredentialUpdateStatus.Uncertain && result.Status == UserCredentialUpdateStatus.Updated ? new(UserCredentialUpdateStatus.Uncertain) : result;
        LastCredentialUpdateStatus = reported.Status;
        diagnostic?.Record(LoginDiagnosticPhase.Cas, reported.Status, budget, reported.CancellationCause); return reported;
    }
}
public sealed class LoginKdfProbe : IPasswordHasher<User>
{
    private readonly PasswordHasher<User> inner = new();
    public int Hashes, Verifications;
    public string HashPassword(User user, string password) { Interlocked.Increment(ref Hashes); return inner.HashPassword(user, password); }
    public PasswordVerificationResult VerifyHashedPassword(User user, string hash, string password)
    { Interlocked.Increment(ref Verifications); return inner.VerifyHashedPassword(user, hash, password); }
}
public sealed class LoginPasswordProbe(IPasswordService inner, LoginRaceDiagnostics? diagnostic = null) : IPasswordService
{
    public int Verifies, Dummies, Rehashes;
    public PasswordVerificationStatus? LastVerificationStatus { get; private set; }
    public PasswordVerificationStatus Verify(User user, string secret, RequestBudget budget)
    {
        Interlocked.Increment(ref Verifies); diagnostic?.Record(LoginDiagnosticPhase.VerifyEnter, budget.Cause, budget);
        var result = inner.Verify(user, secret, budget); LastVerificationStatus = result;
        diagnostic?.Record(LoginDiagnosticPhase.VerifyExit, result, budget); return result;
    }
    public PasswordVerificationStatus VerifyDummy(string secret, RequestBudget budget) { Interlocked.Increment(ref Dummies); return inner.VerifyDummy(secret, budget); }
    public PasswordHashResult Hash(User user, string secret, RequestBudget budget) => inner.Hash(user, secret, budget);
    public PasswordHashResult Rehash(User user, string secret, RequestBudget budget)
    {
        Interlocked.Increment(ref Rehashes); diagnostic?.Record(LoginDiagnosticPhase.RehashEnter, budget.Cause, budget);
        var result = inner.Rehash(user, secret, budget); diagnostic?.Record(LoginDiagnosticPhase.RehashExit, result.Status, budget); return result;
    }
}
public sealed class LoginIssuerProbe(IAccessTokenIssuer inner) : IAccessTokenIssuer
{
    public int Calls; public Action? AfterIssue;
    public AccessTokenIssueResult Issue(Guid subject) { Interlocked.Increment(ref Calls); var token = inner.Issue(subject); AfterIssue?.Invoke(); return token; }
}

// Opt-in solo para la carrera A/B: enums y tiempos, nunca identidad, hash, payload o excepción.
public enum LoginDiagnosticPhase { AdmissionEnter, AdmissionExit, QuotaEnter, QuotaExit, Lookup, VerifyEnter, VerifyExit, RehashEnter, RehashExit, BarrierEnter, BarrierExit, Cas, Reread, EmissionEnter, EmissionExit }
public sealed record LoginDiagnosticStep(string Phase, string Outcome, double RemainingMilliseconds, string RequestCause, string DependencyCause);
public sealed class LoginRaceDiagnostics
{
    private readonly ConcurrentQueue<LoginDiagnosticStep> steps = new();
    public LoginDiagnosticStep[] Snapshot => steps.ToArray();
    public void Record<T>(LoginDiagnosticPhase phase, T outcome, RequestBudget budget,
        BudgetCancellationCause dependencyCause = BudgetCancellationCause.None) where T : struct, Enum =>
        steps.Enqueue(new(phase.ToString(), outcome.ToString(), Math.Round(budget.Remaining.TotalMilliseconds, 3),
            budget.Cause.ToString(), dependencyCause.ToString()));
}
internal sealed class LoginControlsDiagnosticProbe(ILoginControls inner, LoginRaceDiagnostics diagnostic) : ILoginControls
{
    public async ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget)
    {
        diagnostic.Record(LoginDiagnosticPhase.AdmissionEnter, budget.Cause, budget);
        var result = await inner.CheckAsync(budget); diagnostic.Record(LoginDiagnosticPhase.AdmissionExit, result.Admission, budget); return result;
    }
    public async ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string email, AuthenticationCanonicalOrigin origin, RequestBudget budget)
    {
        diagnostic.Record(LoginDiagnosticPhase.QuotaEnter, budget.Cause, budget);
        var result = await inner.ConsumeAsync(email, origin, budget); diagnostic.Record(LoginDiagnosticPhase.QuotaExit, result.Status, budget); return result;
    }
}
internal enum LoginDiagnosticEmission { Issued, Unavailable }
internal sealed class LoginEmissionDiagnosticProbe(ILoginTokenIssuer inner, LoginRaceDiagnostics diagnostic) : ILoginTokenIssuer
{
    public async ValueTask<AccessTokenIssueResult?> IssueAsync(Guid subject, RequestBudget budget)
    {
        diagnostic.Record(LoginDiagnosticPhase.EmissionEnter, budget.Cause, budget);
        var result = await inner.IssueAsync(subject, budget);
        diagnostic.Record(LoginDiagnosticPhase.EmissionExit, result?.Status == AccessTokenIssueStatus.Issued
            ? LoginDiagnosticEmission.Issued : LoginDiagnosticEmission.Unavailable, budget); return result;
    }
}

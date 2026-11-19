using System.Diagnostics;
using System.Text.RegularExpressions;
using Enver.API.Composition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

/// <summary>Pipeline real en memoria, sin host/API iniciado, sockets ni clientes de dependencias.</summary>
public sealed class LocalHealthTransportTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Testing")]
    public async Task Exact_http_get_health_returns_existing_liveness_response_with_policy_disabled(string environment)
    {
        using var fixture = new Pipeline(environment);
        var context = await fixture.Request("GET", "/health");
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Headers.Location.Count);
        context.Response.Body.Position = 0;
        Assert.Equal("Healthy", await new StreamReader(context.Response.Body).ReadToEndAsync());
        Assert.False(fixture.Services.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value.AllowLocalHttp);
        Assert.Empty(fixture.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
    }

    [Theory]
    [InlineData("POST", "/api/v1/auth/register")]
    [InlineData("POST", "/api/v1/auth/login")]
    [InlineData("POST", "/api/v1/auth/logout")]
    [InlineData("GET", "/arbitrary")]
    [InlineData("GET", "/api/health")]
    [InlineData("GET", "/health/")]
    [InlineData("GET", "/health/anything")]
    [InlineData("GET", "/healthz")]
    [InlineData("GET", "/HEALTH")]
    [InlineData("GET", "/Health")]
    [InlineData("GET", "/health%2Fanything")]
    [InlineData("POST", "/health")]
    [InlineData("HEAD", "/health")]
    [InlineData("PUT", "/health")]
    public async Task Other_http_requests_preserve_https_redirect_without_reaching_endpoint(string method, string path)
    {
        using var fixture = new Pipeline();
        var context = await fixture.Request(method, path);
        Assert.Equal(307, context.Response.StatusCode);
        Assert.StartsWith("https://unit.invalid/", context.Response.Headers.Location.ToString());
        Assert.False(fixture.FallbackReached);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Theory]
    [InlineData("GET", "/health", 200)]
    [InlineData("POST", "/api/v1/auth/register", 204)]
    [InlineData("POST", "/api/v1/auth/login", 204)]
    [InlineData("POST", "/api/v1/auth/logout", 204)]
    [InlineData("GET", "/arbitrary", 204)]
    public async Task Https_continues_unchanged_without_executing_any_business_endpoint(string method, string path, int status)
    {
        using var fixture = new Pipeline();
        var context = await fixture.Request(method, path, "https");
        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Headers.Location.Count);
    }

    [Fact]
    public async Task Path_base_is_not_a_bypass_and_query_does_not_broaden_the_path()
    {
        using var fixture = new Pipeline();
        var based = await fixture.Request("GET", "/health", pathBase: "/prefix");
        Assert.Equal(307, based.Response.StatusCode);
        var exact = await fixture.Request("GET", "/health", query: "?local=synthetic");
        Assert.Equal(200, exact.Response.StatusCode);
    }

    [Fact]
    public void Defaults_remain_false_health_has_no_dependencies_and_no_readiness_is_added()
    {
        using var fixture = new Pipeline();
        Assert.False(new AuthenticationTransportOptions().AllowLocalHttp);
        Assert.False(fixture.Services.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value.AllowLocalHttp);
        Assert.Empty(fixture.Services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
        var endpoints = fixture.Endpoints.DataSources.SelectMany(source => source.Endpoints).Cast<RouteEndpoint>().ToArray();
        Assert.Equal("/health", Assert.Single(endpoints).RoutePattern.RawText);
        var program = File.ReadAllText(Path.Combine(Root(), "Enver.API/Program.cs"));
        Assert.Single(Regex.Matches(program, "app.MapHealthChecks\\(\"/health\"\\)").Cast<Match>());
        Assert.Contains("builder.Services.AddHealthChecks();", program);
        Assert.Contains("app.MapHealthChecks(\"/health\").AllowAnonymous();", program);
        Assert.DoesNotContain("/ready", program, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AddCheck", program);
    }

    [Fact]
    public void Future_docker_binding_is_exactly_one_loopback_only_mapping_and_other_guards_remain()
    {
        var deploy = Path.Combine(Root(), "tools/Enver.RuntimeConfig.Materializer/deploy");
        var start = File.ReadAllText(Path.Combine(deploy, "start-api.sh"));
        var bindings = Regex.Matches(start, @"(?:^|\s)(?:--publish|-p)(?:\s+|=)([^\s\\]+)");
        Assert.Equal("127.0.0.1:8080:8080", Assert.Single(bindings.Cast<Match>()).Groups[1].Value);
        Assert.DoesNotContain("0.0.0.0", start);
        Assert.Contains("--network bridge", start);
        Assert.Contains("--user 1654:1654", start);
        Assert.Contains("--log-driver journald", start);
        Assert.Contains("dst=/run/enver/appsettings.Runtime.json,readonly", start);
        Assert.Contains("test -f /etc/enver/api-start.authorized", start);
        Assert.Contains("\"${ENVER_API_IMAGE}\"", start);
        Assert.Contains("@sha256:", start);
        Assert.DoesNotContain("AllowLocalHttp", start);
        var unit = File.ReadAllText(Path.Combine(deploy, "enver-api.service"));
        Assert.Contains("EnvironmentFile=/etc/enver/api-image.env", unit);
        Assert.Contains("ConditionPathExists=/etc/enver/api-start.authorized", unit);
        Assert.Contains("StandardOutput=journal", unit);
        Assert.Contains("StandardError=journal", unit);
        Assert.Contains("Restart=no", unit);
    }

    private static string Root()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Enver.API/Enver.API.csproj"))) return directory.FullName;
        throw new InvalidOperationException("Repository root not found.");
    }

    private sealed class Pipeline : IDisposable
    {
        public ServiceProvider Services { get; }
        public IEndpointRouteBuilder Endpoints { get; }
        public bool FallbackReached { get; private set; }
        private readonly RequestDelegate application;

        public Pipeline(string environment = "Development")
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton<IHostEnvironment>(new LocalEnvironment { EnvironmentName = environment });
            var diagnostic = new DiagnosticListener("LocalHealthContract");
            services.AddSingleton(diagnostic);
            services.AddSingleton<DiagnosticSource>(diagnostic);
            services.AddRouting();
            services.AddHealthChecks(); // Mismo liveness sin checks; ningún cliente externo registrado.
            services.AddOptions<AuthenticationTransportOptions>();
            services.AddHttpsRedirection(options => options.HttpsPort = 443);
            services.AddHsts(_ => { });
            Services = services.BuildServiceProvider();
            var app = new ApplicationBuilder(Services);
            app.UseAuthenticationTransport();
            app.UseRouting();
            IEndpointRouteBuilder? endpoints = null;
            app.UseEndpoints(builder => { endpoints = builder; builder.MapHealthChecks("/health").AllowAnonymous(); });
            Endpoints = endpoints!;
            app.Run(context => { FallbackReached = true; context.Response.StatusCode = 204; return Task.CompletedTask; });
            application = app.Build();
        }

        public async Task<DefaultHttpContext> Request(string method, string path, string scheme = "http", string pathBase = "", string query = "")
        {
            FallbackReached = false;
            var context = new DefaultHttpContext { RequestServices = Services };
            context.Request.Method = method; context.Request.Scheme = scheme;
            context.Request.Host = new HostString("unit.invalid");
            context.Request.Path = path; context.Request.PathBase = pathBase; context.Request.QueryString = new QueryString(query);
            context.Response.Body = new MemoryStream();
            await application(context);
            return context;
        }

        public void Dispose() => Services.Dispose();
    }

    private sealed class LocalEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "LocalHealthContract";
        public string ContentRootPath { get; set; } = Root();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

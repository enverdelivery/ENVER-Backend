using System.Text.Json.Nodes;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Controllers;
using Enver.API.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class RuntimeStartupContractTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private const string Canary = "LOCAL-STARTUP-CANARY-NOT-A-SECRET";
    private static string Root => FindRoot();

    [Fact]
    public void Absolute_runtime_is_required_final_provider_with_no_reload_and_no_inherited_key_ring_tail()
    {
        Assert.Equal("/run/enver/appsettings.Runtime.json", RuntimeConfiguration.Path);
        using var file = new RuntimeFixture();
        var builder = Defaults().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Redis:ConnectionString"] = Canary, ["MongoDb:ConnectionString"] = Canary,
            ["RateLimiting:PseudonymizationKeyBase64"] = Canary, ["Jwt:ActiveKid"] = Canary,
            ["Jwt:Keys:1:Kid"] = Canary, ["Jwt:Keys:1:KeyBase64"] = Canary
        });
        RuntimeConfiguration.AddRuntimeFile(builder, file.Path);
        var source = Assert.IsAssignableFrom<JsonConfigurationSource>(builder.Sources.Last());
        Assert.False(source.Optional); Assert.False(source.ReloadOnChange);
        using var config = (ConfigurationRoot)builder.Build();
        Assert.Equal("local-runtime", config["Jwt:ActiveKid"]);
        Assert.Null(config["Jwt:Keys:1:KeyBase64"]);
        Assert.Single(config.GetSection("Jwt:Keys").GetChildren());
        Assert.DoesNotContain(Canary, config["Redis:ConnectionString"]!);
        using var services = Options(config);
        services.GetRequiredService<IStartupValidator>().Validate();
        Assert.Single(services.GetRequiredService<IOptions<JwtOptions>>().Value.Keys);
        var rate = services.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        Assert.Equal(5000, rate.MaxActivePartitions);
        Assert.Equal("73041e6288899e1528f3b8f25c39e888", services.GetRequiredService<IOptions<AuthenticationDependencyOptions>>().Value.QuotaGeneration);
    }

    [Fact]
    public void Real_default_development_providers_precede_runtime_without_starting_host()
    {
        using var file = new RuntimeFixture();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(RuntimeStartupContractTests).Assembly.GetName().Name,
            EnvironmentName = "Development", ContentRootPath = System.IO.Path.Combine(Root, "Enver.API"),
            Args = ["--Jwt:ActiveKid=" + Canary]
        });
        RuntimeConfiguration.AddRuntimeFile(builder.Configuration, file.Path);
        var sources = builder.Configuration.Sources;
        var jsonPaths = sources.OfType<JsonConfigurationSource>().Select(source => System.IO.Path.GetFileName(source.Path)).ToArray();
        Assert.Equal(new[] { "appsettings.json", "appsettings.Development.json", "Enver.API.UnitTests.settings.json",
            "Enver.API.UnitTests.settings.Development.json", "appsettings.Runtime.json" }, jsonPaths);
        foreach (var source in sources)
        {
            var suffix = source switch
            {
                JsonConfigurationSource json => System.IO.Path.GetFileName(json.Path),
                Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource env => env.Prefix ?? "unprefixed",
                _ => "no-values"
            };
            output.WriteLine($"provider={source.GetType().Name} source={suffix}");
        }
        Assert.IsAssignableFrom<JsonConfigurationSource>(sources.Last());
        Assert.Equal("local-runtime", builder.Configuration["Jwt:ActiveKid"]);
        ((IDisposable)builder.Configuration).Dispose();
    }

    [Fact]
    public async Task Missing_and_malformed_files_fail_at_process_boundary_without_exception_payload()
    {
        using var file = new RuntimeFixture();
        foreach (var malformed in new[] { false, true })
        {
            if (malformed) File.WriteAllText(file.Path, "{\"" + Canary + "\": invalid");
            else File.Delete(file.Path);
            using var output = new StringWriter();
            var result = await SafeStartup.Run(trace =>
            {
                trace.Phase = StartupPhase.RuntimeConfiguration;
                RuntimeConfiguration.AddRuntimeFile(new ConfigurationManager(), file.Path);
                return Task.CompletedTask;
            }, output);
            Assert.Equal(1, result);
            Assert.Equal("event=startup status=failed phase=runtime-config" + Environment.NewLine, output.ToString());
        }
    }

    [Theory]
    [InlineData("Redis")]
    [InlineData("MongoDb")]
    [InlineData("Jwt")]
    [InlineData("RateLimiting")]
    [InlineData("AuthenticationSecurityAuthority")]
    [InlineData("AuthenticationComposition")]
    public void Missing_runtime_section_cannot_fall_back_to_complete_earlier_json(string section)
    {
        using var file = new RuntimeFixture();
        var builder = Defaults().AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file.Document.ToJsonString())));
        file.Document.Remove(section); file.Save();
        RuntimeConfiguration.AddRuntimeFile(builder, file.Path);
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Theory]
    [InlineData("Redis", "ConnectionString")]
    [InlineData("MongoDb", "ConnectionString")]
    [InlineData("RateLimiting", "PseudonymizationKeyBase64")]
    [InlineData("Jwt", "ActiveKid")]
    [InlineData("Jwt", "Keys")]
    public void Missing_sensitive_runtime_field_never_uses_environment_or_defaults(string section, string key)
    {
        using var file = new RuntimeFixture();
        var builder = Defaults().AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file.Document.ToJsonString())))
            .AddEnvironmentVariables("ENVER_LOCAL_CONTRACT_TEST_");
        ((JsonObject)file.Document[section]!).Remove(key); file.Save();
        RuntimeConfiguration.AddRuntimeFile(builder, file.Path);
        Assert.ThrowsAny<Exception>(() => builder.Build());
    }

    [Theory]
    [InlineData("jwt-active")]
    [InlineData("jwt-material")]
    [InlineData("jwt-empty")]
    [InlineData("jwt-partial")]
    [InlineData("hmac")]
    [InlineData("capacity")]
    [InlineData("quota")]
    [InlineData("binding")]
    public async Task Invalid_runtime_options_fail_before_any_dependency_resolution_and_emit_only_safe_phase(string scenario)
    {
        using var file = new RuntimeFixture();
        switch (scenario)
        {
            case "jwt-active": file.Document["Jwt"]!["ActiveKid"] = Canary; break;
            case "jwt-material": file.Document["Jwt"]!["Keys"]![0]!["KeyBase64"] = Canary; break;
            case "jwt-empty": file.Document["Jwt"]!["Keys"] = new JsonArray(); break;
            case "jwt-partial": ((JsonObject)file.Document["Jwt"]!["Keys"]![0]!).Remove("KeyBase64"); break;
            case "hmac": file.Document["RateLimiting"]!["PseudonymizationKeyBase64"] = Canary; break;
            case "capacity": file.Document["RateLimiting"]!["MaxActivePartitions"] = 0; break;
            case "quota": file.Document["RateLimiting"]!["LoginEmail"] = new JsonObject { ["PermitLimit"] = 0, ["WindowSeconds"] = 60 }; break;
            case "binding": file.Document["RateLimiting"]!["MaxActivePartitions"] = Canary; break;
        }
        file.Save();
        using var output = new StringWriter();
        var result = await SafeStartup.Run(trace =>
        {
            trace.Phase = StartupPhase.RuntimeConfiguration;
            using var config = (ConfigurationRoot)RuntimeConfiguration.AddRuntimeFile(Defaults(), file.Path).Build();
            trace.Phase = StartupPhase.HostBuild;
            using var services = Options(config);
            services.GetRequiredService<IStartupValidator>().Validate();
            throw new Xunit.Sdk.XunitException("Invalid configuration unexpectedly passed.");
        }, output);
        Assert.Equal(1, result);
        Assert.DoesNotContain(Canary, output.ToString());
        Assert.DoesNotContain("Exception", output.ToString());
        Assert.StartsWith("event=startup status=failed phase=", output.ToString());
        // También comprobar el rechazo directamente: no aceptar el throw de la aserción anterior como prueba.
        Assert.ThrowsAny<Exception>(() =>
        {
            using var config = (ConfigurationRoot)RuntimeConfiguration.AddRuntimeFile(Defaults(), file.Path).Build();
            using var services = Options(config);
            services.GetRequiredService<IStartupValidator>().Validate();
        });
    }

    [Theory]
    [InlineData("Microsoft.Extensions.Hosting.Internal.Host", "hosting")]
    [InlineData("Microsoft.Hosting.Lifetime", "hosting-lifecycle")]
    [InlineData("MongoDB.Driver", "mongodb")]
    [InlineData("StackExchange.Redis", "redis")]
    [InlineData("Amazon.Runtime", "aws")]
    [InlineData(Canary, "component")]
    public void Logger_never_formats_exception_state_scopes_category_or_event_name(string category, string expected)
    {
        using var output = new StringWriter();
        using var provider = new SafeConsoleLoggerProvider(output);
        var logger = provider.CreateLogger(category);
        using var scope = logger.BeginScope(Canary);
        logger.Log(LogLevel.Error, new EventId(999, Canary), new PoisonState(), new Exception(Canary),
            (_, _) => throw new InvalidOperationException("Formatter must never execute."));
        Assert.Equal($"event=diagnostic category={expected} severity=error outcome=none{Environment.NewLine}", output.ToString());
    }

    [Fact]
    public void Templates_expose_safe_output_and_only_allowlisted_nonsecret_environment_and_no_runtime_copy()
    {
        var deploy = System.IO.Path.Combine(Root, "tools/Enver.RuntimeConfig.Materializer/deploy");
        var start = File.ReadAllText(System.IO.Path.Combine(deploy, "start-api.sh"));
        var unit = File.ReadAllText(System.IO.Path.Combine(deploy, "enver-api.service"));
        var docker = File.ReadAllText(System.IO.Path.Combine(Root, "Enver.API/Dockerfile"));
        Assert.Contains("--log-driver journald", start); Assert.DoesNotContain("--log-driver none", start);
        Assert.Contains("--platform linux/amd64", start); Assert.Contains("--user 1654:1654", start);
        Assert.Contains("dst=/run/enver/appsettings.Runtime.json,readonly", start);
        Assert.Contains("StandardOutput=journal", unit); Assert.Contains("StandardError=journal", unit);
        Assert.Contains("Restart=no", unit); Assert.Contains("RuntimeDirectoryPreserve=yes", unit);
        Assert.DoesNotContain("rm -f /run/enver/appsettings.Runtime.json", unit);
        Assert.Contains("ExecStartPre=", unit); Assert.DoesNotContain("ExecStartPre=-", unit);
        var envNames = System.Text.RegularExpressions.Regex.Matches(start, @"--env ([A-Za-z0-9_]+)=").Select(match => match.Groups[1].Value).ToArray();
        Assert.Equal(new[] { "ASPNETCORE_ENVIRONMENT", "HOME", "AWS_CONFIG_FILE", "AWS_SHARED_CREDENTIALS_FILE", "AWS_EC2_METADATA_DISABLED",
            "AWS_EC2_METADATA_V1_DISABLED", "AWS_REGION", "DOTNET_EnableDiagnostics" }, envNames);
        Assert.Contains("USER 1654:1654", docker); Assert.DoesNotContain("appsettings.Runtime.json", docker);
        Assert.Contains("**/appsettings.Runtime.json", File.ReadAllText(System.IO.Path.Combine(Root, ".dockerignore")));
        Assert.Contains("**/appsettings.Runtime.json", File.ReadAllText(System.IO.Path.Combine(Root, "Enver.API/Enver.API.csproj")));
        var program = File.ReadAllText(System.IO.Path.Combine(Root, "Enver.API/Program.cs"));
        Assert.Contains("builder.Logging.ClearProviders()", program);
        Assert.True(program.IndexOf("IStartupValidator", StringComparison.Ordinal) < program.IndexOf("app.RunAsync", StringComparison.Ordinal));
        Assert.Contains("app.MapHealthChecks(\"/health\").AllowAnonymous()", program);
        Assert.DoesNotContain("MapHealthChecks(\"/ready", program, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Register", "register")]
    [InlineData("Login", "login")]
    [InlineData("Logout", "logout")]
    public void Authentication_routes_are_unchanged(string method, string suffix)
    {
        var type = typeof(AuthenticationController);
        Assert.Equal("api/v1/auth", type.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>().Single().Template);
        Assert.Equal(suffix, type.GetMethod(method)!.GetCustomAttributes(typeof(HttpPostAttribute), true).Cast<HttpPostAttribute>().Single().Template);
    }

    [Theory]
    [InlineData("Application started. Press Ctrl+C to shut down.", "started")]
    [InlineData("Application is shutting down...", "stopping")]
    [InlineData("Now listening on: {address}", "listening")]
    public void Lifecycle_is_observable_without_echoing_addresses_or_framework_state(string format, string expected)
    {
        using var captured = new StringWriter();
        using var provider = new SafeConsoleLoggerProvider(captured);
        provider.CreateLogger("Microsoft.Hosting.Lifetime").Log(LogLevel.Information, new EventId(0, Canary),
            new Dictionary<string, object?> { ["{OriginalFormat}"] = format, ["address"] = Canary }, new Exception(Canary),
            (_, _) => throw new InvalidOperationException("Do not format."));
        Assert.Equal($"event=diagnostic category=hosting-lifecycle severity=info outcome={expected}{Environment.NewLine}", captured.ToString());
    }

    [Fact]
    public void Runtime_overrides_actual_prefixed_environment_without_requiring_any_secret_env()
    {
        using var file = new RuntimeFixture();
        var prefix = "ENVER_SYNTHETIC_" + Guid.NewGuid().ToString("N") + "_";
        var values = new Dictionary<string, string>
        {
            ["Redis__ConnectionString"] = Canary, ["MongoDb__ConnectionString"] = Canary,
            ["Jwt__ActiveKid"] = Canary, ["Jwt__Keys__1__Kid"] = Canary,
            ["Jwt__Keys__1__KeyBase64"] = Canary, ["RateLimiting__PseudonymizationKeyBase64"] = Canary
        };
        try
        {
            foreach (var (key, value) in values) Environment.SetEnvironmentVariable(prefix + key, value);
            var builder = Defaults().AddEnvironmentVariables(prefix);
            using var config = (ConfigurationRoot)RuntimeConfiguration.AddRuntimeFile(builder, file.Path).Build();
            using var services = Options(config);
            services.GetRequiredService<IStartupValidator>().Validate();
            Assert.Equal("local-runtime", config["Jwt:ActiveKid"]);
            Assert.Single(services.GetRequiredService<IOptions<JwtOptions>>().Value.Keys);
            file.Document["Jwt"]!["ActiveKid"] = "changed-after-load"; file.Save();
            Assert.Equal("local-runtime", config["Jwt:ActiveKid"]); // reloadOnChange=false
        }
        finally { foreach (var key in values.Keys) Environment.SetEnvironmentVariable(prefix + key, null); }
    }

    [Fact]
    public void Cloudfront_secret_can_be_injected_from_environment_and_explicit_runtime_value_takes_precedence()
    {
        using var file = new RuntimeFixture();
        var prefix = "ENVER_SYNTHETIC_" + Guid.NewGuid().ToString("N") + "_";
        var environmentSecret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var runtimeSecret = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var values = new Dictionary<string, string>
        {
            ["AuthenticationTransport__CloudFront__Enabled"] = "true",
            ["AuthenticationTransport__CloudFront__OriginSecret"] = environmentSecret,
            ["AuthenticationTransport__KnownProxies__0"] = "192.0.2.10"
        };
        try
        {
            foreach (var (key, value) in values) Environment.SetEnvironmentVariable(prefix + key, value);
            using var fromEnvironment = (ConfigurationRoot)RuntimeConfiguration.AddRuntimeFile(
                Defaults().AddEnvironmentVariables(prefix), file.Path).Build();
            using var environmentServices = Options(fromEnvironment);
            var environmentPolicy = environmentServices.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value;
            Assert.True(environmentPolicy.CloudFront.Enabled);
            Assert.True(environmentPolicy.CloudFront.OriginSecret == environmentSecret);
            Assert.False(environmentPolicy.AllowLocalHttp);

            file.Document["AuthenticationTransport"] = new JsonObject
            {
                ["CloudFront"] = new JsonObject { ["Enabled"] = true, ["OriginSecret"] = runtimeSecret }
            };
            file.Save();
            using var fromRuntime = (ConfigurationRoot)RuntimeConfiguration.AddRuntimeFile(
                Defaults().AddEnvironmentVariables(prefix), file.Path).Build();
            using var runtimeServices = Options(fromRuntime);
            var runtimePolicy = runtimeServices.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value;
            Assert.True(runtimePolicy.CloudFront.OriginSecret == runtimeSecret);
            // No reload implícito ni cambio de la instancia anterior ya capturada.
            Assert.True(environmentPolicy.CloudFront.OriginSecret == environmentSecret);
        }
        finally { foreach (var key in values.Keys) Environment.SetEnvironmentVariable(prefix + key, null); }
    }

    private sealed class PoisonState { public override string ToString() => throw new InvalidOperationException(Canary); }
    private static IConfigurationBuilder Defaults() => new ConfigurationBuilder()
        .AddJsonFile(System.IO.Path.Combine(Root, "Enver.API/appsettings.json"), false, false)
        .AddJsonFile(System.IO.Path.Combine(Root, "Enver.API/appsettings.Development.json"), false, false);
    private static ServiceProvider Options(IConfiguration config)
    {
        var services = new ServiceCollection(); var env = new LocalEnvironment();
        services.AddSingleton<IHostEnvironment>(env);
        services.AddAuthenticationFoundation(config, env);
        services.AddAuthenticationDependencies(config);
        // No Host.StartAsync ni resolución de clientes/hosted services: solo ValidateOnStart real.
        return services.BuildServiceProvider();
    }
    private sealed class LocalEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "LocalContractTests";
        public string ContentRootPath { get; set; } = Root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    private static string FindRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(System.IO.Path.Combine(directory.FullName, "Enver.API/Enver.API.csproj"))) return directory.FullName;
        throw new InvalidOperationException("Repository root not found.");
    }
    private sealed class RuntimeFixture : IDisposable
    {
        private readonly string directory;
        public string Path { get; }
        public JsonObject Document { get; } = JsonNode.Parse("""
        {"Redis":{"ConnectionString":"localhost:1,user=local,password=LOCAL-ONLY","NamespacePrefix":"enver:development"},
        "MongoDb":{"ConnectionString":"mongodb://local:LOCAL-ONLY@localhost:1/enver_development","DatabaseName":"enver_development"},
        "Jwt":{"ActiveKid":"local-runtime","Keys":[{"Kid":"local-runtime","KeyBase64":"AgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgI="}]},
        "RateLimiting":{"MaxActivePartitions":5000,"PseudonymizationKeyBase64":"AQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQE="},
        "AuthenticationSecurityAuthority":{"TableName":"enver-dev-authentication-authority","OperationTimeoutMilliseconds":2000},
        "AuthenticationComposition":{"AuthorityRegion":"us-east-1","QuotaConfigurationVersion":"quota-development-academic-initial-v1",
        "QuotaIdentityReference":"hmac-development-fecf274e-v1","QuotaGeneration":"73041e6288899e1528f3b8f25c39e888"}}
        """)!.AsObject();
        public RuntimeFixture()
        {
            var temp = OperatingSystem.IsWindows() ? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "opencode") : System.IO.Path.GetTempPath();
            directory = System.IO.Path.Combine(temp, "enver-start-contract-fixture-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); Path = System.IO.Path.Combine(directory, "appsettings.Runtime.json"); Save();
        }
        public void Save() => File.WriteAllText(Path, Document.ToJsonString());
        public void Dispose() { File.Delete(Path); Directory.Delete(directory); }
    }
}

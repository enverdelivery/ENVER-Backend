using System.Text.Json.Nodes;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.RuntimeConfig.Materializer;

internal static class BindingDiagnostic
{
    internal static IConfigurationBuilder Defaults() => new ConfigurationBuilder()
        .AddJsonFile(System.IO.Path.Combine(AppContext.BaseDirectory, "defaults", "appsettings.json"), false, false)
        .AddJsonFile(System.IO.Path.Combine(AppContext.BaseDirectory, "defaults", "appsettings.Development.json"), false, false);

    internal static void InMemory(JsonObject runtime, DeliveryTrace? trace = null)
    {
        trace?.EnterValidation(DeliveryPhase.ConfigParseValidation, ValidationTarget.Memory);
        using var stream = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(runtime));
        var config = Defaults().AddJsonStream(stream).Build();
        try { Validate(config, trace); }
        finally { (config as IDisposable)?.Dispose(); }
    }

    internal static void FileBinding(string path, ValidationTarget target, DeliveryTrace? trace = null)
    {
        if (target is not (ValidationTarget.TemporaryFile or ValidationTarget.FinalFile)) throw new DeliveryFailure("usage-invalid");
        trace?.EnterValidation(target == ValidationTarget.TemporaryFile
            ? DeliveryPhase.TemporaryConfigParseValidation : DeliveryPhase.FinalConfigParseValidation, target);
        // The candidate is intentionally dot-prefixed. AddJsonFile's default
        // PhysicalFileProvider excludes it even when File.Exists(path) is true.
        // Open exactly this required candidate, then parse/bind in memory. No fallback.
        if (target == ValidationTarget.TemporaryFile)
        {
            using var stream = File.OpenRead(path);
            var candidate = Defaults().AddJsonStream(stream).Build();
            try { Validate(candidate, trace, target); }
            finally { (candidate as IDisposable)?.Dispose(); }
            return;
        }
        var config = RuntimeConfiguration.AddRequiredFile(Defaults(), path).Build();
        try { Validate(config, trace, target); }
        finally { (config as IDisposable)?.Dispose(); }
    }

    internal static void Validate(IConfiguration configuration, DeliveryTrace? trace = null, ValidationTarget target = ValidationTarget.Memory)
    {
        trace?.EnterValidation(target switch
        {
            ValidationTarget.TemporaryFile => DeliveryPhase.TemporaryConfigBindingValidation,
            ValidationTarget.FinalFile => DeliveryPhase.FinalConfigBindingValidation,
            _ => DeliveryPhase.ConfigBindingValidation
        }, target);
        try
        {
            // Registration only. No WebApplication/Host, StartAsync, hosted service or client resolution.
            var services = new ServiceCollection();
            services.AddSingleton<IHostEnvironment>(new DiagnosticEnvironment());
            services.AddAuthenticationFoundation(configuration, new DiagnosticEnvironment());
            services.AddAuthenticationDependencies(configuration);
            using var provider = services.BuildServiceProvider();
            T Options<T>() where T : class => provider.GetRequiredService<IOptions<T>>().Value;
            var redis = Options<RedisOptions>();
            var mongo = Options<MongoDbOptions>();
            _ = Options<JwtOptions>();
            _ = Options<AuthenticationOptions>();
            var rate = Options<RateLimitingOptions>();
            var authority = Options<AuthenticationSecurityAuthorityOptions>();
            var composition = Options<AuthenticationDependencyOptions>();
            _ = Options<AuthenticationTransportOptions>();
            _ = RedisConnections.CreateConfigurationOptions(redis); // Parse only; never Connect.
            _ = composition.Capture("Development", rate);
            if (redis.NamespacePrefix != "enver:development" || mongo.DatabaseName != "enver_development" ||
                authority.TableName != "enver-dev-authentication-authority" || authority.OperationTimeoutMilliseconds != 2000 ||
                composition.AuthorityRegion != "us-east-1" || composition.QuotaConfigurationVersion != "quota-development-academic-initial-v1" ||
                composition.QuotaIdentityReference != "hmac-development-fecf274e-v1" || composition.QuotaGeneration != RuntimeMapping.Generation ||
                rate.MaxActivePartitions != 5000) throw new DeliveryFailure("contract-invalid");
        }
        catch (Exception error) { throw DeliveryFailure.Wrap(error); }
    }

    private sealed class DiagnosticEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Enver.RuntimeConfig.Materializer";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

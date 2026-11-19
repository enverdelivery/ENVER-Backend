using Microsoft.Extensions.Logging;

namespace Enver.API.Observability;

public enum StartupPhase { Builder, RuntimeConfiguration, Services, HostBuild, HostRun }

/// <summary>Frontera de proceso: jamás propaga Message/ToString/stack/config al stderr del runtime.</summary>
public sealed class SafeStartup
{
    public StartupPhase Phase { get; set; }

    public static async Task<int> Run(Func<SafeStartup, Task> application, TextWriter output)
    {
        var trace = new SafeStartup();
        try
        {
            await application(trace).ConfigureAwait(false);
            return 0;
        }
        catch
        {
            // Solo enum controlado; no serializar ni siquiera nombres de tipos arbitrarios.
            var phase = trace.Phase switch
            {
                StartupPhase.Builder => "builder", StartupPhase.RuntimeConfiguration => "runtime-config",
                StartupPhase.Services => "services", StartupPhase.HostBuild => "host-build",
                StartupPhase.HostRun => "host-run", _ => "unknown"
            };
            output.WriteLine($"event=startup status=failed phase={phase}");
            return 1;
        }
    }
}

/// <summary>
/// Único sink del proceso API. No invoca formatters ni serializa state, scopes o exceptions.
/// Proyecta eventos del framework/SDK a categorías finitas. stdout adjunto → journald.
/// </summary>
public sealed class SafeConsoleLoggerProvider(TextWriter output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new SafeLogger(categoryName, output);
    public void Dispose() { }

    private sealed class SafeLogger(string category, TextWriter output) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information && logLevel < LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var safeCategory = category switch
            {
                "Microsoft.Hosting.Lifetime" => "hosting-lifecycle",
                "Enver.API.Observability.AuthenticationEvents" => "authentication",
                "Enver.API.Composition.AuthenticationInitializationService" => "mongodb-initialization",
                _ when category.StartsWith("MongoDB.", StringComparison.Ordinal) => "mongodb",
                _ when category.StartsWith("StackExchange.Redis", StringComparison.Ordinal) => "redis",
                _ when category.StartsWith("Amazon.", StringComparison.Ordinal) => "aws",
                _ when category.StartsWith("Microsoft.Extensions.Hosting.", StringComparison.Ordinal) => "hosting",
                _ => "component"
            };
            var severity = logLevel switch
            { LogLevel.Information => "info", LogLevel.Warning => "warning", LogLevel.Error => "error", _ => "critical" };
            // Conservar resultados de eventos auth ya allowlisted sin aceptar labels arbitrarios.
            var outcome = "none";
            if (safeCategory == "hosting-lifecycle" && state is IEnumerable<KeyValuePair<string, object?>> lifecycle)
                outcome = lifecycle.FirstOrDefault(pair => pair.Key == "{OriginalFormat}").Value switch
                {
                    "Now listening on: {address}" => "listening",
                    "Application started. Press Ctrl+C to shut down." => "started",
                    "Application is shutting down..." => "stopping",
                    "Hosting environment: {EnvName}" => "environment-set",
                    "Content root path: {ContentRoot}" => "content-root-set", _ => "none"
                };
            if (safeCategory == "mongodb-initialization" && state is IEnumerable<KeyValuePair<string, object?>> mongoFields)
                outcome = mongoFields.FirstOrDefault(pair => pair.Key == "Status").Value switch
                {
                    Infrastructure.MongoDB.MongoUsersInitializationStatus.Ready => "ready",
                    Infrastructure.MongoDB.MongoUsersInitializationStatus.Incompatible => "incompatible",
                    Infrastructure.MongoDB.MongoUsersInitializationStatus.Unavailable => "unavailable", _ => "unknown"
                };
            if (safeCategory == "authentication" && state is IEnumerable<KeyValuePair<string, object?>> fields)
            {
                var value = fields.FirstOrDefault(pair => pair.Key == "Outcome").Value;
                outcome = value switch
                {
                    "success" => "success", "rejected" => "rejected", "rate_limited" => "rate_limited",
                    "dependency_unavailable" => "dependency_unavailable", "uncertain" => "uncertain",
                    "confirmed" => "confirmed", "rehash_updated" => "rehash_updated", "rehash_race_lost" => "rehash_race_lost",
                    Features.Authentication.Application.Ports.AuthenticationEventOutcome e when Enum.IsDefined(e) => e.ToString(),
                    _ => "none"
                };
            }
            output.WriteLine($"event=diagnostic category={safeCategory} severity={severity} outcome={outcome}");
        }
    }
}

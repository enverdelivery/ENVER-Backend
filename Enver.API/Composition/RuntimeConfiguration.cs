using Microsoft.Extensions.Configuration.Json;

namespace Enver.API.Composition;

public static class RuntimeConfiguration
{
    public const string Path = "/run/enver/appsettings.Runtime.json";

    public static IConfigurationBuilder AddEnverRuntimeConfiguration(this IConfigurationBuilder builder)
    {
        // Fuera del despliegue Linux se mantienen los flujos locales ya existentes.
        // En Docker el archivo nunca es opcional y no se observa con FileSystemWatcher.
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" || File.Exists(Path))
            return AddRuntimeFile(builder, Path);
        return builder;
    }

    public static IConfigurationBuilder AddRequiredFile(IConfigurationBuilder builder, string path)
    {
        try { return builder.AddJsonFile(path, optional: false, reloadOnChange: false); }
        catch { throw new InvalidOperationException("Runtime configuration unavailable."); }
    }

    // Punto de prueba con archivo sintético. Producción solo invoca la ruta absoluta constante.
    public static IConfigurationBuilder AddRuntimeFile(IConfigurationBuilder builder, string path)
    {
        try
        {
            if (!System.IO.Path.IsPathFullyQualified(path)) throw new InvalidOperationException();
            var source = new RuntimeJsonSource { Path = path, Optional = false, ReloadOnChange = false };
            source.ResolveFileProvider();
            return builder.Add(source);
        }
        catch { throw new InvalidOperationException("Runtime configuration unavailable."); }
    }

    private sealed class RuntimeJsonSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new RuntimeJsonProvider(this);
        }
    }

    private sealed class RuntimeJsonProvider(JsonConfigurationSource source) : JsonConfigurationProvider(source)
    {
        private static readonly string[] SensitiveRoots =
            ["Redis:ConnectionString", "MongoDb:ConnectionString", "RateLimiting:PseudonymizationKeyBase64", "Jwt:ActiveKid", "Jwt:Keys"];

        private static bool Sensitive(string? key) => key is not null && SensitiveRoots.Any(root =>
            key.Equals(root, StringComparison.OrdinalIgnoreCase) || key.StartsWith(root + ":", StringComparison.OrdinalIgnoreCase));

        public override void Load(Stream stream)
        {
            try
            {
                base.Load(stream);
                // Son los campos que entrega el materializador, NO defaults opcionales.
                string[] required = ["Redis:ConnectionString", "Redis:NamespacePrefix", "MongoDb:ConnectionString", "MongoDb:DatabaseName",
                    "RateLimiting:PseudonymizationKeyBase64", "RateLimiting:MaxActivePartitions", "Jwt:ActiveKid",
                    "AuthenticationSecurityAuthority:TableName", "AuthenticationSecurityAuthority:OperationTimeoutMilliseconds",
                    "AuthenticationComposition:AuthorityRegion", "AuthenticationComposition:QuotaConfigurationVersion",
                    "AuthenticationComposition:QuotaIdentityReference", "AuthenticationComposition:QuotaGeneration"];
                if (required.Any(key => !Data.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)) ||
                    !Data.Keys.Any(key => key.StartsWith("Jwt:Keys:", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException();
                // Cada elemento necesita ambos campos EN ESTE archivo, nunca en env/defaults.
                var children = base.GetChildKeys([], "Jwt:Keys").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                if (children.Length == 0 || children.Where((key, index) => key != index.ToString(System.Globalization.CultureInfo.InvariantCulture)).Any() ||
                    children.Any(key => !Data.TryGetValue($"Jwt:Keys:{key}:Kid", out var kid) || string.IsNullOrWhiteSpace(kid) ||
                        !Data.TryGetValue($"Jwt:Keys:{key}:KeyBase64", out var material) || string.IsNullOrWhiteSpace(material)))
                    throw new InvalidOperationException();
            }
            catch { throw new InvalidOperationException("Runtime configuration invalid."); }
        }

        public override bool TryGet(string key, out string? value)
        {
            if (base.TryGet(key, out value)) return true;
            // IConfigurationRoot no debe retroceder a fuentes anteriores para secretos.
            return Sensitive(key);
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath) =>
            base.GetChildKeys(Sensitive(parentPath) ? [] : earlierKeys, parentPath);
    }
}

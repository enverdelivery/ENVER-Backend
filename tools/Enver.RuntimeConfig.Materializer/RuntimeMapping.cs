using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using StackExchange.Redis;

namespace Enver.RuntimeConfig.Materializer;

internal static class RuntimeMapping
{
    internal const string RedisHost = "master.enver-dev-valkey-v2.kb2qe0.use1.cache.amazonaws.com";
    internal const string Generation = "73041e6288899e1528f3b8f25c39e888";

    internal static JsonObject Map(IReadOnlyDictionary<string, string> payloads, DeliveryTrace? trace = null)
    {
        trace ??= new();
        try
        {
            trace.Enter(DeliveryPhase.RedisParse);
            using var redisDoc = Parse(payloads["redis"], "username", "password");
            trace.Enter(DeliveryPhase.RedisMap);
            var redis = new ConfigurationOptions { Ssl = true, SslHost = RedisHost, DefaultDatabase = 0,
                User = Text(redisDoc.RootElement, "username"), Password = Text(redisDoc.RootElement, "password"),
                AllowAdmin = false, IncludeDetailInExceptions = false };
            redis.EndPoints.Add(RedisHost, 6379);
            var redisString = redis.ToString(includePassword: true);
            // Parser roundtrip must not allow a credential to inject client settings/endpoints.
            var parsed = ConfigurationOptions.Parse(redisString);
            if (parsed.User != redis.User || parsed.Password != redis.Password || parsed.EndPoints.Count != 1 ||
                parsed.EndPoints[0] is not System.Net.DnsEndPoint endpoint || endpoint.Host != RedisHost || endpoint.Port != 6379 ||
                !parsed.Ssl || parsed.SslHost != RedisHost || parsed.AllowAdmin)
                throw new DeliveryFailure("redis-mapping-invalid");

            trace.Enter(DeliveryPhase.MongodbParse);
            using var mongoDoc = Parse(payloads["mongodb"], "username", "password", "host", "database");
            trace.Enter(DeliveryPhase.MongodbMap);
            var mongo = mongoDoc.RootElement;
            var host = Text(mongo, "host");
            if (!Regex.IsMatch(host, @"\A[a-zA-Z0-9][a-zA-Z0-9.-]*\.mongodb\.net\z", RegexOptions.CultureInvariant) ||
                host.Contains("..", StringComparison.Ordinal) || Text(mongo, "database") != "enver_development")
                throw new DeliveryFailure("mongodb-mapping-invalid");
            var uri = "mongodb+srv://" + Uri.EscapeDataString(Text(mongo, "username")) + ":" +
                Uri.EscapeDataString(Text(mongo, "password")) + "@" + host + "/enver_development?retryWrites=false&w=majority";

            trace.Enter(DeliveryPhase.HmacParse);
            using var hmacDoc = Parse(payloads["hmac"], "keyBase64");
            trace.Enter(DeliveryPhase.HmacMap);
            var hmacKey = Text(hmacDoc.RootElement, "keyBase64");
            trace.Enter(DeliveryPhase.JwtParse);
            using var jwtDoc = Parse(payloads["jwt"], "activeKid", "keys");
            trace.Enter(DeliveryPhase.JwtMap);
            var jwt = jwtDoc.RootElement;
            if (jwt.GetProperty("keys").ValueKind != JsonValueKind.Array) throw new DeliveryFailure("jwt-schema-invalid");
            var ring = new JsonArray();
            foreach (var key in jwt.GetProperty("keys").EnumerateArray())
            {
                Exact(key, "kid", "keyBase64");
                ring.Add(new JsonObject { ["Kid"] = Text(key, "kid"), ["KeyBase64"] = Text(key, "keyBase64") });
            }
            var activeKid = Text(jwt, "activeKid");
            trace.Enter(DeliveryPhase.NonsecretConfig);
            var result = new JsonObject
            {
                ["Redis"] = new JsonObject { ["NamespacePrefix"] = "enver:development" },
                ["MongoDb"] = new JsonObject { ["DatabaseName"] = "enver_development" },
                ["RateLimiting"] = new JsonObject { ["MaxActivePartitions"] = 5000 },
                ["AuthenticationSecurityAuthority"] = new JsonObject { ["TableName"] = "enver-dev-authentication-authority", ["OperationTimeoutMilliseconds"] = 2000 },
                ["AuthenticationComposition"] = new JsonObject { ["AuthorityRegion"] = "us-east-1", ["QuotaConfigurationVersion"] = "quota-development-academic-initial-v1",
                    ["QuotaIdentityReference"] = "hmac-development-fecf274e-v1", ["QuotaGeneration"] = Generation }
            };
            trace.Enter(DeliveryPhase.JsonBuild);
            result["Redis"]!["ConnectionString"] = redisString;
            result["MongoDb"]!["ConnectionString"] = uri;
            result["Jwt"] = new JsonObject { ["ActiveKid"] = activeKid, ["Keys"] = ring };
            result["RateLimiting"]!["PseudonymizationKeyBase64"] = hmacKey;
            return result;
        }
        catch (Exception error) { throw DeliveryFailure.Wrap(error); }
    }

    private static JsonDocument Parse(string text, params string[] names)
    {
        var doc = JsonDocument.Parse(text, new JsonDocumentOptions { MaxDepth = 8 });
        try { Exact(doc.RootElement, names); return doc; }
        catch { doc.Dispose(); throw; }
    }

    private static void Exact(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object) throw new DeliveryFailure("schema-invalid");
        var actual = element.EnumerateObject().Select(p => p.Name).ToArray();
        if (actual.Length != names.Length || actual.Distinct(StringComparer.Ordinal).Count() != names.Length ||
            !names.All(n => actual.Contains(n, StringComparer.Ordinal))) throw new DeliveryFailure("schema-invalid");
    }

    private static string Text(JsonElement element, string key)
    {
        var value = element.GetProperty(key);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString())) throw new DeliveryFailure("schema-invalid");
        return value.GetString()!;
    }
}

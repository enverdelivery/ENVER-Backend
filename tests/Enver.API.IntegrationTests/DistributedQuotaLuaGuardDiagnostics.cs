using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

// Copia generada EXCLUSIVAMENTE de test. Rechaza cualquier otro original mediante hash y
// conserva condiciones, orden de mutación, keys, comandos y semántica del Lua productivo.
// Cada retorno `unavailable` recibe un marcador estable U01..U41 y valores ya leídos en su
// propio camino. Los wrappers de lectura ejecutan el MISMO comando, nunca una lectura extra.
// La perturbación temporal del instrumento es aceptada: un PASS no acredita reparación.
internal static class DistributedQuotaLuaGuardDiagnostics
{
    internal const string OriginalSha256 = "052ec050c7d604976b1e02a9abccbd1412c59c77c6b817c08542b26546f75544";
    internal const int ExpectedGuards = 41;
    internal const int MaximumDataCharacters = 3600;

    internal sealed record Guard(string Code, int Line, string Condition, string Statement);

    internal sealed record Evidence(string Code, int Line, string Condition, string Statement,
        JsonElement? Data, bool DataTruncated, bool DataInvalid);

    private sealed record Generated(string Script, Guard[] Guards, Dictionary<string, Guard> ByCode);

    private static readonly Generated BuildResult = Build();

    internal static string ScriptText => BuildResult.Script;
    internal static IReadOnlyList<Guard> Guards => BuildResult.Guards;

    private static Generated Build()
    {
        var original = RedisAuthenticationQuotaScriptProtocol.ScriptText.Replace("\r\n", "\n");
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(original))).ToLowerInvariant() != OriginalSha256)
            throw new InvalidOperationException("La guardia Lua requiere revisar hash y mapa del script productivo.");
        var lines = original.Split('\n');
        var guards = new List<Guard>(ExpectedGuards);
        for (var index = 0; index < lines.Length; index++)
        {
            if (index == 4)
            {
                // Envoltura {respuesta original de 6 campos, marcador, datos acotados}.
                lines[index] = lines[index].Replace("return {", "return {{") +
                    ", diagBranch, cjson.encode({reads = diagReads, truncated = diagTruncated})}";
                continue;
            }
            if (index == 6)
            {
                lines[index] = "local function unavailable(category, now, branch) diagBranch = branch; return reply('unavailable', category, 0, now) end";
                continue;
            }
            var source = lines[index];
            lines[index] = Regex.Replace(source, @"return unavailable\('([a-z]+)'(, now)?\)", match =>
            {
                var ordinal = guards.Count + 1;
                var code = $"U{ordinal:00}";
                guards.Add(new Guard(code, index + 1, Condition(lines, index), match.Value));
                return $"return unavailable('{match.Groups[1].Value}', {(match.Groups[2].Success ? "now" : "nil")}, '{code}')";
            });
            // La sustitución conserva argumentos, orden y cortocircuito del original.
            lines[index] = Regex.Replace(lines[index], @"redis\.call\('(TYPE|HLEN|PTTL|HMGET|TIME|ZCARD|ZRANGE|PEXPIRETIME|ZSCORE|ZCOUNT)'", "diagRead('$1'");
            if (index == 127)
                lines[index] += "\n    diagAdd({'bucket-input', tostring(i), diagNumber(window), diagNumber(limit)})";
            if (index == 80)
                lines[index] += "\n    diagAdd({'metadata-equality', tostring(v[2] == snapshot), tostring(v[3] == generation)})";
        }
        if (guards.Count != ExpectedGuards)
            throw new InvalidOperationException($"Mapa de guardias Lua inesperado: {guards.Count}.");
        if (guards.Select(guard => guard.Line).Distinct().Count() != guards.Count)
            throw new InvalidOperationException("Mapa de guardias Lua con líneas duplicadas.");
        return new Generated(Prelude + "\n" + string.Join('\n', lines), guards.ToArray(),
            guards.ToDictionary(guard => guard.Code, StringComparer.Ordinal));
    }

    // Condición determinista del retorno: la propia línea si es inline, o el bloque if/elseif
    // inmediatamente superior cuando el retorno ocupa una línea aparte.
    private static string Condition(string[] lines, int index)
    {
        var line = lines[index];
        var ret = line.IndexOf("return unavailable(", StringComparison.Ordinal);
        var elseif = line.IndexOf("elseif ", StringComparison.Ordinal);
        var start = elseif >= 0 ? elseif : line.IndexOf("if ", StringComparison.Ordinal);
        if (start >= 0 && start < ret)
        {
            var then = line.IndexOf(" then", start, StringComparison.Ordinal);
            return line[start..(then > start ? then : line.Length)].Trim();
        }
        var begin = index;
        while (begin > 0 && begin >= index - 16)
        {
            var trimmed = lines[begin].TrimStart();
            if (trimmed.StartsWith("if ", StringComparison.Ordinal) || trimmed.StartsWith("elseif ", StringComparison.Ordinal)) break;
            begin--;
        }
        if (begin < 0 || begin < index - 16) return string.Empty;
        var last = line.IndexOf(" then", StringComparison.Ordinal) > 0 ? index : index - 1;
        if (last < begin) return string.Empty;
        var joined = string.Join(" ", lines[begin..(last + 1)].Select(value => value.Trim()));
        var close = joined.IndexOf(" then", StringComparison.Ordinal);
        return (close > 0 ? joined[..close] : joined).Trim();
    }

    // Proyección cerrada: números exactos como strings (cjson no conserva timestamps µs),
    // enums y posiciones KEYS. Nunca keys, members, ARGV ni snapshots.
    private const string Prelude = """
        local diagReads, diagTruncated, diagBranch = {}, false, 'NON_UNAVAILABLE'
        local function diagNumber(value)
            local n = tonumber(value)
            if not n or n ~= n or n < -9007199254740991 or n > 9007199254740991 or n ~= math.floor(n) then return 'NOT_INTEGER' end
            return string.format('%.0f', n)
        end
        local function diagAdd(row)
            if #diagReads < 40 then table.insert(diagReads, row) else diagTruncated = true end
        end
        local function diagEnum(value, allowed)
            return allowed[value] and value or 'OTHER'
        end
        local function diagRead(...)
            local args = {...}
            local value = redis.call(unpack(args))
            local command, keyIndex = args[1], 0
            if command ~= 'TIME' then
                for i = 1, #KEYS do if args[2] == KEYS[i] then keyIndex = i; break end end
            end
            local row = {command, tostring(keyIndex)}
            if command == 'TYPE' then
                row[3] = diagEnum(value.ok, {none=true, hash=true, zset=true, string=true, list=true, set=true, stream=true})
            elseif command == 'HMGET' then
                row[3] = diagNumber(value[1])
                row[4] = diagEnum(value[4], {ready=true, closed=true})
                for _, i in ipairs({5, 6, 7, 8, 9, 10, 11, 12}) do table.insert(row, diagNumber(value[i])) end
            elseif command == 'TIME' then
                row[3], row[4] = diagNumber(value[1]), diagNumber(value[2])
            elseif command == 'ZRANGE' then
                row[3], row[4], row[5], row[6] = diagNumber(args[3]), diagNumber(args[4]), tostring(#value), diagNumber(value[2])
            else
                row[3] = diagNumber(value)
            end
            diagAdd(row)
            return value
        end
        """;

    internal static RedisResult Unwrap(RedisResult response, out Evidence? evidence)
    {
        var envelope = (RedisResult[]?)response;
        if (envelope is not { Length: 3 })
            throw new InvalidOperationException("Envoltura de guardia Lua no reconocida.");
        var code = envelope[1].ToString();
        if (code == "NON_UNAVAILABLE")
        {
            evidence = new Evidence(code, 0, string.Empty, string.Empty, null, false, false);
            return envelope[0];
        }
        if (!BuildResult.ByCode.TryGetValue(code, out var guard))
            throw new InvalidOperationException("Marcador de guardia Lua no reconocido.");
        var text = envelope[2].ToString();
        if (text.Length > MaximumDataCharacters)
            throw new InvalidOperationException("Evidencia de guardia Lua excede el límite diagnóstico.");
        JsonElement? data = null;
        var truncated = false;
        var invalid = false;
        try
        {
            using var document = JsonDocument.Parse(text);
            data = document.RootElement.Clone();
            truncated = data.Value.TryGetProperty("truncated", out var flag) && flag.ValueKind == JsonValueKind.True;
        }
        catch (JsonException) { invalid = true; }
        evidence = new Evidence(guard.Code, guard.Line, guard.Condition, guard.Statement, data, truncated, invalid);
        // El parser PRODUCTIVO recibe únicamente los seis campos originales, sin cambios.
        return envelope[0];
    }
}

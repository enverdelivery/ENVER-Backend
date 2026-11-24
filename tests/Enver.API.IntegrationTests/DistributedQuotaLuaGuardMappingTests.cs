using System.Text.RegularExpressions;
using Enver.API.Infrastructure.Redis;

namespace Enver.API.IntegrationTests;

// Validación sintética del instrumento: no abre contenedores ni ejecuta Redis.
// Acredita que la copia de test cubre los 41 retornos `unavailable`, conserva sus líneas
// y no agrega ni elimina comandos Redis respecto del script productivo.
public sealed class DistributedQuotaLuaGuardMappingTests
{
    [Fact]
    public void Guard_map_covers_every_unavailable_return_with_stable_lines()
    {
        var production = RedisAuthenticationQuotaScriptProtocol.ScriptText.Replace("\r\n", "\n");
        var productionLines = production.Split('\n');
        var guards = DistributedQuotaLuaGuardDiagnostics.Guards;
        Assert.Equal(41, guards.Count);
        Assert.Equal(Enumerable.Range(1, 41).Select(index => $"U{index:00}"), guards.Select(guard => guard.Code));
        Assert.Equal(guards.Select(guard => guard.Line).OrderBy(line => line), guards.Select(guard => guard.Line));
        Assert.Equal(guards.Count, guards.Select(guard => guard.Line).Distinct().Count());
        foreach (var guard in guards)
        {
            Assert.InRange(guard.Line, 1, productionLines.Length);
            Assert.Contains("return unavailable(", productionLines[guard.Line - 1]);
            Assert.StartsWith("return unavailable(", guard.Statement);
            Assert.Contains($"'{guard.Code}'", DistributedQuotaLuaGuardDiagnostics.ScriptText);
            Assert.False(string.IsNullOrWhiteSpace(guard.Condition));
        }
    }

    [Fact]
    public void Instrumentation_preserves_the_exact_redis_command_multiset()
    {
        var production = RedisAuthenticationQuotaScriptProtocol.ScriptText.Replace("\r\n", "\n");
        var instrumented = DistributedQuotaLuaGuardDiagnostics.ScriptText;
        Assert.Equal(Commands(production, @"redis\.call\('([A-Z]+)'"),
            Commands(instrumented, @"(?:redis\.call|diagRead)\('([A-Z]+)'"));
        Assert.Equal(41, Regex.Matches(instrumented, @"\bU[0-9]{2}\b").Count);
    }

    private static string[] Commands(string script, string pattern) =>
        Regex.Matches(script, pattern).Select(match => match.Groups[1].Value)
            .OrderBy(value => value, StringComparer.Ordinal).ToArray();
}

using System.Globalization;
using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.Infrastructure.Redis;

/// <summary>Snapshot de control confiable y generación fijada; nunca se recargan desde un request.</summary>
public sealed class RedisAuthenticationQuotaConfiguration
{
    public const long MaximumExactInteger = 9007199254740991;
    public RedisAuthenticationQuotaConfiguration(AuthenticationQuotaConfigurationSnapshot snapshot, string generation)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Keys = new(snapshot.Environment);
        if (!RedisAuthenticationQuotaKeyBuilder.IsAttemptId(generation) ||
            !Token(snapshot.ConfigurationVersion) || !Token(snapshot.IdentityConfigurationReference) || !Token(snapshot.Policy.Revision) ||
            snapshot.Capacity.MaxActivePartitions != 5000 || snapshot.Capacity.MemoryBudgetBytes != 67108864)
            throw new ArgumentException("Configuración de cuotas inválida.");
        foreach (var rule in snapshot.Policy.Rules) _ = WindowMicroseconds(rule.Definition.Window);
        Snapshot = snapshot;
        Generation = generation;
    }
    public AuthenticationQuotaConfigurationSnapshot Snapshot { get; }
    public string Generation { get; }
    public RedisAuthenticationQuotaKeyBuilder Keys { get; }
    public static long WindowMicroseconds(TimeSpan window)
    {
        if (window.Ticks <= 0 || window.Ticks % 10 != 0 || window.Ticks / 10 > MaximumExactInteger)
            throw new ArgumentException("Ventana de cuotas no representable.");
        return window.Ticks / 10;
    }
    internal static bool Token(string value) => value.Length is > 0 and <= 128 &&
        value.All(c => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-' or '.');
    internal IEnumerable<AuthenticationQuotaPolicyRule> OrderedRules => Snapshot.Policy.Rules.OrderBy(r => r.Operation).ThenBy(r => r.ScopeType);
    internal static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
    public override string ToString() => nameof(RedisAuthenticationQuotaConfiguration);
}

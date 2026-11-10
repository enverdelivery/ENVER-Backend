using System.Globalization;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using StackExchange.Redis;

namespace Enver.API.Infrastructure.Redis;

/// <summary>Protocolo cerrado y transporte sin retry/read-back. No registra argumentos ni resultados Redis.</summary>
public static class RedisAuthenticationQuotaScriptProtocol
{
    public const string Contract = "enver.auth.quota";
    public const int Version = 1;
    public const CommandFlags Flags = CommandFlags.DemandMaster;
    private static readonly Lazy<string> Script = new(() =>
    {
        using var stream = typeof(RedisAuthenticationQuotaScriptProtocol).Assembly.GetManifestResourceStream(
            "Enver.API.Infrastructure.Redis.Scripts.ConsumeAuthenticationQuota.lua")
            ?? throw new InvalidOperationException("Recurso de cuotas no disponible.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    });
    public static string ScriptText => Script.Value;

    public static RedisAuthenticationQuotaInvocation Encode(RedisAuthenticationQuotaConfiguration config, string mode,
        AuthenticationQuotaConsumeRequest? request = null, IEnumerable<string>? candidates = null, string? nextGeneration = null)
    {
        if (mode is not ("evaluate" or "initialize" or "fence" or "maintain" or "activate"))
            throw new ArgumentException("Modo de cuotas inválido.");
        if (mode == "evaluate" && (request is null || !config.Snapshot.Policy.Accepts(request) ||
            !RedisAuthenticationQuotaKeyBuilder.IsAttemptId(request.InternalAttemptId))) throw new ArgumentException("Solicitud de cuotas inválida.");
        if (mode != "evaluate" && request is not null) throw new ArgumentException("Solicitud de control inválida.");
        if (mode == "fence" && (!RedisAuthenticationQuotaKeyBuilder.IsAttemptId(nextGeneration) || nextGeneration == config.Generation))
            throw new ArgumentException("Generación de cuotas inválida.");
        var scopes = request?.Scopes.OrderBy(s => s.ScopeType).ToArray() ?? [];
        var required = scopes.Select(s => config.Keys.Bucket(request!.Operation, s.ScopeType, s.OpaqueId)).ToArray();
        var hinted = (candidates ?? []).Take(4).Select(config.Keys.CandidateBucket).Distinct().Except(required).ToArray();
        if (hinted.Length > 3) throw new ArgumentException("Lote de cuotas inválido.");
        RedisKey[] keys = [config.Keys.Metadata, config.Keys.Registry, .. required.Select(k => (RedisKey)k), .. hinted.Select(k => (RedisKey)k)];
        var s = config.Snapshot;
        List<RedisValue> args = [Contract, Version, mode, s.Environment, s.ConfigurationVersion, s.IdentityConfigurationReference,
            s.Policy.Revision, 5000, 67108864, config.Generation, nextGeneration ?? "-", request?.InternalAttemptId ?? "-",
            request is null ? "-" : RedisAuthenticationQuotaKeyBuilder.OperationToken(request.Operation), scopes.Length, hinted.Length];
        foreach (var rule in config.OrderedRules)
        {
            args.Add(rule.Definition.PermitLimit);
            args.Add(RedisAuthenticationQuotaConfiguration.WindowMicroseconds(rule.Definition.Window));
        }
        foreach (var scope in scopes) { args.Add(RedisAuthenticationQuotaKeyBuilder.ScopeToken(scope.ScopeType)); args.Add(scope.OpaqueId); }
        return new(keys, args.ToArray());
    }

    public static RedisAuthenticationQuotaReply Parse(RedisResult response)
    {
        try
        {
            var values = (RedisResult[]?)response;
            if (values is not { Length: 6 } || values[0].ToString() != Contract || values[1].ToString() != "1") return Unknown();
            var status = values[2].ToString();
            var category = values[3].ToString();
            if (category is not ("ok" or "quota" or "capacity" or "invalid" or "state" or "closed" or "collision" or "drain")) return Unknown();
            if (!long.TryParse(values[4].ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var retry) ||
                !long.TryParse(values[5].ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var now) ||
                now < 0 || now > RedisAuthenticationQuotaConfiguration.MaximumExactInteger) return Unknown();
            if (status == "rejected" && category == "quota" && retry > 0 && retry <= TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerSecond)
                return new(status, category, retry, now, true);
            if (retry != 0) return Unknown();
            if ((status == "admitted" || status is "initialized" or "fenced" or "maintained" or "activated") && category == "ok" && now > 0)
                return new(status, category, 0, now, true);
            if (status == "unavailable" && category is "capacity" or "invalid" or "state" or "closed" or "collision" or "drain")
                return new(status, category, 0, now, true);
        }
        catch { /* Respuesta no reconocida: no inferir ausencia de efectos. */ }
        return Unknown();
    }

    internal static RedisAuthenticationQuotaReply Unknown() => new("uncertain", "state", 0, 0, false);
    internal static async ValueTask<AuthenticationQuotaConsumeResult?> CheckSecurityAsync(
        Features.Authentication.Application.Ports.IAuthenticationSecurityState local,
        Features.Authentication.Application.Ports.IAuthenticationSecurityState global, RequestBudget budget)
    {
        foreach (var state in new[] { local, global })
        {
            budget.ThrowIfCancellationRequested();
            var decision = await state.EvaluateAsync(budget).ConfigureAwait(false);
            if (!decision.CanContinue) return new(decision.ResultUncertain ? AuthenticationQuotaStoreStatus.Uncertain : AuthenticationQuotaStoreStatus.Unavailable,
                cancellationCause: decision.CancellationCause);
        }
        return null;
    }

    internal static async ValueTask<RedisAuthenticationQuotaReply> ExecuteAsync(RedisConnections connections, RedisOptions options,
        RedisAuthenticationQuotaInvocation invocation, RequestBudget budget,
        Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? execute = null)
    {
        using var dependency = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        var dispatched = false;
        try
        {
            var script = ScriptText;
            IDatabase? database = null;
            if (execute is null) database = (await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false)).GetDatabase();
            dependency.ThrowIfCancellationRequested();
            dependency.MarkDispatched();
            dispatched = true;
            var response = execute is null
                ? await database!.ScriptEvaluateAsync(script, invocation.Keys, invocation.Arguments, Flags).WaitAsync(dependency.Token).ConfigureAwait(false)
                : await execute(dependency, invocation).AsTask().WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            return Parse(response);
        }
        catch
        {
            return new(dispatched ? "uncertain" : "unavailable", "state", 0, 0, !dispatched, dependency.Cause);
        }
    }
}

public sealed class RedisAuthenticationQuotaInvocation(RedisKey[] keys, RedisValue[] arguments)
{
    public RedisKey[] Keys { get; } = keys;
    public RedisValue[] Arguments { get; } = arguments;
    public override string ToString() => nameof(RedisAuthenticationQuotaInvocation);
}

public sealed class RedisAuthenticationQuotaReply(string status, string category, long retryAfterSeconds, long nowMicroseconds,
    bool isSemantic, BudgetCancellationCause cancellationCause = BudgetCancellationCause.None)
{
    public string Status { get; } = status;
    public string Category { get; } = category;
    public long RetryAfterSeconds { get; } = retryAfterSeconds;
    public long NowMicroseconds { get; } = nowMicroseconds;
    public bool IsSemantic { get; } = isSemantic;
    public BudgetCancellationCause CancellationCause { get; } = cancellationCause;
    public AuthenticationQuotaConsumeResult ToConsumeResult() => Status switch
    {
        "admitted" => new(AuthenticationQuotaStoreStatus.Admitted),
        "rejected" => new(AuthenticationQuotaStoreStatus.Rejected, TimeSpan.FromTicks(checked(RetryAfterSeconds * TimeSpan.TicksPerSecond))),
        "unavailable" => new(AuthenticationQuotaStoreStatus.Unavailable, cancellationCause: CancellationCause),
        _ => new(AuthenticationQuotaStoreStatus.Uncertain, cancellationCause: CancellationCause)
    };
    public override string ToString() => nameof(RedisAuthenticationQuotaReply);
}

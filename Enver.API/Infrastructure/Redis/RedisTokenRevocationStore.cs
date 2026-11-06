using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using StackExchange.Redis;

namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Almacena marcas de revocación en Redis sin caché negativa ni reintentos de aplicación.
/// </summary>
public sealed class RedisTokenRevocationStore : ITokenRevocationStore
{
    private const string RevokedValue = "1";
    private const int ScriptConfirmed = 1;
    private readonly RedisConnections connections;
    private readonly IAuthenticationSecurityState securityState;
    private readonly RedisOptions options;
    private readonly TimeProvider clock;
    private readonly string environment;
    private readonly Func<DependencyBudget, RedisKey, string, long, ValueTask<RedisResult>>? revokeOverride;
    private readonly Func<DependencyBudget, RedisKey, ValueTask<RedisValue>>? checkOverride;
    private static readonly Lazy<string> RevokeScript = new(LoadRevokeScript, LazyThreadSafetyMode.ExecutionAndPublication);

    public RedisTokenRevocationStore(
        RedisConnections connections,
        IAuthenticationSecurityState securityState,
        RedisOptions options,
        TimeProvider? clock = null,
        Func<DependencyBudget, RedisKey, string, long, ValueTask<RedisResult>>? revokeOverride = null,
        Func<DependencyBudget, RedisKey, ValueTask<RedisValue>>? checkOverride = null)
    {
        this.connections = connections ?? throw new ArgumentNullException(nameof(connections));
        this.securityState = securityState ?? throw new ArgumentNullException(nameof(securityState));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.OperationTimeoutMilliseconds);
        environment = ExtractEnvironment(options.NamespacePrefix);
        this.clock = clock ?? TimeProvider.System;
        this.revokeOverride = revokeOverride;
        this.checkOverride = checkOverride;
    }

    public async ValueTask<TokenRevocationWriteResult> RevokeAsync(
        TokenRevocationWriteRequest request,
        RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        var admission = await securityState.EvaluateAsync(requestBudget).ConfigureAwait(false);
        if (!admission.CanContinue)
        {
            return new TokenRevocationWriteResult(
                admission.ResultUncertain ? TokenRevocationWriteStatus.Uncertain : TokenRevocationWriteStatus.Unavailable,
                admission.CancellationCause);
        }

        if (!TryCalculateRetainUntil(request, out var retainUntilMilliseconds))
        {
            return new TokenRevocationWriteResult(TokenRevocationWriteStatus.Unavailable);
        }

        if (request.ExpiresAtUtc.Add(request.EffectiveClockSkew).Add(request.OperationalMargin) <= clock.GetUtcNow())
        {
            return new TokenRevocationWriteResult(TokenRevocationWriteStatus.NoOperation);
        }

        if (!TryBuildKey(request.Issuer, request.Jti, out var key))
        {
            return new TokenRevocationWriteResult(TokenRevocationWriteStatus.Unavailable);
        }

        string script;
        try
        {
            script = RevokeScript.Value;
        }
        catch
        {
            return new TokenRevocationWriteResult(TokenRevocationWriteStatus.Unavailable);
        }

        using var dependency = requestBudget.BeginDependency(options.OperationTimeoutMilliseconds);
        var dispatched = false;
        try
        {
            IDatabase? database = null;
            if (revokeOverride is null)
            {
                var multiplexer = await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false);
                dependency.ThrowIfCancellationRequested();
                database = multiplexer.GetDatabase();
            }

            dependency.MarkDispatched();
            dispatched = true;
            var response = revokeOverride is null
                ? await database!.ScriptEvaluateAsync(
                    script,
                    [key],
                    [RevokedValue, retainUntilMilliseconds],
                    CommandFlags.DemandMaster).WaitAsync(dependency.Token).ConfigureAwait(false)
                : await revokeOverride(dependency, key, script, retainUntilMilliseconds).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();

            return IsConfirmed(response)
                ? new TokenRevocationWriteResult(TokenRevocationWriteStatus.Confirmed)
                : new TokenRevocationWriteResult(TokenRevocationWriteStatus.Unavailable);
        }
        catch (BudgetCanceledException exception)
        {
            return WriteCanceled(exception);
        }
        catch (OperationCanceledException)
        {
            return WriteCancellationFromBudget(dependency, dispatched);
        }
        catch (RedisException)
        {
            return new TokenRevocationWriteResult(dispatched ? TokenRevocationWriteStatus.Uncertain : TokenRevocationWriteStatus.Unavailable);
        }
        catch
        {
            return new TokenRevocationWriteResult(dispatched ? TokenRevocationWriteStatus.Uncertain : TokenRevocationWriteStatus.Unavailable);
        }
    }

    public async ValueTask<TokenRevocationReadResult> CheckAsync(
        TokenRevocationReadRequest request,
        RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        var admission = await securityState.EvaluateAsync(requestBudget).ConfigureAwait(false);
        if (!admission.CanContinue)
        {
            return new TokenRevocationReadResult(
                admission.ResultUncertain ? TokenRevocationReadStatus.Uncertain : TokenRevocationReadStatus.Unavailable,
                admission.CancellationCause);
        }

        if (!TryBuildKey(request.Issuer, request.Jti, out var key))
        {
            return new TokenRevocationReadResult(TokenRevocationReadStatus.Unavailable);
        }

        using var dependency = requestBudget.BeginDependency(options.OperationTimeoutMilliseconds);
        var dispatched = false;
        try
        {
            IDatabase? database = null;
            if (checkOverride is null)
            {
                var multiplexer = await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false);
                dependency.ThrowIfCancellationRequested();
                database = multiplexer.GetDatabase();
            }

            dependency.MarkDispatched();
            dispatched = true;
            var value = checkOverride is null
                ? await database!.StringGetAsync(key, CommandFlags.DemandMaster).WaitAsync(dependency.Token).ConfigureAwait(false)
                : await checkOverride(dependency, key).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            return new TokenRevocationReadResult(value.IsNull ? TokenRevocationReadStatus.NotRevoked : TokenRevocationReadStatus.Revoked);
        }
        catch (BudgetCanceledException exception)
        {
            return ReadCanceled(exception);
        }
        catch (OperationCanceledException)
        {
            return ReadCancellationFromBudget(dependency, dispatched);
        }
        catch (RedisException)
        {
            return new TokenRevocationReadResult(dispatched ? TokenRevocationReadStatus.Uncertain : TokenRevocationReadStatus.Unavailable);
        }
        catch
        {
            return new TokenRevocationReadResult(dispatched ? TokenRevocationReadStatus.Uncertain : TokenRevocationReadStatus.Unavailable);
        }
    }

    private static bool TryCalculateRetainUntil(TokenRevocationWriteRequest request, out long retainUntilMilliseconds)
    {
        retainUntilMilliseconds = default;
        if (request.ExpiresAtUtc.Offset != TimeSpan.Zero || request.EffectiveClockSkew < TimeSpan.Zero || request.OperationalMargin < TimeSpan.Zero)
        {
            return false;
        }

        try
        {
            var retainUntil = request.ExpiresAtUtc.Add(request.EffectiveClockSkew).Add(request.OperationalMargin);
            retainUntilMilliseconds = retainUntil.ToUnixTimeMilliseconds();
            if (retainUntil.UtcTicks % TimeSpan.TicksPerMillisecond != 0)
            {
                retainUntilMilliseconds = checked(retainUntilMilliseconds + 1);
            }

            return true;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private bool TryBuildKey(string issuer, string jti, out RedisKey key)
    {
        try
        {
            key = RedisKeyBuilder.BuildRevocationKey(environment, issuer, jti);
            return true;
        }
        catch (ArgumentException)
        {
            key = default;
            return false;
        }
    }

    private static TokenRevocationWriteResult WriteCanceled(BudgetCanceledException exception) => new(
        exception.ResultUncertain ? TokenRevocationWriteStatus.Uncertain : TokenRevocationWriteStatus.Unavailable,
        exception.Cause);

    private static TokenRevocationReadResult ReadCanceled(BudgetCanceledException exception) => new(
        exception.ResultUncertain ? TokenRevocationReadStatus.Uncertain : TokenRevocationReadStatus.Unavailable,
        exception.Cause);

    private static TokenRevocationWriteResult WriteCancellationFromBudget(DependencyBudget dependency, bool dispatched)
    {
        try
        {
            dependency.ThrowIfCancellationRequested();
        }
        catch (BudgetCanceledException exception)
        {
            return WriteCanceled(exception);
        }

        return new TokenRevocationWriteResult(dispatched ? TokenRevocationWriteStatus.Uncertain : TokenRevocationWriteStatus.Unavailable);
    }

    private static TokenRevocationReadResult ReadCancellationFromBudget(DependencyBudget dependency, bool dispatched)
    {
        try
        {
            dependency.ThrowIfCancellationRequested();
        }
        catch (BudgetCanceledException exception)
        {
            return ReadCanceled(exception);
        }

        return new TokenRevocationReadResult(dispatched ? TokenRevocationReadStatus.Uncertain : TokenRevocationReadStatus.Unavailable);
    }

    private static bool IsConfirmed(RedisResult response) => long.TryParse(response.ToString(), out var result) && result == ScriptConfirmed;

    private static string ExtractEnvironment(string namespacePrefix) => namespacePrefix.StartsWith("enver:", StringComparison.Ordinal)
        ? namespacePrefix["enver:".Length..]
        : namespacePrefix;

    private static string LoadRevokeScript()
    {
        const string resourceName = "Enver.API.Infrastructure.Redis.Scripts.RevokeToken.lua";
        using var stream = typeof(RedisTokenRevocationStore).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("No se encontró el recurso de revocación Redis.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

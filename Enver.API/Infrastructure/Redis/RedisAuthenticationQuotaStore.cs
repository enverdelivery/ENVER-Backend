using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using StackExchange.Redis;

namespace Enver.API.Infrastructure.Redis;

/// <summary>Un consumo conjunto Redis, sin DI productiva, retry, compensación ni read-back.</summary>
public sealed class RedisAuthenticationQuotaStore : IAuthenticationQuotaStore
{
    private readonly RedisConnections connections;
    private readonly RedisOptions options;
    private readonly RedisAuthenticationQuotaConfiguration configuration;
    private readonly IAuthenticationSecurityState local;
    private readonly IAuthenticationSecurityState global;
    private readonly Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? execute;
    private readonly Func<ValueTask<string[]>>? hints;

    public RedisAuthenticationQuotaStore(RedisConnections connections, RedisOptions options, RedisAuthenticationQuotaConfiguration configuration,
        IAuthenticationSecurityState localSecurityState, IAuthenticationSecurityState globalSecurityState,
        Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? executeOverride = null,
        Func<ValueTask<string[]>>? hintsOverride = null)
    {
        this.connections = connections ?? throw new ArgumentNullException(nameof(connections));
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.OperationTimeoutMilliseconds);
        // Copia mínima: mutar Options externamente no cambia los presupuestos capturados.
        this.options = new() { OperationTimeoutMilliseconds = options.OperationTimeoutMilliseconds };
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        local = localSecurityState ?? throw new ArgumentNullException(nameof(localSecurityState));
        global = globalSecurityState ?? throw new ArgumentNullException(nameof(globalSecurityState));
        execute = executeOverride;
        hints = hintsOverride;
    }

    public async ValueTask<AuthenticationQuotaConsumeResult> EvaluateAndConsumeAsync(AuthenticationQuotaConsumeRequest request, RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        try
        {
            requestBudget.ThrowIfCancellationRequested();
            // Codificar primero impide despachar siquiera la lectura de pistas con inputs inválidos.
            _ = RedisAuthenticationQuotaScriptProtocol.Encode(configuration, "evaluate", request);
            var failure = await RedisAuthenticationQuotaScriptProtocol.CheckSecurityAsync(local, global, requestBudget).ConfigureAwait(false);
            if (failure is not null) return failure;
            string[] candidates;
            using (var dependency = requestBudget.BeginDependency(options.OperationTimeoutMilliseconds))
            {
                if (hints is not null) candidates = await hints().AsTask().WaitAsync(dependency.Token).ConfigureAwait(false);
                else
                {
                    var database = (await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false)).GetDatabase();
                    dependency.ThrowIfCancellationRequested();
                    var values = await database.SortedSetRangeByRankAsync(configuration.Keys.Registry, 0, 2, Order.Ascending,
                        RedisAuthenticationQuotaScriptProtocol.Flags).WaitAsync(dependency.Token).ConfigureAwait(false);
                    candidates = values.Select(v => v.ToString()).ToArray();
                }
                dependency.ThrowIfCancellationRequested();
            }
            var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(configuration, "evaluate", request, candidates);
            return (await RedisAuthenticationQuotaScriptProtocol.ExecuteAsync(connections, options, invocation, requestBudget, execute).ConfigureAwait(false)).ToConsumeResult();
        }
        catch
        {
            // Ningún EVAL mutador fue despachado por este camino: ExecuteAsync contiene su frontera post-dispatch.
            return new(AuthenticationQuotaStoreStatus.Unavailable, cancellationCause: requestBudget.Cause);
        }
    }
    public override string ToString() => nameof(RedisAuthenticationQuotaStore);
}

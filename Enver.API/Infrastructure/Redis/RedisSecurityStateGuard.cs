using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;

namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Cierra admisión ante fallos o señales de Redis. No detecta integridad ni reabre confianza.
/// </summary>
public sealed class RedisSecurityStateGuard : IAuthenticationSecurityState
{
    private readonly int operationTimeoutMilliseconds;
    private readonly Func<DependencyBudget, ValueTask> verifier;
    private int closed;

    public RedisSecurityStateGuard(RedisConnections connections, RedisOptions options)
        : this(connections, options, null)
    {
    }

    /// <summary>
    /// El verificador opcional permite casos unitarios deterministas; la composición productiva usa RedisConnections.
    /// </summary>
    public RedisSecurityStateGuard(RedisConnections connections, RedisOptions options, Func<DependencyBudget, ValueTask>? verifier)
    {
        ArgumentNullException.ThrowIfNull(connections);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.OperationTimeoutMilliseconds);

        operationTimeoutMilliseconds = options.OperationTimeoutMilliseconds;
        this.verifier = verifier ?? (budget => VerifyConnectionAsync(connections, budget));
    }

    public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        if (Volatile.Read(ref closed) != 0)
        {
            return ValueTask.FromResult(new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Unavailable));
        }

        return EvaluateCoreAsync(requestBudget);
    }

    /// <summary>
    /// Una señal controlada permite probar reacción; no constituye un detector real ni reabre confianza.
    /// </summary>
    public void ObserveConnectivitySignal(RedisConnectionSignal signal)
    {
        if (signal is RedisConnectionSignal.ConnectionFailed or RedisConnectionSignal.ErrorMessage or RedisConnectionSignal.InternalError)
        {
            CloseAdmission();
        }
    }

    private async ValueTask<AuthenticationSecurityDecision> EvaluateCoreAsync(RequestBudget requestBudget)
    {
        using var dependency = requestBudget.BeginDependency(operationTimeoutMilliseconds);
        try
        {
            await verifier(dependency).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            return new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Verifiable);
        }
        catch (BudgetCanceledException exception)
        {
            if (exception.ResultUncertain)
            {
                CloseAdmission();
            }

            return new AuthenticationSecurityDecision(
                exception.ResultUncertain ? AuthenticationSecurityAdmission.Uncertain : AuthenticationSecurityAdmission.Unavailable,
                exception.Cause);
        }
        catch (OperationCanceledException)
        {
            try
            {
                dependency.ThrowIfCancellationRequested();
            }
            catch (BudgetCanceledException exception)
            {
                if (exception.ResultUncertain)
                {
                    CloseAdmission();
                }

                return new AuthenticationSecurityDecision(
                    exception.ResultUncertain ? AuthenticationSecurityAdmission.Uncertain : AuthenticationSecurityAdmission.Unavailable,
                    exception.Cause);
            }

            CloseAdmission();
            return new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Unavailable);
        }
        catch
        {
            CloseAdmission();
            return new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Unavailable);
        }
    }

    private static async ValueTask VerifyConnectionAsync(RedisConnections connections, DependencyBudget dependency)
    {
        _ = await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false);
        dependency.ThrowIfCancellationRequested();
    }

    private void CloseAdmission() => Interlocked.Exchange(ref closed, 1);

}

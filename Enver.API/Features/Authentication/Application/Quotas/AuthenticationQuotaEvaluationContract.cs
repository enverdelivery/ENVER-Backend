using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Quotas;

public enum AuthenticationQuotaScriptStep
{
    ValidateRequestAndTrustedDescriptor,
    ValidateTopologyBeforeDispatch,
    ValidateArgumentCountsTypesAndRanges,
    ValidateOperationScopesDefinitionsAndRevision,
    ValidateExistingStateAndPredictablePreconditions,
    ReadRedisTimeOnce,
    EvaluateAllLiveWindowsWithSharedNow,
    IfBlockedReturnMaximumRetryAfterWithoutInsertions,
    OtherwisePruneInsertSameAttemptInAllScopesAndApplyRetention
}

/// <summary>Intención contractual; no ejecuta ni acredita una escritura Redis.</summary>
public sealed record AuthenticationQuotaInsertion(AuthenticationQuotaPartitionIdentity Partition, string InternalAttemptId);

/// <summary>
/// Contrato para 021: todas las claves deben poder operarse en una sola ejecución Lua atómica
/// en el destino efectivo. No selecciona Cluster, hash slot, clave agregada ni atomicidad cross-slot.
/// Validar topología corresponde al adapter antes del despacho, no exclusivamente al script.
/// No hay rollback general Lua: error posterior a escritura puede dejar efectos desconocidos/parciales.
/// Solo un resultado semántico confirmado acredita todos-o-ninguno. Sin compensación, retry ciego o fallback local.
/// </summary>
public static class AuthenticationQuotaEvaluationContract
{
    public static IReadOnlyList<AuthenticationQuotaScriptStep> RequiredSteps { get; } =
        Array.AsReadOnly(Enum.GetValues<AuthenticationQuotaScriptStep>());

    /// <summary>
    /// null significa precondiciones satisfechas, nunca admisión ni prueba de capacidad/continuidad.
    /// El futuro adapter deberá convertir también entradas malformadas a fallo cerrado antes de despachar.
    /// </summary>
    public static AuthenticationQuotaConsumeResult? GetPreDispatchFailure(AuthenticationQuotaConsumeRequest request,
        AuthenticationQuotaPolicyDescriptor activePolicy, bool topologySupportsSingleAtomicExecution)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(activePolicy);
        return !activePolicy.Accepts(request) || !topologySupportsSingleAtomicExecution
            ? new(AuthenticationQuotaStoreStatus.Unavailable) : null;
    }

    public static AuthenticationQuotaConsumeResult MissingSemanticResult(bool dispatched,
        BudgetCancellationCause cancellationCause = BudgetCancellationCause.None) =>
        new(dispatched ? AuthenticationQuotaStoreStatus.Uncertain : AuthenticationQuotaStoreStatus.Unavailable,
            cancellationCause: cancellationCause);

    /// <summary>
    /// Describe el efecto exigido a resultados confirmados: un evento por ámbito al admitir, cero al rechazar.
    /// No evalúa cuotas en memoria, no aplica efectos y no sirve para inferir efectos de Uncertain/Unavailable.
    /// InternalAttemptId es interno: no ofrece idempotencia pública ni exactly-once entre requests.
    /// </summary>
    public static IReadOnlyList<AuthenticationQuotaInsertion> DescribeConfirmedInsertions(
        AuthenticationQuotaConsumeRequest request, AuthenticationQuotaConsumeResult result)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(result);
        return result.Status switch
        {
            AuthenticationQuotaStoreStatus.Admitted => Array.AsReadOnly(request.Scopes
                .Select(scope => new AuthenticationQuotaInsertion(request.GetPartitionIdentity(scope), request.InternalAttemptId)).ToArray()),
            AuthenticationQuotaStoreStatus.Rejected => Array.Empty<AuthenticationQuotaInsertion>(),
            _ => throw new ArgumentException("Un resultado no confirmado no describe inserciones confirmadas.", nameof(result))
        };
    }
}

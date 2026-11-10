using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Quotas;

public enum AuthenticationQuotaStoreStatus { Admitted, Rejected, Unavailable, Uncertain }
public enum AuthenticationQuotaScopeType { Email, Origin, OriginEmail, Account }

public sealed record AuthenticationQuotaDefinition
{
    public AuthenticationQuotaDefinition(int permitLimit, TimeSpan window)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(permitLimit);
        if (window <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(window));
        PermitLimit = permitLimit;
        Window = window;
    }

    public int PermitLimit { get; }
    public TimeSpan Window { get; }
}

/// <summary>
/// Identidad conceptual dentro de un ambiente/finalidad; no es una clave Redis.
/// La revisión no forma parte de esta identidad. Su codificación física corresponde a 021/053.
/// </summary>
public sealed record AuthenticationQuotaPartitionIdentity(
    AuthenticationOperation Operation, AuthenticationQuotaScopeType ScopeType, string OpaqueId);

public sealed class AuthenticationQuotaScope
{
    /// <summary>
    /// opaqueId debe venir ya derivado, seguro y coherente entre instancias por 053.
    /// La validación sintáctica no demuestra pseudonimización ni origen confiable.
    /// </summary>
    public AuthenticationQuotaScope(AuthenticationQuotaScopeType scopeType, string opaqueId, AuthenticationQuotaDefinition definition)
    {
        if (!Enum.IsDefined(scopeType)) throw new ArgumentOutOfRangeException(nameof(scopeType));
        QuotaIdentifiers.RequireValid(opaqueId);
        ArgumentNullException.ThrowIfNull(definition);
        ScopeType = scopeType;
        OpaqueId = opaqueId;
        Definition = definition;
    }

    public AuthenticationQuotaScopeType ScopeType { get; }
    public string OpaqueId { get; }
    public AuthenticationQuotaDefinition Definition { get; }
}

public sealed class AuthenticationQuotaConsumeRequest
{
    public AuthenticationQuotaConsumeRequest(AuthenticationOperation operation, string internalAttemptId,
        string? policyRevision, IEnumerable<AuthenticationQuotaScope> scopes)
    {
        var required = AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation);
        QuotaIdentifiers.RequireValid(internalAttemptId);
        ArgumentNullException.ThrowIfNull(scopes);
        var captured = scopes.ToArray();
        if (captured.Any(scope => scope is null) || captured.Length != required.Count ||
            captured.Select(scope => scope.ScopeType).Distinct().Count() != captured.Length ||
            !required.All(type => captured.Any(scope => scope.ScopeType == type)))
        {
            throw new ArgumentException("La evaluación requiere todos sus ámbitos, sin duplicados.", nameof(scopes));
        }

        Operation = operation;
        InternalAttemptId = internalAttemptId;
        // Se conserva incluso una revisión inválida para resolverla como Unavailable, no como HTTP 400.
        PolicyRevision = policyRevision;
        Scopes = Array.AsReadOnly(captured);
    }

    public AuthenticationOperation Operation { get; }
    public string InternalAttemptId { get; }
    public string? PolicyRevision { get; }
    public IReadOnlyList<AuthenticationQuotaScope> Scopes { get; }
    public AuthenticationQuotaPartitionIdentity GetPartitionIdentity(AuthenticationQuotaScope scope)
    {
        if (!Scopes.Contains(scope)) throw new ArgumentException("El ámbito no pertenece a la evaluación.", nameof(scope));
        return new(Operation, scope.ScopeType, scope.OpaqueId);
    }
}

public sealed record AuthenticationQuotaConsumeResult
{
    public AuthenticationQuotaConsumeResult(AuthenticationQuotaStoreStatus status, TimeSpan? retryAfter = null,
        BudgetCancellationCause cancellationCause = BudgetCancellationCause.None)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        if (!Enum.IsDefined(cancellationCause)) throw new ArgumentOutOfRangeException(nameof(cancellationCause));
        if (status == AuthenticationQuotaStoreStatus.Rejected ? retryAfter is null || retryAfter <= TimeSpan.Zero : retryAfter is not null)
            throw new ArgumentException("RetryAfter positivo es exclusivo de un rechazo confirmado.", nameof(retryAfter));
        if ((status is AuthenticationQuotaStoreStatus.Admitted or AuthenticationQuotaStoreStatus.Rejected) && cancellationCause != BudgetCancellationCause.None)
            throw new ArgumentException("Un resultado confirmado no representa una cancelación.", nameof(cancellationCause));
        Status = status;
        RetryAfter = retryAfter;
        CancellationCause = cancellationCause;
    }

    public AuthenticationQuotaStoreStatus Status { get; }
    public TimeSpan? RetryAfter { get; }
    public BudgetCancellationCause CancellationCause { get; }
    public bool ResultUncertain => Status == AuthenticationQuotaStoreStatus.Uncertain;
}

internal static class QuotaIdentifiers
{
    internal static bool IsValid(string? value) => !string.IsNullOrWhiteSpace(value) &&
        !value.Any(character => char.IsWhiteSpace(character) || char.IsControl(character));

    internal static void RequireValid(string? value)
    {
        if (!IsValid(value)) throw new ArgumentException("Identificador interno inválido.");
    }
}

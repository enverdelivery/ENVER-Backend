using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Quotas;

public sealed record AuthenticationQuotaPolicyRule
{
    public AuthenticationQuotaPolicyRule(AuthenticationOperation operation, AuthenticationQuotaScopeType scopeType,
        AuthenticationQuotaDefinition definition)
    {
        if (!AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Contains(scopeType))
            throw new ArgumentException("Ámbito incompatible con la operación.", nameof(scopeType));
        ArgumentNullException.ThrowIfNull(definition);
        Operation = operation;
        ScopeType = scopeType;
        Definition = definition;
    }

    public AuthenticationOperation Operation { get; }
    public AuthenticationQuotaScopeType ScopeType { get; }
    public AuthenticationQuotaDefinition Definition { get; }
}

/// <summary>Snapshot confiable e inmutable de las siete definiciones; no se enlaza al host en 052.</summary>
public sealed class AuthenticationQuotaPolicyDescriptor
{
    private static readonly IReadOnlyList<AuthenticationQuotaScopeType> RegisterScopes =
        Array.AsReadOnly(new[] { AuthenticationQuotaScopeType.Email, AuthenticationQuotaScopeType.Origin });
    private static readonly IReadOnlyList<AuthenticationQuotaScopeType> LoginScopes =
        Array.AsReadOnly(new[] { AuthenticationQuotaScopeType.OriginEmail, AuthenticationQuotaScopeType.Email, AuthenticationQuotaScopeType.Origin });
    private static readonly IReadOnlyList<AuthenticationQuotaScopeType> LogoutScopes =
        Array.AsReadOnly(new[] { AuthenticationQuotaScopeType.Account, AuthenticationQuotaScopeType.Origin });

    public AuthenticationQuotaPolicyDescriptor(string revision, IEnumerable<AuthenticationQuotaPolicyRule> rules)
    {
        QuotaIdentifiers.RequireValid(revision);
        ArgumentNullException.ThrowIfNull(rules);
        var captured = rules.ToArray();
        var requiredCount = Enum.GetValues<AuthenticationOperation>().Sum(operation => RequiredScopes(operation).Count);
        if (captured.Any(rule => rule is null) || captured.Length != requiredCount ||
            captured.Select(rule => (rule.Operation, rule.ScopeType)).Distinct().Count() != requiredCount)
            throw new ArgumentException("El descriptor requiere las siete definiciones únicas.", nameof(rules));
        Revision = revision;
        Rules = Array.AsReadOnly(captured);
        MaximumWindow = captured.Max(rule => rule.Definition.Window);
    }

    public string Revision { get; }
    public IReadOnlyList<AuthenticationQuotaPolicyRule> Rules { get; }
    public TimeSpan MaximumWindow { get; }

    public static IReadOnlyList<AuthenticationQuotaScopeType> RequiredScopes(AuthenticationOperation operation) => operation switch
    {
        AuthenticationOperation.Register => RegisterScopes,
        AuthenticationOperation.Login => LoginScopes,
        AuthenticationOperation.Logout => LogoutScopes,
        _ => throw new ArgumentOutOfRangeException(nameof(operation))
    };

    public AuthenticationQuotaDefinition GetDefinition(AuthenticationOperation operation, AuthenticationQuotaScopeType scopeType) =>
        Rules.Single(rule => rule.Operation == operation && rule.ScopeType == scopeType).Definition;

    public bool Accepts(AuthenticationQuotaConsumeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return string.Equals(request.PolicyRevision, Revision, StringComparison.Ordinal) &&
            request.Scopes.All(scope => scope.Definition == GetDefinition(request.Operation, scope.ScopeType));
    }

    public static AuthenticationQuotaPolicyDescriptor AcademicInitial { get; } = new("academic-initial",
    [
        new(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, new(3, TimeSpan.FromMinutes(15))),
        new(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin, new(20, TimeSpan.FromMinutes(15))),
        new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail, new(5, TimeSpan.FromMinutes(1))),
        new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, new(10, TimeSpan.FromMinutes(1))),
        new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Origin, new(60, TimeSpan.FromMinutes(1))),
        new(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, new(30, TimeSpan.FromMinutes(1))),
        new(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, new(300, TimeSpan.FromMinutes(1)))
    ]);
}

public enum AuthenticationQuotaPolicyTransitionStep
{
    PrepareNewDescriptor,
    StopNewAdmissionsAcrossInstances,
    ResolveInFlightOperations,
    EstablishLastPossibleConsumption,
    PreservePreviousDescriptorAndState,
    DrainFromLastPossibleConsumption,
    ActivateNewRevisionAcrossInstances,
    ReopenOnlyWithVerifiableContinuity
}

/// <summary>
/// Contrato de drenaje, no coordinador runtime. Una sola revisión admite; no hay fallback ni lectura dual.
/// Esperar mientras siguen entrando consumos no drena. Operaciones inciertas sin resolver impiden reabrir.
/// No borra estado ni cambia identidades de partición. La prueba del cese/tiempo compartido queda para 021/054.
/// </summary>
public sealed class AuthenticationQuotaPolicyTransition
{
    public AuthenticationQuotaPolicyTransition(AuthenticationQuotaPolicyDescriptor previous, AuthenticationQuotaPolicyDescriptor next)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(next);
        if (string.Equals(previous.Revision, next.Revision, StringComparison.Ordinal))
            throw new ArgumentException("Una revisión identifica un descriptor inmutable.", nameof(next));
        Previous = previous;
        Next = next;
        DrainWindow = previous.MaximumWindow > next.MaximumWindow ? previous.MaximumWindow : next.MaximumWindow;
    }

    public AuthenticationQuotaPolicyDescriptor Previous { get; }
    public AuthenticationQuotaPolicyDescriptor Next { get; }
    public TimeSpan DrainWindow { get; }
    public static IReadOnlyList<AuthenticationQuotaPolicyTransitionStep> RequiredSteps { get; } =
        Array.AsReadOnly(Enum.GetValues<AuthenticationQuotaPolicyTransitionStep>());
}

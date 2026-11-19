using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>
/// Decide si una operación que depende del estado compartido de seguridad puede continuar.
/// </summary>
public interface IAuthenticationSecurityState
{
    ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget);
}

/// <summary>Resultado conservador de la comprobación, sin semántica HTTP.</summary>
public enum AuthenticationSecurityAdmission
{
    Verifiable,
    Unavailable,
    Uncertain
}

/// <summary>No admitir equivale a que el llamador deberá aplicar su traducción operativa segura.</summary>
public readonly record struct AuthenticationSecurityDecision(AuthenticationSecurityAdmission Admission, BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool CanContinue => Admission == AuthenticationSecurityAdmission.Verifiable;
    public bool ResultUncertain => Admission == AuthenticationSecurityAdmission.Uncertain;
}

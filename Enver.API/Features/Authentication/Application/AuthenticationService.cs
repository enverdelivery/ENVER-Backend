using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application;

/// <summary>Un intento, una cuota conjunta, una inserción; sin lectura previa, retry, token o compensación.</summary>
public sealed class AuthenticationService(IRegisterAdmission admission, IRegisterQuota quotas,
    InstitutionalEmailNormalizer email, AuthenticationInputValidator validator, IPasswordService passwords,
    IUserStore users, IAuthenticationEvents events)
{
    public async ValueTask<RegistrationResult> RegisterAsync(RegisterInput input, AuthenticationCanonicalOrigin? origin, RequestBudget budget)
    {
        var security = await admission.CheckAsync(budget).ConfigureAwait(false);
        events.Record(AuthenticationEventKind.SecurityAdmission, security.CanContinue && origin is not null
            ? AuthenticationEventOutcome.Admitted : AuthenticationEventOutcome.Unavailable);
        if (!security.CanContinue || origin is null || budget.Cause != BudgetCancellationCause.None)
            return Finish(new(RegistrationStatus.Unavailable));
        if (!input.ReadableBody) return Finish(new(RegistrationStatus.ValidationFailed, invalidFields: [AuthenticationValidationField.Body]));

        // D012: identidad primero. Nombre/password se resuelven SOLO después de cuota admitida.
        if (!email.TryNormalize(input.Email, out var normalized))
            return Finish(new(RegistrationStatus.ValidationFailed, invalidFields: [AuthenticationValidationField.Email]));
        var quota = await quotas.ConsumeAsync(normalized!, origin, budget).ConfigureAwait(false);
        events.Record(AuthenticationEventKind.Quota, quota.Status switch
        {
            AuthenticationQuotaStoreStatus.Admitted => AuthenticationEventOutcome.Admitted,
            AuthenticationQuotaStoreStatus.Rejected => AuthenticationEventOutcome.Rejected,
            AuthenticationQuotaStoreStatus.Uncertain => AuthenticationEventOutcome.Uncertain,
            _ => AuthenticationEventOutcome.Unavailable
        });
        if (quota.Status == AuthenticationQuotaStoreStatus.Rejected)
            return Finish(new(RegistrationStatus.RateLimited, retryAfter: quota.RetryAfter));
        if (quota.Status != AuthenticationQuotaStoreStatus.Admitted || budget.Cause != BudgetCancellationCause.None)
            return Finish(new(RegistrationStatus.Unavailable));
        var assessment = validator.AssessCreation(input.FullName, input.Email, input.Password);
        if (assessment.InvalidFields.Count > 0)
            return Finish(new(RegistrationStatus.ValidationFailed, invalidFields: assessment.InvalidFields));
        _ = validator.TryNormalizeFullName(input.FullName, out var name);

        // El hasher requiere contexto User. Identidad/instante pertenecen al backend y se reutilizan;
        // el User enviado al store se construye únicamente después de obtener el hash.
        var candidate = User.Create(name!, normalized!, string.Empty, DateTime.UtcNow);
        var hashed = passwords.Hash(candidate, input.Password!, budget);
        events.Record(AuthenticationEventKind.Hashing, hashed.Status == PasswordHashStatus.Completed
            ? AuthenticationEventOutcome.Admitted : AuthenticationEventOutcome.Saturated);
        if (hashed.Status != PasswordHashStatus.Completed || hashed.Hash is null || budget.Cause != BudgetCancellationCause.None)
            return Finish(new(RegistrationStatus.Unavailable));
        var user = new User(candidate.Id, name!, normalized!, hashed.Hash, candidate.Status, candidate.CreatedAt, candidate.UpdatedAt);
        var inserted = await users.InsertAsync(user, budget).ConfigureAwait(false);
        var outcome = inserted.Status switch
        {
            UserInsertStatus.Confirmed => AuthenticationEventOutcome.Confirmed,
            UserInsertStatus.Conflict => AuthenticationEventOutcome.Conflict,
            UserInsertStatus.Uncertain => AuthenticationEventOutcome.Uncertain,
            _ => AuthenticationEventOutcome.Unavailable
        };
        events.Record(AuthenticationEventKind.Persistence, outcome);
        events.Record(AuthenticationEventKind.Dependency, outcome);
        return Finish(inserted.Status switch
        {
            UserInsertStatus.Confirmed => new(RegistrationStatus.Confirmed, new(user.Id, user.FullName, user.NormalizedEmail)),
            UserInsertStatus.Conflict => new(RegistrationStatus.Conflict),
            _ => new(RegistrationStatus.Unavailable)
        });
    }

    private RegistrationResult Finish(RegistrationResult result)
    {
        events.Record(AuthenticationEventKind.Register, result.Status == RegistrationStatus.Confirmed
            ? AuthenticationEventOutcome.Admitted : AuthenticationEventOutcome.Rejected);
        return result;
    }
}

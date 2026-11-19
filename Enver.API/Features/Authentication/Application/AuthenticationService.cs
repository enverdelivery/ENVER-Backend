using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application;

/// <summary>Casos de uso de autenticación. Cuotas conjuntas; sin retries de intentos ni compensación.</summary>
public sealed class AuthenticationService(IRegisterAdmission admission, IRegisterQuota quotas,
    InstitutionalEmailNormalizer email, AuthenticationInputValidator validator, IPasswordService passwords,
    IUserStore users, IAuthenticationEvents events, ILogoutControls logout, ITokenRevocationStore revocations, ILoginControls login,
    ILoginTokenIssuer tokens)
{
    public async ValueTask<LoginResult> LoginAsync(LoginInput input, AuthenticationCanonicalOrigin? origin, RequestBudget budget)
    {
        LoginResult? result = null;
        try { result = await LoginCoreAsync(input, origin, budget).ConfigureAwait(false); return result; }
        finally
        {
            events.Record(AuthenticationEventKind.Login, result?.Status switch
            {
                LoginStatus.Issued => AuthenticationEventOutcome.Success,
                LoginStatus.ValidationFailed or LoginStatus.AuthenticationFailed => AuthenticationEventOutcome.Rejected,
                LoginStatus.RateLimited => AuthenticationEventOutcome.RateLimited,
                _ => result?.Uncertain == true ? AuthenticationEventOutcome.Uncertain : AuthenticationEventOutcome.Unavailable
            });
        }
    }

    private async ValueTask<LoginResult> LoginCoreAsync(LoginInput input, AuthenticationCanonicalOrigin? origin, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (budget.Cause != BudgetCancellationCause.None) return new(LoginStatus.Unavailable);
        var security = await login.CheckAsync(budget).ConfigureAwait(false);
        if (!security.CanContinue || origin is null || budget.Cause != BudgetCancellationCause.None)
            return new(LoginStatus.Unavailable, uncertain: security.Admission == AuthenticationSecurityAdmission.Uncertain);
        if (!input.ReadableBody) return new(LoginStatus.ValidationFailed, invalidFields: [AuthenticationValidationField.Body]);
        if (!email.TryNormalize(input.Email, out var normalized))
            return new(LoginStatus.ValidationFailed, invalidFields: [AuthenticationValidationField.Email]);
        var quota = await login.ConsumeAsync(normalized!, origin, budget).ConfigureAwait(false);
        if (budget.Cause != BudgetCancellationCause.None) return new(LoginStatus.Unavailable);
        if (quota.Status == AuthenticationQuotaStoreStatus.Rejected) return new(LoginStatus.RateLimited, retryAfter: quota.RetryAfter);
        if (quota.Status != AuthenticationQuotaStoreStatus.Admitted)
            return new(LoginStatus.Unavailable, uncertain: quota.Status == AuthenticationQuotaStoreStatus.Uncertain);
        var invalid = input.InvalidFields.ToList();
        if (!validator.IsValidLoginSecret(input.Password)) invalid.Add(AuthenticationValidationField.Password);
        if (invalid.Count > 0) return new(LoginStatus.ValidationFailed, invalidFields: invalid.Distinct().ToArray());
        var read = await users.FindByNormalizedEmailAsync(normalized!, budget).ConfigureAwait(false);
        if (budget.Cause != BudgetCancellationCause.None || read.Status == UserReadStatus.Unavailable) return new(LoginStatus.Unavailable);
        if (read.Status == UserReadStatus.NotFound)
        {
            var dummy = passwords.VerifyDummy(input.Password!, budget);
            return new(dummy == PasswordVerificationStatus.Unavailable || budget.Cause != BudgetCancellationCause.None
                ? LoginStatus.Unavailable : LoginStatus.AuthenticationFailed);
        }
        var check = passwords.Verify(read.User!, input.Password!, budget);
        if (budget.Cause != BudgetCancellationCause.None || check == PasswordVerificationStatus.Unavailable) return new(LoginStatus.Unavailable);
        if (read.User!.Status == UserStatus.Disabled || check == PasswordVerificationStatus.Failed) return new(LoginStatus.AuthenticationFailed);
        if (check == PasswordVerificationStatus.SuccessRehashNeeded)
        {
            var rehash = await new PasswordRehashCoordinator(users, passwords).ResolveAsync(read.User, check, input.Password!, budget).ConfigureAwait(false);
            if (rehash.Updated) events.Record(AuthenticationEventKind.Rehash, AuthenticationEventOutcome.RehashUpdated);
            if (rehash.RaceLost) events.Record(AuthenticationEventKind.Rehash, AuthenticationEventOutcome.RehashRaceLost);
            if (rehash.Status != PasswordRehashStatus.Verified)
                return new(rehash.Status == PasswordRehashStatus.Rejected ? LoginStatus.AuthenticationFailed : LoginStatus.Unavailable, uncertain: rehash.Uncertain);
        }
        if (budget.Cause != BudgetCancellationCause.None) return new(LoginStatus.Unavailable);
        var issued = await tokens.IssueAsync(read.User.Id, budget).ConfigureAwait(false);
        return issued?.Status == AccessTokenIssueStatus.Issued && issued.AccessToken is not null && budget.Cause == BudgetCancellationCause.None
            ? new(LoginStatus.Issued, accessToken: issued.AccessToken) : new(LoginStatus.Unavailable);
    }

    public async ValueTask<LogoutResult> LogoutAsync(LogoutInput input, AuthenticationCanonicalOrigin? origin,
        bool emptyBody, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (budget.Cause != BudgetCancellationCause.None) return FinishLogout(new(LogoutStatus.Unavailable));
        var security = await logout.CheckAsync(budget).ConfigureAwait(false);
        if (!security.CanContinue || origin is null || budget.Cause != BudgetCancellationCause.None)
            return FinishLogout(new(LogoutStatus.Unavailable), security.Admission == AuthenticationSecurityAdmission.Uncertain);
        var quota = await logout.ConsumeAsync(input.Account, origin, budget).ConfigureAwait(false);
        if (budget.Cause != BudgetCancellationCause.None) return FinishLogout(new(LogoutStatus.Unavailable));
        if (quota.Status == AuthenticationQuotaStoreStatus.Rejected) return FinishLogout(new(LogoutStatus.RateLimited, quota.RetryAfter));
        if (quota.Status != AuthenticationQuotaStoreStatus.Admitted)
            return FinishLogout(new(LogoutStatus.Unavailable), quota.Status == AuthenticationQuotaStoreStatus.Uncertain);
        // D012: el defecto de cuerpo no evita el cargo conjunto con identidad autenticada.
        if (!emptyBody) return FinishLogout(new(LogoutStatus.ValidationFailed));
        var result = await revocations.RevokeAsync(new(input.Issuer, input.Jti, input.ExpiresAtUtc,
            logout.EffectiveClockSkew, logout.OperationalMargin), budget).ConfigureAwait(false);
        return FinishLogout(new(result.Status == TokenRevocationWriteStatus.Confirmed && budget.Cause == BudgetCancellationCause.None
            ? LogoutStatus.Confirmed : LogoutStatus.Unavailable), result.Status == TokenRevocationWriteStatus.Uncertain);
    }

    private LogoutResult FinishLogout(LogoutResult result, bool uncertain = false)
    {
        events.Record(AuthenticationEventKind.Logout, result.Status switch
        {
            LogoutStatus.Confirmed => AuthenticationEventOutcome.Confirmed,
            LogoutStatus.ValidationFailed => AuthenticationEventOutcome.Rejected,
            LogoutStatus.RateLimited => AuthenticationEventOutcome.RateLimited,
            _ => uncertain ? AuthenticationEventOutcome.Uncertain : AuthenticationEventOutcome.Unavailable
        });
        return result;
    }

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

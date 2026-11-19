using Enver.API.Features.Authentication.Validation;

namespace Enver.API.Features.Authentication.Application;

public enum RegistrationStatus { Confirmed, ValidationFailed, Conflict, RateLimited, Unavailable }

public enum LoginStatus { Issued, ValidationFailed, AuthenticationFailed, RateLimited, Unavailable }

/// <summary>Resultado interno; nunca serializar automáticamente ni registrar el access token.</summary>
public sealed class LoginResult
{
    internal LoginResult(LoginStatus status, string? accessToken = null,
        IReadOnlyList<AuthenticationValidationField>? invalidFields = null, TimeSpan? retryAfter = null, bool uncertain = false)
    { Status = status; AccessToken = accessToken; InvalidFields = invalidFields?.ToArray() ?? []; RetryAfter = retryAfter; Uncertain = uncertain; }
    public LoginStatus Status { get; }
    public string? AccessToken { get; }
    public IReadOnlyList<AuthenticationValidationField> InvalidFields { get; }
    public TimeSpan? RetryAfter { get; }
    public bool Uncertain { get; }
    public override string ToString() => nameof(LoginResult);
}

public enum LogoutStatus { Confirmed, ValidationFailed, RateLimited, Unavailable }

public sealed class LogoutResult
{
    internal LogoutResult(LogoutStatus status, TimeSpan? retryAfter = null) { Status = status; RetryAfter = retryAfter; }
    public LogoutStatus Status { get; }
    public TimeSpan? RetryAfter { get; }
    public override string ToString() => $"LogoutResult: {Status}";
}

public sealed class RegisteredIdentity(Guid id, string fullName, string email)
{
    public Guid Id { get; } = id;
    public string FullName { get; } = fullName;
    public string Email { get; } = email;
    public override string ToString() => nameof(RegisteredIdentity);
}

public sealed class RegistrationResult
{
    internal RegistrationResult(RegistrationStatus status, RegisteredIdentity? identity = null,
        IReadOnlyCollection<AuthenticationValidationField>? invalidFields = null, TimeSpan? retryAfter = null)
    {
        Status = status; Identity = identity; InvalidFields = invalidFields ?? []; RetryAfter = retryAfter;
    }
    public RegistrationStatus Status { get; }
    public RegisteredIdentity? Identity { get; }
    public IReadOnlyCollection<AuthenticationValidationField> InvalidFields { get; }
    public TimeSpan? RetryAfter { get; }
    public override string ToString() => $"RegistrationResult: {Status}";
}

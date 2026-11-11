using Enver.API.Features.Authentication.Validation;

namespace Enver.API.Features.Authentication.Application;

public enum RegistrationStatus { Confirmed, ValidationFailed, Conflict, RateLimited, Unavailable }

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

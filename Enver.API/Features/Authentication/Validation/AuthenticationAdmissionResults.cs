namespace Enver.API.Features.Authentication.Validation;

public enum AuthenticationValidationField { FullName, Email, Password, Body, Authorization }
public sealed record AuthenticationInputAssessment(bool HasQuotaIdentity, IReadOnlyCollection<AuthenticationValidationField> InvalidFields);
public enum AuthenticationQuotaStatus { Admitted, Rejected, Unavailable }
public sealed record AuthenticationQuotaResult(AuthenticationQuotaStatus Status, TimeSpan? RetryAfter = null);
public sealed record AuthenticationCoordinationResult(int? HttpStatus, IReadOnlyCollection<AuthenticationValidationField> InvalidFields, TimeSpan? RetryAfter = null, bool ClientCanceled = false);

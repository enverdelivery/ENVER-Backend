using Enver.API.Features.Authentication.Core;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

public enum PasswordHashStatus { Completed, InvalidInput, Unavailable }
public enum PasswordVerificationStatus { Failed, Success, SuccessRehashNeeded, Unavailable }

/// <summary>Resultado interno: jamás serializar el hash ni el objeto como respuesta/evento.</summary>
public sealed class PasswordHashResult
{
    internal PasswordHashResult(PasswordHashStatus status, string? hash = null) { Status = status; Hash = hash; }
    public PasswordHashStatus Status { get; }
    public string? Hash { get; }
    public override string ToString() => nameof(PasswordHashResult);
}

public interface IPasswordService
{
    PasswordHashResult Hash(User user, string secret, RequestBudget budget);
    PasswordVerificationStatus Verify(User user, string secret, RequestBudget budget);
    PasswordVerificationStatus VerifyDummy(string secret, RequestBudget budget);
}

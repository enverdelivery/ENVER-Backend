namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>Primitivo de emisión; el llamador debe satisfacer antes los controles de aplicación.</summary>
public interface IAccessTokenIssuer
{
    AccessTokenIssueResult Issue(Guid subject);
}

public enum AccessTokenIssueStatus { Issued, Unavailable }

/// <summary>No serializar ni registrar este resultado: contiene un secreto solo en éxito.</summary>
public sealed class AccessTokenIssueResult
{
    public AccessTokenIssueStatus Status { get; }
    public string? AccessToken { get; }
    public DateTimeOffset? IssuedAtUtc { get; }
    public DateTimeOffset? ExpiresAtUtc { get; }
    private AccessTokenIssueResult(AccessTokenIssueStatus status, string? token = null,
        DateTimeOffset? issued = null, DateTimeOffset? expires = null)
    { Status = status; AccessToken = token; IssuedAtUtc = issued; ExpiresAtUtc = expires; }
    internal static AccessTokenIssueResult Success(string token, DateTimeOffset issued, DateTimeOffset expires) =>
        new(AccessTokenIssueStatus.Issued, token, issued, expires);
    internal static AccessTokenIssueResult Unavailable() => new(AccessTokenIssueStatus.Unavailable);
    public override string ToString() => nameof(AccessTokenIssueResult);
}

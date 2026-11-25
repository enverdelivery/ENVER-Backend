namespace Enver.API.Features.Users.Core;

public enum ActiveRole { Solicitante, Rescatista }

public sealed record UserProfile(string? FullName, string? Phone, ActiveRole ActiveRole);

// A present phone field can explicitly clear the value. Mode is a separate operation.
public sealed record ProfileChange(string? FullName, bool HasPhone, string? Phone, ActiveRole? ActiveRole = null);

namespace Enver.API.Features.Authentication.Core;

/// <summary>Modelo mínimo interno; no es un contrato HTTP ni contiene validación de registro.</summary>
public sealed class User
{
    public User(Guid id, string fullName, string normalizedEmail, string passwordHash,
        UserStatus status, DateTime createdAt, DateTime updatedAt)
    {
        if (id.Version != 4 || (id.ToByteArray()[8] & 0xc0) != 0x80)
        {
            throw new ArgumentException("Identidad UUIDv4 requerida.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(normalizedEmail);
        ArgumentNullException.ThrowIfNull(passwordHash);
        if (status is not (UserStatus.Active or UserStatus.Disabled))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Estado de usuario incompatible.");
        }

        Id = id;
        FullName = fullName;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        Status = status;
        CreatedAt = MillisecondUtc(createdAt);
        UpdatedAt = MillisecondUtc(updatedAt);
    }

    public Guid Id { get; }
    public string FullName { get; }
    public string NormalizedEmail { get; }
    public string PasswordHash { get; }
    public UserStatus Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    /// <summary>Identidad y estado iniciales del servidor; el instante procede del reloj del backend.</summary>
    public static User Create(string fullName, string normalizedEmail, string passwordHash, DateTime utcNow) =>
        new(Guid.NewGuid(), fullName, normalizedEmail, passwordHash, UserStatus.Active, utcNow, utcNow);

    private static DateTime MillisecondUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Instante UTC requerido.", nameof(value));
        }

        return DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(value).ToUnixTimeMilliseconds()).UtcDateTime;
    }
}

namespace Enver.API.Features.Authentication.Application;

/// <summary>Datos efímeros. Nunca registrar/serializar como evento, ni conservar después de la operación.</summary>
public sealed class RegisterInput(string? fullName, string? email, string? password, bool readableBody = true)
{
    public string? FullName { get; } = fullName;
    public string? Email { get; } = email;
    public string? Password { get; } = password;
    public bool ReadableBody { get; } = readableBody;
    public override string ToString() => nameof(RegisterInput);
}

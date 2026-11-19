using System.Text.Json.Serialization;

namespace Enver.API.Features.Authentication.Contracts;

public sealed class RegisterResponse(Guid id, string fullName, string email)
{
    [JsonPropertyName("id")] public Guid Id { get; } = id;
    [JsonPropertyName("fullName")] public string FullName { get; } = fullName;
    [JsonPropertyName("email")] public string Email { get; } = email;
    public override string ToString() => nameof(RegisterResponse);
}

using System.Text.Json.Serialization;

namespace Enver.API.Features.Authentication.Contracts;

public sealed class RegisterRequest
{
    [JsonPropertyName("fullName")] public required string FullName { get; init; }
    [JsonPropertyName("email")] public required string Email { get; init; }
    [JsonPropertyName("password")] public required string Password { get; init; }
    public override string ToString() => nameof(RegisterRequest);
}

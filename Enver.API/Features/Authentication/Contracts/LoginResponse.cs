using System.Text.Json.Serialization;

namespace Enver.API.Features.Authentication.Contracts;

public sealed class LoginResponse(string accessToken)
{
    [JsonPropertyName("accessToken")] public string AccessToken { get; } = accessToken;
    [JsonPropertyName("tokenType")] public string TokenType => "Bearer";
    [JsonPropertyName("expiresIn")] public int ExpiresIn => 900;
    public override string ToString() => nameof(LoginResponse);
}

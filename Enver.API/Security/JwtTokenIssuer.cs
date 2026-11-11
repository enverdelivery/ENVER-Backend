using System.Security.Cryptography;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Ports;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.Security;

public sealed class JwtTokenIssuer(JwtKeyRing keyRing, TimeProvider clock) : IAccessTokenIssuer
{
    public AccessTokenIssueResult Issue(Guid subject)
    {
        if (!JwtClaimsValidator.IsCanonicalSubject(subject.ToString("D")) || !keyRing.TryGetSigningCredentials(out var signing))
            return AccessTokenIssueResult.Unavailable();
        try
        {
            var issued = clock.GetUtcNow().ToUnixTimeSeconds();
            var expires = checked(issued + 900);
            var payload = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["sub"] = subject.ToString("D"),
                ["jti"] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)),
                ["iss"] = "enver-api", ["aud"] = "enver-mobile", ["iat"] = issued, ["exp"] = expires
            });
            var handler = new JsonWebTokenHandler { SetDefaultTimesOnTokenCreation = false };
            var token = handler.CreateToken(payload, signing);
            return AccessTokenIssueResult.Success(token, DateTimeOffset.FromUnixTimeSeconds(issued), DateTimeOffset.FromUnixTimeSeconds(expires));
        }
        catch (Exception error) when (error is CryptographicException or SecurityTokenException or ArgumentException or OverflowException)
        { return AccessTokenIssueResult.Unavailable(); }
    }
}

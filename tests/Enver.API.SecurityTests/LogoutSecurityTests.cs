using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Infrastructure.Redis;
using Enver.API.Http;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class LogoutSecurityTests(AuthenticationDependenciesFixture fixture)
{
    [Theory, Trait("Task", "046")]
    [InlineData("missing")] [InlineData("ambiguous")] [InlineData("malformed")] [InlineData("signature")]
    [InlineData("algorithm")] [InlineData("issuer")] [InlineData("audience")] [InlineData("kid")]
    [InlineData("expired")] [InlineData("claims")] [InlineData("oversized")] [InlineData("duplicate-sub")]
    public async Task Untrusted_Bearer_is_uniform_401_before_read_quota_or_write(string defect)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var subject = Guid.NewGuid().ToString("D"); var jti = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var p = new Dictionary<string, object> { ["sub"] = subject, ["jti"] = jti, ["iss"] = host.Jwt.Issuer,
            ["aud"] = host.Jwt.Audience, ["iat"] = now, ["exp"] = now + 900 };
        var header = new Dictionary<string, object> { ["alg"] = "HS256", ["kid"] = host.Jwt.ActiveKid! };
        switch (defect)
        {
            case "issuer": p["iss"] = "untrusted"; break;
            case "audience": p["aud"] = "untrusted"; break;
            case "kid": header["kid"] = "unknown"; break;
            case "algorithm": header["alg"] = "HS512"; break;
            case "expired": p["iat"] = now - 1000; p["exp"] = now - 100; break;
            case "claims": p["email"] = Sentinel; break;
        }
        var payload = JsonSerializer.Serialize(p);
        if (defect == "duplicate-sub") payload = payload[..^1] + ",\"sub\":\"" + subject + "\"}";
        var key = host.Jwt.Keys!.Single(k => k!.Kid == host.Jwt.ActiveKid)!.KeyBase64!;
        var compact = Base64UrlEncoder.Encode(JsonSerializer.SerializeToUtf8Bytes(header)) + "." + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payload));
        var token = compact + "." + Base64UrlEncoder.Encode(HMACSHA256.HashData(Convert.FromBase64String(key), Encoding.ASCII.GetBytes(compact)));
        if (defect == "signature") token = compact + "." + Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        if (defect == "malformed") token = "malformed";
        if (defect == "oversized") token = new string('a', 4097);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        if (defect != "missing") request.Headers.TryAddWithoutValidation("Authorization", defect == "ambiguous"
            ? new[] { "Bearer " + token, "Bearer " + token } : new[] { "Bearer " + token });
        using var response = await host.Client.SendAsync(request);
        await SafeProblemAsync(response, 401, "authentication_failed", token, subject, jti, key);
        Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Revocations.Writes); Assert.Equal(0, host.Quotas.Calls);
        SafeLogs(scope, token, subject, jti, key);
    }

    [Theory, Trait("Task", "046")]
    [InlineData("query")] [InlineData("cookie")] [InlineData("body")] [InlineData("custom-header")]
    public async Task Token_outside_Authorization_is_never_an_authentication_source(string source)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout" + (source == "query" ? "?access_token=" + token : ""));
        if (source == "cookie") request.Headers.TryAddWithoutValidation("Cookie", "access_token=" + token);
        if (source == "custom-header") request.Headers.TryAddWithoutValidation("X-Access-Token", token);
        if (source == "body") request.Content = new StringContent(JsonSerializer.Serialize(new { access_token = token }));
        using var response = await host.Client.SendAsync(request); await SafeProblemAsync(response, 401, "authentication_failed", token);
        Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes); SafeLogs(scope, token);
    }

    [Theory, Trait("Task", "046")]
    [InlineData("{}")] [InlineData(" \r\n\t")]
    [InlineData("{\"sub\":{\"$ne\":null},\"jti\":\"attacker\",\"password\":\"logout-sensitive-sentinel\"}")]
    [InlineData("<script>logout-sensitive-sentinel</script>")] [InlineData("invalid-utf8")]
    public async Task Every_nonempty_body_is_rejected_after_one_charge_without_parsing_or_revocation(string body)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var content = new ByteArrayContent(body == "invalid-utf8" ? [0xff, 0xfe] : Encoding.UTF8.GetBytes(body));
        using var response = await host.SendAsync(token, body: content); await SafeProblemAsync(response, 400, "validation_failed", token, Sentinel);
        Assert.Equal(1, host.Revocations.Reads); Assert.Equal(1, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
        SafeLogs(scope, token, Sentinel); Assert.Contains("Authentication logout rejected", scope.Logs.Messages);
    }

    [Fact, Trait("Task", "046")]
    public async Task More_than_16KiB_is_413_even_before_missing_Bearer()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        using var response = await host.SendAsync(null, body: new ByteArrayContent(new byte[16385]));
        await SafeProblemAsync(response, 413, "payload_too_large");
        Assert.Equal(0, host.Revocations.Reads); Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
    }

    [Fact, Trait("Task", "046")]
    public async Task Revoked_JWT_cannot_reuse_Logout_policy_on_another_route_or_method()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var subject = Guid.NewGuid(); var token = host.Issuer.Issue(subject).AccessToken!; var input = await host.InputAsync(token); scope.Track(input);
        using (var first = await host.SendAsync(token))
        {
            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode); Assert.Empty(await first.Content.ReadAsByteArrayAsync());
            Assert.Null(first.Content.Headers.ContentType); Assert.Equal("no-store", first.Headers.CacheControl?.ToString());
        }
        foreach (var (path, method) in new[] { ("/_logout/protected", HttpMethod.Get), ("/_logout/copied", HttpMethod.Post), ("/api/v1/auth/logout", HttpMethod.Get) })
        {
            using var rejected = await host.SendAsync(token, path, method); await SafeProblemAsync(rejected, 401, "authentication_failed", token, input.Jti, subject.ToString("D"));
        }
        Assert.Equal(1, host.Revocations.Writes); Assert.Equal(1, host.Quotas.Calls);
        SafeLogs(scope, token, input.Jti, subject.ToString("D"), scope.MarkKey(input));
    }

    [Theory, Trait("Task", "046")]
    [InlineData(TokenRevocationReadStatus.Unavailable)] [InlineData(TokenRevocationReadStatus.Uncertain)]
    public async Task Unverifiable_dependency_is_503_not_an_authentication_challenge(TokenRevocationReadStatus status)
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First; host.Revocations.ReadOutcome = status;
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var response = await host.SendAsync(token, body: new StringContent("{}")); await SafeProblemAsync(response, 503, "temporarily_unavailable", token);
        Assert.Equal(0, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes); SafeLogs(scope, token);
    }

    [Fact, Trait("Task", "046")]
    public async Task Quota_abuse_cannot_evade_account_by_changing_jti_or_putting_account_in_body()
    {
        await using var scope = await LogoutTestScope.CreateAsync(fixture); var host = scope.First;
        var subject = Guid.NewGuid();
        for (var i = 0; i < 30; i++)
        {
            var token = host.Issuer.Issue(subject).AccessToken!;
            using var response = await host.SendAsync(token, body: new StringContent("{}")); await SafeProblemAsync(response, 400, "validation_failed", token);
        }
        var last = host.Issuer.Issue(subject).AccessToken!;
        using var limited = await host.SendAsync(last, body: new StringContent("{\"sub\":\"different-account\"}"));
        await SafeProblemAsync(limited, 429, "rate_limited", last, subject.ToString("D")); Assert.InRange(limited.Headers.RetryAfter!.Delta!.Value.TotalSeconds, 1, 60);
        Assert.Equal(31, host.Quotas.Calls); Assert.Equal(0, host.Revocations.Writes);
        SafeLogs(scope, last, subject.ToString("D")); Assert.Contains("Authentication logout rate_limited", scope.Logs.Messages);
    }

    private const string Sentinel = "logout-sensitive-sentinel";
    private static void SafeLogs(LogoutTestScope scope, params string[] sensitive)
    {
        var messages = string.Join('\n', scope.Logs.Messages);
        foreach (var secret in sensitive.Append("127.0.0.1").Append("Authorization").Append(Sentinel)) Assert.DoesNotContain(secret, messages);
        Assert.All(scope.Logs.Messages, message => Assert.Contains(message, new[] { "Authentication logout confirmed", "Authentication logout rejected",
            "Authentication logout rate_limited", "Authentication logout dependency_unavailable", "Authentication logout uncertain" }));
    }
    private static async Task SafeProblemAsync(HttpResponseMessage response, int status, string code, params string[] sensitive)
    {
        Assert.Equal(status, (int)response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        if (status == 401) Assert.Equal("Bearer", Assert.Single(response.Headers.GetValues("WWW-Authenticate")));
        else Assert.False(response.Headers.Contains("WWW-Authenticate"));
        var body = await response.Content.ReadAsStringAsync(); using var p = JsonDocument.Parse(body);
        Assert.Equal(status, p.RootElement.GetProperty("status").GetInt32()); Assert.Equal(code, p.RootElement.GetProperty("code").GetString());
        Assert.Equal("about:blank", p.RootElement.GetProperty("type").GetString());
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), p.RootElement.GetProperty("traceId").GetString());
        if (status == 401) Assert.Equal("No fue posible autenticar la solicitud.", p.RootElement.GetProperty("detail").GetString());
        foreach (var secret in sensitive.Append("error_description").Append("error_uri").Append("error=").Append("IDX").Append("StackTrace")) Assert.DoesNotContain(secret, body);
    }
}

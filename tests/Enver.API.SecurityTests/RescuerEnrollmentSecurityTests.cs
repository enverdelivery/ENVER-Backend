using System.Net;
using System.Text;
using System.Text.Json;
using Enver.API.Controllers;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Application;
using Enver.API.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Fact]
    public async Task Rescuer_enrollment_rejects_untrusted_input_and_uses_only_validated_bearer_subject()
    {
        var service = new EnrollmentProbe();
        using var host = new Harness(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);
            // Do not run Profile for this route. The real profile/auth-store integration is tested separately.
            services.AddScoped(_ => new UserProfileService(null!, null!, null!));
            services.AddSingleton<InstitutionalEmailNormalizer>();
            services.AddSingleton<PasswordPolicy>();
            services.AddSingleton<AuthenticationInputValidator>();
            services.AddSingleton<IRescuerEnrollment>(service);
        });
        const string path = "/api/v1/users/me/rescuer-enrollment";
        const string body = """{"acceptedRules":true,"rulesVersion":"test.rules"}""";
        var subject = Guid.NewGuid();
        var token = host.Issuer.Issue(subject).AccessToken;

        using (var noBearer = await Send(null, body)) await host.AssertProblemAsync(noBearer, 401, "");
        using (var invalid = await host.SendAsync("invalid", path, HttpMethod.Post))
            await host.AssertProblemAsync(invalid, 401, "invalid");
        using (var absent = await host.SendAsync(token, path, HttpMethod.Post))
            await AssertProblem(absent, 400, "validation_failed");
        using (var emptyJson = await Send(token, ""))
            await AssertProblem(emptyJson, 400, "validation_failed");
        using (var invalidJson = await Send(token, "{"))
            await AssertProblem(invalidJson, 400, "validation_failed");
        foreach (var field in new[] { "userId", "rescuerId", "rescuerEnabled", "status", "permissions", "decisionBy", "phone", "unknown" })
        {
            using var response = await Send(token, body.Insert(1, $"\"{field}\":null,"));
            await AssertProblem(response, 400, "validation_failed");
        }
        using (var duplicate = await Send(token, body.Insert(1, "\"acceptedRules\":true,")))
            await AssertProblem(duplicate, 400, "validation_failed");
        using (var query = await Send(token, body, suffix: "?userId=other"))
            await AssertProblem(query, 400, "validation_failed");
        using (var large = await Send(token, body + new string(' ', 4096)))
            await AssertProblem(large, 413, "payload_too_large");
        using (var wrongType = await Send(token, body, type: "text/plain"))
            await AssertProblem(wrongType, 415, "unsupported_media_type");

        host.Store.Status = TokenRevocationReadStatus.Revoked;
        using (var revoked = await Send(token, body)) await host.AssertProblemAsync(revoked, 401, token!);
        Assert.Empty(service.Actors);
        host.Store.Status = TokenRevocationReadStatus.NotRevoked;
        using var allowed = await Send(token, body);
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        Assert.Equal("no-store", allowed.Headers.CacheControl?.ToString());
        Assert.Equal(subject, Assert.Single(service.Actors));
        host.AssertPrivate(token!);

        Task<HttpResponseMessage> Send(string? bearer, string json, string suffix = "", string type = "application/json") =>
            host.SendAsync(bearer, path + suffix, HttpMethod.Post, new StringContent(json, Encoding.UTF8, type));

        static async Task AssertProblem(HttpResponseMessage response, int status, string code)
        {
            Assert.Equal(status, (int)response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(code, json.RootElement.GetProperty("code").GetString());
            Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), json.RootElement.GetProperty("traceId").GetString());
        }
    }

    private sealed class EnrollmentProbe : IRescuerEnrollment
    {
        internal List<Guid> Actors { get; } = [];
        public Task<EnrollmentResult> ExecuteAsync(Guid id, string rulesVersion, RequestBudget budget)
        {
            Actors.Add(id);
            return Task.FromResult(new EnrollmentResult(EnrollmentOutcome.Enabled));
        }
    }
}

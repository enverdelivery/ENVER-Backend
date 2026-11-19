using System.Security.Claims;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.IntegrationTests;

public sealed class PreparedCompositionTests
{
    [Fact]
    public async Task Safe_problem_details_composition_writes_a_sanitized_503_without_starting_a_host()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        ApiProblemDetails.AddSafeApiErrors(services);
        await using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Response.Body = new MemoryStream();

        var written = await ApiProblemDetails.TryWriteAsync(context, 503);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.True(written);
        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Contains("temporarily_unavailable", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task D012_consumes_joint_quota_only_once_for_the_same_request()
    {
        var quota = new CountingQuotaAdmission();
        var coordinator = new AuthenticationValidationCoordinator(new VerifiableAdmission(), new ValidIdentityAssessment(), quota);
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity("synthetic"));
        var boundary = new AuthenticationBoundaryResult(AuthenticationOperation.Login, 400, [ApiErrorField.Password], true);

        var results = await Task.WhenAll(
            coordinator.CoordinateAsync(context, boundary),
            coordinator.CoordinateAsync(context, boundary));

        Assert.All(results, result =>
        {
            Assert.Equal(400, result.HttpStatus);
            Assert.Contains(AuthenticationValidationField.Password, result.InvalidFields);
        });
        Assert.Equal(1, quota.Calls);
    }

    private sealed class VerifiableAdmission : IAuthenticationAdmissionChecks
    {
        public ValueTask<bool> IsVerifiableAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => ValueTask.FromResult(true);
    }

    private sealed class ValidIdentityAssessment : IAuthenticationInputAssessment
    {
        public ValueTask<AuthenticationInputAssessment> AssessAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => ValueTask.FromResult(new AuthenticationInputAssessment(true, []));
    }

    private sealed class CountingQuotaAdmission : IAuthenticationQuotaAdmission
    {
        public int Calls { get; private set; }
        public async ValueTask<AuthenticationQuotaResult> EvaluateAndConsumeAsync(HttpContext context, AuthenticationOperation operation, string internalAttemptId, CancellationToken token)
        {
            Calls++;
            await Task.Yield();
            return new(AuthenticationQuotaStatus.Admitted);
        }
    }
}

using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.IntegrationTests;

public sealed class AuthenticationValidationCompositionTests
{
    [Fact]
    public async Task Unverifiable_preflight_returns_503_before_a_boundary_400_and_never_consumes_quota()
    {
        var admission = new ControlledAdmission(false);
        var quota = new ControlledQuota(AuthenticationQuotaStatus.Admitted);
        var coordinator = Coordinator(admission, new ControlledInputs(true), quota);

        var result = await coordinator.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: true));

        Assert.Equal(503, result.HttpStatus);
        Assert.Equal(0, quota.Calls);
    }

    [Fact]
    public async Task Unreadable_json_and_invalid_identity_never_create_quota_buckets()
    {
        var unreadableQuota = new ControlledQuota(AuthenticationQuotaStatus.Admitted);
        var invalidQuota = new ControlledQuota(AuthenticationQuotaStatus.Admitted);
        var unreadable = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), unreadableQuota);
        var invalid = Coordinator(new ControlledAdmission(true), new ControlledInputs(false, [AuthenticationValidationField.Email]), invalidQuota);

        var unreadableResult = await unreadable.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: false));
        var invalidResult = await invalid.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: true));

        Assert.Equal(400, unreadableResult.HttpStatus);
        Assert.Equal(400, invalidResult.HttpStatus);
        Assert.Equal(0, unreadableQuota.Calls);
        Assert.Equal(0, invalidQuota.Calls);
    }

    [Fact]
    public async Task Rejected_and_unavailable_quota_results_preserve_d012_statuses_without_retry()
    {
        var rejectedQuota = new ControlledQuota(AuthenticationQuotaStatus.Rejected, TimeSpan.FromSeconds(2));
        var unavailableQuota = new ControlledQuota(AuthenticationQuotaStatus.Unavailable);
        var rejected = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), rejectedQuota);
        var unavailable = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), unavailableQuota);

        var rejectedResult = await rejected.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: true));
        var unavailableResult = await unavailable.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: true));

        Assert.Equal(429, rejectedResult.HttpStatus);
        Assert.Equal(TimeSpan.FromSeconds(2), rejectedResult.RetryAfter);
        Assert.Equal(503, unavailableResult.HttpStatus);
        Assert.Equal(1, rejectedQuota.Calls);
        Assert.Equal(1, unavailableQuota.Calls);
    }

    [Fact]
    public async Task Uncertain_quota_failure_is_503_and_is_not_retried()
    {
        var quota = new ThrowingQuota();
        var coordinator = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), quota);

        var result = await coordinator.CoordinateAsync(new DefaultHttpContext(), Boundary(jsonReadable: true));

        Assert.Equal(503, result.HttpStatus);
        Assert.Equal(1, quota.Calls);
    }

    [Fact]
    public async Task Concurrent_and_reentrant_coordination_consumes_once_per_request()
    {
        var quota = new ControlledQuota(AuthenticationQuotaStatus.Admitted, yieldBeforeResult: true);
        var coordinator = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), quota);
        var context = new DefaultHttpContext();
        var boundary = Boundary(jsonReadable: true);

        var results = await Task.WhenAll(
            coordinator.CoordinateAsync(context, boundary),
            coordinator.CoordinateAsync(context, boundary),
            coordinator.CoordinateAsync(context, boundary, [AuthenticationValidationField.Password]));

        Assert.All(results, result => Assert.Equal(400, result.HttpStatus));
        Assert.Equal(1, quota.Calls);
        Assert.Contains(AuthenticationValidationField.Password, results[2].InvalidFields);
    }

    [Fact]
    public async Task Logout_without_authenticated_identity_returns_401_without_quota()
    {
        var quota = new ControlledQuota(AuthenticationQuotaStatus.Admitted);
        var coordinator = Coordinator(new ControlledAdmission(true), new ControlledInputs(true), quota);
        var context = new DefaultHttpContext();
        var boundary = new AuthenticationBoundaryResult(AuthenticationOperation.Logout, null, [], false);

        var result = await coordinator.CoordinateAsync(context, boundary);

        Assert.Equal(401, result.HttpStatus);
        Assert.Equal(0, quota.Calls);
    }

    [Fact]
    public async Task Filter_waits_for_async_preflight_before_returning_the_automatic_400()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var admission = new AsyncAdmission(completion.Task);
        var quota = new ControlledQuota(AuthenticationQuotaStatus.Admitted);
        var coordinator = Coordinator(admission, new ControlledInputs(true), quota);
        var filter = new AuthenticationValidationFilter(coordinator);
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new AuthenticationRequestMetadata(AuthenticationOperation.Login)), "synthetic"));
        context.Features.Set(Boundary(jsonReadable: true));
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("password", "synthetic-value-must-not-leak");
        var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor(), modelState);
        var executing = new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
        var nextCalled = false;

        var running = filter.OnActionExecutionAsync(executing, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], new object()));
        });

        Assert.False(running.IsCompleted);
        completion.SetResult(true);
        await running;

        Assert.False(nextCalled);
        Assert.NotNull(executing.Result);
        Assert.Equal(1, quota.Calls);
    }

    private static AuthenticationValidationCoordinator Coordinator(IAuthenticationAdmissionChecks admission, IAuthenticationInputAssessment inputs, IAuthenticationQuotaAdmission quota) => new(admission, inputs, quota);

    private static AuthenticationBoundaryResult Boundary(bool jsonReadable) => new(AuthenticationOperation.Login, 400, [ApiErrorField.Password], jsonReadable);

    private sealed class ControlledAdmission(bool verifiable) : IAuthenticationAdmissionChecks
    {
        public ValueTask<bool> IsVerifiableAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => ValueTask.FromResult(verifiable);
    }

    private sealed class AsyncAdmission(Task<bool> result) : IAuthenticationAdmissionChecks
    {
        public async ValueTask<bool> IsVerifiableAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => await result.WaitAsync(token);
    }

    private sealed class ControlledInputs(bool hasIdentity, IReadOnlyCollection<AuthenticationValidationField>? fields = null) : IAuthenticationInputAssessment
    {
        public ValueTask<AuthenticationInputAssessment> AssessAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => ValueTask.FromResult(new AuthenticationInputAssessment(hasIdentity, fields ?? []));
    }

    private sealed class ControlledQuota(AuthenticationQuotaStatus status, TimeSpan? retryAfter = null, bool yieldBeforeResult = false) : IAuthenticationQuotaAdmission
    {
        public int Calls { get; private set; }
        public async ValueTask<AuthenticationQuotaResult> EvaluateAndConsumeAsync(HttpContext context, AuthenticationOperation operation, string internalAttemptId, CancellationToken token)
        {
            Calls++;
            if (yieldBeforeResult) await Task.Yield();
            return new(status, retryAfter);
        }
    }

    private sealed class ThrowingQuota : IAuthenticationQuotaAdmission
    {
        public int Calls { get; private set; }
        public ValueTask<AuthenticationQuotaResult> EvaluateAndConsumeAsync(HttpContext context, AuthenticationOperation operation, string internalAttemptId, CancellationToken token)
        {
            Calls++;
            throw new TimeoutException("synthetic uncertain outcome");
        }
    }
}

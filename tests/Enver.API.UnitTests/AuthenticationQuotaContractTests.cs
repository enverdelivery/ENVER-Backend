using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Microsoft.AspNetCore.Http;

namespace Enver.API.UnitTests;

public sealed class AuthenticationQuotaContractTests
{
    [Theory]
    [InlineData(AuthenticationQuotaStoreStatus.Admitted, false)]
    [InlineData(AuthenticationQuotaStoreStatus.Rejected, false)]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable, false)]
    [InlineData(AuthenticationQuotaStoreStatus.Uncertain, true)]
    public void Four_statuses_have_derived_uncertainty_and_exclusive_retry_after(AuthenticationQuotaStoreStatus status, bool uncertain)
    {
        var result = new AuthenticationQuotaConsumeResult(status,
            status == AuthenticationQuotaStoreStatus.Rejected ? TimeSpan.FromSeconds(1) : null);

        Assert.Equal(status, result.Status);
        Assert.Equal(uncertain, result.ResultUncertain);
        Assert.Equal(status == AuthenticationQuotaStoreStatus.Rejected, result.RetryAfter.HasValue);
        Assert.Equal(BudgetCancellationCause.None, result.CancellationCause);
        Assert.Equal(4, Enum.GetValues<AuthenticationQuotaStoreStatus>().Length);
        Assert.Null(typeof(AuthenticationQuotaConsumeResult).GetProperty(nameof(result.ResultUncertain))!.SetMethod);
    }

    [Theory]
    [InlineData(AuthenticationQuotaStoreStatus.Admitted)]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable)]
    [InlineData(AuthenticationQuotaStoreStatus.Uncertain)]
    public void Non_rejections_cannot_contain_retry_after(AuthenticationQuotaStoreStatus status) =>
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeResult(status, TimeSpan.FromSeconds(1)));

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Rejection_requires_positive_retry_after(int? seconds) =>
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeResult(AuthenticationQuotaStoreStatus.Rejected,
            seconds is null ? null : TimeSpan.FromSeconds(seconds.Value)));

    [Fact]
    public void Invalid_enums_and_contradictory_confirmed_cancellation_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaConsumeResult((AuthenticationQuotaStoreStatus)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaConsumeResult(AuthenticationQuotaStoreStatus.Unavailable,
            cancellationCause: (BudgetCancellationCause)999));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeResult(AuthenticationQuotaStoreStatus.Admitted,
            cancellationCause: BudgetCancellationCause.Client));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeResult(AuthenticationQuotaStoreStatus.Rejected,
            TimeSpan.FromSeconds(1), BudgetCancellationCause.DependencyTimeout));
    }

    [Theory]
    [InlineData(false, BudgetCancellationCause.None)]
    [InlineData(false, BudgetCancellationCause.Client)]
    [InlineData(false, BudgetCancellationCause.RequestBudget)]
    [InlineData(true, BudgetCancellationCause.DependencyTimeout)]
    [InlineData(true, BudgetCancellationCause.Client)]
    [InlineData(true, BudgetCancellationCause.None)]
    public void Missing_result_preserves_dispatch_boundary_and_cancellation(bool dispatched, BudgetCancellationCause cause)
    {
        var result = AuthenticationQuotaEvaluationContract.MissingSemanticResult(dispatched, cause);
        Assert.Equal(dispatched ? AuthenticationQuotaStoreStatus.Uncertain : AuthenticationQuotaStoreStatus.Unavailable, result.Status);
        Assert.Equal(dispatched, result.ResultUncertain);
        Assert.Equal(cause, result.CancellationCause);
        Assert.Null(result.RetryAfter);
    }

    [Theory]
    [InlineData(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, 3, 900)]
    [InlineData(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin, 20, 900)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail, 5, 60)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, 10, 60)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Origin, 60, 60)]
    [InlineData(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, 30, 60)]
    [InlineData(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, 300, 60)]
    public void Academic_initial_preserves_seven_exact_quotas(AuthenticationOperation operation, AuthenticationQuotaScopeType scope, int limit, int seconds)
    {
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        var definition = policy.GetDefinition(operation, scope);
        Assert.Equal(7, policy.Rules.Count);
        Assert.Equal("academic-initial", policy.Revision);
        Assert.Equal(limit, definition.PermitLimit);
        Assert.Equal(TimeSpan.FromSeconds(seconds), definition.Window);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Definitions_require_positive_limits_and_windows(int limit, int seconds) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaDefinition(limit, TimeSpan.FromSeconds(seconds)));

    [Theory]
    [InlineData(AuthenticationOperation.Register, 2)]
    [InlineData(AuthenticationOperation.Login, 3)]
    [InlineData(AuthenticationOperation.Logout, 2)]
    public void Request_captures_all_scopes_and_is_immutable(AuthenticationOperation operation, int count)
    {
        var request = QuotaTestData.Request(operation);
        Assert.Equal(count, request.Scopes.Count);
        Assert.True(AuthenticationQuotaPolicyDescriptor.AcademicInitial.Accepts(request));
        var input = request.Scopes.ToArray();
        var captured = new AuthenticationQuotaConsumeRequest(operation, "attempt", request.PolicyRevision, input);
        input[0] = new(input[0].ScopeType, "replacement", input[0].Definition);
        Assert.NotEqual("replacement", captured.Scopes[0].OpaqueId);
        Assert.Throws<NotSupportedException>(() => ((IList<AuthenticationQuotaScope>)captured.Scopes).Clear());
        Assert.All(typeof(AuthenticationQuotaConsumeRequest).GetProperties(), property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void Duplicate_missing_extra_and_incompatible_scopes_are_rejected()
    {
        var request = QuotaTestData.Request();
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, "attempt", request.PolicyRevision,
            [request.Scopes[0], request.Scopes[0]]));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, "attempt", request.PolicyRevision, []));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, "attempt", request.PolicyRevision,
            [.. request.Scopes, request.Scopes[0]]));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, "attempt", request.PolicyRevision,
            [request.Scopes[0], new(AuthenticationQuotaScopeType.Account, "opaque", request.Scopes[0].Definition)]));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, "attempt", request.PolicyRevision,
            [request.Scopes[0], null!]));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("contains space")]
    [InlineData("control\n")]
    public void Attempt_and_opaque_identifiers_reject_empty_whitespace_and_controls(string value)
    {
        var request = QuotaTestData.Request();
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(request.Operation, value, request.PolicyRevision, request.Scopes));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaScope(AuthenticationQuotaScopeType.Email, value, new(1, TimeSpan.FromSeconds(1))));
    }

    [Fact]
    public void No_unapproved_identifier_length_limit_is_imposed()
    {
        var value = new string('a', 5000);
        var scope = new AuthenticationQuotaScope(AuthenticationQuotaScopeType.Email, value, new(1, TimeSpan.FromSeconds(1)));
        var request = QuotaTestData.Request();
        Assert.Equal(value, scope.OpaqueId);
        Assert.Equal(value, new AuthenticationQuotaConsumeRequest(request.Operation, value, request.PolicyRevision, request.Scopes).InternalAttemptId);
    }

    [Fact]
    public void Invalid_operation_scope_and_null_definition_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => QuotaTestData.Request((AuthenticationOperation)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaScope((AuthenticationQuotaScopeType)999, "opaque", new(1, TimeSpan.FromSeconds(1))));
        Assert.Throws<ArgumentNullException>(() => new AuthenticationQuotaScope(AuthenticationQuotaScopeType.Email, "opaque", null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("unknown")]
    [InlineData("ACADEMIC-INITIAL")]
    public void Invalid_or_inactive_revision_is_unavailable_before_dispatch(string? revision)
    {
        var request = QuotaTestData.Request(revision: revision);
        var result = AuthenticationQuotaEvaluationContract.GetPreDispatchFailure(request, AuthenticationQuotaPolicyDescriptor.AcademicInitial, true);
        Assert.NotNull(result);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Status);
        Assert.False(result.ResultUncertain);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void Valid_revision_still_requires_trusted_definitions_and_atomic_topology()
    {
        var request = QuotaTestData.Request();
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        Assert.Null(AuthenticationQuotaEvaluationContract.GetPreDispatchFailure(request, policy, true));
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaEvaluationContract.GetPreDispatchFailure(request, policy, false)!.Status);
        var changed = request.Scopes.Select(scope => new AuthenticationQuotaScope(scope.ScopeType, scope.OpaqueId,
            new(scope.Definition.PermitLimit + 1, scope.Definition.Window)));
        var incompatible = new AuthenticationQuotaConsumeRequest(request.Operation, request.InternalAttemptId, request.PolicyRevision, changed);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaEvaluationContract.GetPreDispatchFailure(incompatible, policy, true)!.Status);
    }

    [Fact]
    public void Policy_requires_unique_complete_definitions_and_valid_revision()
    {
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaPolicyDescriptor("", policy.Rules));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaPolicyDescriptor("next", policy.Rules.Skip(1)));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaPolicyDescriptor("next", [.. policy.Rules.Skip(1), policy.Rules[1]]));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaPolicyRule(AuthenticationOperation.Register,
            AuthenticationQuotaScopeType.Account, new(1, TimeSpan.FromSeconds(1))));
    }

    [Theory]
    [InlineData(1200, 1800, 1800)]
    [InlineData(1800, 1200, 1800)]
    [InlineData(7, 11, 11)]
    public void Drain_window_is_derived_from_both_descriptors(int previousSeconds, int nextSeconds, int expected)
    {
        var previous = QuotaTestData.Policy("previous", previousSeconds);
        var next = QuotaTestData.Policy("next", nextSeconds);
        var transition = new AuthenticationQuotaPolicyTransition(previous, next);
        Assert.Same(previous, transition.Previous);
        Assert.Same(next, transition.Next);
        Assert.Equal(TimeSpan.FromSeconds(expected), transition.DrainWindow);
    }

    [Fact]
    public void Revision_change_preserves_partition_identity_and_descriptors_without_counter_reset()
    {
        var previous = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        var next = new AuthenticationQuotaPolicyDescriptor("new-revision", previous.Rules);
        var transition = new AuthenticationQuotaPolicyTransition(previous, next);
        var before = QuotaTestData.Request();
        var after = QuotaTestData.Request(revision: next.Revision);
        // Estado sintético externo: el contrato no crea otro namespace ni escribe/borra contadores.
        var state = before.Scopes.ToDictionary(before.GetPartitionIdentity, _ => 2);
        Assert.All(after.Scopes, scope => Assert.Equal(2, state[after.GetPartitionIdentity(scope)]));
        Assert.False(next.Accepts(before));
        Assert.True(next.Accepts(after));
        Assert.Same(previous, transition.Previous);
        Assert.Equal(new[]
        {
            AuthenticationQuotaPolicyTransitionStep.PrepareNewDescriptor,
            AuthenticationQuotaPolicyTransitionStep.StopNewAdmissionsAcrossInstances,
            AuthenticationQuotaPolicyTransitionStep.ResolveInFlightOperations,
            AuthenticationQuotaPolicyTransitionStep.EstablishLastPossibleConsumption,
            AuthenticationQuotaPolicyTransitionStep.PreservePreviousDescriptorAndState,
            AuthenticationQuotaPolicyTransitionStep.DrainFromLastPossibleConsumption,
            AuthenticationQuotaPolicyTransitionStep.ActivateNewRevisionAcrossInstances,
            AuthenticationQuotaPolicyTransitionStep.ReopenOnlyWithVerifiableContinuity
        }, AuthenticationQuotaPolicyTransition.RequiredSteps);
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaPolicyTransition(previous, previous));
    }

    [Theory]
    [InlineData(AuthenticationOperation.Register)]
    [InlineData(AuthenticationOperation.Login)]
    [InlineData(AuthenticationOperation.Logout)]
    public void Confirmed_contract_is_all_scopes_with_same_id_or_zero_insertions(AuthenticationOperation operation)
    {
        var request = QuotaTestData.Request(operation);
        var admitted = AuthenticationQuotaEvaluationContract.DescribeConfirmedInsertions(request, new(AuthenticationQuotaStoreStatus.Admitted));
        var rejected = AuthenticationQuotaEvaluationContract.DescribeConfirmedInsertions(request,
            new(AuthenticationQuotaStoreStatus.Rejected, TimeSpan.FromSeconds(1)));
        Assert.Equal(request.Scopes.Count, admitted.Count);
        Assert.Equal(request.Scopes.Count, admitted.Select(insertion => insertion.Partition).Distinct().Count());
        Assert.All(admitted, insertion => Assert.Equal(request.InternalAttemptId, insertion.InternalAttemptId));
        Assert.Empty(rejected);
        Assert.Throws<ArgumentException>(() => AuthenticationQuotaEvaluationContract.DescribeConfirmedInsertions(request, new(AuthenticationQuotaStoreStatus.Uncertain)));
        Assert.Throws<ArgumentException>(() => AuthenticationQuotaEvaluationContract.DescribeConfirmedInsertions(request, new(AuthenticationQuotaStoreStatus.Unavailable)));
    }

    [Fact]
    public void Future_script_contract_validates_before_single_time_read_and_joint_mutation()
    {
        Assert.Equal(new[]
        {
            AuthenticationQuotaScriptStep.ValidateRequestAndTrustedDescriptor,
            AuthenticationQuotaScriptStep.ValidateTopologyBeforeDispatch,
            AuthenticationQuotaScriptStep.ValidateArgumentCountsTypesAndRanges,
            AuthenticationQuotaScriptStep.ValidateOperationScopesDefinitionsAndRevision,
            AuthenticationQuotaScriptStep.ValidateExistingStateAndPredictablePreconditions,
            AuthenticationQuotaScriptStep.ReadRedisTimeOnce,
            AuthenticationQuotaScriptStep.EvaluateAllLiveWindowsWithSharedNow,
            AuthenticationQuotaScriptStep.IfBlockedReturnMaximumRetryAfterWithoutInsertions,
            AuthenticationQuotaScriptStep.OtherwisePruneInsertSameAttemptInAllScopesAndApplyRetention
        }, AuthenticationQuotaEvaluationContract.RequiredSteps);
        Assert.Single(AuthenticationQuotaEvaluationContract.RequiredSteps, step => step == AuthenticationQuotaScriptStep.ReadRedisTimeOnce);
        Assert.True(typeof(IAuthenticationQuotaStore).IsInterface);
        var method = Assert.Single(typeof(IAuthenticationQuotaStore).GetMethods());
        Assert.True(method.IsAbstract);
        Assert.Equal(typeof(ValueTask<AuthenticationQuotaConsumeResult>), method.ReturnType);
    }

    [Theory]
    [InlineData(AuthenticationQuotaStoreStatus.Unavailable)]
    [InlineData(AuthenticationQuotaStoreStatus.Uncertain)]
    public async Task Existing_http_coordinator_accepts_internal_failures_as_503_without_public_uncertainty(AuthenticationQuotaStoreStatus status)
    {
        // Adaptador solo de prueba: no implementa HTTP productivo ni un store.
        var quota = new BoundaryQuota(new(status));
        var coordinator = new AuthenticationValidationCoordinator(new VerifiableAdmission(), new ValidInputs(), quota);
        var result = await coordinator.CoordinateAsync(new DefaultHttpContext(), new(AuthenticationOperation.Login, null, [], true));
        Assert.Equal(503, result.HttpStatus);
        Assert.Null(result.RetryAfter);
        Assert.Equal(1, quota.Calls);
    }

    [Fact]
    public void Models_have_no_cost_or_redis_http_transport_payloads()
    {
        Type[] models = [typeof(AuthenticationQuotaConsumeRequest), typeof(AuthenticationQuotaScope), typeof(AuthenticationQuotaDefinition),
            typeof(AuthenticationQuotaConsumeResult), typeof(AuthenticationQuotaPolicyDescriptor), typeof(AuthenticationQuotaPartitionIdentity)];
        foreach (var type in models)
        {
            Assert.DoesNotContain(type.GetProperties(), property => property.Name is "Cost" or "Burst" or "Weight" or "Tokens" or "RedisKey" or "Email" or "IpAddress" or "Jwt");
            foreach (var property in type.GetProperties()) AssertNoTransport(property.PropertyType);
        }
        Assert.DoesNotContain(typeof(AuthenticationQuotaPartitionIdentity).GetProperties(), property => property.Name.Contains("Revision", StringComparison.Ordinal));
    }

    private static void AssertNoTransport(Type type)
    {
        Assert.False((type.Namespace ?? "").StartsWith("StackExchange.Redis", StringComparison.Ordinal));
        Assert.False((type.Namespace ?? "").StartsWith("Microsoft.AspNetCore.Http", StringComparison.Ordinal));
        Assert.False((type.Namespace ?? "").StartsWith("System.Net", StringComparison.Ordinal));
        foreach (var argument in type.GenericTypeArguments) AssertNoTransport(argument);
    }

    private sealed class VerifiableAdmission : IAuthenticationAdmissionChecks
    {
        public ValueTask<bool> IsVerifiableAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) => ValueTask.FromResult(true);
    }

    private sealed class ValidInputs : IAuthenticationInputAssessment
    {
        public ValueTask<AuthenticationInputAssessment> AssessAsync(HttpContext context, AuthenticationOperation operation, CancellationToken token) =>
            ValueTask.FromResult(new AuthenticationInputAssessment(true, []));
    }

    private sealed class BoundaryQuota(AuthenticationQuotaConsumeResult result) : IAuthenticationQuotaAdmission
    {
        public int Calls { get; private set; }
        public ValueTask<AuthenticationQuotaResult> EvaluateAndConsumeAsync(HttpContext context, AuthenticationOperation operation, string internalAttemptId, CancellationToken token)
        {
            Calls++;
            return ValueTask.FromResult(new AuthenticationQuotaResult(result.Status switch
            {
                AuthenticationQuotaStoreStatus.Admitted => AuthenticationQuotaStatus.Admitted,
                AuthenticationQuotaStoreStatus.Rejected => AuthenticationQuotaStatus.Rejected,
                _ => AuthenticationQuotaStatus.Unavailable
            }, result.RetryAfter));
        }
    }
}

internal static class QuotaTestData
{
    internal static AuthenticationQuotaConsumeRequest Request(AuthenticationOperation operation = AuthenticationOperation.Register,
        string? revision = "academic-initial") => new(operation, "internal-attempt", revision,
        AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Select(type => new AuthenticationQuotaScope(type,
            "synthetic-opaque-" + type, AuthenticationQuotaPolicyDescriptor.AcademicInitial.GetDefinition(operation, type))));

    internal static AuthenticationQuotaPolicyDescriptor Policy(string revision, int windowSeconds) => new(revision,
        AuthenticationQuotaPolicyDescriptor.AcademicInitial.Rules.Select(rule => new AuthenticationQuotaPolicyRule(rule.Operation,
            rule.ScopeType, new(rule.Definition.PermitLimit, TimeSpan.FromSeconds(windowSeconds)))));
}

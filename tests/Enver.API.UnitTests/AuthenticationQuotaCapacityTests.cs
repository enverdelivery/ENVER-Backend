using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Microsoft.Extensions.Configuration;

namespace Enver.API.UnitTests;

public sealed class AuthenticationQuotaCapacityTests
{
    [Fact]
    public void Explicit_academic_activation_captures_5000_and_64_MiB_without_mutable_options()
    {
        var options = new RateLimitingOptions { MaxActivePartitions = 5000 };
        var descriptor = AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(options.MaxActivePartitions);
        options.MaxActivePartitions = null;
        Assert.Equal(5000, descriptor.MaxActivePartitions);
        Assert.Equal(67108864L, descriptor.MemoryBudgetBytes);
        Assert.Equal(64L * 1024 * 1024, descriptor.MemoryBudgetBytes);
        Assert.All(typeof(AuthenticationQuotaCapacityDescriptor).GetProperties(), property => Assert.Null(property.SetMethod));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(4999)]
    [InlineData(5001)]
    public void Academic_activation_requires_exact_explicit_approved_value(int? value) =>
        Assert.Throws<ArgumentException>(() => AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(value));

    [Theory]
    [InlineData(0, 67108864L)]
    [InlineData(-1, 67108864L)]
    [InlineData(5000, 0L)]
    [InlineData(5000, -1L)]
    public void General_contract_descriptor_requires_positive_bounds(int capacity, long budget) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaCapacityDescriptor(capacity, budget));

    [Fact]
    public void Prepared_options_remain_nullable_but_binding_does_not_supply_an_activation_default()
    {
        var options = new RateLimitingOptions();
        Assert.Null(options.MaxActivePartitions);
        Assert.Throws<ArgumentException>(() => AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(options.MaxActivePartitions));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimiting:MaxActivePartitions"] = "5000"
        }).Build();
        configuration.GetSection(RateLimitingOptions.SectionName).Bind(options);
        Assert.Equal(5000, AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(options.MaxActivePartitions).MaxActivePartitions);
    }

    [Fact]
    public void Partition_identity_reuses_052_and_excludes_revision_and_memory_capacity()
    {
        var identity = CapacityData.Identity();
        Assert.Equal(new AuthenticationQuotaPartitionIdentity(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, new string('a', 43)), identity);
        Assert.Equal(new[] { "Operation", "ScopeType", "OpaqueId" }, typeof(AuthenticationQuotaPartitionIdentity).GetProperties().Select(p => p.Name));
        var before = QuotaTestData.Request(AuthenticationOperation.Login);
        var after = QuotaTestData.Request(AuthenticationOperation.Login, "next-policy");
        Assert.Equal(before.Scopes.Select(before.GetPartitionIdentity), after.Scopes.Select(after.GetPartitionIdentity));
    }

    [Theory]
    [InlineData(4997, true)]
    [InlineData(4998, false)]
    [InlineData(5000, false)]
    public void Joint_capacity_respects_available_exact_and_exhausted_boundaries(long occupied, bool sufficient)
    {
        var required = CapacityData.LoginRequest();
        var result = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), new(occupied, true), required, [], CapacityData.Now);
        Assert.True(result.IsVerifiable);
        Assert.Equal(0, result.ExistingActivePartitionCount);
        Assert.Equal(3, result.NewPartitionCount);
        Assert.Equal(sufficient, result.CapacitySufficient);
        if (sufficient)
        {
            Assert.Null(result.Failure);
            Assert.Equal(3, result.NewPartitions.Count);
        }
        else
        {
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Failure!.Status);
            Assert.Null(result.Failure.RetryAfter);
            Assert.False(result.Failure.ResultUncertain);
            Assert.Empty(result.NewPartitions);
        }
    }

    [Theory]
    [InlineData(5000)]
    [InlineData(5001)]
    public void Existing_only_is_not_blocked_at_or_above_maximum(long occupied)
    {
        var request = CapacityData.LoginRequest();
        var observed = request.Scopes.Select(scope => CapacityData.Observation(request.GetPartitionIdentity(scope), CapacityData.Now));
        var result = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), new(occupied, true), request, observed, CapacityData.Now);
        Assert.True(result.CapacitySufficient);
        Assert.Equal(3, result.ExistingActivePartitionCount);
        Assert.Equal(0, result.NewPartitionCount);
        Assert.Empty(result.NewPartitions);
        Assert.Null(AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, true, null));
    }

    [Theory]
    [InlineData(4999, true)]
    [InlineData(5000, false)]
    public void Mixed_existing_and_new_counts_only_new_required_partitions(long occupied, bool sufficient)
    {
        var request = CapacityData.LoginRequest();
        var observed = request.Scopes.Take(2).Select(scope => CapacityData.Observation(request.GetPartitionIdentity(scope), CapacityData.Now));
        var result = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), new(occupied, true), request, observed, CapacityData.Now);
        Assert.Equal(2, result.ExistingActivePartitionCount);
        Assert.Equal(1, result.NewPartitionCount);
        Assert.Equal(sufficient, result.CapacitySufficient);
    }

    [Fact]
    public void Duplicate_requirements_and_duplicate_identical_observations_do_not_charge_twice()
    {
        var identity = CapacityData.Identity();
        var newResult = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(4999, true),
            [identity, identity with { }], [], CapacityData.Now);
        Assert.True(newResult.CapacitySufficient);
        Assert.Equal(1, newResult.NewPartitionCount);
        Assert.Single(newResult.NewPartitions);
        var observation = CapacityData.Observation(identity, CapacityData.Now);
        var existing = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(5000, true),
            [identity, identity], [observation, CapacityData.Observation(identity, CapacityData.Now)], CapacityData.Now);
        Assert.True(existing.CapacitySufficient);
        Assert.Equal(1, existing.ExistingActivePartitionCount);
        Assert.Equal(0, existing.NewPartitionCount);
    }

    [Fact]
    public void Same_opaque_in_different_operations_or_scopes_counts_as_different_partitions()
    {
        var loginEmail = CapacityData.Identity();
        var registerEmail = loginEmail with { Operation = AuthenticationOperation.Register };
        var loginOrigin = loginEmail with { ScopeType = AuthenticationQuotaScopeType.Origin };
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, true),
            [loginEmail, registerEmail, loginOrigin], [], CapacityData.Now);
        Assert.Equal(3, result.NewPartitionCount);
        Assert.Equal(3, result.NewPartitions.Count);
    }

    [Fact]
    public void Insufficient_joint_capacity_never_exposes_partial_creation_or_mutates_supplied_state()
    {
        var request = CapacityData.LoginRequest();
        var identities = request.Scopes.Select(request.GetPartitionIdentity).ToArray();
        var observed = new List<AuthenticationQuotaPartitionState>();
        var state = new AuthenticationQuotaCapacityState(4998, true);
        var result = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), state, request, observed, CapacityData.Now);
        Assert.False(result.CapacitySufficient);
        Assert.Equal(3, result.NewPartitionCount);
        Assert.Empty(result.NewPartitions);
        Assert.Empty(observed);
        Assert.Equal(4998, state.ActivePartitionCount);
        Assert.Equal(identities, request.Scopes.Select(request.GetPartitionIdentity));
        Assert.Throws<NotSupportedException>(() => ((IList<AuthenticationQuotaPartitionIdentity>)result.NewPartitions).Clear());
    }

    [Fact]
    public void Expired_physical_residue_is_not_an_existing_active_partition_and_unreleased_plazas_remain_charged()
    {
        var identity = CapacityData.Identity();
        var expired = CapacityData.Observation(identity, CapacityData.Now.AddMinutes(-1));
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(4999, true, 1), [identity], [expired], CapacityData.Now);
        Assert.Equal(0, result.ExistingActivePartitionCount);
        Assert.Equal(1, result.NewPartitionCount);
        Assert.False(result.CapacitySufficient);
        var afterVerifiedCleanup = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(4999, true), [identity], [], CapacityData.Now);
        Assert.True(afterVerifiedCleanup.CapacitySufficient);
    }

    [Fact]
    public void Very_large_counts_fail_closed_without_addition_overflow()
    {
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(long.MaxValue, true),
            [CapacityData.Identity()], [], CapacityData.Now);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Failure!.Status);
        Assert.Empty(result.NewPartitions);
        Assert.Throws<OverflowException>(() => new AuthenticationQuotaCapacityState(long.MaxValue, true, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaCapacityState(-1, true));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AuthenticationQuotaCapacityState(0, true, -1));
    }

    [Fact]
    public void Unknown_global_state_never_infers_capacity_free_even_for_existing_only_or_empty_registry()
    {
        var identity = CapacityData.Identity();
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, false), [identity], [], CapacityData.Now);
        Assert.False(result.IsVerifiable);
        Assert.Null(result.NewPartitionCount);
        Assert.Null(result.ExistingActivePartitionCount);
        Assert.Empty(result.NewPartitions);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Failure!.Status);
        Assert.Null(result.Failure.RetryAfter);
        var existing = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(5000, false), [identity],
            [CapacityData.Observation(identity, CapacityData.Now)], CapacityData.Now);
        Assert.False(existing.CapacitySufficient);
    }

    [Theory]
    [InlineData("uncertain")]
    [InlineData("future-event")]
    [InlineData("conflicting-observations")]
    [InlineData("undercounted-registry")]
    [InlineData("wrong-definition")]
    public void Inconsistent_observations_are_unavailable_not_admitted(string scenario)
    {
        var request = CapacityData.LoginRequest();
        var identity = request.GetPartitionIdentity(request.Scopes[0]);
        AuthenticationQuotaPartitionState[] observations = scenario switch
        {
            "uncertain" => [CapacityData.Observation(identity, null, verifiable: false)],
            "future-event" => [CapacityData.Observation(identity, CapacityData.Now.AddTicks(1))],
            "conflicting-observations" => [CapacityData.Observation(identity, CapacityData.Now), CapacityData.Observation(identity, CapacityData.Now, version: 2)],
            "wrong-definition" => [CapacityData.Observation(identity, CapacityData.Now, definition: new(99, TimeSpan.FromSeconds(1)))],
            _ => [CapacityData.Observation(identity, CapacityData.Now)]
        };
        var result = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(),
            new(scenario == "undercounted-registry" ? 0 : 1, true), request, observations, CapacityData.Now);
        Assert.False(result.IsVerifiable);
        Assert.False(result.CapacitySufficient);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Failure!.Status);
        Assert.Empty(result.NewPartitions);
    }

    [Fact]
    public void Invalid_or_empty_required_identity_sets_fail_closed()
    {
        AuthenticationQuotaPartitionIdentity[][] sets = [[], [null!], [CapacityData.Identity() with { Operation = (AuthenticationOperation)999 }],
            [CapacityData.Identity() with { ScopeType = AuthenticationQuotaScopeType.Account }], [CapacityData.Identity() with { OpaqueId = "" }]];
        foreach (var required in sets)
        {
            var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, true), required, [], CapacityData.Now);
            Assert.False(result.IsVerifiable);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result.Failure!.Status);
        }
    }

    [Fact]
    public void Verifiable_quota_rejection_precedes_capacity_exhaustion_without_mutation_or_refresh()
    {
        var request = CapacityData.LoginRequest();
        var observation = CapacityData.Observation(request.GetPartitionIdentity(request.Scopes[0]), CapacityData.Now);
        var capacity = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), new(5000, true), request, [observation], CapacityData.Now);
        Assert.False(capacity.CapacitySufficient);
        var failure = AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(capacity, true, TimeSpan.FromSeconds(7));
        Assert.Equal(AuthenticationQuotaStoreStatus.Rejected, failure!.Status);
        Assert.Equal(TimeSpan.FromSeconds(7), failure.RetryAfter);
        Assert.Equal(CapacityData.Now, observation.LastEventUtc);
        Assert.Equal(CapacityData.Now.AddMinutes(1), observation.InactiveAtUtc);
        Assert.Empty(capacity.NewPartitions);
        var capacityFailure = AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(capacity, true, null);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, capacityFailure!.Status);
        Assert.Null(capacityFailure.RetryAfter);
    }

    [Fact]
    public void Capacity_sufficiency_never_means_admission_and_quota_failure_does_not_create_partitions()
    {
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, true), [CapacityData.Identity()], [], CapacityData.Now);
        Assert.Null(AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, true, null));
        Assert.Equal(AuthenticationQuotaStoreStatus.Rejected,
            AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, true, TimeSpan.FromSeconds(1))!.Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, false, null)!.Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, true, TimeSpan.Zero)!.Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(result, true, TimeSpan.FromSeconds(-1))!.Status);
        var unknown = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, false), [CapacityData.Identity()], [], CapacityData.Now);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetFailureAfterQuotaEvaluation(unknown, true, TimeSpan.FromSeconds(1))!.Status);
    }

    [Theory]
    [InlineData(false, AuthenticationQuotaStoreStatus.Unavailable)]
    [InlineData(true, AuthenticationQuotaStoreStatus.Uncertain)]
    public void Existing_dispatch_boundary_is_preserved_without_a_fifth_store_status(bool dispatched, AuthenticationQuotaStoreStatus expected)
    {
        var result = AuthenticationQuotaEvaluationContract.MissingSemanticResult(dispatched, BudgetCancellationCause.DependencyTimeout);
        Assert.Equal(expected, result.Status);
        Assert.Null(result.RetryAfter);
        Assert.Equal(dispatched, result.ResultUncertain);
        Assert.Equal(new[] { AuthenticationQuotaStoreStatus.Admitted, AuthenticationQuotaStoreStatus.Rejected,
            AuthenticationQuotaStoreStatus.Unavailable, AuthenticationQuotaStoreStatus.Uncertain }, Enum.GetValues<AuthenticationQuotaStoreStatus>());
    }

    [Fact]
    public void Memory_and_revocation_obligations_are_explicit_but_not_claimed_as_empirically_validated()
    {
        var evidence = AuthenticationQuotaCapacityDescriptor.RequiredIntegrationEvidence;
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.MeasuredMemoryWithinBudgetIncludingHousekeeping, evidence);
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.CoexistenceWithoutWeakeningRevocations, evidence);
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.BoundedEventsBucketsAndRegistry, evidence);
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.AtomicQuotaAndCapacityMutation, evidence);
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.PostDispatchUncertainty, evidence);
        Assert.Contains(AuthenticationQuotaCapacityIntegrationEvidence.ConcurrentInstancesRespectGlobalLimit, evidence);
        Assert.Equal(8, evidence.Count);
        Assert.Throws<NotSupportedException>(() => ((IList<AuthenticationQuotaCapacityIntegrationEvidence>)evidence).Clear());
        Assert.DoesNotContain(typeof(AuthenticationQuotaCapacityDescriptor).GetProperties(), p => p.Name is "MemoryValidated" or "RedisMemoryMeasured" or "GuaranteedBytesPerPartition");
    }

    [Fact]
    public void Capacity_models_do_not_expose_raw_PII_secrets_or_transport_and_diagnostics_omit_opaque_ids()
    {
        var identity = CapacityData.Identity();
        var observation = CapacityData.Observation(identity, CapacityData.Now);
        var result = AuthenticationQuotaCapacityContract.Evaluate(CapacityData.Descriptor(), new(0, true), [identity], [], CapacityData.Now);
        object[] models = [CapacityData.Descriptor(), new AuthenticationQuotaCapacityState(0, true), observation, result,
            CapacityData.Snapshot(), new AuthenticationQuotaCapacityTransition(CapacityData.Snapshot(), CapacityData.Snapshot(version: "next", max: 4000))];
        foreach (var model in models)
        {
            Assert.DoesNotContain(identity.OpaqueId, model.ToString()!, StringComparison.Ordinal);
            Assert.DoesNotContain(model.GetType().GetProperties(), p => p.Name is "Email" or "IpAddress" or "AccountId" or "HmacKey" or "RedisKey" or "PolicyRevision");
            Assert.All(model.GetType().GetProperties(), p => AssertNoTransport(p.PropertyType));
        }
        Assert.All(new[] { typeof(AuthenticationQuotaCapacityDescriptor), typeof(AuthenticationQuotaCapacityState), typeof(AuthenticationQuotaPartitionState),
            typeof(AuthenticationQuotaConfigurationSnapshot), typeof(AuthenticationQuotaCapacityEvaluation), typeof(AuthenticationQuotaCapacityTransition) },
            type => Assert.All(type.GetProperties(), property => Assert.Null(property.SetMethod)));
    }

    private static void AssertNoTransport(Type type)
    {
        var ns = type.Namespace ?? "";
        Assert.False(ns.StartsWith("StackExchange.Redis", StringComparison.Ordinal) || ns.StartsWith("Microsoft.AspNetCore.Http", StringComparison.Ordinal) ||
            ns.StartsWith("Amazon", StringComparison.Ordinal) || ns.StartsWith("System.Net", StringComparison.Ordinal));
        foreach (var argument in type.GenericTypeArguments) AssertNoTransport(argument);
    }
}

internal static class CapacityData
{
    internal static DateTimeOffset Now => new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);
    internal static AuthenticationQuotaCapacityDescriptor Descriptor() => AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000);
    internal static AuthenticationQuotaPartitionIdentity Identity() => new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, new string('a', 43));
    internal static AuthenticationQuotaConsumeRequest LoginRequest() => QuotaTestData.Request(AuthenticationOperation.Login);
    internal static AuthenticationQuotaPartitionState Observation(AuthenticationQuotaPartitionIdentity identity, DateTimeOffset? last,
        bool verifiable = true, long version = 1, AuthenticationQuotaDefinition? definition = null) =>
        new(identity, definition ?? AuthenticationQuotaPolicyDescriptor.AcademicInitial.GetDefinition(identity.Operation, identity.ScopeType), last, version, verifiable);
    internal static AuthenticationQuotaConfigurationSnapshot Snapshot(string version = "config-1", int max = 5000, long budget = 67108864,
        string identity = "identity-format-1", AuthenticationQuotaPolicyDescriptor? policy = null, string environment = "testing") =>
        new(environment, version, identity, policy ?? AuthenticationQuotaPolicyDescriptor.AcademicInitial, new(max, budget));
}

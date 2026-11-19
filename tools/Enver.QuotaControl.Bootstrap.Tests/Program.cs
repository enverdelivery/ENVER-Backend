using System.Globalization;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Amazon.Runtime;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Configuration;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.QuotaControl.Bootstrap;
using StackExchange.Redis;
using BootstrapEntryPoint = Enver.QuotaControl.Bootstrap.Program;

namespace Enver.QuotaControl.Bootstrap.Tests;

internal static class Assert
{
    public static void True(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Assertion failed");
    }

    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new InvalidOperationException("Assertion failed");
    }

    public static void Contains(string expected, IReadOnlyList<string> actual)
    {
        if (!actual.Contains(expected, StringComparer.Ordinal)) throw new InvalidOperationException("Assertion failed");
    }

    public static void DoesNotContain(string forbidden, string actual)
    {
        if (actual.Contains(forbidden, StringComparison.Ordinal)) throw new InvalidOperationException("Assertion failed");
    }

    public static void Throws(string code, Action action)
    {
        try
        {
            action();
        }
        catch (BootstrapValidationException exception)
        {
            Equal(code, exception.Code);
            return;
        }

        throw new InvalidOperationException("Expected BootstrapValidationException");
    }
}

internal sealed class FakeBackend : IBootstrapBackend
{
    public PreflightResult Preflight { get; set; } = new(true, "none", "none", true, true);
    public InitializeOutcome Initialize { get; set; } = new("initialized", "ok", true);
    public ReadbackResult Readback { get; set; } = ReadbackResult.Success();
    public int PreflightCalls { get; private set; }
    public int InitializeCalls { get; private set; }
    public int ReadbackCalls { get; private set; }
    public int DisposeCalls { get; private set; }

    public ValueTask<PreflightResult> PreflightAsync(CancellationToken cancellationToken)
    {
        PreflightCalls++;
        return ValueTask.FromResult(Preflight);
    }

    public ValueTask<InitializeOutcome> InitializeAsync(CancellationToken cancellationToken)
    {
        InitializeCalls++;
        return ValueTask.FromResult(Initialize);
    }

    public ValueTask<ReadbackResult> ReadbackAsync(CancellationToken cancellationToken)
    {
        ReadbackCalls++;
        return ValueTask.FromResult(Readback);
    }

    public ValueTask DisposeAsync()
    {
        DisposeCalls++;
        return ValueTask.CompletedTask;
    }
}

// Solo dobles: se ejercita el preflight compartido sin construir el backend operacional ni clientes AWS/Redis.
internal sealed class DiagnosticBackend(string failurePhase, Exception? failure, TextWriter writer) : IBootstrapBackend
{
    public List<string> Calls { get; } = [];
    public bool LocalReady { get; set; } = true;
    public bool GlobalReady { get; set; } = true;
    public int InitializeCalls { get; private set; }
    public int ReadbackCalls { get; private set; }
    public int PreflightCalls { get; private set; }
    public PreflightResult? LastPreflight { get; private set; }
    public Action? BeforeFailure { get; set; }
    public IBootstrapKeyspaceReader? KeyspaceReader { get; set; }

    public async ValueTask<PreflightResult> PreflightAsync(CancellationToken cancellationToken)
    {
        PreflightCalls++;
        LastPreflight = await RedisBootstrapBackend.PreflightAsync(30,
            new DiagnosticState(() => Guard("local-guard", LocalReady)),
            new DiagnosticState(() => Guard("authority-guard", GlobalReady)),
            _ =>
            {
                Visit("redis-database");
                var database = DispatchProxy.Create<IDatabase, DiagnosticDatabaseProxy>();
                ((DiagnosticDatabaseProxy)(object)database).Execute = (command, args, flags) =>
                {
                    Assert.Equal(CommandFlags.DemandMaster, flags);
                    Assert.Equal("TYPE", command); // INFO no puede viajar por la conexión operacional.
                    Assert.Equal(1, args.Length);
                    var phase = (string)args[0] switch
                    {
                        "enver:{development:auth:ratelimit}:metadata" => "metadata-type",
                        "enver:{development:auth:ratelimit}:registry" => "registry-type",
                        _ => throw new InvalidOperationException("Unexpected test key")
                    };
                    Visit(phase);
                    return RedisResult.Create((RedisValue)"none");
                };
                return ValueTask.FromResult(database);
            }, new DelegateKeyspaceReader(async token =>
            {
                Visit("keyspace");
                return KeyspaceReader is null ? "# Keyspace\r\n" : await KeyspaceReader.ReadAsync(token);
            }), BootstrapContract.CreateConfiguration("0123456789abcdef0123456789abcdef"), writer, cancellationToken);
        return LastPreflight;
    }

    private AuthenticationSecurityDecision Guard(string phase, bool ready)
    {
        Visit(phase);
        return new(ready ? AuthenticationSecurityAdmission.Verifiable : AuthenticationSecurityAdmission.Unavailable);
    }

    private void Visit(string phase)
    {
        Calls.Add(phase);
        if (phase != failurePhase || failure is null) return;
        BeforeFailure?.Invoke();
        throw failure;
    }

    public ValueTask<InitializeOutcome> InitializeAsync(CancellationToken cancellationToken)
    {
        InitializeCalls++;
        throw new InvalidOperationException("Initialize must be unreachable in diagnostic error tests");
    }

    public ValueTask<ReadbackResult> ReadbackAsync(CancellationToken cancellationToken)
    {
        ReadbackCalls++;
        throw new InvalidOperationException("Readback must be unreachable in diagnostic error tests");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed class DiagnosticState(Func<AuthenticationSecurityDecision> evaluate) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget budget) => ValueTask.FromResult(evaluate());
    }
}

public class DiagnosticDatabaseProxy : DispatchProxy
{
    public Func<string, object[], CommandFlags, RedisResult> Execute { get; set; } = null!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Assert.Equal("ExecuteAsync", targetMethod!.Name);
        Assert.Equal(3, args!.Length);
        try
        {
            return Task.FromResult(Execute((string)args[0]!, (object[])args[1]!, (CommandFlags)args[2]!));
        }
        catch (Exception exception)
        {
            return Task.FromException<RedisResult>(exception);
        }
    }
}

internal sealed class DelegateKeyspaceReader(Func<CancellationToken, ValueTask<string>> read) : IBootstrapKeyspaceReader
{
    public ValueTask<string> ReadAsync(CancellationToken cancellationToken) => read(cancellationToken);
}

public class AdminInfoProxy : DispatchProxy
{
    public Func<MethodInfo, object?[], object?> Call { get; set; } = null!;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Call(targetMethod!, args ?? []);
}

// Nunca construye ConnectionMultiplexer real: ambos objetos son DispatchProxy.
internal sealed class AdminInfoFixture
{
    internal const string Host = "master.enver-dev-valkey-v2.kb2qe0.use1.cache.amazonaws.com";
    public EndPoint[] Endpoints { get; set; } = [new DnsEndPoint(Host, 6379)];
    public EndPoint ServerEndpoint { get; set; } = new DnsEndPoint(Host, 6379);
    public bool Connected { get; set; } = true;
    public bool Replica { get; set; }
    public ServerType Type { get; set; } = ServerType.Standalone;
    public string? Info { get; set; } = "# Keyspace\r\n";
    public Exception? Failure { get; set; }
    public Action? OnInfo { get; set; }
    public Task<string?>? InfoTask { get; set; }
    public Task<IConnectionMultiplexer>? ConnectTask { get; set; }
    public ConfigurationOptions? CapturedConfiguration { get; private set; }
    public int ConnectCalls { get; private set; }
    public int InfoCalls { get; private set; }
    public int GetServerCalls { get; private set; }
    public int DisposeCalls { get; private set; }
    public TaskCompletionSource Disposed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public IConnectionMultiplexer Multiplexer { get; }
    public RedisOptions Options { get; }
    public RedisBootstrapKeyspaceReader Reader { get; }

    public AdminInfoFixture()
    {
        Options = new RedisOptions
        {
            ConnectionString = RedisConnectionStringFactory.Create(Host, new RedisCredential("test-only-user", "TestOnly-Password-1234567890")),
            ConnectionTimeoutMilliseconds = BootstrapContract.ConnectionTimeoutMilliseconds,
            OperationTimeoutMilliseconds = BootstrapContract.OperationTimeoutMilliseconds
        };
        var server = DispatchProxy.Create<IServer, AdminInfoProxy>();
        ((AdminInfoProxy)(object)server).Call = (method, args) => method.Name switch
        {
            "get_EndPoint" => ServerEndpoint,
            "get_IsConnected" => Connected,
            "get_IsReplica" => Replica,
            "get_ServerType" => Type,
            "InfoRawAsync" => ExecuteInfo(args),
            _ => throw new InvalidOperationException("Unexpected server capability")
        };
        Multiplexer = DispatchProxy.Create<IConnectionMultiplexer, AdminInfoProxy>();
        ((AdminInfoProxy)(object)Multiplexer).Call = (method, args) =>
        {
            switch (method.Name)
            {
                case "GetEndPoints":
                    Assert.Equal(true, (bool)args[0]!);
                    return Endpoints;
                case "GetServer":
                    GetServerCalls++;
                    Assert.Equal(2, args.Length);
                    Assert.Equal(new DnsEndPoint(Host, 6379), (EndPoint)args[0]!);
                    return server;
                case "Dispose":
                    DisposeCalls++;
                    Disposed.TrySetResult();
                    return null;
                default: throw new InvalidOperationException("Unexpected multiplexer capability");
            }
        };
        Reader = new RedisBootstrapKeyspaceReader(Options, Host, configuration =>
        {
            ConnectCalls++;
            CapturedConfiguration = configuration;
            return ConnectTask ?? Task.FromResult(Multiplexer);
        });
    }

    private Task<string?> ExecuteInfo(object?[] args)
    {
        InfoCalls++;
        Assert.Equal(2, args.Length);
        Assert.Equal((RedisValue)"keyspace", (RedisValue)args[0]!);
        Assert.Equal(CommandFlags.DemandMaster, (CommandFlags)args[1]!);
        OnInfo?.Invoke();
        return Failure is not null ? Task.FromException<string?>(Failure) : InfoTask ?? Task.FromResult(Info);
    }
}

internal sealed class FailingDiagnosticWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;
    public override void WriteLine(string? value) => throw new IOException("Synthetic diagnostic writer failure");
}

internal static class Program
{
    private const string Endpoint = "master.enver-dev-valkey-v2.kb2qe0.use1.cache.amazonaws.com";
    private const string Generation = "0123456789abcdef0123456789abcdef";
    private const string TestSecret = "TestOnly-Password-1234567890";

    private static readonly BootstrapRequest Request = new(
        Endpoint, Generation, BootstrapContract.AuthorityTable, BootstrapContract.Region, 30);

    private static string[] ValidArgs() =>
    [
        "--execute-initialize",
        "--secret-stdin",
        "--endpoint", Endpoint,
        "--generation", Generation,
        "--authority-table", BootstrapContract.AuthorityTable,
        "--region", BootstrapContract.Region
    ];

    private static async Task Main()
    {
        var tests = new (string Name, Func<Task> Body)[]
        {
            ("parse_valid_arguments", ParseValidArguments),
            ("missing_execute_flag_rejected", MissingExecuteFlagRejected),
            ("missing_secret_stdin_rejected", MissingSecretStdinRejected),
            ("old_endpoint_rejected", OldEndpointRejected),
            ("other_endpoint_rejected", OtherEndpointRejected),
            ("invalid_generation_rejected", InvalidGenerationRejected),
            ("wrong_authority_table_rejected", WrongAuthorityTableRejected),
            ("wrong_region_rejected", WrongRegionRejected),
            ("secret_argument_rejected_without_echo", SecretArgumentRejectedWithoutEcho),
            ("snapshot_text_exact", SnapshotTextExact),
            ("configuration_capture_exact", ConfigurationCaptureExact),
            ("incorrect_c_aborts", IncorrectConfigurationVersionAborts),
            ("incorrect_i_aborts", IncorrectIdentityReferenceAborts),
            ("redis_connection_string_roundtrip", RedisConnectionStringRoundtrip),
            ("redis_connection_string_rejects_comma", RedisConnectionStringRejectsComma),
            ("stdin_secret_parses_and_redacts", StdinSecretParsesAndRedacts),
            ("stdin_secret_invalid_does_not_echo", StdinSecretInvalidDoesNotEcho),
            ("readback_exact_passes", ReadbackExactPasses),
            ("readback_dirty_mismatch", ReadbackDirtyMismatch),
            ("readback_snapshot_mismatch", ReadbackSnapshotMismatch),
            ("readback_registry_present_mismatch", ReadbackRegistryPresentMismatch),
            ("readback_keyspace_extra_mismatch", ReadbackKeyspaceExtraMismatch),
            ("service_metadata_present_stops", ServiceMetadataPresentStops),
            ("service_registry_present_stops", ServiceRegistryPresentStops),
            ("service_keyspace_not_empty_stops", ServiceKeyspaceNotEmptyStops),
            ("service_guard_closed_stops", ServiceGuardClosedStops),
            ("service_authority_closed_stops", ServiceAuthorityClosedStops),
            ("service_preflight_unavailable_stops", ServicePreflightUnavailableStops),
            ("service_initialize_pass", ServiceInitializePass),
            ("service_initialize_uncertain_stops", ServiceInitializeUncertainStops),
            ("service_readback_dirty_stops", ServiceReadbackDirtyStops),
            ("secret_never_in_logs_or_argv", SecretNeverInLogsOrArgv),
            ("preflight_diagnostic_redis_database", () => DiagnosticPhase("redis-database")),
            ("preflight_diagnostic_metadata_type", () => DiagnosticPhase("metadata-type")),
            ("preflight_diagnostic_registry_type", () => DiagnosticPhase("registry-type")),
            ("preflight_diagnostic_keyspace", () => DiagnosticPhase("keyspace")),
            ("preflight_diagnostic_local_guard_exception", () => DiagnosticPhase("local-guard")),
            ("preflight_diagnostic_authority_guard_exception", () => DiagnosticPhase("authority-guard")),
            ("preflight_diagnostic_aws_allowlisted_code", DiagnosticAwsAllowlistedCode),
            ("preflight_diagnostic_aws_unknown_code_redacted", DiagnosticAwsUnknownCodeRedacted),
            ("preflight_diagnostic_redis_timeout_category", DiagnosticRedisTimeoutCategory),
            ("preflight_diagnostic_redis_server_message_redacted", DiagnosticRedisServerMessageRedacted),
            ("preflight_diagnostic_cancellation_requested", DiagnosticCancellationRequested),
            ("preflight_diagnostic_aggregate_inner_types", DiagnosticAggregateInnerTypes),
            ("preflight_diagnostic_writer_failure_fail_closed", DiagnosticWriterFailureFailClosed),
            ("preflight_success_silent_and_order_unchanged", PreflightSuccessSilentAndOrderUnchanged),
            ("preflight_local_denial_unchanged", () => PreflightDenialUnchanged(false)),
            ("preflight_authority_denial_unchanged", () => PreflightDenialUnchanged(true)),
            ("parse_preflight_only_without_execute", ParsePreflightOnly),
            ("preflight_only_conflicting_modes_rejected", PreflightOnlyConflictingModesRejected),
            ("preflight_only_reverse_conflicting_modes_rejected", PreflightOnlyReverseConflictingModesRejected),
            ("preflight_only_duplicate_flag_rejected", PreflightOnlyDuplicateFlagRejected),
            ("preflight_only_stdin_required", PreflightOnlyStdinRequired),
            ("preflight_only_contract_validation_unchanged", PreflightOnlyContractValidationUnchanged),
            ("preflight_only_success_no_initialize", () => PreflightOnlyResult(new(true, "none", "none", true, true), 0)),
            ("preflight_only_unavailable_no_initialize", () => PreflightOnlyResult(PreflightResult.Unavailable(), 2)),
            ("preflight_only_local_closed_no_initialize", () => PreflightOnlyResult(new(true, "unknown", "unknown", false, false), 2)),
            ("preflight_only_authority_closed_no_initialize", () => PreflightOnlyResult(new(true, "unknown", "unknown", true, false), 2)),
            ("preflight_only_metadata_present_no_initialize", () => PreflightOnlyResult(new(true, "hash", "none", true, true), 2)),
            ("preflight_only_registry_present_no_initialize", () => PreflightOnlyResult(new(true, "none", "zset", true, true), 2)),
            ("preflight_only_keyspace_nonempty_no_initialize", () => PreflightOnlyResult(new(true, "none", "none", true, true, 1), 2)),
            ("preflight_only_exception_local_guard", () => PreflightOnlyPhaseException("local-guard")),
            ("preflight_only_exception_authority_guard", () => PreflightOnlyPhaseException("authority-guard")),
            ("preflight_only_exception_redis_database", () => PreflightOnlyPhaseException("redis-database")),
            ("preflight_only_exception_metadata_type", () => PreflightOnlyPhaseException("metadata-type")),
            ("preflight_only_exception_registry_type", () => PreflightOnlyPhaseException("registry-type")),
            ("preflight_only_exception_keyspace", () => PreflightOnlyPhaseException("keyspace")),
            ("preflight_only_real_flow_with_doubles_success", PreflightOnlyRealFlowWithDoublesSuccess),
            ("preflight_only_output_payload_redacted", PreflightOnlyOutputPayloadRedacted),
            ("preflight_only_runner_call_graph_read_only", PreflightOnlyRunnerCallGraphReadOnly),
            ("normal_entry_dispatch_unchanged", NormalEntryDispatchUnchanged),
            ("admin_info_operational_allowadmin_false", AdminInfoOperationalAllowAdminFalse),
            ("admin_info_isolated_allowadmin_true_same_settings", AdminInfoIsolatedConfiguration),
            ("admin_info_reader_capability_closed", AdminInfoReaderCapabilityClosed),
            ("admin_info_empty_global_count", () => AdminInfoCounts("# Keyspace\r\n", 0, 0)),
            ("admin_info_db0_empty_db1_present", () => AdminInfoCounts("# Keyspace\r\ndb0:keys=0,expires=0,avg_ttl=0\r\ndb1:keys=1,expires=0,avg_ttl=0\r\n", 1, 0)),
            ("admin_info_db0_one", () => AdminInfoCounts("# Keyspace\r\ndb0:keys=1,expires=0,avg_ttl=0\r\n", 1, 1)),
            ("admin_info_multiple_databases_sum", () => AdminInfoCounts("# Keyspace\r\ndb0:keys=1,expires=0,avg_ttl=0\r\ndb2:keys=3,expires=2,avg_ttl=500\r\n", 4, 1)),
            ("admin_info_disconnected_rejected", () => AdminInfoSelectionRejected(f => f.Connected = false)),
            ("admin_info_replica_rejected", () => AdminInfoSelectionRejected(f => f.Replica = true)),
            ("admin_info_ambiguous_endpoint_rejected", () => AdminInfoSelectionRejected(f => f.Endpoints = [new DnsEndPoint(Endpoint, 6379), new DnsEndPoint(Endpoint, 6379)])),
            ("admin_info_missing_endpoint_rejected", () => AdminInfoSelectionRejected(f => f.Endpoints = [])),
            ("admin_info_wrong_endpoint_rejected", () => AdminInfoSelectionRejected(f => f.Endpoints = [new DnsEndPoint("other.invalid", 6379)])),
            ("admin_info_wrong_port_rejected", () => AdminInfoSelectionRejected(f => f.Endpoints = [new DnsEndPoint(Endpoint, 6380)])),
            ("admin_info_server_endpoint_mismatch_rejected", () => AdminInfoSelectionRejected(f => f.ServerEndpoint = new DnsEndPoint("other.invalid", 6379))),
            ("admin_info_cluster_server_rejected", () => AdminInfoSelectionRejected(f => f.Type = ServerType.Cluster)),
            ("admin_info_server_exception_unavailable_redacted", AdminInfoServerException),
            ("admin_info_cancel_during_info_fail_closed", AdminInfoCancellation),
            ("admin_info_cancel_before_connect", AdminInfoCancellationBeforeConnect),
            ("admin_info_cancel_connect_eventual_disposal", AdminInfoCancellationDuringConnect),
            ("admin_info_null_reply_fail_closed", AdminInfoNullReply),
            ("admin_info_db1_blocks_operational_initialize", AdminInfoDb1BlocksInitialize),
            ("admin_info_preflight_only_success_no_initialize", AdminInfoPreflightOnlySuccess),
            ("admin_info_readback_global_and_db0_preserved", AdminInfoReadbackCounts)
        };

        var passed = 0;
        foreach (var (name, body) in tests)
        {
            try
            {
                await body();
                Console.WriteLine("PASS " + name);
                passed++;
            }
            catch (Exception exception)
            {
                Console.WriteLine("FAIL " + name + ": " + exception.GetType().Name);
            }
        }

        Console.WriteLine($"TESTS PASS={passed}/{tests.Length}");
        Environment.ExitCode = passed == tests.Length ? 0 : 1;
    }

    private static Task ParseValidArguments()
    {
        var parsed = BootstrapCommandLine.Parse(ValidArgs());
        Assert.True(parsed.Success);
        Assert.Equal(Endpoint, parsed.Arguments!.Endpoint);
        Assert.Equal(Generation, parsed.Arguments.Generation);
        Assert.Equal(BootstrapContract.AuthorityTable, parsed.Arguments.AuthorityTable);
        Assert.Equal(BootstrapContract.Region, parsed.Arguments.Region);
        Assert.Equal(30, parsed.Arguments.TimeoutSeconds);
        return Task.CompletedTask;
    }

    private static Task MissingExecuteFlagRejected()
    {
        var args = ValidArgs().Where(argument => argument != "--execute-initialize").ToArray();
        Assert.Equal("execute-initialize-required", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task MissingSecretStdinRejected()
    {
        var args = ValidArgs().Where(argument => argument != "--secret-stdin").ToArray();
        Assert.Equal("secret-stdin-required", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task OldEndpointRejected()
    {
        var args = ValidArgs();
        args[3] = BootstrapContract.OldEndpoint;
        Assert.Equal("endpoint-not-new-v2", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task OtherEndpointRejected()
    {
        var args = ValidArgs();
        args[3] = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com";
        Assert.Equal("endpoint-not-new-v2", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task InvalidGenerationRejected()
    {
        var args = ValidArgs();
        args[5] = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";
        Assert.Equal("generation-invalid", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task WrongAuthorityTableRejected()
    {
        var args = ValidArgs();
        args[7] = "enver-dev-other-authority";
        Assert.Equal("authority-table-not-approved", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task WrongRegionRejected()
    {
        var args = ValidArgs();
        args[9] = "us-west-2";
        Assert.Equal("region-not-approved", BootstrapCommandLine.Parse(args).Error);
        return Task.CompletedTask;
    }

    private static Task SecretArgumentRejectedWithoutEcho()
    {
        var parsed = BootstrapCommandLine.Parse([.. ValidArgs(), "--password", TestSecret]);
        Assert.Equal("secret-argument-forbidden", parsed.Error);
        Assert.DoesNotContain(TestSecret, parsed.Error ?? string.Empty);
        return Task.CompletedTask;
    }

    private static Task SnapshotTextExact()
    {
        Assert.Equal(
            "development|quota-development-academic-initial-v1|hmac-development-fecf274e-v1|academic-initial|5000|67108864|" +
            "3|900000000|20|900000000|10|60000000|60|60000000|5|60000000|300|60000000|30|60000000",
            BootstrapContract.SnapshotText);
        Assert.Equal(900000000L, BootstrapContract.MaximumWindowMicroseconds);
        return Task.CompletedTask;
    }

    private static Task ConfigurationCaptureExact()
    {
        var configuration = BootstrapContract.CreateConfiguration(Generation);
        Assert.Equal(Generation, configuration.Generation);
        Assert.Equal(BootstrapContract.Environment, configuration.Snapshot.Environment);
        Assert.Equal(BootstrapContract.ConfigurationVersion, configuration.Snapshot.ConfigurationVersion);
        Assert.Equal(BootstrapContract.IdentityReference, configuration.Snapshot.IdentityConfigurationReference);
        Assert.Equal(BootstrapContract.PolicyRevision, configuration.Snapshot.Policy.Revision);
        Assert.Equal(5000, configuration.Snapshot.Capacity.MaxActivePartitions);
        Assert.Equal(67108864L, configuration.Snapshot.Capacity.MemoryBudgetBytes);
        Assert.Equal(7, configuration.Snapshot.Policy.Rules.Count);
        return Task.CompletedTask;
    }

    private static Task IncorrectConfigurationVersionAborts()
    {
        Assert.Throws("configuration-reference-mismatch",
            () => BootstrapContract.ValidateConfiguration("quota-other-v1", BootstrapContract.IdentityReference));
        return Task.CompletedTask;
    }

    private static Task IncorrectIdentityReferenceAborts()
    {
        Assert.Throws("configuration-reference-mismatch",
            () => BootstrapContract.ValidateConfiguration(BootstrapContract.ConfigurationVersion, "hmac-other-v1"));
        return Task.CompletedTask;
    }

    private static Task RedisConnectionStringRoundtrip()
    {
        var text = RedisConnectionStringFactory.Create(Endpoint, new RedisCredential("enver-dev-runtime", TestSecret));
        var parsed = ConfigurationOptions.Parse(text);
        Assert.True(parsed.Ssl);
        Assert.Equal(0, parsed.DefaultDatabase);
        Assert.Equal("enver-dev-runtime", parsed.User);
        Assert.Equal(TestSecret, parsed.Password);
        Assert.Equal(1, parsed.EndPoints.Count);
        return Task.CompletedTask;
    }

    private static Task RedisConnectionStringRejectsComma()
    {
        Assert.Throws("redis-credential-not-representable",
            () => RedisConnectionStringFactory.Create(Endpoint, new RedisCredential("enver-dev-runtime", "with,comma")));
        return Task.CompletedTask;
    }

    private static async Task StdinSecretParsesAndRedacts()
    {
        var source = new StdinSecretSource(
            new StringReader("{\"username\":\"enver-dev-runtime\",\"password\":\"" + TestSecret + "\"}"),
            true);
        var credential = await source.ReadAsync(CancellationToken.None);
        Assert.Equal("enver-dev-runtime", credential.Username);
        Assert.Equal(TestSecret, credential.Password);
        Assert.Equal("[REDACTED]", credential.ToString());
        Assert.DoesNotContain(TestSecret, credential.ToString());
    }

    private static async Task StdinSecretInvalidDoesNotEcho()
    {
        var source = new StdinSecretSource(new StringReader("not-json-" + TestSecret), true);
        try
        {
            await source.ReadAsync(CancellationToken.None);
            throw new InvalidOperationException("Expected failure");
        }
        catch (BootstrapValidationException exception)
        {
            Assert.Equal("secret-input-invalid", exception.Code);
            Assert.DoesNotContain(TestSecret, exception.Message);
        }
    }

    private static Task ReadbackExactPasses()
    {
        var result = ReadbackValidator.Validate(SuccessfulInput(), Generation);
        Assert.True(result.IsMatch);
        Assert.Equal(0, result.Mismatches.Count);
        return Task.CompletedTask;
    }

    private static Task ReadbackDirtyMismatch()
    {
        var input = SuccessfulInput();
        var metadata = new Dictionary<string, string?>(input.Metadata, StringComparer.Ordinal) { ["dirty"] = "1" };
        var result = ReadbackValidator.Validate(input with { Metadata = metadata }, Generation);
        Assert.True(!result.IsMatch);
        Assert.Contains("dirty", result.Mismatches);
        return Task.CompletedTask;
    }

    private static Task ReadbackSnapshotMismatch()
    {
        var input = SuccessfulInput();
        var metadata = new Dictionary<string, string?>(input.Metadata, StringComparer.Ordinal) { ["snapshot"] = "other" };
        var result = ReadbackValidator.Validate(input with { Metadata = metadata }, Generation);
        Assert.True(!result.IsMatch);
        Assert.Contains("snapshot", result.Mismatches);
        return Task.CompletedTask;
    }

    private static Task ReadbackRegistryPresentMismatch()
    {
        var result = ReadbackValidator.Validate(SuccessfulInput() with { RegistryType = "zset", RegistryTtl = -1 }, Generation);
        Assert.True(!result.IsMatch);
        Assert.Contains("registry-type", result.Mismatches);
        Assert.Contains("registry-pttl", result.Mismatches);
        return Task.CompletedTask;
    }

    private static Task ReadbackKeyspaceExtraMismatch()
    {
        var result = ReadbackValidator.Validate(SuccessfulInput() with { KeyspaceTotal = 2 }, Generation);
        Assert.True(!result.IsMatch);
        Assert.Contains("keyspace", result.Mismatches);
        return Task.CompletedTask;
    }

    private static async Task ServiceMetadataPresentStops()
    {
        var backend = new FakeBackend { Preflight = new(true, "hash", "none", true, true) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("metadata-present", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
    }

    private static async Task ServiceRegistryPresentStops()
    {
        var backend = new FakeBackend { Preflight = new(true, "none", "zset", true, true) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("registry-present", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task ServiceGuardClosedStops()
    {
        var backend = new FakeBackend { Preflight = new(true, "unknown", "unknown", false, true) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("guard-closed", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task ServiceKeyspaceNotEmptyStops()
    {
        var backend = new FakeBackend { Preflight = new(true, "none", "none", true, true, 1) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("keyspace-not-empty", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task ServiceAuthorityClosedStops()
    {
        var backend = new FakeBackend { Preflight = new(true, "unknown", "unknown", true, false) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("authority-closed", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task ServicePreflightUnavailableStops()
    {
        var backend = new FakeBackend { Preflight = PreflightResult.Unavailable() };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("preflight-unavailable", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task ServiceInitializePass()
    {
        var backend = new FakeBackend();
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Passed, result.Outcome);
        Assert.Equal("passed", result.Code);
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(1, backend.InitializeCalls);
        Assert.Equal(1, backend.ReadbackCalls);
    }

    private static async Task ServiceInitializeUncertainStops()
    {
        var backend = new FakeBackend { Initialize = new("uncertain", "state", false) };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("initialize-uncertain", result.Code);
        Assert.Equal(0, backend.ReadbackCalls);
    }

    private static async Task ServiceReadbackDirtyStops()
    {
        var backend = new FakeBackend { Readback = ReadbackResult.Failure("dirty") };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("readback-mismatch", result.Code);
        Assert.Contains("dirty", result.Detail);
    }

    private static async Task SecretNeverInLogsOrArgv()
    {
        var backend = new FakeBackend();
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        using var output = new StringWriter();
        BootstrapReporter.Write(output, result);
        Assert.DoesNotContain(TestSecret, output.ToString());

        var credential = new RedisCredential("enver-dev-runtime", TestSecret);
        Assert.DoesNotContain(TestSecret, credential.ToString());
        Assert.DoesNotContain(TestSecret, string.Join(' ', ValidArgs()));
    }

    private static async Task DiagnosticPhase(string phase)
    {
        var exception = new RedisConnectionException(ConnectionFailureType.SocketFailure, CommandFlags.DemandMaster,
            "Potentially sensitive exception message " + TestSecret,
            new InvalidOperationException("Sensitive inner message " + TestSecret, new IOException(TestSecret)), CommandStatus.Unknown);
        var diagnostic = await AssertDiagnosticFailure(phase, exception);
        Assert.True(diagnostic.Contains("inner=InvalidOperationException,IOException", StringComparison.Ordinal));
        Assert.True(diagnostic.Contains("category=redis-connection code=SocketFailure", StringComparison.Ordinal));
    }

    private static async Task<string> AssertDiagnosticFailure(string phase, Exception exception,
        CancellationTokenSource? cancellation = null)
    {
        using var output = new StringWriter();
        var backend = new DiagnosticBackend(phase, exception, output);
        if (cancellation is not null) backend.BeforeFailure = cancellation.Cancel;
        var result = await new BootstrapService().ExecuteAsync(Request, backend, cancellation?.Token ?? CancellationToken.None);
        await AssertStoppedBeforeInitialize(backend, result);
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        var text = output.ToString();
        Assert.True(text.StartsWith("PREFLIGHT DIAGNOSTIC: phase=" + phase + " exception=" + exception.GetType().Name + " ", StringComparison.Ordinal));
        Assert.True(text.Contains("cancelled=" + (cancellation is null ? "false" : "true"), StringComparison.Ordinal));
        Assert.Equal(1, text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
        Assert.DoesNotContain(TestSecret, text);
        Assert.DoesNotContain(exception.Message, text);
        Assert.DoesNotContain(Endpoint, text);
        Assert.DoesNotContain("StackTrace", text);
        Assert.Equal(phase, backend.Calls[^1]);
        return text;
    }

    private static Task AssertStoppedBeforeInitialize(DiagnosticBackend backend, BootstrapResult result)
    {
        Assert.Equal(BootstrapOutcome.Stopped, result.Outcome);
        Assert.Equal("preflight-unavailable", result.Code);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        return Task.CompletedTask;
    }

    private static async Task DiagnosticAwsAllowlistedCode()
    {
        var exception = new AmazonServiceException(TestSecret) { ErrorCode = "AccessDeniedException" };
        var text = await AssertDiagnosticFailure("authority-guard", exception);
        Assert.True(text.Contains("category=aws code=AccessDeniedException", StringComparison.Ordinal));
    }

    private static async Task DiagnosticAwsUnknownCodeRedacted()
    {
        var exception = new AmazonServiceException(TestSecret) { ErrorCode = "Unknown-" + TestSecret + "\r\nInjected" };
        var text = await AssertDiagnosticFailure("authority-guard", exception);
        Assert.True(text.Contains("category=aws code=unclassified", StringComparison.Ordinal));
        Assert.DoesNotContain("Injected", text);
    }

    private static async Task DiagnosticRedisTimeoutCategory()
    {
        var text = await AssertDiagnosticFailure("metadata-type", new RedisTimeoutException(CommandFlags.DemandMaster, TestSecret, CommandStatus.Unknown));
        Assert.True(text.Contains("category=redis-timeout code=Unknown", StringComparison.Ordinal));
    }

    private static async Task DiagnosticRedisServerMessageRedacted()
    {
        var text = await AssertDiagnosticFailure("keyspace", new RedisServerException(RedisErrorKind.NoPermission,
            CommandFlags.DemandMaster, "NOPERM " + TestSecret + " " + Endpoint));
        Assert.True(text.Contains("category=redis-server code=NoPermission", StringComparison.Ordinal));
        Assert.DoesNotContain("NOPERM", text); // No extraer códigos analizando Message.
    }

    private static async Task DiagnosticCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        await AssertDiagnosticFailure("registry-type", new OperationCanceledException(TestSecret), cancellation);
    }

    private static async Task DiagnosticAggregateInnerTypes()
    {
        var exception = new AggregateException(TestSecret, new IOException(TestSecret),
            new InvalidOperationException(TestSecret, new ArgumentException(TestSecret)));
        var text = await AssertDiagnosticFailure("redis-database", exception);
        Assert.True(text.Contains("inner=IOException,InvalidOperationException,ArgumentException", StringComparison.Ordinal));
    }

    private static async Task DiagnosticWriterFailureFailClosed()
    {
        using var output = new FailingDiagnosticWriter();
        var backend = new DiagnosticBackend("metadata-type", new InvalidOperationException(TestSecret), output);
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        await AssertStoppedBeforeInitialize(backend, result);
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
    }

    private static async Task PreflightSuccessSilentAndOrderUnchanged()
    {
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, output);
        var result = await backend.PreflightAsync(CancellationToken.None); // Nunca ExecuteAsync con éxito operacional.
        Assert.Equal(new PreflightResult(true, "none", "none", true, true, 0), result);
        Assert.Equal("local-guard,authority-guard,redis-database,metadata-type,registry-type,keyspace", string.Join(',', backend.Calls));
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(0, backend.InitializeCalls);
    }

    private static async Task PreflightDenialUnchanged(bool global)
    {
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, output) { LocalReady = global, GlobalReady = false };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal(global ? "authority-closed" : "guard-closed", result.Code);
        Assert.Equal(global ? "local-guard,authority-guard" : "local-guard", string.Join(',', backend.Calls));
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
    }

    private static string[] PreflightOnlyArgs() => ValidArgs()
        .Select(argument => argument == "--execute-initialize" ? "--preflight-only" : argument).ToArray();

    private static Task ParsePreflightOnly()
    {
        var parsed = BootstrapCommandLine.Parse(PreflightOnlyArgs());
        Assert.True(parsed.Success);
        Assert.True(parsed.Arguments!.PreflightOnly);
        Assert.Equal(Endpoint, parsed.Arguments.Endpoint);
        Assert.Equal(Generation, parsed.Arguments.Generation);
        Assert.True(!BootstrapCommandLine.Parse(ValidArgs()).Arguments!.PreflightOnly);
        return Task.CompletedTask;
    }

    private static Task PreflightOnlyConflictingModesRejected()
    {
        Assert.Equal("execution-mode-conflict", BootstrapCommandLine.Parse([.. ValidArgs(), "--preflight-only"]).Error);
        return Task.CompletedTask;
    }

    private static Task PreflightOnlyReverseConflictingModesRejected()
    {
        Assert.Equal("execution-mode-conflict", BootstrapCommandLine.Parse([.. PreflightOnlyArgs(), "--execute-initialize"]).Error);
        return Task.CompletedTask;
    }

    private static Task PreflightOnlyDuplicateFlagRejected()
    {
        Assert.Equal("duplicate-argument", BootstrapCommandLine.Parse([.. PreflightOnlyArgs(), "--preflight-only"]).Error);
        return Task.CompletedTask;
    }

    private static Task PreflightOnlyStdinRequired()
    {
        Assert.Equal("secret-stdin-required", BootstrapCommandLine.Parse(PreflightOnlyArgs()
            .Where(argument => argument != "--secret-stdin").ToArray()).Error);
        return Task.CompletedTask;
    }

    private static Task PreflightOnlyContractValidationUnchanged()
    {
        foreach (var (index, value, expected) in new[]
        {
            (3, BootstrapContract.OldEndpoint, "endpoint-not-new-v2"),
            (5, "00000000000000000000000000000000", "generation-invalid"),
            (7, "other-table", "authority-table-not-approved"),
            (9, "us-west-2", "region-not-approved")
        })
        {
            var args = PreflightOnlyArgs();
            args[index] = value;
            Assert.Equal(expected, BootstrapCommandLine.Parse(args).Error);
        }
        Assert.Equal("timeout-invalid", BootstrapCommandLine.Parse([.. PreflightOnlyArgs(), "--timeout-seconds", "4"]).Error);
        Assert.Equal("secret-argument-forbidden", BootstrapCommandLine.Parse([.. PreflightOnlyArgs(), "--password", TestSecret]).Error);
        return Task.CompletedTask;
    }

    private static async Task<int> RunEntryWithDouble(IBootstrapBackend backend, TextWriter output, bool preflightOnly = true,
        CancellationToken cancellationToken = default)
    {
        var parsed = BootstrapCommandLine.Parse(preflightOnly ? PreflightOnlyArgs() : ValidArgs());
        Assert.True(parsed.Success);
        var secret = new StdinSecretSource(new StringReader(
            "{\"username\":\"test-only-user\",\"password\":\"" + TestSecret + "\"}"), true);
        var constructed = 0;
        var exit = await BootstrapEntryPoint.RunValidatedAsync(parsed.Arguments!, secret, (arguments, credential) =>
        {
            constructed++;
            Assert.Equal(preflightOnly, arguments.PreflightOnly);
            Assert.Equal(Generation, arguments.Generation);
            Assert.Equal("test-only-user", credential.Username);
            Assert.Equal(TestSecret, credential.Password);
            return backend;
        }, output, cancellationToken);
        Assert.Equal(1, constructed);
        Assert.DoesNotContain(TestSecret, output.ToString()!);
        Assert.DoesNotContain("test-only-user", output.ToString()!);
        return exit;
    }

    private static async Task PreflightOnlyResult(PreflightResult preflight, int expectedExit)
    {
        using var output = new StringWriter();
        var backend = new FakeBackend { Preflight = preflight };
        var exit = await RunEntryWithDouble(backend, output);
        Assert.Equal(expectedExit, exit);
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(1, backend.DisposeCalls);
        Assert.True(output.ToString().StartsWith(preflight.ProbeSucceeded
            ? "PREFLIGHT-ONLY RESULT: AVAILABLE" : "PREFLIGHT-ONLY RESULT: UNAVAILABLE", StringComparison.Ordinal));
        Assert.True(output.ToString().Contains("ProbeSucceeded=" + preflight.ProbeSucceeded, StringComparison.Ordinal));
        Assert.True(output.ToString().Contains("LocalReady=" + preflight.LocalReady, StringComparison.Ordinal));
        Assert.True(output.ToString().Contains("GlobalReady=" + preflight.GlobalReady, StringComparison.Ordinal));
        Assert.DoesNotContain("BOOTSTRAP PASSED", output.ToString());
    }

    private static async Task PreflightOnlyPhaseException(string phase)
    {
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var exception = new InvalidOperationException(TestSecret, new IOException(TestSecret));
        var backend = new DiagnosticBackend(phase, exception, diagnostic);
        var exit = await RunEntryWithDouble(backend, output);
        Assert.Equal(2, exit);
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        Assert.True(diagnostic.ToString().Contains("phase=" + phase + " exception=InvalidOperationException inner=IOException cancelled=false", StringComparison.Ordinal));
        Assert.True(output.ToString().StartsWith("PREFLIGHT-ONLY RESULT: UNAVAILABLE", StringComparison.Ordinal));
        Assert.DoesNotContain(TestSecret, diagnostic.ToString());
        Assert.DoesNotContain(exception.Message, diagnostic.ToString());
    }

    private static async Task PreflightOnlyRealFlowWithDoublesSuccess()
    {
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic);
        Assert.Equal(0, await RunEntryWithDouble(backend, output));
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal("local-guard,authority-guard,redis-database,metadata-type,registry-type,keyspace", string.Join(',', backend.Calls));
        Assert.Equal(string.Empty, diagnostic.ToString());
    }

    private static async Task PreflightOnlyOutputPayloadRedacted()
    {
        using var output = new StringWriter();
        var backend = new FakeBackend { Preflight = new(true, TestSecret + "\r\nInjected", TestSecret, true, true) };
        Assert.Equal(2, await RunEntryWithDouble(backend, output));
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.True(output.ToString().Contains("MetadataType=unknown RegistryType=unknown", StringComparison.Ordinal));
        Assert.DoesNotContain("Injected", output.ToString());
    }

    private static Task PreflightOnlyRunnerCallGraphReadOnly()
    {
        var runner = typeof(PreflightOnlyRunner).GetMethod("RunAsync", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.Equal(typeof(Func<CancellationToken, ValueTask<PreflightResult>>), runner.GetParameters()[0].ParameterType);
        var pending = new Queue<MethodBase>();
        var seen = new HashSet<MethodBase>();
        pending.Enqueue(runner);
        while (pending.Count > 0)
        {
            var method = pending.Dequeue();
            if (!seen.Add(method)) continue;
            if (method.GetCustomAttribute<AsyncStateMachineAttribute>() is { } asyncMethod)
                pending.Enqueue(asyncMethod.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!);
            foreach (var called in CalledMethods(method))
            {
                Assert.True(called.DeclaringType != typeof(BootstrapService));
                Assert.True(called.DeclaringType != typeof(RedisBootstrapBackend));
                Assert.True(called.Name is not ("InitializeAsync" or "InitializeVerifiedNewEnvironmentAsync" or "ReadbackAsync"));
                if (called.DeclaringType?.Assembly == runner.DeclaringType!.Assembly) pending.Enqueue(called);
            }
        }
        Assert.True(seen.Count >= 3); // Incluye cuerpo async y proyección de salida, no solo el stub inicial.
        return Task.CompletedTask;
    }

    private static IEnumerable<MethodBase> CalledMethods(MethodBase method)
    {
        var bytes = method.GetMethodBody()?.GetILAsByteArray() ?? [];
        var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode)).Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opcode => unchecked((ushort)opcode.Value));
        for (var index = 0; index < bytes.Length;)
        {
            ushort value = bytes[index++];
            if (value == 0xfe) value = (ushort)(0xfe00 | bytes[index++]);
            var opcode = opcodes[value];
            var start = index;
            index += opcode.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(bytes, start),
                _ => 4
            };
            if (opcode.OperandType == OperandType.InlineMethod)
                yield return method.Module.ResolveMethod(BitConverter.ToInt32(bytes, start),
                    method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null)!;
        }
    }

    private static async Task NormalEntryDispatchUnchanged()
    {
        using var output = new StringWriter();
        var backend = new FakeBackend();
        Assert.Equal(0, await RunEntryWithDouble(backend, output, preflightOnly: false));
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(1, backend.InitializeCalls); // Solo contador del FakeBackend; nunca lifecycle real.
        Assert.Equal(1, backend.ReadbackCalls);
        Assert.Equal(1, backend.DisposeCalls);
        Assert.Equal("BOOTSTRAP PASSED: initialize + readback exact" + Environment.NewLine, output.ToString());
    }

    private static Task AdminInfoOperationalAllowAdminFalse()
    {
        var fixture = new AdminInfoFixture();
        var normal = RedisConnections.CreateConfigurationOptions(fixture.Options);
        Assert.True(!normal.AllowAdmin);
        Assert.Equal(0, normal.DefaultDatabase);
        Assert.True(normal.Ssl);
        Assert.Equal(0, fixture.ConnectCalls);
        return Task.CompletedTask;
    }

    private static async Task AdminInfoIsolatedConfiguration()
    {
        var fixture = new AdminInfoFixture();
        var normal = RedisConnections.CreateConfigurationOptions(fixture.Options);
        await fixture.Reader.ReadAsync(CancellationToken.None);
        var admin = fixture.CapturedConfiguration!;
        Assert.True(admin.AllowAdmin);
        Assert.True(!normal.AllowAdmin);
        Assert.True(!RedisConnections.CreateConfigurationOptions(fixture.Options).AllowAdmin);
        Assert.Equal(normal.Ssl, admin.Ssl);
        Assert.Equal(normal.SslHost, admin.SslHost);
        Assert.Equal(Endpoint, admin.SslHost);
        Assert.Equal(normal.DefaultDatabase, admin.DefaultDatabase);
        Assert.Equal(normal.AbortOnConnectFail, admin.AbortOnConnectFail);
        Assert.Equal(normal.ConnectTimeout, admin.ConnectTimeout);
        Assert.Equal(normal.SyncTimeout, admin.SyncTimeout);
        Assert.Equal(normal.AsyncTimeout, admin.AsyncTimeout);
        Assert.Equal(normal.IncludeDetailInExceptions, admin.IncludeDetailInExceptions);
        Assert.Equal(normal.User, admin.User);
        Assert.Equal(normal.Password, admin.Password);
        Assert.Equal(TestSecret, admin.Password);
        Assert.Equal(1, admin.EndPoints.Count);
        Assert.Equal(normal.EndPoints[0], admin.EndPoints[0]);
        Assert.Equal(normal.BacklogPolicy, admin.BacklogPolicy);
        Assert.Equal(normal.ReconnectRetryPolicy.GetType(), admin.ReconnectRetryPolicy.GetType());
        Assert.Equal(1, fixture.InfoCalls);
        Assert.Equal(1, fixture.GetServerCalls);
        Assert.Equal(1, fixture.DisposeCalls);
    }

    private static Task AdminInfoReaderCapabilityClosed()
    {
        Assert.Equal("ReadAsync", typeof(IBootstrapKeyspaceReader).GetMethods().Single().Name);
        Assert.Equal(typeof(ValueTask<string>), typeof(IBootstrapKeyspaceReader).GetMethods().Single().ReturnType);
        Assert.Equal("ReadAsync", typeof(RedisBootstrapKeyspaceReader)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Single().Name);
        var pending = new Queue<MethodBase>();
        var seen = new HashSet<MethodBase>();
        pending.Enqueue(typeof(RedisBootstrapKeyspaceReader).GetMethod("ReadAsync")!);
        var serverCalls = new HashSet<string>();
        while (pending.Count > 0)
        {
            var method = pending.Dequeue();
            if (!seen.Add(method)) continue;
            if (method.GetCustomAttribute<AsyncStateMachineAttribute>() is { } asyncMethod)
                pending.Enqueue(asyncMethod.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!);
            foreach (var called in CalledMethods(method))
            {
                Assert.True(called.DeclaringType != typeof(BootstrapService));
                Assert.True(called.Name is not ("InitializeAsync" or "InitializeVerifiedNewEnvironmentAsync" or "GetDatabase" or "ExecuteAsync"));
                if (called.DeclaringType == typeof(IServer)) serverCalls.Add(called.Name);
                if (called.DeclaringType?.Assembly == typeof(RedisBootstrapKeyspaceReader).Assembly) pending.Enqueue(called);
            }
        }
        Assert.Equal("InfoRawAsync,get_EndPoint,get_IsConnected,get_IsReplica,get_ServerType",
            string.Join(',', serverCalls.OrderBy(name => name, StringComparer.Ordinal)));
        return Task.CompletedTask;
    }

    private static async Task AdminInfoCounts(string raw, long total, long db0)
    {
        var fixture = new AdminInfoFixture { Info = raw };
        var counts = await RedisBootstrapBackend.KeyspaceAsync(fixture.Reader, CancellationToken.None);
        Assert.Equal(total, counts.Total);
        Assert.Equal(db0, counts.Db0);
        Assert.Equal(1, fixture.ConnectCalls);
        Assert.Equal(1, fixture.InfoCalls);
        Assert.Equal(1, fixture.GetServerCalls);
        Assert.Equal(1, fixture.DisposeCalls);
    }

    private static async Task AdminInfoSelectionRejected(Action<AdminInfoFixture> configure)
    {
        var fixture = new AdminInfoFixture();
        configure(fixture);
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        Assert.Equal(2, await RunEntryWithDouble(backend, output));
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(0, fixture.InfoCalls);
        Assert.Equal(1, fixture.ConnectCalls);
        Assert.Equal(1, fixture.DisposeCalls);
        Assert.True(diagnostic.ToString().Contains("phase=keyspace exception=InvalidOperationException", StringComparison.Ordinal));
        Assert.DoesNotContain(TestSecret, diagnostic.ToString());
        Assert.DoesNotContain(Endpoint, diagnostic.ToString());
    }

    private static async Task AdminInfoServerException()
    {
        var fixture = new AdminInfoFixture { Failure = new RedisCommandException(TestSecret) };
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        await AssertStoppedBeforeInitialize(backend, result);
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        Assert.Equal(1, fixture.InfoCalls);
        Assert.Equal(1, fixture.DisposeCalls);
        Assert.True(diagnostic.ToString().Contains("phase=keyspace exception=RedisCommandException", StringComparison.Ordinal));
        BootstrapReporter.Write(output, result);
        Assert.DoesNotContain(TestSecret, diagnostic.ToString() + output);
    }

    private static async Task AdminInfoCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new AdminInfoFixture
        {
            InfoTask = new TaskCompletionSource<string?>().Task,
            OnInfo = cancellation.Cancel
        };
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        Assert.Equal(2, await RunEntryWithDouble(backend, output, cancellationToken: cancellation.Token));
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(1, fixture.InfoCalls);
        Assert.Equal(1, fixture.DisposeCalls);
        Assert.True(diagnostic.ToString().Contains("phase=keyspace", StringComparison.Ordinal));
        Assert.True(diagnostic.ToString().Contains("cancelled=true", StringComparison.Ordinal));
        Assert.DoesNotContain(TestSecret, diagnostic.ToString());
    }

    private static async Task AdminInfoCancellationBeforeConnect()
    {
        var fixture = new AdminInfoFixture();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { await fixture.Reader.ReadAsync(cancellation.Token); }
        catch (OperationCanceledException)
        {
            Assert.Equal(0, fixture.ConnectCalls);
            Assert.Equal(0, fixture.InfoCalls);
            return;
        }
        throw new InvalidOperationException("Cancellation not honored");
    }

    private static async Task AdminInfoCancellationDuringConnect()
    {
        var connected = new TaskCompletionSource<IConnectionMultiplexer>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fixture = new AdminInfoFixture { ConnectTask = connected.Task };
        using var cancellation = new CancellationTokenSource();
        var read = fixture.Reader.ReadAsync(cancellation.Token).AsTask();
        cancellation.Cancel();
        var canceled = false;
        try { await read; }
        catch (OperationCanceledException) { canceled = true; }
        Assert.True(canceled);
        connected.SetResult(fixture.Multiplexer);
        await fixture.Disposed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(1, fixture.ConnectCalls);
        Assert.Equal(0, fixture.InfoCalls);
        Assert.Equal(1, fixture.DisposeCalls);
    }

    private static async Task AdminInfoNullReply()
    {
        var fixture = new AdminInfoFixture { Info = null };
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        Assert.Equal(2, await RunEntryWithDouble(backend, output));
        Assert.Equal(PreflightResult.Unavailable(), backend.LastPreflight);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(1, fixture.DisposeCalls);
    }

    private static async Task AdminInfoDb1BlocksInitialize()
    {
        var fixture = new AdminInfoFixture { Info = "# Keyspace\r\ndb1:keys=1,expires=0,avg_ttl=0\r\n" };
        using var diagnostic = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        var result = await new BootstrapService().ExecuteAsync(Request, backend, CancellationToken.None);
        Assert.Equal("keyspace-not-empty", result.Code);
        Assert.Equal(1L, backend.LastPreflight!.KeyspaceTotal);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(string.Empty, diagnostic.ToString());
    }

    private static async Task AdminInfoPreflightOnlySuccess()
    {
        var fixture = new AdminInfoFixture();
        using var diagnostic = new StringWriter();
        using var output = new StringWriter();
        var backend = new DiagnosticBackend("", null, diagnostic) { KeyspaceReader = fixture.Reader };
        Assert.Equal(0, await RunEntryWithDouble(backend, output));
        Assert.Equal(1, backend.PreflightCalls);
        Assert.Equal(0, backend.InitializeCalls);
        Assert.Equal(0, backend.ReadbackCalls);
        Assert.Equal(0L, backend.LastPreflight!.KeyspaceTotal);
        Assert.Equal(1, fixture.InfoCalls);
        Assert.Equal(1, fixture.DisposeCalls);
        Assert.DoesNotContain(TestSecret, diagnostic.ToString() + output);
    }

    private static async Task AdminInfoReadbackCounts()
    {
        foreach (var (info, match) in new[]
        {
            ("# Keyspace\r\ndb0:keys=1,expires=0,avg_ttl=0\r\n", true),
            ("# Keyspace\r\ndb1:keys=1,expires=0,avg_ttl=0\r\n", false),
            ("# Keyspace\r\ndb0:keys=1,expires=0,avg_ttl=0\r\ndb1:keys=1,expires=0,avg_ttl=0\r\n", false)
        })
        {
            var fixture = new AdminInfoFixture { Info = info };
            var counts = await RedisBootstrapBackend.KeyspaceAsync(fixture.Reader, CancellationToken.None);
            var result = ReadbackValidator.Validate(SuccessfulInput() with { KeyspaceTotal = counts.Total, Db0Keys = counts.Db0 }, Generation);
            Assert.Equal(match, result.IsMatch);
            if (!match) Assert.Contains("keyspace", result.Mismatches);
        }
    }

    private static QuotaReadbackInput SuccessfulInput()
    {
        var metadata = ReadbackValidator.ExpectedFields(Generation).ToDictionary(
            item => item.Field,
            item => item.Expected,
            StringComparer.Ordinal);
        metadata["last"] = 123456789L.ToString(CultureInfo.InvariantCulture);
        return new QuotaReadbackInput("hash", "none", 12, -1, metadata, -2, 1, 1);
    }
}

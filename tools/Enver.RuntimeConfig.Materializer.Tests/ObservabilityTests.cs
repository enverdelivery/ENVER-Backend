using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Enver.RuntimeConfig.Materializer;
using Microsoft.Extensions.Configuration;

internal static class ObservabilityTests
{
    private const string Canary = "LOCAL-CANARY-never-a-secret";
    private static int assertions, cases;
    private static void Check(bool ok) { assertions++; if (!ok) throw new Exception("Observability assertion failed (no payload)."); }

    internal static async Task Run(Dictionary<string, string> good, string[] canaries)
    {
        // Every fixed operation (except usage and cleanup) is failed, both with and without
        // a historic runtime. The real orchestration, mapping and options are shared.
        // Preserve the original 70 scenarios. New distinct file phases have their own regression suite.
        foreach (var phase in Enum.GetValues<DeliveryPhase>().Except([DeliveryPhase.Arguments, DeliveryPhase.TempCleanup,
            DeliveryPhase.TemporaryConfigParseValidation, DeliveryPhase.TemporaryConfigBindingValidation,
            DeliveryPhase.FinalConfigParseValidation, DeliveryPhase.FinalConfigBindingValidation]))
        foreach (var historic in new[] { false, true })
        {
            var runtime = new MemoryRuntime(good, historic);
            var trace = new DeliveryTrace(p => { if (p == phase) throw new IOException(Canary, new FormatException(Canary)); });
            await Failure(runtime, trace, phase, "IOException", canaries);
        }

        // Real parser failures, not only before-operation hooks.
        foreach (var (name, phase) in new[] { ("redis", DeliveryPhase.RedisParse), ("mongodb", DeliveryPhase.MongodbParse),
            ("hmac", DeliveryPhase.HmacParse), ("jwt", DeliveryPhase.JwtParse) })
        {
            var bad = new Dictionary<string, string>(good) { [name] = "{invalid:" + Canary };
            await Failure(new(bad, true), new(), phase, "JsonException", canaries);
        }

        // AWS errors go through the real pinned reader and full orchestration; no HTTP handler is used.
        foreach (var (name, phase) in new[] { ("redis", DeliveryPhase.RedisSecretRead), ("mongodb", DeliveryPhase.MongodbSecretRead),
            ("hmac", DeliveryPhase.HmacSecretRead), ("jwt", DeliveryPhase.JwtSecretRead) })
        {
            using var client = new AwsFailureClient(good, name, "AccessDeniedException");
            var runtime = new MemoryRuntime(good, true) { Reader = new PinnedSecretReader(client) };
            var result = await Failure(runtime, new(), phase, "AmazonServiceException", canaries);
            Check(result.GetProperty("category").GetString() == "aws");
            Check(result.GetProperty("code").GetString() == "AccessDeniedException");
            Check(client.Calls.Values.All(n => n == 1));
        }
        using (var client = new AwsFailureClient(good, "redis", Canary))
        {
            var runtime = new MemoryRuntime(good, false) { Reader = new PinnedSecretReader(client) };
            var result = await Failure(runtime, new(), DeliveryPhase.RedisSecretRead, "AmazonServiceException", canaries);
            Check(result.GetProperty("code").GetString() == "aws-error-unclassified");
        }
        var cancelled = new MemoryRuntime(good, false);
        var cancelledTrace = new DeliveryTrace(p => { if (p == DeliveryPhase.AwsCredentials) throw new HttpRequestException(Canary, new OperationCanceledException(Canary)); });
        var cancellation = await Failure(cancelled, cancelledTrace, DeliveryPhase.AwsCredentials, "HttpRequestException", canaries);
        Check(cancellation.GetProperty("cancelled").GetBoolean());
        Check(cancellation.GetProperty("inner")[0].GetString() == "OperationCanceledException");
        var unknownTrace = new DeliveryTrace(p => { if (p == DeliveryPhase.AwsCredentials) throw new LOCAL_CANARY_TypeName(); });
        await Failure(new(good, true), unknownTrace, DeliveryPhase.AwsCredentials, "Exception", [.. canaries, "LOCAL_CANARY_TypeName"]);
        var deepTrace = new DeliveryTrace(p =>
        {
            if (p == DeliveryPhase.AwsCredentials)
            {
                Exception nested = new FormatException(Canary);
                for (var i = 0; i < 10; i++) nested = new IOException(Canary, nested);
                throw nested;
            }
        });
        var bounded = await Failure(new(good, true), deepTrace, DeliveryPhase.AwsCredentials, "IOException", canaries);
        Check(bounded.GetProperty("inner").GetArrayLength() == 4);

        // A partial temp write is cleaned up without changing historic runtime or unrelated temp.
        await Failure(new(good, true) { PartialWriteFailure = true }, new(), DeliveryPhase.FileWrite, "IOException", canaries);
        await Failure(new(good, true) { PermissionFailure = true }, new(), DeliveryPhase.FilePermissions, "UnauthorizedAccessException", canaries);
        await Failure(new(good, true) { RenameFailure = true }, new(), DeliveryPhase.AtomicRename, "IOException", canaries);
        await Failure(new(good, true) { FinalValidationFailure = true }, new(), DeliveryPhase.FinalFileCheck, "IOException", canaries);
        await Failure(new(good, true) { PartialWriteFailure = true, CleanupFailure = true }, new(), DeliveryPhase.TempCleanup, "IOException", canaries);
        var tempBinding = new MemoryRuntime(good, true);
        await Failure(tempBinding, new(p => { if (p == DeliveryPhase.TemporaryConfigParseValidation && tempBinding.Temp is not null) throw new JsonException(Canary); }),
            DeliveryPhase.TemporaryConfigParseValidation, "JsonException", canaries);

        // Pin mismatch is preserved as a controlled safe code, without fallback/retry.
        using (var client = new AwsFailureClient(good, "redis", "unused") { PinMismatch = true })
        {
            var runtime = new MemoryRuntime(good, true) { Reader = new PinnedSecretReader(client) };
            var result = await Failure(runtime, new(), DeliveryPhase.RedisSecretRead, "DeliveryFailure", canaries);
            Check(result.GetProperty("code").GetString() == "secret-envelope-invalid");
            Check(client.Calls.Count == 1 && client.Calls["redis"] == 1);
        }

        // Complete success: no diagnostic failure record and a validated whole JSON commit once.
        var success = new MemoryRuntime(good, true);
        var sequence = new List<DeliveryPhase>();
        using (var output = new StringWriter())
        {
            Check(await Enver.RuntimeConfig.Materializer.Program.Run(["--materialize"], success, output, new(sequence.Add)) == 0);
            Check(success.Final is not null && success.Final != MemoryRuntime.Historic);
            Check(success.Temp is null && success.OtherTemp == MemoryRuntime.Other);
            Check(success.Commits == 1 && success.ResolveCalls == 1 && !success.ApiStarted);
            Check(success.Reads.Keys.SequenceEqual(Pins.All.Select(p => p.Name)) && success.Reads.Values.All(n => n == 1));
            Check(sequence.Last() == DeliveryPhase.AtomicRename);
            Check(sequence.IndexOf(DeliveryPhase.FinalFileCheck) < sequence.IndexOf(DeliveryPhase.AtomicRename));
            Check(canaries.All(s => !output.ToString().Contains(s, StringComparison.Ordinal)));
            using var parsed = JsonDocument.Parse(success.Final!);
            Check(parsed.RootElement.EnumerateObject().Count() == 6);
        }
        cases++;
        Console.WriteLine($"Observability scenarios: {cases}/{cases} PASS; {assertions}/{assertions} assertions PASS; real AWS/API calls and runtime filesystem writes=0.");
    }

    private static async Task<JsonElement> Failure(MemoryRuntime runtime, DeliveryTrace trace, DeliveryPhase phase,
        string type, string[] canaries)
    {
        using var output = new StringWriter();
        using var stderr = new StringWriter();
        var oldError = Console.Error;
        Console.SetError(stderr);
        int exit;
        try { exit = await Enver.RuntimeConfig.Materializer.Program.Run(["--materialize"], runtime, output, trace); }
        finally { Console.SetError(oldError); }
        Check(exit == 1);
        var text = output.ToString();
        Check(canaries.All(s => !text.Contains(s, StringComparison.Ordinal) && !stderr.ToString().Contains(s, StringComparison.Ordinal)));
        Check(stderr.ToString() == string.Empty);
        using var parsed = JsonDocument.Parse(text);
        var value = parsed.RootElement;
        Check(value.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(new[] { "phase", "exception", "inner", "cancelled", "category", "code", "validation-target" }.Order()));
        Check(value.GetProperty("phase").GetString() == DeliveryTrace.Label(phase));
        Check(value.GetProperty("exception").GetString() == type);
        Check(value.GetProperty("inner").ValueKind == JsonValueKind.Array);
        Check(value.GetProperty("category").ValueKind == JsonValueKind.String && value.GetProperty("code").ValueKind == JsonValueKind.String);
        Check(runtime.Final == (runtime.HadHistoric ? MemoryRuntime.Historic : null));
        Check(runtime.Temp is null && runtime.OtherTemp == MemoryRuntime.Other);
        Check(!runtime.ApiStarted && runtime.Commits == 0);
        Check(runtime.ResolveCalls <= 1 && runtime.Creates <= 1 && runtime.Reads.Values.All(n => n == 1));
        Check(runtime.Operations.Values.All(n => n == 1));
        Check(runtime.Disposed);
        cases++;
        return value.Clone();
    }

    private sealed class MemoryRuntime : IDeliveryRuntime, IRuntimeFileStore, ISecretReader
    {
        internal const string Historic = "historic-valid-file-preserved";
        internal const string Other = "unrelated-temp-preserved";
        private readonly Dictionary<string, string> payloads;
        internal readonly Dictionary<string, int> Reads = [], Operations = [];
        internal string? Final, Temp;
        internal string OtherTemp = Other;
        internal readonly bool HadHistoric;
        internal bool ApiStarted => false; // No API startup capability exists in the injected runtime.
        internal bool Disposed, PartialWriteFailure, PermissionFailure, RenameFailure, FinalValidationFailure, CleanupFailure;
        internal int ResolveCalls, Creates, Commits;
        internal ISecretReader? Reader;
        internal MemoryRuntime(Dictionary<string, string> values, bool historic)
        { payloads = values; HadHistoric = historic; Final = historic ? Historic : null; }
        public IRuntimeFileStore Files => this;
        public void Prepare() => Operation("prepare");
        public Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken)
        { ResolveCalls++; return Task.FromResult(Reader ?? this); }
        public Task<string> Read(SecretPin pin, CancellationToken cancellationToken)
        { Reads[pin.Name] = Reads.GetValueOrDefault(pin.Name) + 1; return Task.FromResult(payloads[pin.Name]); }
        public void ValidateRuntime(DeliveryTrace trace) => throw new InvalidOperationException("Not used in these tests.");
        public IRuntimeFileAttempt CreateTemporary() { Creates++; Temp = string.Empty; return new Attempt(this); }
        public void Dispose() { Disposed = true; }
        private void Operation(string name) => Operations[name] = Operations.GetValueOrDefault(name) + 1;
        private sealed class Attempt(MemoryRuntime owner) : IRuntimeFileAttempt
        {
            private bool flushed, permissions, validated, committed;
            public void Write(JsonObject runtime)
            {
                owner.Operation("write");
                if (owner.PartialWriteFailure) { owner.Temp = "{partial:" + Canary; throw new IOException(Canary); }
                owner.Temp = runtime.ToJsonString();
            }
            public void Flush() { owner.Operation("flush"); flushed = true; }
            public void ValidateConfiguration(DeliveryTrace trace)
            {
                owner.Operation("validate-configuration");
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(owner.Temp!));
                var config = BindingDiagnostic.Defaults().AddJsonStream(stream).Build();
                try { BindingDiagnostic.Validate(config, trace, ValidationTarget.TemporaryFile); }
                finally { (config as IDisposable)?.Dispose(); }
            }
            public void SetPermissions()
            {
                owner.Operation("permissions");
                if (owner.PermissionFailure) throw new UnauthorizedAccessException(Canary);
                permissions = true;
            }
            public void ValidateFile()
            {
                owner.Operation("validate-file");
                if (!flushed || !permissions || owner.FinalValidationFailure) throw new IOException(Canary);
                validated = true;
            }
            public void SyncDirectory() => owner.Operation("sync-directory");
            public void Commit()
            {
                owner.Operation("rename");
                if (!validated || owner.RenameFailure) throw new IOException(Canary);
                owner.Final = owner.Temp; owner.Temp = null; committed = true; owner.Commits++;
            }
            public void Dispose()
            {
                owner.Operation("attempt-dispose");
                if (!committed) owner.Temp = null;
                if (owner.CleanupFailure) throw new IOException(Canary);
            }
        }
    }

    private sealed class AwsFailureClient(Dictionary<string, string> payloads, string failedSecret, string code)
        : AmazonSecretsManagerClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.USEast1)
    {
        internal readonly Dictionary<string, int> Calls = [];
        internal bool PinMismatch;
        public override Task<GetSecretValueResponse> GetSecretValueAsync(GetSecretValueRequest request, CancellationToken cancellationToken = default)
        {
            var pin = Pins.All.Single(p => p.Arn == request.SecretId);
            Check(request.VersionId == pin.Version && request.VersionStage is null);
            Calls[pin.Name] = Calls.GetValueOrDefault(pin.Name) + 1;
            if (pin.Name == failedSecret && !PinMismatch)
                throw new AmazonServiceException(Canary, new IOException(Canary), ErrorType.Sender, code, Canary, HttpStatusCode.Forbidden);
            return Task.FromResult(new GetSecretValueResponse { ARN = pin.Arn, VersionId = PinMismatch ? Canary : pin.Version, SecretString = payloads[pin.Name] });
        }
    }

    private sealed class LOCAL_CANARY_TypeName() : Exception(Canary);
}

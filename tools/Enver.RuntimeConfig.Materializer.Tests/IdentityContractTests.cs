using System.Text.Json;
using Enver.RuntimeConfig.Materializer;

internal static class IdentityContractTests
{
    private const string A = "i-0a66085b7b2411393";
    private const string B = "i-00de11a3f3c1a8aa5";
    private const string Role = "enver-dev-api-runtime";
    private static int assertions;

    private static void Check(bool ok)
    {
        assertions++;
        if (!ok) throw new Exception("Identity contract assertion failed (no payload).");
    }

    internal static async Task Run()
    {
        // The same pure decision called by production after IMDSv2 reads. No HTTP/AWS client.
        (string? Instance, string? Role, bool Allowed)[] fixtures =
        [
            (A, Role, true), (B, Role, true),
            ("i-11111111111111111", Role, false),
            (A, "wrong-role", false), (B, "wrong-role", false),
            ("i-11111111111111111", "wrong-role", false),
            (null, Role, false), ("", Role, false), (" ", Role, false),
            (A, null, false), (B, null, false), (A, "", false), (B, "", false),
            (null, null, false),
            ("*", Role, false), ("i-*", Role, false),
            (A[..^1], Role, false), (B[..^1], Role, false),
            (A + "0", Role, false), (B + "0", Role, false),
            (A.ToUpperInvariant(), Role, false), (B.ToUpperInvariant(), Role, false),
            (A, Role.ToUpperInvariant(), false), (B, Role + "-extra", false),
            (" " + A, Role, false), (B + " ", Role, false),
            (A, " " + Role, false), (B, Role + " ", false),
            (A, "*", false), (B, "arn:aws:iam::926925605666:role/" + Role, false)
        ];

        foreach (var fixture in fixtures)
        {
            var accepted = true;
            try { InstanceIdentity.RequireAllowedIdentity(fixture.Instance, fixture.Role); }
            catch (DeliveryFailure failure)
            {
                accepted = false;
                Check(failure.Safe.Code == "instance-role-mismatch" && failure.InnerException is null);
            }
            Check(accepted == fixture.Allowed);

            // Exercise real orchestration with an identity-only fake runtime. Rejection must
            // precede any secret read or temporary-file creation; acceptance stops at a sentinel.
            var runtime = new IdentityFixtureRuntime(fixture.Instance, fixture.Role);
            var phases = new List<DeliveryPhase>();
            using var output = new StringWriter();
            Check(await Enver.RuntimeConfig.Materializer.Program.Run(["--materialize"], runtime, output, new(phases.Add)) == 1);
            using var receipt = JsonDocument.Parse(output.ToString());
            Check(receipt.RootElement.GetProperty("phase").GetString() == "aws-credentials");
            Check(receipt.RootElement.GetProperty("code").GetString() ==
                (fixture.Allowed ? "validation-failed" : "instance-role-mismatch"));
            Check(runtime.Accepted == fixture.Allowed && runtime.ResolveCalls == 1 && runtime.Disposed);
            Check(runtime.TemporaryCreates == 0 && runtime.SecretReads == 0);
            Check(phases.SequenceEqual(new[] { DeliveryPhase.Arguments, DeliveryPhase.TmpfsCheck, DeliveryPhase.AwsCredentials }));
            Check(!output.ToString().Contains(A, StringComparison.Ordinal) &&
                !output.ToString().Contains(B, StringComparison.Ordinal) &&
                !output.ToString().Contains(Role, StringComparison.Ordinal));
        }

        Console.WriteLine($"Identity contract fixtures: {fixtures.Length}/{fixtures.Length} PASS; {assertions}/{assertions} assertions PASS; A/B accepted, all other identities fail closed; AWS/network/secret reads/runtime writes=0.");
    }

    private sealed class IdentityFixtureRuntime(string? instance, string? role) : IDeliveryRuntime, IRuntimeFileStore, ISecretReader
    {
        internal bool Accepted, Disposed;
        internal int ResolveCalls, TemporaryCreates, SecretReads;
        public IRuntimeFileStore Files => this;
        public void Prepare() { } // In-memory stand-in; never LinuxRuntimeFileStore.
        public Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken)
        {
            ResolveCalls++;
            InstanceIdentity.RequireAllowedIdentity(instance, role);
            Accepted = true;
            throw new InvalidOperationException(); // Accepted identity sentinel; no credentials resolved.
        }
        public Task<string> Read(SecretPin pin, CancellationToken cancellationToken)
        { SecretReads++; throw new InvalidOperationException(); }
        public IRuntimeFileAttempt CreateTemporary()
        { TemporaryCreates++; throw new InvalidOperationException(); }
        public void ValidateRuntime(DeliveryTrace trace) => throw new InvalidOperationException();
        public void Dispose() => Disposed = true;
    }
}

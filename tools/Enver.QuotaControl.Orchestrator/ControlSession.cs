using System.Diagnostics;
using System.Security.Cryptography;
using Enver.QuotaControl.PipeProducer;

namespace Enver.QuotaControl.Orchestrator;

public static class FixedResources
{
    public const string Account = "926925605666", Region = "us-east-1", Profile = "enver-admin-temp";
    public const string Principal = "arn:aws:iam::926925605666:user/enver-terraform-local";
    public const string Control = "enver-dev-quota-control", Runtime = "enver-dev-runtime", Group = "enver-dev-valkey";
    public const string Endpoint = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com";
    public const string Workspace = @"C:\API_SDD_ENVER\Enver.Backend";
    public const string ProducerSha256 = "4ba45f6762c2f4f4603cd2a1c89d0f175bf2cb06bee761ceca43f977591f4354";
    public const string Acl = "on -@all resetkeys resetchannels ~enver:{development:auth:ratelimit}:metadata ~enver:{development:auth:ratelimit}:registry ~enver:{development:auth:ratelimit}:bucket:* +scan +type +pttl +pexpiretime +hgetall +zcard +zrange +time";
}

public enum Failure { InvalidCommand, Order, Precheck, Drift, Collision, Rejected, Expired, AmbiguousMutation, InventoryStop, Cancelled, Deadline, Cleanup, Transport }
public sealed class ControlFailure(Failure code) : Exception(code.ToString()) { public Failure Code { get; } = code; }
public enum ControlPresence { Absent, Expected, Pending, Unexpected }
public sealed record Snapshot(string RuntimeCanonical, string ResourceCanonical, string[] Members, bool Stable, ControlPresence Control);
public enum SessionState { New, Prepared, Created, Associated, Revoked, Deleted, Restored, Stop, Incident }
public enum Event { Prechecked, Created, Associated, InventoryCompleted, InventoryStopped, Revoked, Deleted, BaselineRestored, Stop, Incident }
public sealed record Receipt(Event Event, SessionState State, Failure? Failure, long ElapsedMilliseconds);
public sealed record InventoryResult(bool Completed, int ExitCode);

// No generic resource/action/parameter escape hatch. All IDs belong to the adapter, not to a caller.
public interface IControlPlane
{
    Task<Snapshot> ReadAsync(CancellationToken token);
    Task CreateAsync(ReadOnlyMemory<byte> password, CancellationToken token);
    Task AssociateAsync(CancellationToken token);
    Task RevokeAsync(CancellationToken token);
    Task DeleteAsync(CancellationToken token);
}
public interface IInventory
{
    void Precheck();
    Task<InventoryResult> RunAsync(ReadOnlyMemory<byte> password, CancellationToken token);
}
public interface ISecretFactory { byte[] Generate(); }
public sealed class RamSecretFactory : ISecretFactory
{
    public byte[] Generate()
    {
        var random = RandomNumberGenerator.GetBytes(32);
        // Direct Base64URL encoding, no immutable string containing CONTROL password.
        var output = new byte[43];
        ReadOnlySpan<byte> alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"u8;
        var accumulator = 0; var bits = 0; var offset = 0;
        try
        {
            foreach (var b in random)
            {
                accumulator = (accumulator << 8) | b; bits += 8;
                while (bits >= 6) { bits -= 6; output[offset++] = alphabet[(accumulator >> bits) & 63]; }
            }
            if (bits > 0) output[offset++] = alphabet[(accumulator << (6 - bits)) & 63];
            if (offset != output.Length) throw new ControlFailure(Failure.Precheck);
            return output;
        }
        catch { CryptographicOperations.ZeroMemory(output); throw; }
        finally { CryptographicOperations.ZeroMemory(random); }
    }
}
public sealed class PipeInventory : IInventory
{
    public void Precheck()
    {
        if (!OperatingSystem.IsWindows() || !Environment.Is64BitProcess || !File.Exists(@"C:\Program Files\dotnet\dotnet.exe"))
            throw new ControlFailure(Failure.Precheck);
        var root = Path.Combine(FixedResources.Workspace, "tools");
        Verify(Path.Combine(root, "Enver.QuotaControl.PipeProducer", "bin", "Release", "net10.0", "Enver.QuotaControl.PipeProducer.dll"), FixedResources.ProducerSha256);
        Verify(typeof(CredentialProducer).Assembly.Location, FixedResources.ProducerSha256);
        Verify(Path.Combine(root, "Enver.QuotaControl", "bin", "Release", "net10.0", "Enver.QuotaControl.dll"), CredentialProducer.ControlDllSha256);
        Verify(Path.Combine(root, "Enver.QuotaControl", "descriptor.approved.json"), CredentialProducer.DescriptorSha256);
    }
    private static void Verify(string path, string expected)
    {
        for (FileSystemInfo? item = new FileInfo(path); item is not null; item = item is FileInfo f ? f.Directory : ((DirectoryInfo)item).Parent)
            if ((item.Attributes & FileAttributes.ReparsePoint) != 0) throw new ControlFailure(Failure.Precheck);
        using var stream = File.OpenRead(path);
        if (!CryptographicOperations.FixedTimeEquals(SHA256.HashData(stream), Convert.FromHexString(expected))) throw new ControlFailure(Failure.Precheck);
    }
    public async Task<InventoryResult> RunAsync(ReadOnlyMemory<byte> password, CancellationToken token)
    {
        Precheck();
        var result = await CredentialProducer.RunInventoryAsync(FixedResources.Workspace, password, token).ConfigureAwait(false);
        return new(result.ExitCode == 0 && result.Evidence is { Completed: true, SessionCoherent: true, FinalCursor: "0", Discrepancies: 0 }, result.ExitCode);
    }
}

// One process owns the password and lifetime. No serialization/resume/adoption of a previous CONTROL.
public sealed class ControlSession(IControlPlane plane, IInventory inventory, ISecretFactory secrets,
    TimeSpan lifetime, TimeProvider? time = null) : IAsyncDisposable
{
    private readonly TimeProvider clock = time ?? TimeProvider.System;
    private readonly long started = (time ?? TimeProvider.System).GetTimestamp();
    private readonly SemaphoreSlim gate = new(1);
    private readonly List<Receipt> receipts = [];
    private Snapshot? baseline;
    private byte[]? password;
    private bool ownsControl, ambiguousCreation, membershipMayChange, inventoryAttempted, finished, incident, stopped;
    public SessionState State { get; private set; } = SessionState.New;
    public IReadOnlyList<Receipt> Receipts => receipts.ToArray();
    public TimeSpan Remaining => lifetime - clock.GetElapsedTime(started);
    private void Record(Event value, Failure? failure = null) => receipts.Add(new(value, State, failure, (long)clock.GetElapsedTime(started).TotalMilliseconds));
    private static bool Members(Snapshot s, bool control) => s.Members.Order().SequenceEqual(
        (control ? new[] { FixedResources.Runtime, FixedResources.Control } : new[] { FixedResources.Runtime }).Order());
    private bool Same(Snapshot s) => baseline is not null && s.Stable && s.RuntimeCanonical == baseline.RuntimeCanonical && s.ResourceCanonical == baseline.ResourceCanonical;
    private void Clear() { if (password is not null) CryptographicOperations.ZeroMemory(password); password = null; }

    public async Task ExecuteAsync(string command, CancellationToken token = default)
    {
        await gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (lifetime <= TimeSpan.Zero || lifetime > TimeSpan.FromMinutes(15)) throw new ControlFailure(Failure.Precheck);
            if (command is not ("precheck" or "create-control" or "associate-control" or "run-inventory" or "revoke-control" or "delete-control" or "verify-baseline" or "abort"))
                throw new ControlFailure(Failure.InvalidCommand);
            if (command == "abort") { stopped = true; await CleanupAsync().ConfigureAwait(false); return; }
            if (finished)
            {
                if (command == "verify-baseline" && !incident && baseline is not null)
                {
                    using var verify = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var current = await plane.ReadAsync(verify.Token).ConfigureAwait(false);
                    if (!Same(current) || !Members(current, false) || current.Control != ControlPresence.Absent)
                    { incident = true; State = SessionState.Incident; Record(Event.Incident, Failure.Drift); }
                    else Record(Event.BaselineRestored);
                    return;
                }
                if (command is "revoke-control" or "delete-control" or "verify-baseline") return;
                throw new ControlFailure(Failure.Order);
            }
            if (Remaining <= TimeSpan.Zero) throw new ControlFailure(Failure.Deadline);
            using var bounded = CancellationTokenSource.CreateLinkedTokenSource(token);
            bounded.CancelAfter(Remaining);
            if (command == "precheck")
            {
                if (State == SessionState.Prepared) return;
                if (State != SessionState.New) throw new ControlFailure(Failure.Order);
                inventory.Precheck();
                var current = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                if (!current.Stable || !Members(current, false)) throw new ControlFailure(Failure.Precheck);
                if (current.Control != ControlPresence.Absent) throw new ControlFailure(Failure.Collision);
                baseline = current with { Members = current.Members.ToArray() };
                State = SessionState.Prepared; Record(Event.Prechecked); return;
            }
            if (baseline is null) throw new ControlFailure(Failure.Order);
            if (command == "create-control")
            {
                if (State is SessionState.Created or SessionState.Associated) return;
                if (State != SessionState.Prepared || Remaining < TimeSpan.FromSeconds(775)) throw new ControlFailure(Failure.Order);
                var before = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                if (!Same(before) || !Members(before, false) || before.Control != ControlPresence.Absent) throw new ControlFailure(Failure.Drift);
                inventory.Precheck();
                password = secrets.Generate();
                if (password.Length != 43 || password.Any(b => !(b is >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z' or >= (byte)'0' and <= (byte)'9' or (byte)'-' or (byte)'_')))
                    throw new ControlFailure(Failure.Precheck);
                // Until Create returns positively, ownership is ambiguous, never assume adoption on a timeout.
                ambiguousCreation = true;
                try { await plane.CreateAsync(password, bounded.Token).ConfigureAwait(false); }
                catch (ControlFailure e) when (e.Code is Failure.Rejected or Failure.Expired) { ambiguousCreation = false; throw; }
                ownsControl = true; ambiguousCreation = false;
                var created = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                using var creation = CancellationTokenSource.CreateLinkedTokenSource(bounded.Token);
                creation.CancelAfter(TimeSpan.FromSeconds(120));
                while (created.Control == ControlPresence.Pending)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), creation.Token).ConfigureAwait(false);
                    created = await plane.ReadAsync(creation.Token).ConfigureAwait(false);
                }
                if (!Same(created) || !Members(created, false) || created.Control != ControlPresence.Expected) throw new ControlFailure(Failure.Drift);
                State = SessionState.Created; Record(Event.Created); return;
            }
            if (command == "associate-control")
            {
                if (State == SessionState.Associated) return;
                if (State != SessionState.Created || Remaining < TimeSpan.FromSeconds(720)) throw new ControlFailure(Failure.Order);
                var before = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                if (!Same(before) || !Members(before, false) || before.Control != ControlPresence.Expected) throw new ControlFailure(Failure.Drift);
                membershipMayChange = true;
                await plane.AssociateAsync(bounded.Token).ConfigureAwait(false);
                var after = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                if (!Same(after) || !Members(after, true) || after.Control != ControlPresence.Expected) throw new ControlFailure(Failure.Drift);
                State = SessionState.Associated; Record(Event.Associated); return;
            }
            if (command == "run-inventory")
            {
                if (State != SessionState.Associated || inventoryAttempted || password is null || Remaining < TimeSpan.FromSeconds(540)) throw new ControlFailure(Failure.Order);
                var before = await plane.ReadAsync(bounded.Token).ConfigureAwait(false);
                if (!Same(before) || !Members(before, true) || before.Control != ControlPresence.Expected) throw new ControlFailure(Failure.Drift);
                inventoryAttempted = true;
                using var inventoryBudget = CancellationTokenSource.CreateLinkedTokenSource(bounded.Token);
                inventoryBudget.CancelAfter(TimeSpan.FromSeconds(55));
                var result = await inventory.RunAsync(password, inventoryBudget.Token).ConfigureAwait(false);
                if (!result.Completed || result.ExitCode != 0) { Record(Event.InventoryStopped, Failure.InventoryStop); throw new ControlFailure(Failure.InventoryStop); }
                Record(Event.InventoryCompleted);
                // PASS also must revoke/delete/verify, not just failure.
                await CleanupAsync().ConfigureAwait(false); return;
            }
            // Explicit revoke/delete/verify all converge on the full mandatory cleanup.
            await CleanupAsync().ConfigureAwait(false);
        }
        catch (Exception failure)
        {
            stopped = true;
            var code = failure switch { ControlFailure f => f.Code, OperationCanceledException => Failure.Cancelled, _ => Failure.Transport };
            State = SessionState.Stop; Record(Event.Stop, code);
            await CleanupAsync().ConfigureAwait(false);
        }
        finally { gate.Release(); }
    }

    private async Task CleanupAsync()
    {
        if (finished) return;
        Clear();
        if (ambiguousCreation) { incident = true; State = SessionState.Incident; Record(Event.Incident, Failure.AmbiguousMutation); finished = true; return; }
        if (baseline is null) { finished = true; State = SessionState.Stop; return; }
        using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(485));
        try
        {
            if (ownsControl)
            {
                var before = await plane.ReadAsync(cleanup.Token).ConfigureAwait(false);
                if (before.Control == ControlPresence.Unexpected) throw new ControlFailure(Failure.Collision);
                if (membershipMayChange || before.Members.Contains(FixedResources.Control)) await plane.RevokeAsync(cleanup.Token).ConfigureAwait(false);
                var revoked = await plane.ReadAsync(cleanup.Token).ConfigureAwait(false);
                if (revoked.Members.Contains(FixedResources.Control) || !revoked.Stable) throw new ControlFailure(Failure.Cleanup);
                State = SessionState.Revoked; Record(Event.Revoked);
                // A possibly stale Describe absence is not a Delete receipt for our acknowledged Create.
                await plane.DeleteAsync(cleanup.Token).ConfigureAwait(false);
                var deleted = await plane.ReadAsync(cleanup.Token).ConfigureAwait(false);
                if (deleted.Control != ControlPresence.Absent || deleted.Members.Contains(FixedResources.Control)) throw new ControlFailure(Failure.Cleanup);
                ownsControl = false; State = SessionState.Deleted; Record(Event.Deleted);
            }
            var final = await plane.ReadAsync(cleanup.Token).ConfigureAwait(false);
            if (!Same(final) || !Members(final, false) || final.Control != ControlPresence.Absent) throw new ControlFailure(Failure.Drift);
            if (Remaining <= TimeSpan.Zero) throw new ControlFailure(Failure.Deadline);
            State = SessionState.Restored; Record(Event.BaselineRestored);
            if (stopped) State = SessionState.Stop;
        }
        catch (Exception failure)
        {
            incident = true; State = SessionState.Incident;
            Record(Event.Incident, failure is ControlFailure f ? f.Code : Failure.Cleanup);
        }
        finally { finished = true; }
    }

    public async ValueTask DisposeAsync()
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try { if (!finished) { stopped = true; await CleanupAsync().ConfigureAwait(false); } Clear(); if (incident) State = SessionState.Incident; }
        finally { gate.Release(); }
    }
}

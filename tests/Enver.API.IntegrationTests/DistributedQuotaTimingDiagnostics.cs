using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Quotas;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

// TEST-ONLY: sin timers, hilos, esperas, Redis adicional ni hooks de EVAL.
// NORMAL_TIMING describe las muestras, no acredita ausencia global de pausas.
internal sealed class DistributedQuotaTimingDiagnostics
{
    internal const double ThresholdMs = 1000; // Diagnóstico, NO timeout/SLA funcional.
    private readonly object sync = new();
    private readonly Func<Stamp> clock;
    private readonly string partition, scenario;
    private readonly Stamp start;
    private Stamp phaseStart;
    private string phase = "setup";
    private long attempts, completed, thrown;
    private readonly long[] decisions = new long[4];
    private Window? firstAnomaly, firstUnavailable, lastDecision, largestPacingGap, finalWindow;
    private RedisSample? previousRedis, lastRedis;
    private bool hostPause, clockStep, schedulingGap, unattributed, incomplete;
    private string outcome = "RUNNING";

    internal DistributedQuotaTimingDiagnostics(string metadataKey, string testName, Func<Stamp>? clock = null)
    {
        this.clock = clock ?? Capture;
        partition = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metadataKey)))[..16];
        scenario = testName switch
        {
            "P1_exact_academic_windows_3000_attempts_paced_over_90_seconds" => "P1",
            "P2_mixed_5000_partitions_102000_events_memory_and_revocations_during_pressure" => "P2",
            "Capacity_boundary_race_release_and_three_reassignment_rounds_never_exceed_5000" => "CAPACITY",
            "Lazy_cleanup_revalidates_renewed_hint_and_releases_only_three_residues_during_other_host_consumption" => "LAZY",
            "Maximum_blocker_retry_rounds_up_and_synthetic_boundary_releases_without_penalty_extension" => "BOUNDARY",
            _ => "OTHER_SCENARIO"
        };
        start = phaseStart = Sample();
    }

    internal readonly record struct Stamp(long WallTicks, long MonoTicks, double? BootMs, double? ActiveMs, double SampleSpanMs);
    internal readonly record struct Delta(double WallMs, double MonoMs, double WallMinusMonoMs,
        double? BootMs, double? ActiveMs, double? SuspendMs, double? WallMinusBootMs,
        double UncertaintyMs, bool HostPause, bool ClockStep, bool SchedulingGap, bool UnattributedGap)
    {
        public string Classification => ClockStep ? "CLOCK_STEP" : HostPause ? "HOST_PAUSE"
            : UnattributedGap ? "UNKNOWN" : SchedulingGap ? "SCHEDULING_GAP" : "NORMAL_TIMING";
    }
    internal readonly record struct Attempt(long Index, string Phase, int Host, Stamp Before);
    private readonly record struct Window(string Phase, long? Attempt, int? Host, string Decision,
        DateTimeOffset BeforeUtc, DateTimeOffset AfterUtc, Delta Clocks, double? PacingLatenessMs);
    private readonly record struct RedisSample(long TimeUs, DateTimeOffset ReceivedUtc, long MonoTicks, double SampleSpanMs);

    internal static Delta Compare(Stamp before, Stamp after, double latenessMs = 0)
    {
        var wall = TimeSpan.FromTicks(after.WallTicks - before.WallTicks).TotalMilliseconds;
        var mono = Stopwatch.GetElapsedTime(before.MonoTicks, after.MonoTicks).TotalMilliseconds;
        double? boot = before.BootMs is { } b0 && after.BootMs is { } b1 ? b1 - b0 : null;
        double? active = before.ActiveMs is { } a0 && after.ActiveMs is { } a1 ? a1 - a0 : null;
        var uncertainty = Math.Max(0, before.SampleSpanMs) + Math.Max(0, after.SampleSpanMs);
        var threshold = ThresholdMs + uncertainty;
        // Sin reloj suspend-inclusive independiente NO distinguir sleep de salto positivo UTC.
        // UTC que retrocede sí acredita discontinuidad respecto del monotónico creciente.
        var pause = boot is { } b && active is { } a && b >= 0 && a >= 0 && b - a > threshold;
        double? bootKnown = boot is { } candidate && candidate >= 0 ? candidate : null;
        var step = mono >= 0 && (bootKnown is { } elapsed ? Math.Abs(wall - elapsed) > threshold : wall < -threshold);
        // Divergencia pared/monotónico sin reloj suspend-inclusive: anomalía medida, tipo no atribuible.
        var unattributed = mono >= 0 && bootKnown is null && Math.Abs(wall - mono) > threshold;
        return new(wall, mono, wall - mono, boot, active, boot - active, boot is null ? null : wall - boot,
            uncertainty, pause, step, latenessMs > threshold, unattributed);
    }

    internal Stamp Sample()
    {
        try { return clock(); }
        catch { incomplete = true; return default; } // Nunca altera resultado funcional.
    }
    internal static Stamp Capture()
    {
        var before = Stopwatch.GetTimestamp();
        var wall = DateTimeOffset.UtcNow.UtcTicks;
        var native = NativeClocks.Read();
        var wallAfter = DateTimeOffset.UtcNow.UtcTicks;
        var after = Stopwatch.GetTimestamp();
        return new(wall, before + (after - before) / 2, native.BootMs, native.ActiveMs,
            Math.Max(native.SpanMs, Math.Max(Math.Abs((wallAfter - wall) / 10000d), Stopwatch.GetElapsedTime(before, after).TotalMilliseconds)));
    }
    private Window Observe(string name, long? attempt, int? host, string decision, Stamp before, Stamp after, double? lateness = null)
    {
        var delta = Compare(before, after, lateness ?? 0);
        var row = new Window(name, attempt, host, decision, Utc(before), Utc(after), delta, lateness);
        hostPause |= delta.HostPause; clockStep |= delta.ClockStep; schedulingGap |= delta.SchedulingGap; unattributed |= delta.UnattributedGap;
        if (delta.Classification != "NORMAL_TIMING") firstAnomaly ??= row;
        return row;
    }
    private static DateTimeOffset Utc(Stamp value) => new(value.WallTicks, TimeSpan.Zero);
    private static string SafePhase(string name) => name switch
    {
        "body" or "blocker-rejection" or "boundary-release" or "P1-1" or "P1-2" or "P1-3" or
        "P2-1" or "P2-2" or "P2-3" or "P2-spare-at-capacity" or "P2-housekeeping" or
        "capacity-start-1" or "capacity-start-2" or "capacity-start-3" or
        "capacity-one-new-1" or "capacity-one-new-2" or "capacity-one-new-3" or
        "capacity-race-1" or "capacity-race-2" or "capacity-race-3" or
        "capacity-two-new-1" or "capacity-two-new-2" or "capacity-two-new-3" or
        "capacity-existing-1" or "capacity-existing-2" or "capacity-existing-3" or
        "capacity-post-expiry-1" or "capacity-post-expiry-2" or "capacity-post-expiry-3" or
        "renew-candidate" or "stale-hint" or "cleanup-batch" or "cleanup-rest" => name,
        _ => "OTHER_PHASE"
    };

    internal void Phase(string name)
    {
        try
        {
            var now = Sample();
            lock (sync) { Observe(phase, null, null, "PHASE_END", phaseStart, now); phaseStart = now; phase = SafePhase(name); }
        }
        catch { incomplete = true; }
    }
    internal Attempt BeginAttempt(int host, string name)
    {
        var now = Sample();
        lock (sync) return new(++attempts, name == "matrix" ? phase : SafePhase(name), host, now);
    }
    internal void EndAttempt(Attempt attempt, AuthenticationQuotaConsumeResult? result)
    {
        try
        {
            var now = Sample();
            lock (sync)
            {
                completed++;
                if (result is null) thrown++; else decisions[(int)result.Status]++;
                lastDecision = Observe(attempt.Phase, attempt.Index, attempt.Host, result?.Status.ToString() ?? "THREW", attempt.Before, now);
                if (result?.Status == AuthenticationQuotaStoreStatus.Unavailable) firstUnavailable ??= lastDecision;
            }
        }
        catch { incomplete = true; }
    }
    internal void Paced(Stamp before, int worker, int index, double latenessMs)
    {
        try
        {
            var now = Sample();
            lock (sync)
            {
                var row = Observe("paced-resume", index, worker % 2, "BEFORE_ATTEMPT", before, now, Math.Max(0, latenessMs));
                if (largestPacingGap is null || row.PacingLatenessMs > largestPacingGap.Value.PacingLatenessMs) largestPacingGap = row;
            }
        }
        catch { incomplete = true; }
    }
    internal void RedisTime(long timeUs)
    {
        try
        {
            var now = Sample();
            lock (sync) { previousRedis = lastRedis; lastRedis = new(timeUs, Utc(now), now.MonoTicks, now.SampleSpanMs); }
        }
        catch { incomplete = true; }
    }
    internal void Finish(string value)
    {
        try
        {
            var now = Sample();
            lock (sync)
            {
                if (outcome != "RUNNING") return;
                Observe(phase, null, null, "PHASE_END", phaseStart, now);
                outcome = value == "PASSED" ? "PASSED" : "FAILED";
                finalWindow = Observe("scenario", null, null, outcome, start, now);
            }
        }
        catch { incomplete = true; }
    }

    internal string Json()
    {
        lock (sync)
        {
            object? redis = lastRedis is not { } last ? null : new { source = "existing-NowAsync-TIME; receipt NOT atomic with Redis TIME",
                lastTimeUs = last.TimeUs, receivedUtc = last.ReceivedUtc,
                ageAtScenarioEndMs = finalWindow is { } end ? (end.AfterUtc - last.ReceivedUtc).TotalMilliseconds : (double?)null,
                sampleSpanMs = last.SampleSpanMs, previousTimeUs = previousRedis?.TimeUs,
                redisDeltaMs = previousRedis is { } p ? (last.TimeUs - p.TimeUs) / 1000d : (double?)null,
                receiptMonoDeltaMs = previousRedis is { } p0 ? Stopwatch.GetElapsedTime(p0.MonoTicks, last.MonoTicks).TotalMilliseconds : (double?)null };
            var nativeClockAvailable = NativeClocks.Available;
            // Sin reloj suspend-inclusive no se puede afirmar NORMAL_TIMING ni distinguir pausa de salto.
            var classification = incomplete ? "UNKNOWN"
                : clockStep ? "CLOCK_STEP" : hostPause ? "HOST_PAUSE" : unattributed ? "UNKNOWN"
                : schedulingGap ? "SCHEDULING_GAP" : nativeClockAvailable ? "NORMAL_TIMING" : "UNKNOWN";
            var json = JsonSerializer.Serialize(new { version = 1, scenario, partition, outcome, classification,
                hostPause, clockStep, schedulingGap, unattributedGap = unattributed, observerIncomplete = incomplete,
                nativeClockAvailable, thresholdMs = ThresholdMs, clockSources = NativeClocks.Sources,
                attempts, completed, thrown, admitted = decisions[0], rejected = decisions[1], unavailable = decisions[2], uncertain = decisions[3],
                finalWindow, firstAnomaly, firstUnavailable, lastDecision, largestPacingGap, redis,
                note = "Lateness includes backlog; Redis receipt is not atomic with TIME; partition is hashed; no causal claim." });
            if (json.Length > 3400)
                json = JsonSerializer.Serialize(new { version = 1, scenario, partition, outcome, classification,
                    hostPause, clockStep, schedulingGap, unattributedGap = unattributed, observerIncomplete = incomplete,
                    nativeClockAvailable, thresholdMs = ThresholdMs, clockSources = NativeClocks.Sources,
                    attempts, completed, thrown, admitted = decisions[0], rejected = decisions[1], unavailable = decisions[2], uncertain = decisions[3],
                    finalWindow, firstAnomaly, firstUnavailable, redis, omitted = "LAST_DECISION_AND_LARGEST_PACING_GAP" });
            return json;
        }
    }
    internal JsonElement? Evidence()
    {
        try { using var document = JsonDocument.Parse(Json()); return document.RootElement.Clone(); }
        catch { return null; } // Evidencia ausente no altera la aserción funcional.
    }
    internal void Report(ITestOutputHelper output)
    {
        try { output.WriteLine("QUOTA_TIMING_DIAGNOSTIC " + Json()); } catch { incomplete = true; }
    }
    internal Exception Failure(Exception original) => new InvalidOperationException("QUOTA_TIMING_DIAGNOSTIC " + Json(), original);

    // Relojes kernel de solo lectura. Linux64 CI: BOOTTIME incluye suspensión,
    // MONOTONIC la excluye. Windows: InterruptTime / UnbiasedInterruptTime equivalentes.
    // Divergencia no identifica quién suspendió el host ni la causa de Unavailable.
    private static class NativeClocks
    {
        private static int backend = OperatingSystem.IsWindows() ? 1 : OperatingSystem.IsLinux() && Environment.Is64BitProcess ? 2 : 0;
        internal static bool Available => Volatile.Read(ref backend) != 0;
        internal static object? Sources => Volatile.Read(ref backend) switch
        {
            1 => "InterruptTime(boot,includessleep)+UnbiasedInterruptTime(active)",
            2 => "CLOCK_BOOTTIME(7,includessleep)+CLOCK_MONOTONIC(1);wall=UtcNow;mono=Stopwatch",
            _ => null
        };
        [StructLayout(LayoutKind.Sequential)] private struct Timespec { public long Seconds, Nanoseconds; }
        [DllImport("libc", EntryPoint = "clock_gettime", ExactSpelling = true)] private static extern int ClockGetTime(int clock, out Timespec value);
        [DllImport("kernelbase.dll", EntryPoint = "QueryInterruptTimePrecise", ExactSpelling = true)] private static extern void QueryInterruptTimePrecise(out ulong value);
        [DllImport("kernelbase.dll", EntryPoint = "QueryUnbiasedInterruptTimePrecise", ExactSpelling = true)] private static extern void QueryUnbiasedInterruptTimePrecise(out ulong value);
        internal static (double? BootMs, double? ActiveMs, double SpanMs) Read()
        {
            try
            {
                switch (Volatile.Read(ref backend))
                {
                    case 1:
                        QueryInterruptTimePrecise(out var boot); QueryUnbiasedInterruptTimePrecise(out var active);
                        QueryInterruptTimePrecise(out var end);
                        return (boot / 10000d, active / 10000d, Math.Abs((double)end - boot) / 10000d);
                    case 2:
                        if (ClockGetTime(7, out var b) == 0 && ClockGetTime(1, out var a) == 0 && ClockGetTime(7, out var e) == 0)
                            return (b.Seconds * 1000d + b.Nanoseconds / 1000000d, a.Seconds * 1000d + a.Nanoseconds / 1000000d,
                                Math.Abs((e.Seconds - b.Seconds) * 1000d + (e.Nanoseconds - b.Nanoseconds) / 1000000d));
                        break;
                    default: return (null, null, 0);
                }
            }
            catch { /* API ausente: no inventar suspensión o salto positivo UTC. */ }
            Interlocked.Exchange(ref backend, 0);
            return (null, null, 0);
        }
    }
}

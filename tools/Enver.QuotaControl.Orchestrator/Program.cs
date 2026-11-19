using System.Globalization;
using System.Text.Json;

namespace Enver.QuotaControl.Orchestrator;

public static class Program
{
    // No resource IDs or passwords are command-line inputs. Merely starting the executable never creates anything.
    public static async Task<int> Main(string[] args)
    {
        if (args.Length != 3 || args[0] != "--explicitly-authorized-existing-window" ||
            !int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out var remainingMs) || remainingMs is <= 485000 or > 900000 ||
            !DateTimeOffset.TryParseExact(args[2], "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var credentialExpiry))
        { Console.WriteLine("STOP: operational authorization, existing-window remainder and credential expiration required; no action performed."); return 2; }
        // This flag is NOT approval or proof of freeze/T0; caller must already have separate authorization/evidence.
        // Console never echoes password: it does not accept passwords at all. Commands contain no secret.
        using var stop = new CancellationTokenSource();
        ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; stop.Cancel(); };
        Console.CancelKeyPress += cancel;
        try
        {
            using var plane = new FixedAwsControlPlane(credentialExpiry);
            await using var session = new ControlSession(plane, new PipeInventory(), new RamSecretFactory(), TimeSpan.FromMilliseconds(remainingMs));
            using var watchdog = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
            // Reserve cleanup before the remaining overall deadline; not an extension of the window.
            watchdog.CancelAfter(TimeSpan.FromMilliseconds(remainingMs - 485000));
            var reported = 0;
            try
            {
                while (true)
                {
                    var command = await Console.In.ReadLineAsync(watchdog.Token).ConfigureAwait(false);
                    if (command is null) break;
                    await session.ExecuteAsync(command, watchdog.Token).ConfigureAwait(false);
                    foreach (var receipt in session.Receipts.Skip(reported)) Console.WriteLine(JsonSerializer.Serialize(receipt));
                    reported = session.Receipts.Count;
                    if (session.State is SessionState.Restored or SessionState.Stop or SessionState.Incident) break;
                }
            }
            catch (OperationCanceledException) { /* Mandatory same-process cleanup below, without cancelled token. */ }
            finally { await session.DisposeAsync().ConfigureAwait(false); }
            foreach (var receipt in session.Receipts.Skip(reported)) Console.WriteLine(JsonSerializer.Serialize(receipt));
            return session.State == SessionState.Restored ? 0 : session.State == SessionState.Incident ? 3 : 2;
        }
        catch { Console.WriteLine("INCIDENT/STOP: sanitized operational failure; no automatic credential renewal or fallback."); return 3; }
        finally { Console.CancelKeyPress -= cancel; }
    }
}

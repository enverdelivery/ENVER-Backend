using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Enver.RuntimeConfig.Materializer;

internal interface IRuntimeFileStore
{
    void Prepare();
    IRuntimeFileAttempt CreateTemporary();
}

internal interface IRuntimeFileAttempt : IDisposable
{
    void Write(JsonObject runtime);
    void Flush();
    void ValidateConfiguration(DeliveryTrace trace);
    void SetPermissions();
    void ValidateFile();
    void SyncDirectory();
    void Commit();
}

internal sealed class LinuxRuntimeFileStore : IRuntimeFileStore
{
    public void Prepare() => AtomicRuntimeFile.Prepare();
    public IRuntimeFileAttempt CreateTemporary() => AtomicRuntimeFile.CreateTemporary();
}

internal static class AtomicRuntimeFile
{
    internal const string DirectoryPath = "/run/enver";
    internal const string Path = DirectoryPath + "/appsettings.Runtime.json";
    internal static void Prepare()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || geteuid() != 0)
            throw new DeliveryFailure("linux-root-required");
        // No persistent fallback. Reject swap rather than silently paging secret buffers to disk.
        if (File.ReadLines("/proc/swaps").Skip(1).Any(l => !string.IsNullOrWhiteSpace(l))) throw new DeliveryFailure("swap-not-permitted");
        if (File.GetAttributes("/run").HasFlag(FileAttributes.ReparsePoint)) throw new DeliveryFailure("runtime-directory-invalid");
        var mount = File.ReadLines("/proc/self/mountinfo").Select(l => l.Split(' '))
            .Where(p => p[4] == "/" || DirectoryPath == p[4] || DirectoryPath.StartsWith(p[4] + "/", StringComparison.Ordinal))
            .OrderByDescending(p => p[4].Length).First();
        if (mount[Array.IndexOf(mount, "-") + 1] != "tmpfs") throw new DeliveryFailure("tmpfs-required");
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath, (UnixFileMode)Convert.ToInt32("700", 8));
            File.SetUnixFileMode(DirectoryPath, (UnixFileMode)Convert.ToInt32("711", 8));
        }
        var directory = Stat(DirectoryPath);
        if (directory.Uid != 0 || directory.Gid != 0 || directory.Mode != Convert.ToInt32("40711", 8))
            throw new DeliveryFailure("runtime-directory-invalid");
        // Inspect only: neither a historic runtime file nor somebody else's temp is cleanup.
        if (File.Exists(Path) || new FileInfo(Path).LinkTarget is not null)
        {
            var file = Stat(Path);
            if (file.Mode != Convert.ToInt32("100600", 8) || file.Uid != 1654 || file.Gid != 1654)
                throw new DeliveryFailure("runtime-file-invalid");
        }
    }

    internal static void Write(JsonObject runtime, DeliveryTrace trace, IRuntimeFileStore store)
    {
        IRuntimeFileAttempt? attempt = null;
        try
        {
            trace.Enter(DeliveryPhase.TempFileCreate);
            attempt = store.CreateTemporary();
            trace.Enter(DeliveryPhase.FileWrite);
            attempt.Write(runtime);
            trace.Enter(DeliveryPhase.FileFlush);
            attempt.Flush();
            trace.EnterValidation(DeliveryPhase.TemporaryConfigParseValidation, ValidationTarget.TemporaryFile);
            attempt.ValidateConfiguration(trace);
            trace.Enter(DeliveryPhase.FilePermissions);
            attempt.SetPermissions();
            trace.Enter(DeliveryPhase.FinalFileCheck);
            attempt.ValidateFile();
            trace.Enter(DeliveryPhase.DirectoryFlush);
            attempt.SyncDirectory();
            // All fallible candidate validation precedes publication. On tmpfs there is no
            // persistent durability claim; rename is the last I/O and cannot publish partial JSON.
            trace.Enter(DeliveryPhase.AtomicRename);
            attempt.Commit();
        }
        catch (Exception error)
        {
            var failure = DeliveryFailure.Wrap(error);
            try { attempt?.Dispose(); }
            catch (Exception cleanup)
            {
                trace.Enter(DeliveryPhase.TempCleanup);
                throw DeliveryFailure.Wrap(cleanup);
            }
            throw failure;
        }
        attempt.Dispose(); // Committed attempt: no filesystem operations, no cleanup of final.
    }

    internal static IRuntimeFileAttempt CreateTemporary()
    {
        if (!OperatingSystem.IsLinux()) throw new DeliveryFailure("linux-required");
        return new LinuxAttempt();
    }

    private sealed class LinuxAttempt : IRuntimeFileAttempt
    {
        private readonly string temp;
        private FileStream? stream;
        private bool committed;
        internal LinuxAttempt()
        {
            if (!OperatingSystem.IsLinux()) throw new DeliveryFailure("linux-required");
            temp = DirectoryPath + "/.runtime-" + Guid.NewGuid().ToString("N") + ".tmp";
            stream = new FileStream(temp, new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write,
                Share = FileShare.None, UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite });
        }
        public void Write(JsonObject runtime)
        {
            using var writer = new Utf8JsonWriter(stream!);
            runtime.WriteTo(writer);
            writer.Flush();
        }
        public void Flush()
        {
            stream!.Flush(flushToDisk: true);
            stream.Dispose();
            stream = null;
        }
        public void ValidateConfiguration(DeliveryTrace trace) => BindingDiagnostic.FileBinding(temp, ValidationTarget.TemporaryFile, trace);
        public void SetPermissions()
        {
            if (chown(temp, 1654, 1654) != 0) throw new DeliveryFailure("runtime-owner-failed");
        }
        public void ValidateFile()
        {
            var actual = Stat(temp);
            if (actual.Mode != Convert.ToInt32("100600", 8) || actual.Uid != 1654 || actual.Gid != 1654)
                throw new DeliveryFailure("runtime-permissions-failed");
        }
        public void SyncDirectory()
        {
            var descriptor = open(DirectoryPath, 0x10000);
            if (descriptor < 0) throw new DeliveryFailure("runtime-directory-sync-failed");
            try { if (fsync(descriptor) != 0) throw new DeliveryFailure("runtime-directory-sync-failed"); }
            finally { close(descriptor); }
        }
        public void Commit()
        {
            File.Move(temp, Path, overwrite: true);
            committed = true;
        }
        public void Dispose()
        {
            try { stream?.Dispose(); }
            finally
            {
                stream = null;
                if (!committed) File.Delete(temp); // Only the CreateNew file owned by this attempt.
            }
        }
    }

    private static (int Mode, int Uid, int Gid) Stat(string path)
    {
        var buffer = Marshal.AllocHGlobal(256);
        try
        {
            if (statx(-100, path, 0x100, 0x7ff, buffer) != 0) throw new DeliveryFailure("runtime-stat-failed");
            return ((ushort)Marshal.ReadInt16(buffer, 28), Marshal.ReadInt32(buffer, 20), Marshal.ReadInt32(buffer, 24));
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }

    [DllImport("libc")] private static extern uint geteuid();
    [DllImport("libc", SetLastError = true)] private static extern int chown(string path, uint owner, uint group);
    [DllImport("libc", SetLastError = true)] private static extern int statx(int dirfd, string path, int flags, uint mask, IntPtr buffer);
    [DllImport("libc", SetLastError = true)] private static extern int open(string path, int flags);
    [DllImport("libc", SetLastError = true)] private static extern int fsync(int fd);
    [DllImport("libc")] private static extern int close(int fd);
}

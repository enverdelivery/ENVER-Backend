using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Enver.QuotaControl.PipeProducer;

// CreateProcessW + STARTUPINFOEX/HANDLE_LIST. No ShellExecute, stdin secreto,
// herencia general, logs de stdout/stderr, perfil de shell o entorno del padre.
internal static class WindowsProcess
{
    private const uint ExtendedStartup = 0x00080000, UnicodeEnvironment = 0x400, Suspended = 4, NoWindow = 0x08000000;
    private const long HandleList = 0x00020002;
    internal static string[] EnvironmentEntries => new[]
    {
        "SystemRoot=" + Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "WINDIR=" + Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "DOTNET_EnableDiagnostics=0", "DOTNET_CLI_TELEMETRY_OPTOUT=1"
    };

    internal static SafeFileHandle DuplicateForChild(SafeHandle source)
    {
        if (!Native.DuplicateHandle(Native.GetCurrentProcess(), source, Native.GetCurrentProcess(), out var copy, 0, true, 2))
            throw new ProducerException(ProducerFailure.ChildStart);
        return copy;
    }

    internal static NativeChild Start(string executable, IReadOnlyList<string> args, IntPtr[] handles, Action beforeResume, IntPtr? stdout = null)
    {
        IntPtr attributes = IntPtr.Zero, entries = IntPtr.Zero, environment = IntPtr.Zero;
        var initialized = false;
        SafeFileHandle? job = null;
        ProcessInfo info = default;
        var created = false;
        try
        {
            if (!Path.IsPathFullyQualified(executable) || !File.Exists(executable)) throw new ProducerException(ProducerFailure.ChildStart);
            var security = new SecurityAttributes { Length = Marshal.SizeOf<SecurityAttributes>(), Inherit = true };
            using var stdin = Native.CreateFile("NUL", 0x80000000, 3, ref security, 3, 0, IntPtr.Zero);
            using var output = Native.CreateFile("NUL", 0x40000000, 3, ref security, 3, 0, IntPtr.Zero);
            if (stdin.IsInvalid || output.IsInvalid) throw new ProducerException(ProducerFailure.ChildStart);
            var inherited = handles.Concat(new[] { stdin.DangerousGetHandle(), output.DangerousGetHandle() })
                .Concat(stdout is null ? Array.Empty<IntPtr>() : new[] { stdout.Value }).Distinct().ToArray();
            nuint size = 0;
            Native.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref size);
            if (size == 0) throw new ProducerException(ProducerFailure.ChildStart);
            attributes = Marshal.AllocHGlobal(checked((int)size));
            if (!Native.InitializeProcThreadAttributeList(attributes, 1, 0, ref size)) throw new ProducerException(ProducerFailure.ChildStart);
            initialized = true;
            entries = Marshal.AllocHGlobal(IntPtr.Size * inherited.Length);
            Marshal.Copy(inherited, 0, entries, inherited.Length);
            if (!Native.UpdateProcThreadAttribute(attributes, 0, (IntPtr)HandleList, entries,
                (nuint)(IntPtr.Size * inherited.Length), IntPtr.Zero, IntPtr.Zero)) throw new ProducerException(ProducerFailure.ChildStart);
            var startup = new StartupInfoEx
            {
                Info = new StartupInfo { Size = Marshal.SizeOf<StartupInfoEx>(), Flags = 0x100,
                    Input = stdin.DangerousGetHandle(), Output = stdout ?? output.DangerousGetHandle(), Error = output.DangerousGetHandle() },
                Attributes = attributes
            };
            var env = string.Join('\0', EnvironmentEntries.Order(StringComparer.OrdinalIgnoreCase)) + "\0\0";
            environment = Marshal.StringToHGlobalUni(env);
            job = Native.CreateJobObject(IntPtr.Zero, null);
            var limits = new JobLimits { Basic = new BasicJobLimits { Flags = 0x2000 } }; // KILL_ON_JOB_CLOSE
            if (job.IsInvalid || !Native.SetInformationJobObject(job, 9, ref limits, (uint)Marshal.SizeOf<JobLimits>()))
                throw new ProducerException(ProducerFailure.ChildStart);
            var command = new StringBuilder(string.Join(' ', args.Prepend(executable).Select(Quote)));
            if (!Native.CreateProcess(executable, command, IntPtr.Zero, IntPtr.Zero, true,
                ExtendedStartup | UnicodeEnvironment | Suspended | NoWindow, environment, Path.GetDirectoryName(executable), ref startup, out info))
                throw new ProducerException(ProducerFailure.ChildStart);
            created = true;
            if (!Native.AssignProcessToJobObject(job, info.Process)) throw new ProducerException(ProducerFailure.ChildStart);
            // Ningún extremo lector del padre debe sobrevivir al inicio del hijo.
            beforeResume();
            stdin.Dispose();
            output.Dispose();
            if (Native.ResumeThread(info.Thread) == uint.MaxValue) throw new ProducerException(ProducerFailure.ChildStart);
            var child = new NativeChild(info.Process, job, (int)info.ProcessId);
            info.Process = IntPtr.Zero;
            job = null;
            return child;
        }
        catch (ProducerException) { throw; }
        catch { throw new ProducerException(ProducerFailure.ChildStart); }
        finally
        {
            if (created && info.Process != IntPtr.Zero)
            {
                Native.TerminateProcess(info.Process, 125);
                Native.WaitForSingleObject(info.Process, 5000);
                Native.CloseHandle(info.Process);
            }
            if (info.Thread != IntPtr.Zero) Native.CloseHandle(info.Thread);
            job?.Dispose();
            if (initialized) Native.DeleteProcThreadAttributeList(attributes);
            if (attributes != IntPtr.Zero) Marshal.FreeHGlobal(attributes);
            if (entries != IntPtr.Zero) Marshal.FreeHGlobal(entries);
            if (environment != IntPtr.Zero) Marshal.FreeHGlobal(environment);
        }
    }

    internal static string Quote(string value)
    {
        if (value.Any(c => c == '\0' || char.IsControl(c))) throw new ProducerException(ProducerFailure.InvalidInput);
        var result = new StringBuilder("\"");
        var slashes = 0;
        foreach (var ch in value)
        {
            if (ch == '\\') { slashes++; continue; }
            result.Append('\\', ch == '"' ? slashes * 2 + 1 : slashes);
            slashes = 0;
            result.Append(ch);
        }
        return result.Append('\\', slashes * 2).Append('"').ToString();
    }
}

internal sealed class NativeChild(IntPtr process, SafeFileHandle job, int id) : IDisposable
{
    public int Id { get; } = id;
    private bool disposed;
    public async Task<int> WaitAsync(CancellationToken token)
    {
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var status = Native.WaitForSingleObject(process, 0);
            if (status == 0) break;
            if (status != 258) throw new ProducerException(ProducerFailure.ChildFailure);
            await Task.Delay(15, token).ConfigureAwait(false);
        }
        if (!Native.GetExitCodeProcess(process, out var code)) throw new ProducerException(ProducerFailure.ChildFailure);
        return unchecked((int)code);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        job.Dispose();
        var closed = Native.WaitForSingleObject(process, 5000) == 0;
        Native.CloseHandle(process);
        if (!closed) throw new ProducerException(ProducerFailure.Cleanup);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct SecurityAttributes { public int Length; public IntPtr Descriptor; [MarshalAs(UnmanagedType.Bool)] public bool Inherit; }
[StructLayout(LayoutKind.Sequential)]
internal struct StartupInfo
{
    public int Size;
    public IntPtr Reserved, Desktop, Title;
    public int X, Y, XSize, YSize, XChars, YChars, Fill, Flags;
    public short Show, ReservedSize;
    public IntPtr ReservedData, Input, Output, Error;
}
[StructLayout(LayoutKind.Sequential)]
internal struct StartupInfoEx { public StartupInfo Info; public IntPtr Attributes; }
[StructLayout(LayoutKind.Sequential)]
internal struct ProcessInfo { public IntPtr Process, Thread; public uint ProcessId, ThreadId; }
[StructLayout(LayoutKind.Sequential)]
internal struct BasicJobLimits
{
    public long ProcessTime, JobTime;
    public uint Flags;
    public UIntPtr MinimumWorkingSet, MaximumWorkingSet;
    public uint ActiveProcesses;
    public UIntPtr Affinity;
    public uint Priority, Scheduling;
}
[StructLayout(LayoutKind.Sequential)]
internal struct IoCounters { public ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; }
[StructLayout(LayoutKind.Sequential)]
internal struct JobLimits
{
    public BasicJobLimits Basic;
    public IoCounters Io;
    public UIntPtr ProcessMemory, JobMemory, PeakProcessMemory, PeakJobMemory;
}

internal static class Native
{
    [DllImport("kernel32.dll")] internal static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DuplicateHandle(IntPtr sourceProcess, SafeHandle source, IntPtr targetProcess, out SafeFileHandle target, uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile(string path, uint access, uint share, ref SecurityAttributes security, uint disposition, uint flags, IntPtr template);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool InitializeProcThreadAttributeList(IntPtr list, int count, int flags, ref nuint size);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UpdateProcThreadAttribute(IntPtr list, uint flags, IntPtr attribute, IntPtr value, nuint size, IntPtr previous, IntPtr returned);
    [DllImport("kernel32.dll")] internal static extern void DeleteProcThreadAttributeList(IntPtr list);
    [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CreateProcess(string application, StringBuilder command, IntPtr processSecurity, IntPtr threadSecurity,
        [MarshalAs(UnmanagedType.Bool)] bool inherit, uint flags, IntPtr environment, string? directory, ref StartupInfoEx startup, out ProcessInfo information);
    [DllImport("kernel32.dll", EntryPoint = "CreateJobObjectW", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern SafeFileHandle CreateJobObject(IntPtr security, string? name);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetInformationJobObject(SafeFileHandle job, int information, ref JobLimits limits, uint length);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);
    [DllImport("kernel32.dll", SetLastError = true)] internal static extern uint ResumeThread(IntPtr thread);
    [DllImport("kernel32.dll", SetLastError = true)] internal static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetExitCodeProcess(IntPtr process, out uint exit);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool TerminateProcess(IntPtr process, uint exit);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle(IntPtr handle);
}

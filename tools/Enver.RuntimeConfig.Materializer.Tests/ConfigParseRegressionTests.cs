using System.Text.Json;
using System.Text.Json.Nodes;
using Enver.RuntimeConfig.Materializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

internal static class ConfigParseRegressionTests
{
    private static int assertions, cases;
    private static void Check(bool ok)
    { assertions++; if (!ok) throw new Exception($"Config parse regression assertion failed: case={cases + 1}, assertion={assertions} (no payload)."); }

    internal static async Task Run(Dictionary<string, string> good, string[] canaries)
    {
        // Only synthetic payloads and a new, owned local fixture directory. Never /run/enver.
        var originalBase = AppContext.BaseDirectory;
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", "opencode", "materializer-config-parse-regressions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "defaults"));
        try
        {
            foreach (var name in new[] { "appsettings.json", "appsettings.Development.json" })
                File.Copy(Path.Combine(originalBase, "defaults", name), Path.Combine(root, "defaults", name));
            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", root + Path.DirectorySeparatorChar);
            var final = Path.Combine(root, "appsettings.Runtime.json");
            var temp = Path.Combine(root, ".runtime-fixture.tmp");
            var runtime = RuntimeMapping.Map(good);

            // 1: The actual precommit code succeeds with no final file. Its only file sources
            // are the two mandatory nonsecret defaults; the runtime source is JsonStream.
            Check(!File.Exists(final));
            var phases = new List<DeliveryPhase>();
            var trace = new DeliveryTrace(phases.Add);
            BindingDiagnostic.InMemory(runtime, trace);
            Check(phases.SequenceEqual(new[] { DeliveryPhase.ConfigParseValidation, DeliveryPhase.ConfigBindingValidation }));
            Check(trace.Target == ValidationTarget.Memory && !File.Exists(final));
            var defaults = BindingDiagnostic.Defaults().Sources.OfType<JsonConfigurationSource>().ToArray();
            Check(defaults.Length == 2 && defaults.All(s => !s.Optional && !s.ReloadOnChange));
            Check(defaults.Select(s => Path.GetFileName(s.Path)).Order().SequenceEqual(
                new[] { "appsettings.Development.json", "appsettings.json" }.Order()));
            cases++;

            // 2: The actual file provider validates the written/flushed temporary, not final.
            File.WriteAllText(temp, runtime.ToJsonString());
            // Exact old chain: required AddJsonFile -> PhysicalFileProvider -> hidden
            // .runtime-*.tmp treated as nonexistent. This is a real local reproduction,
            // not a simulated exception or a dependency on any /run/enver file.
            using (var provider = new PhysicalFileProvider(root))
            {
                Check(File.Exists(temp) && !provider.GetFileInfo(Path.GetFileName(temp)).Exists);
                using var unfiltered = new PhysicalFileProvider(root, Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
                Check(unfiltered.GetFileInfo(Path.GetFileName(temp)).Exists);
            }
            try
            {
                var legacy = Enver.API.Composition.RuntimeConfiguration.AddRequiredFile(BindingDiagnostic.Defaults(), temp).Build();
                (legacy as IDisposable)?.Dispose();
                Check(false);
            }
            catch (FileNotFoundException error) { Check(SafeException.Capture(error).Type == "FileNotFoundException"); }
            phases.Clear();
            BindingDiagnostic.FileBinding(temp, ValidationTarget.TemporaryFile, trace);
            Check(!File.Exists(final));
            Check(phases.SequenceEqual(new[] { DeliveryPhase.TemporaryConfigParseValidation, DeliveryPhase.TemporaryConfigBindingValidation }));
            Check(trace.Target == ValidationTarget.TemporaryFile);
            cases++;

            // 3: Optional explicit final-file validation is legal only after fixture rename.
            File.Move(temp, final);
            phases.Clear();
            BindingDiagnostic.FileBinding(final, ValidationTarget.FinalFile, trace);
            Check(File.Exists(final) && !File.Exists(temp));
            Check(phases.SequenceEqual(new[] { DeliveryPhase.FinalConfigParseValidation, DeliveryPhase.FinalConfigBindingValidation }));
            Check(trace.Target == ValidationTarget.FinalFile);
            cases++;
            File.Delete(final);

            // 4: Shared orchestration with a physical synthetic candidate: every requested
            // binding read is counted. No final path binding is requested before rename.
            var files = new FixtureStore(temp, final);
            phases.Clear();
            BindingDiagnostic.InMemory(runtime, trace);
            AtomicRuntimeFile.Write(runtime, trace, files);
            Check(files.FinalReadsBeforeRename == 0 && files.TemporaryReads == 1);
            Check(files.Committed && File.Exists(final) && !File.Exists(temp));
            Check(phases.Last() == DeliveryPhase.AtomicRename);
            Check(phases.IndexOf(DeliveryPhase.ConfigBindingValidation) < phases.IndexOf(DeliveryPhase.TempFileCreate));
            Check(phases.IndexOf(DeliveryPhase.FileFlush) < phases.IndexOf(DeliveryPhase.TemporaryConfigParseValidation));
            Check(phases.IndexOf(DeliveryPhase.TemporaryConfigBindingValidation) < phases.IndexOf(DeliveryPhase.AtomicRename));
            Check(!phases.Contains(DeliveryPhase.FinalConfigParseValidation));
            cases++;
            File.Delete(final);

            // 5-6: Reproduce each possible missing defaults file without guessing that either
            // caused the historical remote failure. FileName is compared, never logged.
            foreach (var name in new[] { "appsettings.json", "appsettings.Development.json" })
            {
                var path = Path.Combine(root, "defaults", name);
                File.Delete(path);
                try
                {
                    try { BindingDiagnostic.InMemory(runtime, trace); Check(false); }
                    catch (FileNotFoundException error)
                    {
                        Check(trace.Current == DeliveryPhase.ConfigParseValidation && trace.Target == ValidationTarget.Memory);
                        Check(SafeException.Capture(error).Code == "io-failed");
                        CheckSafeFailure(trace, error, "memory", canaries);
                    }
                }
                finally { File.Copy(Path.Combine(originalBase, "defaults", name), path); }
                cases++;
            }

            // 7-10: Each new phase carries a fixed target, including failures BEFORE the
            // operation. Exception text/filename/payload never become diagnostic values.
            File.WriteAllText(temp, runtime.ToJsonString());
            foreach (var (target, parse, bind, path) in new[]
            {
                (ValidationTarget.TemporaryFile, DeliveryPhase.TemporaryConfigParseValidation, DeliveryPhase.TemporaryConfigBindingValidation, temp),
                (ValidationTarget.FinalFile, DeliveryPhase.FinalConfigParseValidation, DeliveryPhase.FinalConfigBindingValidation, final)
            })
            {
                if (target == ValidationTarget.FinalFile) File.Move(temp, final);
                foreach (var phase in new[] { parse, bind })
                {
                    var fault = new DeliveryTrace(p => { if (p == phase) throw new FileNotFoundException(canaries[0], canaries[0]); });
                    try { BindingDiagnostic.FileBinding(path, target, fault); Check(false); }
                    catch (FileNotFoundException error)
                    {
                        Check(fault.Current == phase && fault.Target == target);
                        Check(SafeException.Capture(error).Code == "io-failed");
                        CheckSafeFailure(fault, error, target == ValidationTarget.FinalFile ? "final-file" : "temporary-file", canaries);
                    }
                    cases++;
                }
            }
            File.Delete(final);

            // 11: Invalid candidate binding remains fail-closed: no rename, no partial final.
            var invalid = runtime.DeepClone().AsObject();
            invalid["Redis"]!["NamespacePrefix"] = "wrong-contract";
            var rejected = new FixtureStore(temp, final);
            try { AtomicRuntimeFile.Write(invalid, new(), rejected); Check(false); }
            catch (DeliveryFailure) { Check(!rejected.Committed && !File.Exists(final) && !File.Exists(temp)); }
            cases++;

            // 12: Invalid JSON is not made optional; the required file provider rejects it.
            File.WriteAllText(temp, "{invalid-json");
            try { BindingDiagnostic.FileBinding(temp, ValidationTarget.TemporaryFile, trace); Check(false); }
            catch (JsonException) { Check(trace.Current == DeliveryPhase.TemporaryConfigParseValidation); }
            Check(!File.Exists(final));
            cases++;

            // 13: End-to-end injected orchestration still maps, parses, binds and commits once.
            File.Delete(temp);
            var whole = new FixtureRuntime(good, new FixtureStore(temp, final));
            using var output = new StringWriter();
            Check(await Enver.RuntimeConfig.Materializer.Program.Run(["--materialize"], whole, output, new()) == 0);
            Check(whole.Store.Committed && whole.Store.FinalReadsBeforeRename == 0 && whole.Store.TemporaryReads == 1);
            Check(whole.Reads == 4 && canaries.All(c => !output.ToString().Contains(c, StringComparison.Ordinal)));
            cases++;
        }
        catch (Exception error)
        {
            var fileId = "unclassified";
            if (error is FileNotFoundException missing)
            {
                foreach (var known in new[] { "appsettings.json", "appsettings.Development.json", "appsettings.Runtime.json",
                    ".runtime-fixture.tmp", "Enver.API", "Enver.RuntimeConfig.Materializer" })
                    if (missing.FileName == known || missing.FileName == Path.Combine(root, known) ||
                        missing.FileName == Path.Combine(root, "defaults", known) || missing.FileName?.StartsWith(known + ", Version=", StringComparison.Ordinal) == true)
                        fileId = known;
            }
            Console.WriteLine(JsonSerializer.Serialize(new { regressionFailureCase = cases + 1, safeType = SafeException.Capture(error).Type, fileId }));
            throw;
        }
        finally
        {
            AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBase);
            Directory.Delete(root, recursive: true); // Only this test's freshly created fixture.
        }
        Console.WriteLine($"Config parse regression scenarios: {cases}/{cases} PASS; {assertions}/{assertions} assertions PASS; synthetic local fixtures only; AWS/API calls=0.");
    }

    private static void CheckSafeFailure(DeliveryTrace trace, Exception error, string target, string[] canaries)
    {
        using var output = new StringWriter();
        trace.WriteFailure(output, error, default);
        var text = output.ToString();
        using var doc = JsonDocument.Parse(text);
        Check(doc.RootElement.GetProperty("validation-target").GetString() == target);
        Check(doc.RootElement.GetProperty("exception").GetString() == "FileNotFoundException");
        Check(doc.RootElement.GetProperty("category").GetString() == "filesystem");
        Check(canaries.All(c => !text.Contains(c, StringComparison.Ordinal)));
        Check(doc.RootElement.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(
            new[] { "phase", "exception", "inner", "cancelled", "category", "code", "validation-target" }.Order()));
    }

    private sealed class FixtureStore(string temp, string final) : IRuntimeFileStore
    {
        internal bool Committed;
        internal int FinalReadsBeforeRename, TemporaryReads;
        public void Prepare() { }
        public IRuntimeFileAttempt CreateTemporary() => new Attempt(this, temp, final);
        private void Bind(string path, DeliveryTrace trace)
        {
            if (path == final && !Committed) FinalReadsBeforeRename++;
            if (path == temp) TemporaryReads++;
            BindingDiagnostic.FileBinding(path, ValidationTarget.TemporaryFile, trace);
        }
        private sealed class Attempt(FixtureStore owner, string temp, string final) : IRuntimeFileAttempt
        {
            // This is not the Linux production writer. Only synthetic local data is written.
            private FileStream? stream = new(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            private bool flushed, permissions, validated, synced;
            public void Write(JsonObject config) { using var json = new Utf8JsonWriter(stream!); config.WriteTo(json); json.Flush(); }
            public void Flush() { stream!.Flush(true); stream.Dispose(); stream = null; flushed = true; }
            public void ValidateConfiguration(DeliveryTrace trace) { Check(flushed && !File.Exists(final)); owner.Bind(temp, trace); }
            public void SetPermissions() { permissions = true; } // Simulated; no chmod claim on Windows.
            public void ValidateFile() { Check(flushed && permissions); validated = true; }
            public void SyncDirectory() { Check(validated); synced = true; }
            public void Commit() { Check(synced); File.Move(temp, final); owner.Committed = true; }
            public void Dispose() { stream?.Dispose(); if (!owner.Committed) File.Delete(temp); }
        }
    }

    private sealed class FixtureRuntime(Dictionary<string, string> payloads, FixtureStore store) : IDeliveryRuntime, ISecretReader
    {
        internal FixtureStore Store => store;
        internal int Reads;
        public IRuntimeFileStore Files => store;
        public Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken) => Task.FromResult<ISecretReader>(this);
        public Task<string> Read(SecretPin pin, CancellationToken cancellationToken) { Reads++; return Task.FromResult(payloads[pin.Name]); }
        public void ValidateRuntime(DeliveryTrace trace) => throw new InvalidOperationException();
        public void Dispose() { }
    }
}

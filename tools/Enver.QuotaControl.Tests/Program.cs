using System.Text.Json;
using System.Text.RegularExpressions;
using Enver.QuotaControl;

namespace Enver.QuotaControl.Tests;

internal static class Assert
{
    public static void True(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Assertion failed");
    }
    public static void Equal<T>(T expected, T actual) => True(EqualityComparer<T>.Default.Equals(expected, actual));
    public static void Throws(Failure failure, Action action)
    {
        try { action(); }
        catch (ControlException exception) { Equal(failure, exception.Failure); return; }
        throw new InvalidOperationException("Expected failure");
    }
    public static async Task ThrowsAsync(Failure failure, Func<Task> action)
    {
        try { await action(); }
        catch (ControlException exception) { Equal(failure, exception.Failure); return; }
        throw new InvalidOperationException("Expected failure");
    }
}

internal static class Program
{
    // Fixture literal NO operativa. No RNG, Guid.NewGuid o generacion de epoca en tests/producto.
    private const string G = "11111111111111111111111111111111";
    private static readonly CancellationToken Token = CancellationToken.None;
    private static ApprovedDescriptor Descriptor => ApprovedDescriptor.Baseline;
    private const string Member = "login:email:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    public static async Task<int> Main()
    {
        var tests = new List<(string Name, Func<Task> Body)>();
        void Sync(string name, Action test) => tests.Add((name, () => { test(); return Task.CompletedTask; }));
        void Async(string name, Func<Task> test) => tests.Add((name, test));

        Sync("descriptor y snapshot exactos", () =>
        {
            Assert.Equal("development|quota-development-academic-initial-v1|hmac-development-fecf274e-v1|academic-initial|5000|67108864|3|900000000|20|900000000|10|60000000|60|60000000|5|60000000|300|60000000|30|60000000", Descriptor.Snapshot());
            Assert.Equal(20, Descriptor.Snapshot().Split('|').Length);
            ApprovedDescriptor.ParseJson(JsonSerializer.Serialize(Descriptor));
        });
        Sync("descriptor distinto y C/I distintas abortan", () =>
        {
            foreach (var value in new[] { Descriptor with { Environment = "dev" }, Descriptor with { ConfigurationVersion = "other" },
                Descriptor with { IdentityReference = "other" }, Descriptor with { PolicyRevision = "other" },
                Descriptor with { Capacity = 4999 }, Descriptor with { MemoryBudgetBytes = 1 },
                Descriptor with { Rules = Descriptor.Rules.Reverse().ToArray() },
                Descriptor with { Rules = [.. Descriptor.Rules.Skip(1)] } })
                Assert.Throws(Failure.InvalidDescriptor, value.Validate);
        });
        Sync("descriptor rechaza campos desconocidos duplicados y ausentes", () =>
        {
            var json = JsonSerializer.Serialize(Descriptor);
            foreach (var value in new[] { "{\"Password\":\"NOT_A_SECRET\"," + json[1..],
                "{\"Environment\":\"development\"," + json[1..], "{}", "null" })
                Assert.Throws(Failure.InvalidDescriptor, () => ApprovedDescriptor.ParseJson(value));
        });
        Sync("G invalida aborta", () =>
        {
            foreach (var generation in new[] { "", new string('0', 32), new string('A', 32), new string('g', 32),
                new string('1', 31), new string('1', 33), " " + G })
                Assert.Throws(Failure.InvalidGeneration, () => ClosedPlan.Build(Descriptor, generation, 1));
        });
        Sync("plan exacto de doce campos", () =>
        {
            var p = ClosedPlan.Build(Descriptor, G, 1700000000000000);
            Assert.Equal(12, p.Fields.Count);
            foreach (var pair in new Dictionary<string, string> { ["state"] = "closed", ["dirty"] = "0", ["count"] = "0",
                ["horizon"] = "0", ["version"] = "1", ["maximumWindow"] = "900000000", ["drain"] = "1700000900000000" })
                Assert.Equal(pair.Value, p.Fields[pair.Key]);
            Assert.Equal("UNKNOWN", p.BaseGenerationStatus);
            Assert.True(!p.OperationallyApproved);
        });
        Sync("tiempo invalido y overflow abortan", () =>
        {
            foreach (var time in new[] { 0L, -1L, Contract.MaximumExactInteger - Contract.MaximumWindow + 1, long.MaxValue })
                Assert.Throws(Failure.InvalidTime, () => ClosedPlan.Build(Descriptor, G, time));
            ClosedPlan.Build(Descriptor, G, Contract.MaximumExactInteger - Contract.MaximumWindow);
        });
        Sync("parser CLI solo comandos y opciones explicitos", () =>
        {
            Assert.Equal("plan-closed", CommandLine.Parse(["plan-closed", "--descriptor", "d", "--generation", G, "--server-time-us", "1"]).Command);
            foreach (var args in new string[][] { ["activate"], ["initialize"], ["inventory", "--password", "NOT_A_SECRET"],
                ["inventory", "--connection-string", "NOT_A_SECRET"], ["inventory", "--host", "localhost"],
                ["inventory", "--descriptor", "d", "--descriptor", "e"], ["inventory", "--descriptor"],
                ["plan-closed", "--descriptor", "d", "--generation", G], ["precheck", "--descriptor", "d", "--generation", G] })
                Assert.Throws(Failure.InvalidArguments, () => CommandLine.Parse(args));
        });
        Async("precheck vacio no escribe", async () =>
        {
            var fake = new FakeRedis();
            Assert.True((await new ControlService(fake).PrecheckAsync(Descriptor, Token)).IsEmpty);
            Assert.True(fake.Commands.All(c => c.Verb != RedisVerb.EVAL));
            Assert.Equal(0, fake.HashWrites);
        });
        foreach (var key in new[] { Contract.Metadata, Contract.Registry })
        {
            var captured = key;
            Async(captured == Contract.Metadata ? "metadata existente aborta antes EVAL" : "registry existente aborta antes EVAL", async () =>
            {
                var fake = new FakeRedis();
                fake.Keys[captured] = new(captured == Contract.Metadata ? "hash" : "zset");
                await Assert.ThrowsAsync(Failure.ResidueFound, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
                Assert.True(fake.Commands.All(c => c.Verb != RedisVerb.EVAL));
                Assert.Equal(0, fake.HashWrites);
            });
        }
        Async("bucket encontrado aborta", async () =>
        {
            var fake = new FakeRedis();
            fake.Keys[Contract.BucketPrefix + Member] = new("zset");
            await Assert.ThrowsAsync(Failure.ResidueFound, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal(0, fake.HashWrites);
        });
        Async("SCAN cursor hasta cero paginas vacias duplicados TYPE sin filtro", async () =>
        {
            var fake = new FakeRedis();
            var key = Contract.BucketPrefix + Member;
            fake.Keys[key] = new("string"); // Debe descubrirse tambien con tipo incorrecto.
            fake.ScanPages.Enqueue(("12", []));
            fake.ScanPages.Enqueue(("8", [key, key]));
            fake.ScanPages.Enqueue(("0", [key]));
            var result = await new ControlService(fake).InventoryAsync(Descriptor, Token);
            Assert.Equal(1, result.BucketCount);
            Assert.True(result.Discrepancies > 0);
            Assert.Equal(3, fake.Commands.Count(c => c.Verb == RedisVerb.SCAN));
            Assert.Equal(1, fake.Commands.Count(c => c.Verb == RedisVerb.TYPE && c.Arguments[0] == key));
            Assert.True(fake.Commands.Where(c => c.Verb == RedisVerb.SCAN).Select(c => c.Arguments[0]).SequenceEqual(new[] { "0", "12", "8" }));
        });
        Async("registry y bucket zset se leen con rangos WITHSCORES", async () =>
        {
            var fake = new FakeRedis();
            fake.Keys[Contract.Registry] = new("zset");
            fake.Keys[Contract.Registry].Members.Add(new(Member, fake.Now));
            fake.Keys[Contract.BucketPrefix + Member] = new("zset");
            fake.Keys[Contract.BucketPrefix + Member].Members.Add(new(G, fake.Now - 60000000));
            var result = await new ControlService(fake).InventoryAsync(Descriptor, Token);
            Assert.Equal(1, result.RegistryMemberCount);
            Assert.True(fake.Commands.Any(c => c.Verb == RedisVerb.ZRANGE && c.Arguments[0] == Contract.Registry && c.Arguments[3] == "WITHSCORES"));
            Assert.True(fake.Commands.Any(c => c.Verb == RedisVerb.ZRANGE && c.Arguments[0].StartsWith(Contract.BucketPrefix, StringComparison.Ordinal)));
            Assert.True(!JsonSerializer.Serialize(result.Sanitize()).Contains(Member, StringComparison.Ordinal));
        });
        Async("cursor invalido o repetido no prueba inventario", async () =>
        {
            var fake = new FakeRedis();
            fake.ScanPages.Enqueue(("3", [])); fake.ScanPages.Enqueue(("3", []));
            await Assert.ThrowsAsync(Failure.InventoryIncomplete, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
            fake = new FakeRedis(); fake.ScanPages.Enqueue(("-1", []));
            await Assert.ThrowsAsync(Failure.UnexpectedResponse, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
        });
        Async("SCAN no acepta keys fuera del scope", async () =>
        {
            var fake = new FakeRedis(); fake.ScanPages.Enqueue(("0", ["other:NOT_A_SECRET"]));
            await Assert.ThrowsAsync(Failure.UnexpectedResponse, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
            Assert.True(fake.Commands.All(c => c.Verb == RedisVerb.SCAN || !c.Arguments.Contains("other:NOT_A_SECRET")));
        });
        Async("cambio de session invalida inventario", async () =>
        {
            var fake = new FakeRedis();
            fake.BeforeCommand = c => { if (c.Verb == RedisVerb.SCAN) fake.SessionIdentity = new(); };
            await Assert.ThrowsAsync(Failure.InventoryIncomplete, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
        });
        Async("apply bloqueado para transporte no simulado", async () =>
        {
            var fake = new FakeRedis { IsLocalSimulation = false };
            await Assert.ThrowsAsync(Failure.ApplyDisabled, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal(0, fake.Commands.Count);
        });
        Async("apply CAS simulado crea solo closed y readback", async () =>
        {
            var fake = new FakeRedis();
            var result = await new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token);
            Assert.Equal("created_closed", result.Status);
            Assert.True(result.ClosedReadbackVerified);
            Assert.Equal(1, fake.HashWrites);
            Assert.Equal(1, fake.Commands.Count(c => c.Verb == RedisVerb.EVAL));
            Assert.Equal(1, fake.Keys.Count);
            Assert.Equal("closed", fake.Keys[Contract.Metadata].Fields["state"]);
        });
        foreach (var key in new[] { Contract.Metadata, Contract.Registry })
        {
            var captured = key;
            Async(captured == Contract.Metadata ? "CAS carrera metadata aborta sin sobrescribir" : "CAS carrera registry aborta sin escribir", async () =>
            {
                var fake = new FakeRedis();
                fake.BeforeCommand = c => { if (c.Verb == RedisVerb.EVAL) fake.Keys[captured] = new("string"); };
                await Assert.ThrowsAsync(captured == Contract.Metadata ? Failure.MetadataExists : Failure.RegistryExists,
                    () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
                Assert.Equal(0, fake.HashWrites);
                Assert.Equal("string", fake.Keys[captured].Type);
            });
        }
        Async("respuesta perdida deja closed y no reintenta", async () =>
        {
            var fake = new FakeRedis { LoseApplyResponse = true };
            await Assert.ThrowsAsync(Failure.ResultUncertain, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal(1, fake.HashWrites);
            Assert.Equal(1, fake.Commands.Count(c => c.Verb == RedisVerb.EVAL));
            Assert.Equal("closed", fake.Keys[Contract.Metadata].Fields["state"]);
        });
        Async("respuesta EVAL inesperada falla cerrada", async () =>
        {
            var fake = new FakeRedis { Override = c => c.Verb == RedisVerb.EVAL ? RedisReply.String("NOT_A_SECRET_UNEXPECTED") : null };
            await Assert.ThrowsAsync(Failure.ResultUncertain, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal(1, fake.Commands.Count(c => c.Verb == RedisVerb.EVAL));
        });
        Sync("parser Lua rechaza forma contrato status y tiempos inesperados", () =>
        {
            foreach (var reply in new[] { RedisReply.Null(), RedisReply.Error(), RedisReply.Array(),
                FakeRedis.ApplyReply("ready", "ok", 1), FakeRedis.ApplyReply("created_closed", "ok", 0),
                FakeRedis.ApplyReply("created_closed", "ok", long.MaxValue), FakeRedis.ApplyReply("aborted", "unknown", 0) })
                Assert.Throws(Failure.ResultUncertain, () => ControlService.ParseApplyResponse(reply));
        });
        Async("readback closed valido", async () =>
        {
            var fake = new FakeRedis(); fake.PutClosed(G);
            Assert.Equal("closed", (await new ControlService(fake).ReadbackAsync(Descriptor, G, Token)).Fields["state"]);
            Assert.True(fake.Commands.Any(c => c.Verb == RedisVerb.HGETALL));
        });
        foreach (var mutation in new[] { "extra", "missing", "ttl", "state", "snapshot", "generation", "dirty", "count", "horizon", "version", "drain", "protocol", "maximumWindow", "last" })
        {
            var captured = mutation;
            Async("readback aborta " + captured, async () =>
            {
                var fake = new FakeRedis(); fake.PutClosed(G);
                var key = fake.Keys[Contract.Metadata];
                switch (captured)
                {
                    case "extra": key.Fields["extra"] = "NOT_A_SECRET"; break;
                    case "missing": key.Fields.Remove("last"); break;
                    case "ttl": key.Pttl = 1000; key.ExpireTime = fake.Now / 1000 + 1000; break;
                    case "state": key.Fields["state"] = "ready"; break;
                    case "snapshot": key.Fields["snapshot"] = "other"; break;
                    case "generation": key.Fields["generation"] = new string('2', 32); break;
                    default: key.Fields[captured] = "9"; break;
                }
                await Assert.ThrowsAsync(Failure.ReadbackMismatch, () => new ControlService(fake).ReadbackAsync(Descriptor, G, Token));
            });
        }
        Async("readback no acepta registry o bucket final", async () =>
        {
            foreach (var key in new[] { Contract.Registry, Contract.BucketPrefix + Member })
            {
                var fake = new FakeRedis(); fake.PutClosed(G); fake.Keys[key] = new("zset");
                await Assert.ThrowsAsync(Failure.ReadbackMismatch, () => new ControlService(fake).ReadbackAsync(Descriptor, G, Token));
            }
        });
        Sync("Lua auditoria estructural comandos y snapshot", () =>
        {
            var lua = RecoveryScript.Text;
            var calls = Regex.Matches(lua, @"redis\.call\('([A-Z]+)'", RegexOptions.CultureInvariant).Select(m => m.Groups[1].Value).ToArray();
            Assert.True(calls.SequenceEqual(new[] { "TYPE", "TYPE", "TIME", "HSET" }));
            var snapshot = Regex.Match(lua, @"local snapshot = '([^']+)'", RegexOptions.CultureInvariant).Groups[1].Value;
            Assert.Equal(Descriptor.Snapshot(), snapshot);
            Assert.True(lua.IndexOf("'HSET'", StringComparison.Ordinal) > lua.IndexOf("'registry_exists'", StringComparison.Ordinal));
            Assert.True(lua.Contains("#KEYS ~= 2 or #ARGV ~= 2", StringComparison.Ordinal));
            Assert.True(lua.Contains("'state', 'closed'", StringComparison.Ordinal));
            foreach (var name in Contract.FieldNames) Assert.True(lua.Contains("'" + name + "'", StringComparison.Ordinal));
            Assert.True(!Regex.IsMatch(lua, @"'state'\s*,\s*'ready'", RegexOptions.CultureInvariant));
            var body = Regex.Match(lua, @"redis\.call\('HSET', KEYS\[1\],(.*?)\)", RegexOptions.Singleline | RegexOptions.CultureInvariant).Groups[1].Value;
            var pairs = Regex.Matches(body, @"'([^']+)'\s*,\s*('[^']*'|[A-Za-z]+)", RegexOptions.CultureInvariant)
                .Select(match => match.Groups[1].Value + "=" + match.Groups[2].Value).ToArray();
            Assert.True(pairs.SequenceEqual(new[] { "protocol='1'", "snapshot=snapshot", "generation=generation", "state='closed'",
                "count='0'", "horizon='0'", "dirty='0'", "version='1'", "fence=instant", "drain=drain", "last=instant", "maximumWindow='900000000'" }));
        });
        Async("descriptor o G invalidos impiden toda E/S", async () =>
        {
            var fake = new FakeRedis();
            await Assert.ThrowsAsync(Failure.InvalidDescriptor, () => new ControlService(fake).ApplyClosedAsync(Descriptor with { IdentityReference = "other" }, G, Token));
            await Assert.ThrowsAsync(Failure.InvalidGeneration, () => new ControlService(fake).ApplyClosedAsync(Descriptor, "invalid", Token));
            Assert.Equal(0, fake.Commands.Count);
        });
        Async("HGETALL duplicado o respuesta impar falla cerrada", async () =>
        {
            foreach (var fields in new[] { RedisReply.Array(RedisReply.String("state")),
                RedisReply.Array(RedisReply.String("state"), RedisReply.String("closed"), RedisReply.String("state"), RedisReply.String("closed")) })
            {
                var fake = new FakeRedis(); fake.PutClosed(G);
                fake.Override = c => c.Verb == RedisVerb.HGETALL ? fields : null;
                await Assert.ThrowsAsync(Failure.UnexpectedResponse, () => new ControlService(fake).ReadbackAsync(Descriptor, G, Token));
            }
        });
        Async("limites de inventario no se truncan", async () =>
        {
            var fake = new FakeRedis(); fake.Keys[Contract.Registry] = new("zset");
            fake.Override = c => c.Verb == RedisVerb.ZCARD ? RedisReply.Integer(5001) : null;
            await Assert.ThrowsAsync(Failure.InventoryIncomplete, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
            fake = new FakeRedis(); fake.Keys[Contract.BucketPrefix + Member] = new("zset");
            fake.Override = c => c.Verb == RedisVerb.ZCARD ? RedisReply.Integer(301) : null;
            await Assert.ThrowsAsync(Failure.InventoryIncomplete, () => new ControlService(fake).InventoryAsync(Descriptor, Token));
        });
        Async("cambios metadata durante SCAN impiden precheck", async () =>
        {
            var fake = new FakeRedis();
            fake.BeforeCommand = c => { if (c.Verb == RedisVerb.SCAN) fake.PutClosed(G); };
            await Assert.ThrowsAsync(Failure.ResidueFound, () => new ControlService(fake).PrecheckAsync(Descriptor, Token));
            Assert.Equal(0, fake.HashWrites);
        });
        Async("bucket tardio no se borra y readback detecta exclusion rota", async () =>
        {
            var fake = new FakeRedis();
            fake.BeforeCommand = c => { if (c.Verb == RedisVerb.EVAL) fake.Keys[Contract.BucketPrefix + Member] = new("string"); };
            await Assert.ThrowsAsync(Failure.ResultUncertain, () => new ControlService(fake).ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal("closed", fake.Keys[Contract.Metadata].Fields["state"]);
            Assert.True(fake.Keys.ContainsKey(Contract.BucketPrefix + Member));
            Assert.Equal(1, fake.HashWrites);
        });
        Async("segunda aplicacion no reescribe metadata", async () =>
        {
            var fake = new FakeRedis();
            var service = new ControlService(fake);
            await service.ApplyClosedAsync(Descriptor, G, Token);
            await Assert.ThrowsAsync(Failure.ResidueFound, () => service.ApplyClosedAsync(Descriptor, G, Token));
            Assert.Equal(1, fake.HashWrites);
        });
        Sync("errores de transporte y TTL malformed no exponen payload", () =>
        {
            Assert.Equal("Error", RedisReply.Error().ToString());
            Assert.Equal("EVAL", RedisCommand.Recover(Descriptor, G).ToString());
            Assert.Throws(Failure.InvalidArguments, () => RedisCommand.ReadKey(RedisVerb.TYPE, "outside"));
        });
        Async("cancelacion anterior al despacho no escribe", async () =>
        {
            var fake = new FakeRedis();
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            try { await new ControlService(fake).ApplyClosedAsync(Descriptor, G, cancellation.Token); throw new InvalidOperationException(); }
            catch (OperationCanceledException) { Assert.Equal(0, fake.HashWrites); }
        });
        Async("CLI no eco de entradas rechaza secretos y no conecta", async () =>
        {
            using var writer = new StringWriter();
            var code = await Enver.QuotaControl.Program.RunAsync(["inventory", "--password", "NOT_A_SECRET_SENTINEL"], writer);
            Assert.Equal(InventoryExit.Policy, code);
            Assert.True(!writer.ToString().Contains("NOT_A_SECRET_SENTINEL", StringComparison.Ordinal));
            await Assert.ThrowsAsync(Failure.RemoteTransportDisabled, () => new ControlService(new DisabledRedisClient()).InventoryAsync(Descriptor, Token));
        });
        Async("CLI plan local y bloqueo total de comandos remotos", async () =>
        {
            var descriptor = Path.Combine(AppContext.BaseDirectory, "descriptor.approved.json");
            using var writer = new StringWriter();
            var code = await Enver.QuotaControl.Program.RunAsync(["plan-closed", "--descriptor", descriptor,
                "--generation", G, "--server-time-us", "1700000000000000"], writer);
            Assert.Equal(0, code);
            using var json = JsonDocument.Parse(writer.ToString());
            Assert.Equal("planned_not_applied", json.RootElement.GetProperty("status").GetString());
            Assert.Equal(12, json.RootElement.GetProperty("fields").EnumerateObject().Count());
            Assert.True(!json.RootElement.GetProperty("serverTimeVerified").GetBoolean());
            foreach (var command in new[] { "precheck", "inventory", "readback", "apply-closed" })
            {
                using var blocked = new StringWriter();
                string[] args = command is "readback" or "apply-closed"
                    ? [command, "--descriptor", descriptor, "--generation", G]
                    : [command, "--descriptor", descriptor];
                Assert.Equal(command == "inventory" ? InventoryExit.Incomplete : 2, await Enver.QuotaControl.Program.RunAsync(args, blocked));
                Assert.True(blocked.ToString().Contains(command == "apply-closed" ? "ApplyDisabled" : "RemoteTransportDisabled", StringComparison.Ordinal));
            }
        });

        RemoteTests.Add(tests);
        var failed = 0;
        foreach (var (name, test) in tests)
        {
            try { await test(); Console.WriteLine("PASS " + name); }
            catch { failed++; Console.WriteLine("FAIL " + name); } // Nunca imprime datos de entrada ni payloads.
        }
        Console.WriteLine(JsonSerializer.Serialize(new { suite = "quota-control-local-unit", total = tests.Count,
            passed = tests.Count - failed, failed, redisReal = false, docker = false, aws = false, operationalGenerationCreated = false,
            luaExecution = "not_executed_static_audit_and_cas_model_only" }));
        return failed == 0 ? 0 : 1;
    }
}

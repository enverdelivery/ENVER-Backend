using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using Enver.QuotaControl;

namespace Enver.QuotaControl.Tests;

internal static class RemoteTests
{
    private static readonly RemoteOptions Options = new(ConnectionRoute.SsmTunnel, 16379);
    private static ApprovedDescriptor Descriptor => ApprovedDescriptor.Baseline;

    public static void Add(List<(string Name, Func<Task> Body)> tests)
    {
        void Sync(string name, Action test) => tests.Add(("remote: " + name, () => { test(); return Task.CompletedTask; }));
        void Async(string name, Func<Task> test) => tests.Add(("remote: " + name, test));

        Sync("TLS fijo, DB0, SNI real aun con socket loopback", () =>
        {
            Assert.Equal(RemoteOptions.Endpoint, new RemoteOptions(ConnectionRoute.Direct).ConnectHost);
            Assert.Equal(6379, new RemoteOptions(ConnectionRoute.Direct).ConnectPort);
            Assert.Equal("127.0.0.1", Options.ConnectHost);
            Assert.Equal(16379, Options.ConnectPort);
            Assert.Equal(0, Options.Database);
            var tls = Options.TlsOptions();
            Assert.Equal(RemoteOptions.Endpoint, tls.TargetHost);
            Assert.Equal(SslProtocols.Tls12 | SslProtocols.Tls13, tls.EnabledSslProtocols);
            Assert.True(!tls.AllowTlsResume);
            Assert.True(tls.RemoteCertificateValidationCallback is not null);
            Assert.Equal(System.Security.Cryptography.X509Certificates.X509RevocationMode.Online, tls.CertificateRevocationCheckMode);
            foreach (var error in new[] { SslPolicyErrors.None, SslPolicyErrors.RemoteCertificateNameMismatch, SslPolicyErrors.RemoteCertificateChainErrors })
                Assert.True(!RemoteOptions.ValidateCertificate(new(), null, null, error));
        });
        Sync("no downgrade/ruta/puerto arbitrarios", () =>
        {
            Assert.Throws(Failure.PolicyFailure, () => new RemoteOptions(ConnectionRoute.Direct, 6379));
            Assert.Throws(Failure.PolicyFailure, () => new RemoteOptions(ConnectionRoute.SsmTunnel));
            Assert.Throws(Failure.PolicyFailure, () => new RemoteOptions(ConnectionRoute.SsmTunnel, 80));
            Assert.Throws(Failure.PolicyFailure, () => new RemoteOptions((ConnectionRoute)99));
        });
        Sync("allowlist exacta y por subcomando", () =>
        {
            foreach (var verb in new[] { "AUTH", "SCAN", "TYPE", "PTTL", "PEXPIRETIME", "HGETALL", "ZCARD", "ZRANGE", "TIME" })
            {
                RemotePolicy.EnsureAllowed("inventory", verb);
                foreach (var operation in new[] { "apply-closed", "readback", "precheck", "plan-closed", "other" })
                    Assert.Throws(Failure.PolicyFailure, () => RemotePolicy.EnsureAllowed(operation, verb));
            }
            foreach (var verb in new[] { "KEYS", "EVAL", "EVALSHA", "HSET", "DEL", "ZREM", "PERSIST", "FLUSHDB", "FLUSHALL", "CONFIG", "MONITOR", "CLIENT", "CLIENT KILL", "COMMAND", "SELECT", "PING", "HELLO", "SCRIPT", "scan" })
                Assert.Throws(Failure.PolicyFailure, () => RemotePolicy.EnsureAllowed("inventory", verb));
        });
        Async("frame credencial, ToString saneado y dispose", async () =>
        {
            using var frame = CredentialFixture.Frame();
            var credential = await PipeCredential.ReadAsync(frame, CancellationToken.None);
            Assert.Equal("[REDACTED]", credential.ToString());
            credential.Dispose(); credential.Dispose();
        });
        Async("credencial invalida no abre transporte", async () =>
        {
            foreach (var secret in new[] { "short", new string('x', 129), "INVALID_AT_CHARACTER@", "INVALID_SLASH_CHAR/" })
            {
                var factory = new LocalSessionFactory();
                using var frame = CredentialFixture.Frame(secret);
                using var output = new StringWriter();
                Assert.Equal(60, await InventoryExecution.RunAsync(Options, Descriptor, frame, factory, output));
                Assert.Equal(0, factory.Calls);
                Assert.True(!output.ToString().Contains(secret, StringComparison.Ordinal));
            }
        });
        Async("credencial con trailing bytes o truncada aborta", async () =>
        {
            using var extra = CredentialFixture.Frame(extra: true);
            await Assert.ThrowsAsync(Failure.CredentialFailure, () => PipeCredential.ReadAsync(extra, CancellationToken.None));
            using var shortFrame = new MemoryStream(new byte[] { 0, 0, 0, 20, 65 });
            await Assert.ThrowsAsync(Failure.CredentialFailure, () => PipeCredential.ReadAsync(shortFrame, CancellationToken.None));
        });
        Async("RESP tipos validos y nulls", async () =>
        {
            foreach (var (wire, kind) in new[] { ("+OK\r\n", ReplyKind.SimpleString), (":-2\r\n", ReplyKind.Integer),
                ("$3\r\nabc\r\n", ReplyKind.Text), ("$0\r\n\r\n", ReplyKind.Text), ("*0\r\n", ReplyKind.Array),
                ("$-1\r\n", ReplyKind.Null), ("*-1\r\n", ReplyKind.Null), ("*2\r\n$1\r\n0\r\n*0\r\n", ReplyKind.Array) })
                Assert.Equal(kind, (await Parse(wire)).Kind);
        });
        Async("RESP errores saneados clasificados", async () =>
        {
            foreach (var (wire, category) in new[] { ("-WRONGPASS " + CredentialFixture.Sentinel + "\r\n", ReplyError.Authentication),
                ("-NOAUTH denied\r\n", ReplyError.Authentication), ("-NOPERM denied\r\n", ReplyError.NoPermission),
                ("-MOVED 1 unexpected:6379\r\n", ReplyError.Topology), ("-ERR denied\r\n", ReplyError.Unknown) })
            {
                var reply = await Parse(wire);
                Assert.Equal(category, reply.ErrorCategory);
                Assert.Equal("Error", reply.ToString());
            }
        });
        Async("RESP malformed, limites y profundidad", async () =>
        {
            foreach (var wire in new[] { "", "+OK\n", ":+1\r\n", ":01\r\n", ":-0\r\n", ":9223372036854775808\r\n", "$-2\r\n",
                "$4097\r\n", "$3\r\nab\r\n", "$1\r\na\n", "*20001\r\n", "*1\r\n*1\r\n*1\r\n:1\r\n", "%0\r\n", "+" + new string('x', 1025) + "\r\n" })
                await Assert.ThrowsAsync(Failure.ProtocolFailure, () => Parse(wire));
            using var invalidUtf8 = new MemoryStream(new byte[] { 36, 49, 13, 10, 255, 13, 10 });
            await Assert.ThrowsAsync(Failure.ProtocolFailure, () => RespCodec.ReadAsync(invalidUtf8, CancellationToken.None));
        });
        Sync("respuesta incompatible con comando no se acepta", () =>
        {
            Assert.Throws(Failure.ProtocolFailure, () => RemotePolicy.ValidateReply(RedisCommand.ReadKey(RedisVerb.TYPE, Contract.Metadata), RedisReply.String("none")));
            Assert.Throws(Failure.ProtocolFailure, () => RemotePolicy.ValidateReply(RedisCommand.Time(), RedisReply.Null()));
            Assert.Throws(Failure.ProtocolFailure, () => RemotePolicy.ValidateReply(RedisCommand.ReadKey(RedisVerb.PTTL, Contract.Metadata), RedisReply.String("-2")));
            Assert.Throws(Failure.ProtocolFailure, () => RemotePolicy.ValidateReply(RedisCommand.ReadKey(RedisVerb.HGETALL, Contract.Metadata), RedisReply.Array(RedisReply.String("one"))));
        });
        Async("inventario completo vacio exit0 y evidencia sin secretos", async () =>
        {
            var factory = new LocalSessionFactory();
            factory.Wire.MaximumReadChunk = 1;
            factory.Wire.Data.ScanPages.Enqueue(("12", []));
            factory.Wire.Data.ScanPages.Enqueue(("0", []));
            var (code, output) = await Run(factory);
            Assert.Equal(0, code);
            using var json = JsonDocument.Parse(output);
            var root = json.RootElement;
            Assert.Equal("CURRENT INVENTORY EMPTY", root.GetProperty("conclusion").GetString());
            Assert.True(!root.GetProperty("exclusionAccreditedByTool").GetBoolean());
            var trace = root.GetProperty("execution");
            Assert.Equal("0", trace.GetProperty("finalCursor").GetString());
            Assert.Equal(2, trace.GetProperty("pages").GetInt32());
            Assert.True(trace.GetProperty("completed").GetBoolean());
            Assert.Equal(0, trace.GetProperty("duplicateKeys").GetInt32());
            Assert.Equal(factory.Wire.Data.Now, trace.GetProperty("serverTimeMicroseconds").GetInt64());
            Assert.True(trace.GetProperty("scanStartedUtc").ValueKind == JsonValueKind.String);
            Assert.True(trace.GetProperty("scanFinishedUtc").ValueKind == JsonValueKind.String);
            Assert.True(factory.Session.Disposed && factory.Wire.Disposed);
            Assert.Equal(1, factory.Calls);
            Assert.Equal(1, factory.Wire.Verbs.Count(v => v == "AUTH"));
            Assert.True(!output.Contains(CredentialFixture.Sentinel, StringComparison.Ordinal));
            Assert.True(!output.Contains("UNDER ACCREDITED EXCLUSION", StringComparison.Ordinal));
        });
        foreach (var (name, key, type) in new[] { ("metadata", Contract.Metadata, "hash"), ("registry", Contract.Registry, "zset"),
            ("bucket", Contract.BucketPrefix + "login:email:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "zset") })
        {
            var capturedKey = key; var capturedType = type;
            Async(name + " presente exit10", async () =>
            {
                var factory = new LocalSessionFactory(); factory.Wire.Data.Keys[capturedKey] = new(capturedType);
                var (code, output) = await Run(factory);
                Assert.Equal(10, code);
                Assert.True(!output.Contains("CURRENT INVENTORY EMPTY", StringComparison.Ordinal));
                Assert.Equal(0, factory.Wire.Data.HashWrites);
            });
        }
        Async("SCAN incompleto exit20 sin reanudar/retry", async () =>
        {
            var factory = new LocalSessionFactory();
            factory.Wire.Data.ScanPages.Enqueue(("8", [])); factory.Wire.Data.ScanPages.Enqueue(("8", []));
            Assert.Equal(20, (await Run(factory)).Code);
            Assert.Equal(2, factory.Wire.Verbs.Count(v => v == "SCAN"));
            Assert.Equal(1, factory.Calls);
        });
        Async("duplicados conservadores exit20 y conteo deduplicado", async () =>
        {
            var factory = new LocalSessionFactory();
            var key = Contract.BucketPrefix + "login:email:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            factory.Wire.Data.Keys[key] = new("zset");
            factory.Wire.Data.ScanPages.Enqueue(("0", [key, key]));
            var (code, output) = await Run(factory);
            Assert.Equal(20, code);
            using var json = JsonDocument.Parse(output);
            Assert.Equal(1, json.RootElement.GetProperty("execution").GetProperty("uniqueKeys").GetInt32());
            Assert.Equal(1, json.RootElement.GetProperty("execution").GetProperty("duplicateKeys").GetInt32());
        });
        foreach (var (name, verb, response, expected) in new[] {
            ("AUTH failure", "AUTH", "-WRONGPASS " + CredentialFixture.Sentinel + "\r\n", 30),
            ("NOPERM", "SCAN", "-NOPERM " + CredentialFixture.Sentinel + "\r\n", 50),
            ("TIME malformed", "TIME", "*2\r\n$1\r\n0\r\n$7\r\n1000000\r\n", 40),
            ("MOVED no follow", "SCAN", "-MOVED 1 other:6379\r\n", 20),
            ("AUTH bulkOK no simple", "AUTH", "$2\r\nOK\r\n", 40),
            ("TYPE bulk no simple", "TYPE", "$4\r\nnone\r\n", 40) })
        {
            var v = verb; var r = response; var e = expected;
            Async(name + " exit" + e, async () =>
            {
                var factory = new LocalSessionFactory { Wire = { ResponseOverride = value => value == v ? Encoding.UTF8.GetBytes(r) : null } };
                var (code, output) = await Run(factory);
                Assert.Equal(e, code);
                Assert.True(!output.Contains(CredentialFixture.Sentinel, StringComparison.Ordinal));
                Assert.Equal(1, factory.Calls);
                Assert.True(factory.Session.Disposed);
            });
        }
        Async("TLS failure exit30 sin AUTH", async () =>
        {
            var factory = new LocalSessionFactory { OpenFailure = Failure.AuthTlsFailure };
            Assert.Equal(30, (await Run(factory)).Code);
            Assert.Equal(0, factory.Wire.Verbs.Count);
        });
        Async("sesion/topologia incoherente exit20", async () =>
        {
            var factory = new LocalSessionFactory();
            factory.Wire.BeforeResponse = v => { if (v == "SCAN") factory.Session.Coherent = false; };
            Assert.Equal(20, (await Run(factory)).Code);
            Assert.Equal(1, factory.Calls);
        });
        Async("comando prohibido aborta antes de enviar", async () =>
        {
            var factory = new LocalSessionFactory();
            var client = new RemoteTransport(factory.Session);
            using var input = CredentialFixture.Frame();
            using var credential = await PipeCredential.ReadAsync(input, CancellationToken.None);
            await client.AuthenticateAsync(credential, CancellationToken.None);
            await Assert.ThrowsAsync(Failure.PolicyFailure, async () => await client.ExecuteAsync(
                RedisCommand.Recover(Descriptor, "11111111111111111111111111111111"), CancellationToken.None));
            Assert.True(factory.Wire.Verbs.SequenceEqual(new[] { "AUTH" }));
            Assert.True(factory.Wire.Disposed);
            await factory.Session.DisposeAsync();
        });
        Async("apply remoto sigue bloqueado por guard independiente", async () =>
        {
            var factory = new LocalSessionFactory();
            var client = new RemoteTransport(factory.Session);
            await Assert.ThrowsAsync(Failure.ApplyDisabled, () => new ControlService(client).ApplyClosedAsync(Descriptor,
                "11111111111111111111111111111111", CancellationToken.None));
            Assert.Equal(0, factory.Wire.Verbs.Count);
            await factory.Session.DisposeAsync();
        });
        Sync("CLI solo pipehandle y rutaexplicita sin secretos", () =>
        {
            var parsed = CommandLine.Parse(["inventory", "--descriptor", "d", "--transport", "ssm-tunnel", "--tunnel-port", "16379", "--credential-pipe", "123"]);
            Assert.Equal(RemoteOptions.Endpoint, parsed.Remote!.TlsOptions().TargetHost);
            foreach (var args in new string[][] {
                ["inventory", "--descriptor", "d", "--transport", "direct"],
                ["inventory", "--descriptor", "d", "--transport", "direct", "--credential-pipe", "0"],
                ["readback", "--descriptor", "d", "--generation", "11111111111111111111111111111111", "--transport", "direct", "--credential-pipe", "123"] })
                Assert.Throws(Failure.PolicyFailure, () => CommandLine.Parse(args));
        });
        Async("cancelacion inicial no conecta y exit20", async () =>
        {
            var factory = new LocalSessionFactory();
            using var frame = CredentialFixture.Frame();
            using var output = new StringWriter();
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            Assert.Equal(20, await InventoryExecution.RunAsync(Options, Descriptor, frame, factory, output, cancellation.Token));
            Assert.Equal(0, factory.Calls);
        });
        Sync("mapa exhaustivo de exit codes", () =>
        {
            foreach (var (failure, code) in new[] { (Failure.ResidueFound, 10), (Failure.InventoryIncomplete, 20),
                (Failure.AuthTlsFailure, 30), (Failure.UnexpectedResponse, 40), (Failure.PolicyFailure, 50),
                (Failure.CredentialFailure, 60), (Failure.LocalOperationFailed, 70) }) Assert.Equal(code, InventoryExit.For(failure));
        });
        Async("presencia tardia de metadata no se oculta y exit10", async () =>
        {
            var factory = new LocalSessionFactory();
            factory.Wire.BeforeResponse = verb => { if (verb == "SCAN") factory.Wire.Data.Keys[Contract.Metadata] = new("hash"); };
            var (code, output) = await Run(factory);
            Assert.Equal(10, code);
            using var json = JsonDocument.Parse(output);
            Assert.True(json.RootElement.GetProperty("inventory").GetProperty("metadataPresent").GetBoolean());
        });
        Async("presencia tardia de registry no se oculta y exit10", async () =>
        {
            var factory = new LocalSessionFactory();
            factory.Wire.BeforeResponse = verb => { if (verb == "SCAN") factory.Wire.Data.Keys[Contract.Registry] = new("zset"); };
            Assert.Equal(10, (await Run(factory)).Code);
        });
        Async("pipe bloqueado respeta cancelacion sin conectar", async () =>
        {
            var factory = new LocalSessionFactory();
            using var frame = new BlockingInput();
            using var output = new StringWriter();
            using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));
            Assert.Equal(20, await InventoryExecution.RunAsync(Options, Descriptor, frame, factory, output, cancel.Token));
            Assert.Equal(0, factory.Calls);
        });
        Async("AUTH unico, fallo invalida sesion y no reautentica", async () =>
        {
            var factory = new LocalSessionFactory();
            var client = new RemoteTransport(factory.Session);
            using var frame = CredentialFixture.Frame();
            using var credential = await PipeCredential.ReadAsync(frame, CancellationToken.None);
            await client.AuthenticateAsync(credential, CancellationToken.None);
            var identity = client.SessionIdentity;
            await Assert.ThrowsAsync(Failure.PolicyFailure, () => client.AuthenticateAsync(credential, CancellationToken.None));
            Assert.True(!ReferenceEquals(identity, client.SessionIdentity));
            Assert.Equal(1, factory.Wire.Verbs.Count);
            Assert.True(factory.Wire.Disposed);
            await factory.Session.DisposeAsync();
        });
        Async("sin AUTH no se envia comando", async () =>
        {
            var factory = new LocalSessionFactory();
            var client = new RemoteTransport(factory.Session);
            await Assert.ThrowsAsync(Failure.AuthTlsFailure, async () => await client.ExecuteAsync(RedisCommand.Time(), CancellationToken.None));
            Assert.Equal(0, factory.Wire.Verbs.Count);
            await factory.Session.DisposeAsync();
        });
        Async("allowlist de otro subcomando no permite ni AUTH", async () =>
        {
            var factory = new LocalSessionFactory();
            var client = new RemoteTransport(factory.Session, "apply-closed");
            using var frame = CredentialFixture.Frame();
            using var credential = await PipeCredential.ReadAsync(frame, CancellationToken.None);
            await Assert.ThrowsAsync(Failure.PolicyFailure, () => client.AuthenticateAsync(credential, CancellationToken.None));
            Assert.Equal(0, factory.Wire.Verbs.Count);
            await factory.Session.DisposeAsync();
        });
        Async("RESP error con UTF8 invalido rechazado", async () =>
        {
            using var bytes = new MemoryStream(new byte[] { 45, 69, 82, 82, 32, 255, 13, 10 });
            await Assert.ThrowsAsync(Failure.ProtocolFailure, () => RespCodec.ReadAsync(bytes, CancellationToken.None));
        });
        Async("metadata extra opaca nunca aparece en salida", async () =>
        {
            var factory = new LocalSessionFactory();
            var key = new FakeKey("hash"); key.Fields["opaque"] = CredentialFixture.Sentinel;
            factory.Wire.Data.Keys[Contract.Metadata] = key;
            var (code, output) = await Run(factory);
            Assert.Equal(10, code);
            Assert.True(!output.Contains(CredentialFixture.Sentinel, StringComparison.Ordinal));
        });
        foreach (var blockedVerb in new[] { "AUTH", "SCAN" })
        {
            var verb = blockedVerb;
            Async("cancelacion durante " + verb + " cierra sesion sin retry", async () =>
            {
                var factory = new LocalSessionFactory();
                using var cancel = new CancellationTokenSource();
                factory.Wire.BeforeResponse = value =>
                {
                    if (value == verb) { factory.Wire.BlockReads = true; cancel.CancelAfter(30); }
                };
                using var frame = CredentialFixture.Frame();
                using var output = new StringWriter();
                Assert.Equal(20, await InventoryExecution.RunAsync(Options, Descriptor, frame, factory, output, cancel.Token));
                Assert.True(factory.Session.Disposed && factory.Wire.Disposed);
                Assert.Equal(1, factory.Calls);
                Assert.Equal(1, factory.Wire.Verbs.Count(v => v == verb));
                Assert.True(RemoteOptions.StepTimeout > TimeSpan.Zero && RemoteOptions.StepTimeout < RemoteOptions.OperationTimeout);
            });
        }
        Async("RESP presupuesto total 8MiB no se excede", async () =>
        {
            var bulk = "$4096\r\n" + new string('x', 4096) + "\r\n";
            var wire = "*2100\r\n" + string.Concat(Enumerable.Repeat(bulk, 2100));
            await Assert.ThrowsAsync(Failure.ProtocolFailure, () => Parse(wire));
        });
    }

    private static async Task<RedisReply> Parse(string wire)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(wire));
        return await RespCodec.ReadAsync(input, CancellationToken.None);
    }
    private static async Task<(int Code, string Output)> Run(LocalSessionFactory factory)
    {
        using var frame = CredentialFixture.Frame();
        using var output = new StringWriter();
        var code = await InventoryExecution.RunAsync(Options, Descriptor, frame, factory, output);
        return (code, output.ToString());
    }
}

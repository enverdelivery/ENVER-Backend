using System.Buffers.Binary;
using System.Text;
using Enver.QuotaControl;

namespace Enver.QuotaControl.Tests;

internal sealed class LocalSession(ScriptedChannel stream) : IRemoteSession
{
    public Stream Channel => stream;
    public object Identity { get; } = new();
    public bool Coherent { get; set; } = true;
    public bool IsCoherent => Coherent && !Disposed;
    public bool Disposed { get; private set; }
    public ConnectionEvidence Evidence { get; } = new(RemoteOptions.Endpoint, "127.0.0.1:16379", "Tls13", "TLS_AES_128_GCM_SHA256");
    public ValueTask DisposeAsync() { Disposed = true; stream.Dispose(); return ValueTask.CompletedTask; }
}

internal sealed class LocalSessionFactory : IRemoteSessionFactory
{
    public ScriptedChannel Wire { get; } = new();
    public LocalSession Session { get; }
    public int Calls { get; private set; }
    public Failure? OpenFailure { get; set; }
    public LocalSessionFactory() => Session = new(Wire);
    public Task<IRemoteSession> OpenAsync(RemoteOptions options, CancellationToken token)
    {
        Calls++;
        token.ThrowIfCancellationRequested();
        if (OpenFailure is not null) throw new ControlException(OpenFailure.Value);
        return Task.FromResult<IRemoteSession>(Session);
    }
}

// Doble duplex puro: cada request recibe bytes RESP prefabricados/modelados, sin sockets ni TLS handshake.
internal sealed class ScriptedChannel : Stream
{
    private byte[] pending = [];
    private int position;
    public FakeRedis Data { get; } = new();
    public Func<string, byte[]?>? ResponseOverride { get; set; }
    public Action<string>? BeforeResponse { get; set; }
    public List<string> Verbs { get; } = [];
    public bool Disposed { get; private set; }
    public int MaximumReadChunk { get; set; } = 3;
    public bool BlockReads { get; set; }
    public override bool CanRead => !Disposed;
    public override bool CanWrite => !Disposed;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (Disposed || position != pending.Length) throw new IOException();
        if (buffer.Span.StartsWith("*3\r\n$4\r\nAUTH\r\n"u8))
        {
            Verbs.Add("AUTH");
            BeforeResponse?.Invoke("AUTH");
            pending = ResponseOverride?.Invoke("AUTH") ?? "+OK\r\n"u8.ToArray();
        }
        else
        {
            using var request = new MemoryStream(buffer.ToArray());
            var parts = (await RespCodec.ReadAsync(request, token)).Items.Select(p => p.Text).ToArray();
            var verb = parts[0];
            Verbs.Add(verb);
            BeforeResponse?.Invoke(verb);
            var custom = ResponseOverride?.Invoke(verb);
            if (custom is not null) pending = custom;
            else
            {
                var command = verb switch
                {
                    "TIME" => RedisCommand.Time(),
                    "SCAN" => RedisCommand.Scan(parts[1]),
                    "ZRANGE" => RedisCommand.Range(parts[1], int.Parse(parts[2]), int.Parse(parts[3])),
                    _ => RedisCommand.ReadKey(Enum.Parse<RedisVerb>(verb), parts[1])
                };
                var reply = await Data.ExecuteAsync(command, token);
                pending = Encode(verb == "TYPE" ? RedisReply.Simple(reply.Text) : reply);
            }
        }
        position = 0;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        if (BlockReads) await Task.Delay(Timeout.Infinite, token);
        token.ThrowIfCancellationRequested();
        if (Disposed) throw new IOException();
        var length = Math.Min(Math.Min(buffer.Length, MaximumReadChunk), pending.Length - position);
        pending.AsMemory(position, length).CopyTo(buffer);
        position += length;
        return length;
    }

    private static byte[] Encode(RedisReply reply)
    {
        var text = reply.Kind switch
        {
            ReplyKind.SimpleString => "+" + reply.Text + "\r\n",
            ReplyKind.Text => "$" + Encoding.UTF8.GetByteCount(reply.Text) + "\r\n" + reply.Text + "\r\n",
            ReplyKind.Integer => ":" + Contract.Decimal(reply.Number) + "\r\n",
            ReplyKind.Null => "$-1\r\n",
            _ => ""
        };
        if (reply.Kind != ReplyKind.Array) return Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        output.Write(Encoding.ASCII.GetBytes("*" + reply.Items.Count + "\r\n"));
        foreach (var item in reply.Items) output.Write(Encode(item));
        return output.ToArray();
    }

    protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    public override void Flush() { }
    public override Task FlushAsync(CancellationToken token) { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}

internal static class CredentialFixture
{
    // Solo material determinista ficticio. No es password operativa ni generacion de credenciales.
    public const string Sentinel = "LOCAL_TEST_ONLY_SENTINEL_12345678";
    public static MemoryStream Frame(string value = Sentinel, bool extra = false)
    {
        var data = Encoding.ASCII.GetBytes(value);
        var frame = new byte[4 + data.Length + (extra ? 1 : 0)];
        BinaryPrimitives.WriteUInt32BigEndian(frame, (uint)data.Length);
        data.CopyTo(frame, 4);
        return new MemoryStream(frame);
    }
}

internal sealed class BlockingInput : Stream
{
    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
    {
        await Task.Delay(Timeout.Infinite, token);
        return 0;
    }
    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}

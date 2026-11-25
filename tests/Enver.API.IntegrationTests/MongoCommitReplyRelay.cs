using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Enver.API.IntegrationTests;

/// <summary>Test-only relay for the uncompressed, plaintext Testcontainers MongoDB wire connection.</summary>
internal sealed class MongoCommitReplyRelay : IAsyncDisposable
{
    private const int HeaderLength = 16;
    private const int OpMsg = 2013;
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource stop = new();
    private readonly ConcurrentBag<Task> connections = [];
    private readonly ConcurrentDictionary<int, bool> discard = new();
    private readonly Task acceptLoop;
    private int acknowledgedCommitsDiscarded, otherCommitRepliesDiscarded, auditRepliesDiscarded;

    internal MongoCommitReplyRelay(string host, int port)
    {
        acceptLoop = AcceptAsync(host, port);
    }

    internal int Port => ((IPEndPoint)listener.LocalEndpoint).Port;
    internal int AcknowledgedCommitsDiscarded => Volatile.Read(ref acknowledgedCommitsDiscarded);
    internal int OtherCommitRepliesDiscarded => Volatile.Read(ref otherCommitRepliesDiscarded);
    internal int AuditRepliesDiscarded => Volatile.Read(ref auditRepliesDiscarded);

    // requestId comes from CommandStarted, before the driver sends the command.
    internal void DiscardReply(int requestId, bool commit) => discard[requestId] = commit;

    private async Task AcceptAsync(string host, int port)
    {
        listener.Start();
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var inbound = await listener.AcceptTcpClientAsync(stop.Token);
                connections.Add(RelayAsync(inbound, host, port));
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
        catch (SocketException) when (stop.IsCancellationRequested) { }
    }

    private async Task RelayAsync(TcpClient inbound, string host, int port)
    {
        using var client = inbound;
        using var server = new TcpClient();
        using var connection = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
        try
        {
            await server.ConnectAsync(host, port, connection.Token);
            client.NoDelay = server.NoDelay = true;
            var requests = client.GetStream().CopyToAsync(server.GetStream(), connection.Token);
            var replies = ForwardRepliesAsync(server.GetStream(), client.GetStream(), connection.Token);
            await Task.WhenAny(requests, replies);
            await connection.CancelAsync();
            await Task.WhenAll(requests, replies);
        }
        catch (OperationCanceledException) when (connection.IsCancellationRequested) { }
        catch (IOException) { /* A peer closed a relayed connection. */ }
    }

    private async Task ForwardRepliesAsync(Stream server, Stream client, CancellationToken token)
    {
        var header = new byte[HeaderLength];
        while (await server.ReadAtLeastAsync(header, HeaderLength, throwOnEndOfStream: false, cancellationToken: token) == HeaderLength)
        {
            var length = BinaryPrimitives.ReadInt32LittleEndian(header);
            if (length is < HeaderLength or > 48_000_000)
                throw new InvalidDataException("Unexpected MongoDB frame length in local test.");
            var body = new byte[length - HeaderLength];
            await server.ReadExactlyAsync(body, token);
            var responseTo = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(8));
            if (discard.TryRemove(responseTo, out var commit))
            {
                if (!commit) Interlocked.Increment(ref auditRepliesDiscarded);
                else if (IsAcknowledgedCommit(header, body)) Interlocked.Increment(ref acknowledgedCommitsDiscarded);
                else Interlocked.Increment(ref otherCommitRepliesDiscarded);
                return; // Close the connection without forwarding any bytes of this reply to the driver.
            }
            await client.WriteAsync(header, token);
            await client.WriteAsync(body, token);
        }
    }

    private static bool IsAcknowledgedCommit(ReadOnlySpan<byte> header, byte[] body)
    {
        // OP_MSG: flags (4 bytes), followed by the kind-0 BSON response (1 type byte).
        if (BinaryPrimitives.ReadInt32LittleEndian(header[12..]) != OpMsg || body.Length < 10 || body[4] != 0)
            return false;
        using var bson = new MemoryStream(body, 5, body.Length - 5, writable: false);
        var reply = BsonSerializer.Deserialize<BsonDocument>(bson);
        return reply.GetValue("ok", 0).ToDouble() == 1 && !reply.Contains("writeConcernError") && !reply.Contains("writeErrors");
    }

    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync();
        listener.Stop();
        await acceptLoop;
        await Task.WhenAll(connections);
        stop.Dispose();
    }
}

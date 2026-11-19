using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Enver.QuotaControl;

public static class RespCodec
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    public const int MaximumReplyBytes = 8 * 1024 * 1024;
    public const int MaximumBulkBytes = 4096;

    public static byte[] Encode(RedisCommand command)
    {
        RemotePolicy.Validate("inventory", command);
        return EncodeParts(new[] { command.Verb.ToString() }.Concat(command.Arguments).Select(Utf8.GetBytes).ToArray());
    }

    internal static byte[] EncodeAuth(PipeCredential credential)
    {
        RemotePolicy.EnsureAllowed("inventory", "AUTH");
        var secret = credential.Bytes.ToArray();
        try { return EncodeParts(["AUTH"u8.ToArray(), Utf8.GetBytes(RemoteOptions.ControlUser), secret]); }
        finally { CryptographicOperations.ZeroMemory(secret); }
    }

    private static byte[] EncodeParts(byte[][] parts)
    {
        using var output = new MemoryStream();
        void Header(string value) => output.Write(Encoding.ASCII.GetBytes(value));
        try
        {
            Header("*" + parts.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
            foreach (var part in parts)
            {
                Header("$" + part.Length.ToString(CultureInfo.InvariantCulture) + "\r\n");
                output.Write(part);
                output.Write("\r\n"u8);
            }
            return output.ToArray();
        }
        finally
        {
            if (output.TryGetBuffer(out var buffer)) CryptographicOperations.ZeroMemory(buffer.AsSpan());
        }
    }

    public static async Task<RedisReply> ReadAsync(Stream stream, CancellationToken token)
    {
        try { return await new Reader(stream).Read(0, token).ConfigureAwait(false); }
        catch (OperationCanceledException) { throw; }
        catch (IOException) { throw new ControlException(Failure.InventoryIncomplete); }
        catch { throw new ControlException(Failure.ProtocolFailure); }
    }

    private sealed class Reader(Stream stream)
    {
        private int remaining = MaximumReplyBytes;
        private int nodes = 22000;
        private readonly byte[] single = new byte[1];

        private async ValueTask<byte> Byte(CancellationToken token)
        {
            if (--remaining < 0) Bad();
            if (await stream.ReadAsync(single, token).ConfigureAwait(false) != 1) Bad();
            return single[0];
        }

        private async Task<byte[]> Line(CancellationToken token)
        {
            var line = new byte[1024];
            var length = 0;
            try
            {
                while (true)
                {
                    var b = await Byte(token).ConfigureAwait(false);
                    if (b == 13)
                    {
                        if (await Byte(token).ConfigureAwait(false) != 10) Bad();
                        return line[..length];
                    }
                    if (b == 10 || length == line.Length) Bad();
                    line[length++] = b;
                }
            }
            finally { CryptographicOperations.ZeroMemory(line); }
        }

        public async Task<RedisReply> Read(int depth, CancellationToken token)
        {
            if (--nodes < 0) Bad();
            var marker = await Byte(token).ConfigureAwait(false);
            if (marker is not ((byte)'+' or (byte)'-' or (byte)':' or (byte)'$' or (byte)'*')) Bad();
            var line = await Line(token).ConfigureAwait(false);
            try
            {
                if (marker == '+') return RedisReply.Simple(Utf8.GetString(line));
                if (marker == '-')
                {
                    // Solo conservar clasificacion cerrada; nunca payload/mensajes del servidor.
                    _ = Utf8.GetCharCount(line);
                    var end = Array.IndexOf(line, (byte)' ');
                    var length = end < 0 ? line.Length : end;
                    if (length == 0 || line.Take(length).Any(b => !(b is >= (byte)'A' and <= (byte)'Z'))) Bad();
                    var code = line.AsSpan(0, length);
                    return RedisReply.Error(code.SequenceEqual("WRONGPASS"u8) || code.SequenceEqual("NOAUTH"u8) ? ReplyError.Authentication :
                        code.SequenceEqual("NOPERM"u8) ? ReplyError.NoPermission :
                        code.SequenceEqual("MOVED"u8) || code.SequenceEqual("ASK"u8) || code.SequenceEqual("READONLY"u8) ||
                        code.SequenceEqual("CLUSTERDOWN"u8) || code.SequenceEqual("MASTERDOWN"u8) || code.SequenceEqual("TRYAGAIN"u8) ||
                        code.SequenceEqual("LOADING"u8) ? ReplyError.Topology : ReplyError.Unknown);
                }
                var text = Utf8.GetString(line);
                if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number) ||
                    number.ToString(CultureInfo.InvariantCulture) != text) Bad();
                if (marker == ':') return RedisReply.Integer(number);
                if (number == -1) return RedisReply.Null();
                if (number < 0) Bad();
                if (marker == '$')
                {
                    if (number > MaximumBulkBytes || number + 2 > remaining) Bad();
                    var bytes = new byte[(int)number];
                    try
                    {
                        remaining -= bytes.Length;
                        try { await stream.ReadExactlyAsync(bytes, token).ConfigureAwait(false); }
                        catch (EndOfStreamException) { Bad(); }
                        if (await Byte(token).ConfigureAwait(false) != 13 || await Byte(token).ConfigureAwait(false) != 10) Bad();
                        return RedisReply.String(Utf8.GetString(bytes));
                    }
                    finally { CryptographicOperations.ZeroMemory(bytes); }
                }
                if (depth >= 2 || number > 20000 || number > nodes) Bad();
                var values = new RedisReply[(int)number];
                for (var i = 0; i < values.Length; i++) values[i] = await Read(depth + 1, token).ConfigureAwait(false);
                return RedisReply.Array(values);
            }
            finally { CryptographicOperations.ZeroMemory(line); }
        }
    }

    private static void Bad() => throw new ControlException(Failure.ProtocolFailure);
}

using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Enver.QuotaControl;

// Nunca convertir el secreto a string, record o excepcion. Propiedad del llamador hasta Dispose.
public sealed class PipeCredential : IDisposable
{
    private byte[]? bytes;
    private PipeCredential(byte[] value) => bytes = value;
    internal ReadOnlyMemory<byte> Bytes => bytes ?? throw new ControlException(Failure.CredentialFailure);
    public override string ToString() => "[REDACTED]";
    public void Dispose()
    {
        if (bytes is not null) CryptographicOperations.ZeroMemory(bytes);
        bytes = null;
    }

    // Frame: uint32 big-endian (16..128), N bytes ASCII imprimibles compatibles RBAC, EOF.
    // El CLI solo lo llama con AnonymousPipeClientStream, nunca archivo/stdin arbitrario.
    public static async Task<PipeCredential> ReadAsync(Stream pipe, CancellationToken token)
    {
        var prefix = new byte[4];
        byte[]? secret = null;
        var extra = new byte[1];
        try
        {
            await pipe.ReadExactlyAsync(prefix, token).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadUInt32BigEndian(prefix);
            if (length is < 16 or > 128) throw new ControlException(Failure.CredentialFailure);
            secret = new byte[(int)length];
            await pipe.ReadExactlyAsync(secret, token).ConfigureAwait(false);
            if (secret.Any(b => b < 33 || b > 126 || b is (byte)',' or (byte)'"' or (byte)'/' or (byte)'@') ||
                await pipe.ReadAsync(extra, token).ConfigureAwait(false) != 0) throw new ControlException(Failure.CredentialFailure);
            token.ThrowIfCancellationRequested();
            var credential = new PipeCredential(secret);
            secret = null;
            return credential;
        }
        catch (OperationCanceledException) { throw; }
        catch { throw new ControlException(Failure.CredentialFailure); }
        finally
        {
            CryptographicOperations.ZeroMemory(prefix);
            CryptographicOperations.ZeroMemory(extra);
            if (secret is not null) CryptographicOperations.ZeroMemory(secret);
        }
    }
}

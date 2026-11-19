using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Enver.QuotaControl;

public enum ConnectionRoute { Direct, SsmTunnel }

public sealed class RemoteOptions
{
    public const string Endpoint = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com";
    public const string ControlUser = "enver-dev-quota-control";
    public ConnectionRoute Route { get; }
    public string ConnectHost => Route == ConnectionRoute.Direct ? Endpoint : "127.0.0.1";
    public int ConnectPort { get; }
    public int Database => 0;
    public static TimeSpan StepTimeout => TimeSpan.FromSeconds(5);
    public static TimeSpan OperationTimeout => TimeSpan.FromSeconds(30);

    public RemoteOptions(ConnectionRoute route, int? tunnelPort = null)
    {
        if (!Enum.IsDefined(route) || (route == ConnectionRoute.Direct && tunnelPort is not null) ||
            (route == ConnectionRoute.SsmTunnel && tunnelPort is not (>= 1024 and <= 65535)))
            throw new ControlException(Failure.PolicyFailure);
        Route = route;
        ConnectPort = route == ConnectionRoute.Direct ? 6379 : tunnelPort!.Value;
    }

    public SslClientAuthenticationOptions TlsOptions() => new()
    {
        TargetHost = Endpoint, // TCP localhost no cambia ni SNI ni validacion del hostname.
        EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
        AllowTlsResume = false,
        CertificateRevocationCheckMode = X509RevocationMode.Online,
        RemoteCertificateValidationCallback = ValidateCertificate
    };

    public static bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors) =>
        errors == SslPolicyErrors.None && certificate is not null && chain is not null &&
        chain.ChainElements.Count >= 2 && certificate.Subject != certificate.Issuer &&
        chain.ChainStatus.All(s => s.Status == X509ChainStatusFlags.NoError);

    public override string ToString() => nameof(RemoteOptions);
}

public sealed record ConnectionEvidence(string LogicalEndpoint, string ResolvedTarget, string TlsProtocol, string Cipher,
    string TopologyScope = "SINGLE_PINNED_TLS_CONNECTION_NOT_INDEPENDENT_PRIMARY_ATTESTATION");

public interface IRemoteSession : IAsyncDisposable
{
    Stream Channel { get; }
    object Identity { get; }
    bool IsCoherent { get; }
    ConnectionEvidence Evidence { get; }
}

public interface IRemoteSessionFactory
{
    Task<IRemoteSession> OpenAsync(RemoteOptions options, CancellationToken token);
}

// Unico lugar con sockets. Nunca usado por la suite de pruebas.
public sealed class TlsSessionFactory : IRemoteSessionFactory
{
    public async Task<IRemoteSession> OpenAsync(RemoteOptions options, CancellationToken token)
    {
        Socket? socket = null;
        SslStream? tls = null;
        var tlsStarted = false;
        using var step = CancellationTokenSource.CreateLinkedTokenSource(token);
        step.CancelAfter(RemoteOptions.StepTimeout);
        try
        {
            IPAddress address;
            if (options.Route == ConnectionRoute.SsmTunnel) address = IPAddress.Loopback;
            else
            {
                var addresses = await Dns.GetHostAddressesAsync(RemoteOptions.Endpoint, step.Token).ConfigureAwait(false);
                address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                    ?? throw new ControlException(Failure.InventoryIncomplete);
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            // Una direccion/un intento; sin retry DNS/IP, sin pooling o multiplexacion.
            await socket.ConnectAsync(new IPEndPoint(address, options.ConnectPort), step.Token).ConfigureAwait(false);
            tls = new SslStream(new NetworkStream(socket, ownsSocket: false), leaveInnerStreamOpen: false);
            tlsStarted = true;
            await tls.AuthenticateAsClientAsync(options.TlsOptions(), step.Token).ConfigureAwait(false);
            if (!tls.IsAuthenticated || !tls.IsEncrypted || tls.SslProtocol is not (SslProtocols.Tls12 or SslProtocols.Tls13))
                throw new ControlException(Failure.AuthTlsFailure);
            step.Token.ThrowIfCancellationRequested();
            var session = new TlsSession(socket, tls, new(RemoteOptions.Endpoint,
                ((IPEndPoint)socket.RemoteEndPoint!).ToString(), tls.SslProtocol.ToString(), tls.NegotiatedCipherSuite.ToString()));
            socket = null;
            tls = null;
            return session;
        }
        catch (AuthenticationException) { throw new ControlException(Failure.AuthTlsFailure); }
        catch (ControlException) { throw; }
        catch (OperationCanceledException) { throw new ControlException(tlsStarted && !token.IsCancellationRequested ? Failure.AuthTlsFailure : Failure.InventoryIncomplete); }
        catch { throw new ControlException(tlsStarted ? Failure.AuthTlsFailure : Failure.InventoryIncomplete); }
        finally
        {
            tls?.Dispose();
            socket?.Dispose();
        }
    }

    private sealed class TlsSession(Socket socket, SslStream tls, ConnectionEvidence evidence) : IRemoteSession
    {
        private bool disposed;
        public Stream Channel => tls;
        public object Identity { get; } = new();
        public ConnectionEvidence Evidence => evidence;
        public bool IsCoherent
        {
            get
            {
                try { return !disposed && socket.Connected &&
                    socket.RemoteEndPoint?.ToString() == evidence.ResolvedTarget &&
                    !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0); }
                catch { return false; }
            }
        }
        public ValueTask DisposeAsync()
        {
            disposed = true;
            try { tls.Dispose(); }
            finally { socket.Dispose(); }
            return ValueTask.CompletedTask;
        }
    }
}

public sealed class RemoteTransport(IRemoteSession session, string operation = "inventory") : IRedisCommandClient
{
    private bool authenticated;
    private bool faulted;
    private int busy;
    private readonly object failedIdentity = new();
    public object SessionIdentity => faulted || !session.IsCoherent ? failedIdentity : session.Identity;
    public bool IsLocalSimulation => false;
    public bool IsCoherent => authenticated && !faulted && session.IsCoherent;

    public async Task AuthenticateAsync(PipeCredential credential, CancellationToken token)
    {
        try
        {
            RemotePolicy.EnsureAllowed(operation, "AUTH");
            if (authenticated) throw new ControlException(Failure.PolicyFailure);
            var response = await Exchange(RespCodec.EncodeAuth(credential), token).ConfigureAwait(false);
            RemotePolicy.CheckError(response, authenticating: true);
            if (response.Kind != ReplyKind.SimpleString || response.Text != "OK") throw new ControlException(Failure.ProtocolFailure);
            authenticated = true;
        }
        catch { FailClosed(); throw; }
    }

    public async ValueTask<RedisReply> ExecuteAsync(RedisCommand command, CancellationToken cancellationToken)
    {
        try
        {
            RemotePolicy.Validate(operation, command); // Antes de construir bytes/enviar.
            if (!authenticated) throw new ControlException(Failure.AuthTlsFailure);
            var response = await Exchange(RespCodec.Encode(command), cancellationToken).ConfigureAwait(false);
            RemotePolicy.ValidateReply(command, response);
            return response;
        }
        catch { FailClosed(); throw; }
    }

    private async Task<RedisReply> Exchange(byte[] frame, CancellationToken token)
    {
        var entered = false;
        using var step = CancellationTokenSource.CreateLinkedTokenSource(token);
        step.CancelAfter(RemoteOptions.StepTimeout);
        try
        {
            if (faulted || !session.IsCoherent) throw new ControlException(Failure.InventoryIncomplete);
            if (Interlocked.CompareExchange(ref busy, 1, 0) != 0) throw new ControlException(Failure.PolicyFailure);
            entered = true;
            token.ThrowIfCancellationRequested();
            await session.Channel.WriteAsync(frame, step.Token).AsTask().WaitAsync(step.Token).ConfigureAwait(false);
            await session.Channel.FlushAsync(step.Token).WaitAsync(step.Token).ConfigureAwait(false);
            var response = await RespCodec.ReadAsync(session.Channel, step.Token).WaitAsync(step.Token).ConfigureAwait(false);
            if (!session.IsCoherent) throw new ControlException(Failure.InventoryIncomplete);
            return response;
        }
        catch (ControlException) { faulted = true; throw; }
        catch { faulted = true; throw new ControlException(Failure.InventoryIncomplete); }
        finally
        {
            CryptographicOperations.ZeroMemory(frame); // Incluye siempre el frame AUTH.
            if (entered) Interlocked.Exchange(ref busy, 0);
            if (faulted) FailClosed(); // Desbloquea I/O pendiente; jamas reusar/reconectar.
        }
    }

    private void FailClosed()
    {
        faulted = true;
        try { session.Channel.Dispose(); }
        catch { /* No conservar mensajes externos, ni ocultar el codigo de fallo original. */ }
    }
}

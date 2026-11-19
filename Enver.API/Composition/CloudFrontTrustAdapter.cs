using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;

namespace Enver.API.Composition;

/// <summary>
/// Confianza por secreto de distribución, sin fijar IPs de nodos ALB. El futuro origen CloudFront debe
/// SOBRESCRIBIR X-Enver-Origin-Verify y generar los dos headers CloudFront por policy.
/// La frontera de red debe mantener el backend accesible solo desde ALB. Un secreto
/// comprometido permite replay/suplantación; no es una prueba independiente de identidad AWS.
/// No proporciona cifrado del tramo HTTP al ALB ni acredita el despliegue de esa frontera.
/// </summary>
internal static class CloudFrontTrustAdapter
{
    private const string OriginHeader = "X-Enver-Origin-Verify";
    private const string ProtoHeader = "CloudFront-Forwarded-Proto";
    private const string AddressHeader = "CloudFront-Viewer-Address";

    internal static IApplicationBuilder UseCloudFrontTrust(this IApplicationBuilder app, AuthenticationTransportOptions policy)
    {
        // También protege consumidores que compongan el middleware sin ValidateOnStart.
        if (policy.AllowLocalHttp || policy.DirectConnectionApproved ||
            !CloudFrontTransportOptions.IsValidSecret(policy.CloudFront.OriginSecret))
            throw new InvalidOperationException("AuthenticationTransport: política incompatible.");

        var expectedDigest = SHA256.HashData(Encoding.ASCII.GetBytes(policy.CloudFront.OriginSecret));

        app.Use(async (context, next) =>
        {
            var originHeader = context.Request.Headers[OriginHeader];
            // No propagar el secreto a endpoints/observadores posteriores, incluso en /health.
            context.Request.Headers.Remove(OriginHeader);
            IPAddress? viewer = null;
            var viewerPort = 0;
            var verified = originHeader.Count == 1 && CloudFrontTransportOptions.IsValidSecret(originHeader[0]) &&
                CryptographicOperations.FixedTimeEquals(expectedDigest,
                    SHA256.HashData(Encoding.ASCII.GetBytes(originHeader[0]!))) &&
                Single(context.Request.Headers, ProtoHeader, out var proto) && proto == "https" &&
                Single(context.Request.Headers, AddressHeader, out var address) &&
                TryViewerAddress(address, out viewer, out viewerPort);

            context.Features.Set(AuthenticationOriginTrustContext.RequireVerifiedForwarding(
                verified ? AuthenticationCanonicalOrigin.FromAddress(viewer!) : null));

            if (verified)
            {
                context.Request.Scheme = "https";
                context.Connection.RemoteIpAddress = Normalize(viewer!);
                context.Connection.RemotePort = viewerPort;
                // Host no se altera; X-Forwarded-Host/Proto/For arbitrarios nunca acreditan confianza.
            }
            else if (!AuthenticationTransport.IsExactHealthRequest(context))
            {
                // Sin fallback a conexión directa/headers legacy ni redirect al hostname del ALB.
                await ApiProblemDetails.TryWriteAsync(context, StatusCodes.Status503ServiceUnavailable);
                return;
            }
            await next(context);
        });
        return app;
    }

    private static IPAddress Normalize(IPAddress address) => address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

    private static bool Single(IHeaderDictionary headers, string name, out string value)
    {
        var values = headers[name];
        value = values.Count == 1 ? values[0] ?? "" : "";
        return value.Length > 0;
    }

    private static bool TryViewerAddress(string value, out IPAddress? address, out int port)
    {
        address = null; port = 0;
        if (value.Length > 128 || value.Any(c => char.IsWhiteSpace(c) || c is ',' or '%')) return false;
        string ip, portText;
        if (value.StartsWith('['))
        {
            var end = value.IndexOf(']');
            if (end <= 1 || end + 1 >= value.Length || value[end + 1] != ':') return false;
            ip = value[1..end]; portText = value[(end + 2)..];
            if (!IPAddress.TryParse(ip, out address) || address.AddressFamily != AddressFamily.InterNetworkV6) return false;
        }
        else
        {
            var separator = value.IndexOf(':');
            if (separator <= 0 || separator != value.LastIndexOf(':')) return false;
            ip = value[..separator]; portText = value[(separator + 1)..];
            // IPAddress admite representaciones IPv4 abreviadas/hex/octal: aquí solo decimal canónico.
            if (!IPAddress.TryParse(ip, out address) || address.AddressFamily != AddressFamily.InterNetwork ||
                address.ToString() != ip) return false;
        }
        return int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out port) && port is > 0 and <= 65535;
    }
}

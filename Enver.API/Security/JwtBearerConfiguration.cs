using Enver.API.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Enver.API.Security;

/// <summary>Configurador compartido por el host productivo y las pruebas del mismo esquema Bearer.</summary>
public sealed class JwtBearerConfiguration(JwtClaimsValidator validator) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options) => Configure(AuthenticationPolicies.Scheme, options);
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != AuthenticationPolicies.Scheme) return;
        options.MapInboundClaims = false; options.SaveToken = false; options.IncludeErrorDetails = false;
        options.UseSecurityTokenValidators = false; options.RefreshOnIssuerKeyNotFound = false;
        options.Authority = null; options.MetadataAddress = null!; options.ConfigurationManager = null;
        options.TokenValidationParameters = validator.CreateParameters();
        validator.Handler.MaximumTokenSizeInBytes = JwtClaimsValidator.MaximumCompactBytes;
        ((JsonWebTokenHandler)validator.Handler).MapInboundClaims = false;
        options.TokenHandlers.Clear(); options.TokenHandlers.Add(validator.Handler);
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // 058 / PLD-AUTH-19: estos endpoints son públicos explícitos. Un Bearer opcional
                // no se parsea ni verifica y no convierte el request en una consulta de revocación.
                var endpoint = context.HttpContext.GetEndpoint();
                if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null &&
                    endpoint.Metadata.GetMetadata<AuthenticationRequestMetadata>()?.Operation is
                        AuthenticationOperation.Register or AuthenticationOperation.Login)
                { context.NoResult(); return Task.CompletedTask; }
                var values = context.Request.Headers.Authorization;
                if (values.Count == 0) { context.NoResult(); return Task.CompletedTask; }
                if (values.Count != 1 || values[0] is not { } value || !value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                { context.Fail("No fue posible autenticar la solicitud."); return Task.CompletedTask; }
                var compact = value[7..];
                if (compact.Length > JwtClaimsValidator.MaximumCompactBytes || !validator.TryRead(compact, out _))
                    context.Fail("No fue posible autenticar la solicitud.");
                else context.Token = compact;
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context.SecurityToken is not JsonWebToken jwt || context.Principal is null
                    || !validator.TryRead(jwt.EncodedToken, out var p)) context.Fail("No fue posible autenticar la solicitud.");
                else context.HttpContext.Features.Set(new ValidatedJwtIdentity(context.Principal, p.Subject, p.Jti));
                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            { context.HandleResponse(); await ApiProblemDetails.TryWriteAsync(context.HttpContext, 401); }
        };
    }
}

internal sealed record ValidatedJwtIdentity(System.Security.Claims.ClaimsPrincipal Principal, string Subject, string Jti);

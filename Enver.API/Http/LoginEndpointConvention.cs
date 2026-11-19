using Enver.API.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Enver.API.Features.Authentication.Application.Ports;

namespace Enver.API.Http;

/// <summary>Evita duplicar el evento de aplicación y cubre rechazos de frontera antes del controller.</summary>
internal sealed class LoginAttemptEvents
{
    public bool HandedToApplication { get; set; }
    public static async Task ObserveAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<AuthenticationRequestMetadata>()?.Operation != AuthenticationOperation.Login)
        { await next(context); return; }
        var attempt = new LoginAttemptEvents(); context.Features.Set(attempt); var returned = false;
        try { await next(context); returned = true; }
        finally
        {
            if (!attempt.HandedToApplication)
                context.RequestServices.GetService<IAuthenticationEvents>()?.Record(AuthenticationEventKind.Login,
                    returned && context.Response.StatusCode is 400 or 401 or 413 or 415 ? AuthenticationEventOutcome.Rejected : AuthenticationEventOutcome.Unavailable);
            context.Features.Set<LoginAttemptEvents>(null);
        }
    }
}

internal sealed class LoginEndpointConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.Where(c => c.ControllerType.AsType() == typeof(AuthenticationController))
                     .SelectMany(c => c.Actions).Where(a => a.ActionName == nameof(AuthenticationController.Login)))
            foreach (var selector in action.Selectors)
                selector.EndpointMetadata.Add(new AuthenticationRequestMetadata(AuthenticationOperation.Login));
    }
}

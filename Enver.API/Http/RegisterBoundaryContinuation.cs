using Enver.API.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Enver.API.Http;

/// <summary>Los 400 de register pasan al caso de uso para su preflight D012; 413/415 siguen terminales.</summary>
public sealed class RegisterBoundaryContinuation : IAuthenticationBoundaryContinuation
{
    public async Task ContinueAsync(HttpContext context, AuthenticationBoundaryResult result, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerTypeInfo.AsType() == typeof(AuthenticationController))
            await next(context);
        else if (result.ErrorStatus is { } status) await ApiProblemDetails.TryWriteAsync(context, status, result.InvalidFields);
        else await next(context);
    }
}

internal sealed class RegisterEndpointConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.Where(c => c.ControllerType.AsType() == typeof(AuthenticationController))
                     .SelectMany(c => c.Actions).Where(a => a.ActionName == nameof(AuthenticationController.Register)))
            foreach (var selector in action.Selectors)
                selector.EndpointMetadata.Add(new AuthenticationRequestMetadata(AuthenticationOperation.Register));
    }
}

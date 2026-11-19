using Enver.API.Features.Authentication.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Enver.API.Http;

/// <summary>Preparado para MVC: corre antes del ModelStateInvalidFilter (-2000), no se registra en el host.</summary>
public sealed class AuthenticationValidationFilter(AuthenticationValidationCoordinator coordinator) : IAsyncActionFilter, IOrderedFilter
{
    public int Order => -3000;
    public static IServiceCollection AddPreparedAuthenticationValidation(IServiceCollection services)
    {
        services.AddScoped<AuthenticationValidationCoordinator>();
        services.AddScoped<IAuthenticationBoundaryContinuation>(p => p.GetRequiredService<AuthenticationValidationCoordinator>());
        services.AddScoped<AuthenticationValidationFilter>();
        services.Configure<MvcOptions>(o => o.Filters.AddService<AuthenticationValidationFilter>(-3000));
        services.Configure<JsonOptions>(o => AuthenticationRequestLimitsMiddleware.ConfigureSerializer(o.JsonSerializerOptions));
        return services;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<AuthenticationRequestMetadata>() is null) { await next(); return; }
        var boundary = context.HttpContext.Features.Get<AuthenticationBoundaryResult>();
        if (boundary is null) { context.Result = new SafeResult(new(503, [])); return; }
        var modelErrors = context.ModelState.Where(x => x.Value?.Errors.Count > 0).Select(x => ToField(x.Key));
        var decision = await coordinator.CoordinateAsync(context.HttpContext, boundary, modelErrors);
        if (decision.ClientCanceled) { context.HttpContext.Abort(); context.Result = new EmptyResult(); }
        else if (decision.HttpStatus is not null) context.Result = new SafeResult(decision);
        else await next();
    }
    private static AuthenticationValidationField ToField(string key) => key[(key.LastIndexOf('.') + 1)..].ToLowerInvariant() switch
    {
        "fullname" => AuthenticationValidationField.FullName, "email" => AuthenticationValidationField.Email, "password" => AuthenticationValidationField.Password, _ => AuthenticationValidationField.Body
    };
    private sealed class SafeResult(AuthenticationCoordinationResult decision) : IActionResult
    {
        public Task ExecuteResultAsync(ActionContext context) => ApiProblemDetails.TryWriteAsync(context.HttpContext, decision.HttpStatus!.Value, decision.InvalidFields.Select(AuthenticationValidationCoordinator.ToApi), decision.RetryAfter).AsTask();
    }
}

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Enver.API.Http;

public enum ApiErrorField { FullName, Email, Password, Body, Authorization }

public sealed class ApiProblemDetails : IProblemDetailsWriter
{
    private static readonly object WrittenKey = new();
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
    public static IServiceCollection AddSafeApiErrors(IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IProblemDetailsWriter, ApiProblemDetails>());
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        return services;
    }

    public bool CanWrite(ProblemDetailsContext context) => context.HttpContext.Response.StatusCode is 400 or 401 or 409 or 413 or 415 or 429 or 500 or 503;
    public static ProblemDetails Create(HttpContext context, int status, IEnumerable<ApiErrorField>? fields = null)
    {
        var (title, code, detail) = status switch
        {
            400 => ("Bad Request", "validation_failed", "La solicitud contiene datos inválidos."),
            401 => ("Unauthorized", "authentication_failed", "No fue posible autenticar la solicitud."),
            409 => ("Conflict", "registration_conflict", "No fue posible completar el registro."),
            413 => ("Content Too Large", "payload_too_large", "La solicitud excede el tamaño permitido."),
            415 => ("Unsupported Media Type", "unsupported_media_type", "El formato de la solicitud no está soportado."),
            429 => ("Too Many Requests", "rate_limited", "Se superó temporalmente el límite de solicitudes."),
            500 => ("Internal Server Error", "internal_error", "Ocurrió un error al procesar la solicitud."),
            503 => ("Service Unavailable", "temporarily_unavailable", "El servicio no está disponible temporalmente."),
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
        var problem = new ProblemDetails { Type = "about:blank", Title = title, Status = status, Detail = detail };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = RequestContextMiddleware.GetOrCreateTraceId(context);
        var errors = fields?.Distinct().ToDictionary(FieldName, _ => new[] { "El campo no es válido." });
        if (status == 400 && errors?.Count > 0) problem.Extensions["errors"] = errors;
        return problem;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var http = context.HttpContext;
        if (http.Response.HasStarted || http.RequestAborted.IsCancellationRequested || http.Items.ContainsKey(WrittenKey)) return;
        IEnumerable<ApiErrorField> fields = [];
        if (context.ProblemDetails.Extensions.TryGetValue("errors", out var supplied) && supplied is IDictionary<string, string[]> map)
            fields = Enum.GetValues<ApiErrorField>().Where(f => map.ContainsKey(FieldName(f)));
        http.Items[WrittenKey] = true;
        http.Response.Headers.CacheControl = "no-store";
        var problem = Create(http, http.Response.StatusCode, fields);
        ApplyAuthenticationChallenge(http, problem);
        await http.Response.WriteAsJsonAsync(problem, Json, "application/problem+json", http.RequestAborted);
    }

    public static async ValueTask<bool> TryWriteAsync(HttpContext context, int status, IEnumerable<ApiErrorField>? fields = null, TimeSpan? retryAfter = null)
    {
        if (context.Response.HasStarted || context.RequestAborted.IsCancellationRequested || context.Items.ContainsKey(WrittenKey)) return false;
        if (status == 204)
        {
            context.Items[WrittenKey] = true; context.Response.StatusCode = 204; context.Response.ContentType = null; context.Response.ContentLength = null; context.Response.Headers.CacheControl = "no-store"; return true;
        }
        context.Response.StatusCode = status;
        if (status == 429 && retryAfter is { } wait && wait > TimeSpan.Zero)
            context.Response.Headers.RetryAfter = Math.Ceiling(wait.TotalSeconds).ToString("0", CultureInfo.InvariantCulture);
        var problem = Create(context, status, fields);
        // Antes de delegar: MVC puede seleccionar su writer nativo en lugar del nuestro.
        ApplyAuthenticationChallenge(context, problem);
        return await context.RequestServices.GetRequiredService<IProblemDetailsService>().TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
    }

    private static void ApplyAuthenticationChallenge(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.StatusCode != 401 || problem.Status != 401
            || !problem.Extensions.TryGetValue("code", out var code) || code is not string value || value != "authentication_failed") return;
        var current = context.Response.Headers.WWWAuthenticate;
        if (current.Count == 1 && current[0] == "Bearer") return;
        // Asignación, no Append: contrato único y sin parámetros privados, incluso ante un header previo incorrecto.
        context.Response.Headers.WWWAuthenticate = "Bearer";
    }

    private static string FieldName(ApiErrorField field) => field switch
    {
        ApiErrorField.FullName => "fullName", ApiErrorField.Email => "email", ApiErrorField.Password => "password", ApiErrorField.Body => "body", ApiErrorField.Authorization => "authorization", _ => throw new ArgumentOutOfRangeException(nameof(field))
    };
}

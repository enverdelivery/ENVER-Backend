using System.Diagnostics;

namespace Enver.API.Http;

/// <summary>Correlación acotada, no identidad ni confianza del cliente.</summary>
public sealed class RequestContextMiddleware(RequestDelegate next)
{
    public const string CorrelationHeader = "X-Correlation-ID";
    public static bool IsAllowedTraceId(string? value) => value is { Length: 32 }
        && value.Any(c => c != '0') && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    public static string GetOrCreateTraceId(HttpContext context)
    {
        if (!IsAllowedTraceId(context.TraceIdentifier)) context.TraceIdentifier = ActivityTraceId.CreateRandom().ToString();
        return context.TraceIdentifier;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[CorrelationHeader];
        context.TraceIdentifier = supplied.Count == 1 && IsAllowedTraceId(supplied[0]) ? supplied[0]! : ActivityTraceId.CreateRandom().ToString();
        void Apply() { context.Response.Headers.CacheControl = "no-store"; context.Response.Headers[CorrelationHeader] = context.TraceIdentifier; }
        Apply();
        context.Response.OnStarting(() => { Apply(); return Task.CompletedTask; });
        try { await next(context); }
        finally { if (!context.Response.HasStarted) Apply(); }
    }
}

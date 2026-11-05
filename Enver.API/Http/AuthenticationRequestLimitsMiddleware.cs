using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enver.API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Enver.API.Http;

public sealed record AuthenticationRequestMetadata(AuthenticationOperation Operation);
public sealed record AuthenticationBoundaryResult(AuthenticationOperation Operation, int? ErrorStatus, IReadOnlyList<ApiErrorField> InvalidFields, bool JsonReadable);
public interface IAuthenticationBoundaryContinuation { Task ContinueAsync(HttpContext context, AuthenticationBoundaryResult result, RequestDelegate next); }

/// <summary>Frontera aislable: no publica endpoints ni autentica tokens.</summary>
public sealed class AuthenticationRequestLimitsMiddleware
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly RequestDelegate next;
    private readonly AuthenticationOptions options;
    public AuthenticationRequestLimitsMiddleware(RequestDelegate next, IOptions<AuthenticationOptions> options)
    {
        this.next = next;
        this.options = options.Value;
        if (this.options.MaxRequestBodyBytes != 16384 || this.options.MaxJsonDepth != 4 || this.options.MaxJwtBytes != 4096)
            throw new ArgumentException("Límites HTTP incompatibles con la política inicial aprobada.", nameof(options));
    }

    public static void ConfigureSerializer(JsonSerializerOptions serializer)
    {
        serializer.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializer.PropertyNameCaseInsensitive = false;
        serializer.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        serializer.AllowDuplicateProperties = false;
        serializer.MaxDepth = 4;
        serializer.AllowTrailingCommas = false;
        serializer.ReadCommentHandling = JsonCommentHandling.Disallow;
        serializer.NumberHandling = JsonNumberHandling.Strict;
        serializer.RespectNullableAnnotations = true;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var metadata = context.GetEndpoint()?.Metadata.GetMetadata<AuthenticationRequestMetadata>();
        if (metadata is null) { await next(context); return; }
        var existing = context.Features.Get<RequestBudget>();
        using var owned = existing is null ? RequestBudget.Create(metadata.Operation, options, context.RequestAborted) : null;
        var budget = existing ?? owned!;
        context.Features.Set(budget);
        var buffer = ArrayPool<byte>.Shared.Rent(options.MaxRequestBodyBytes + 1);
        var original = context.Request.Body;
        try
        {
            if (context.Request.ContentLength > options.MaxRequestBodyBytes) { await ApiProblemDetails.TryWriteAsync(context, 413); return; }
            var count = 0;
            while (count < options.MaxRequestBodyBytes + 1)
            {
                budget.ThrowIfCancellationRequested();
                var read = await original.ReadAsync(buffer.AsMemory(count, options.MaxRequestBodyBytes + 1 - count), budget.Token);
                if (read == 0) break;
                count += read;
            }
            if (count > options.MaxRequestBodyBytes) { await ApiProblemDetails.TryWriteAsync(context, 413); return; }
            var result = Inspect(context, metadata.Operation, buffer.AsMemory(0, count));
            context.Features.Set(result);
            using var replay = new MemoryStream(buffer, 0, count, writable: false);
            context.Request.Body = replay;
            if (result.ErrorStatus is not null and not 400) { await ApiProblemDetails.TryWriteAsync(context, result.ErrorStatus.Value); return; }
            var continuation = context.RequestServices.GetService<IAuthenticationBoundaryContinuation>();
            if (continuation is not null) await continuation.ContinueAsync(context, result, next);
            else if (result.ErrorStatus == 400) await ApiProblemDetails.TryWriteAsync(context, 400, result.InvalidFields);
            else await next(context);
        }
        catch (OperationCanceledException) when (budget.Cause != BudgetCancellationCause.None)
        {
            if (budget.Cause != BudgetCancellationCause.Client) await ApiProblemDetails.TryWriteAsync(context, 503);
        }
        finally { context.Request.Body = original; context.Features.Set(existing); ArrayPool<byte>.Shared.Return(buffer, clearArray: true); }
    }

    private AuthenticationBoundaryResult Inspect(HttpContext context, AuthenticationOperation operation, ReadOnlyMemory<byte> body)
    {
        AuthenticationBoundaryResult Error(int status, bool readable = false, params ApiErrorField[] fields) => new(operation, status, fields, readable);
        if (operation == AuthenticationOperation.Logout)
            return !body.IsEmpty ? Error(400, false, ApiErrorField.Body) : IsBoundedBearer(context.Request) ? new(operation, null, [], false) : Error(401);
        if (!IsJsonUtf8(context.Request)) return Error(415);
        try
        {
            _ = StrictUtf8.GetCharCount(body.Span);
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = options.MaxJsonDepth, AllowTrailingCommas = false, CommentHandling = JsonCommentHandling.Disallow, AllowDuplicateProperties = false });
            if (document.RootElement.ValueKind != JsonValueKind.Object) return Error(400, false, ApiErrorField.Body);
            var required = operation == AuthenticationOperation.Register ? new[] { "fullName", "email", "password" } : new[] { "email", "password" };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var invalid = new HashSet<ApiErrorField>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!required.Contains(property.Name, StringComparer.Ordinal)) { invalid.Add(ApiErrorField.Body); continue; }
                seen.Add(property.Name);
                if (property.Value.ValueKind != JsonValueKind.String || !ValidUtf16(property.Value.GetString()!)) invalid.Add(ToField(property.Name));
            }
            foreach (var missing in required.Where(x => !seen.Contains(x))) invalid.Add(ToField(missing));
            return invalid.Count > 0 ? new(operation, 400, invalid.ToArray(), true) : new(operation, null, [], true);
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException) { return Error(400, false, ApiErrorField.Body); }
    }

    private static ApiErrorField ToField(string name) => name switch { "fullName" => ApiErrorField.FullName, "email" => ApiErrorField.Email, "password" => ApiErrorField.Password, _ => ApiErrorField.Body };
    private static bool ValidUtf16(string value)
    {
        for (var i = 0; i < value.Length; i++) if (char.IsSurrogate(value[i]) && (!char.IsHighSurrogate(value[i]) || ++i >= value.Length || !char.IsLowSurrogate(value[i]))) return false;
        return true;
    }
    private static bool IsJsonUtf8(HttpRequest request)
    {
        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var media) || !media.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)) return false;
        if (media.Parameters.Count > 1 || media.Parameters.Any(p => !p.Name.Equals("charset", StringComparison.OrdinalIgnoreCase))) return false;
        var charset = HeaderUtilities.RemoveQuotes(media.Charset);
        if (media.Parameters.Count == 1 && !charset.HasValue) return false;
        if (charset.HasValue && !charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase)) return false;
        var encoding = request.Headers.ContentEncoding;
        return encoding.Count == 0 || encoding.Count == 1 && string.Equals(encoding[0], "identity", StringComparison.OrdinalIgnoreCase);
    }
    private bool IsBoundedBearer(HttpRequest request)
    {
        var values = request.Headers.Authorization;
        if (values.Count != 1 || values[0] is not { } value || !value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;
        var token = value.AsSpan(7);
        if (token.Length == 0 || token.Length > options.MaxJwtBytes) return false;
        var dots = 0; var segment = 0;
        foreach (var character in token)
        {
            if (character == '.') { if (segment == 0) return false; dots++; segment = 0; }
            else if (character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-') segment++;
            else return false;
        }
        return dots == 2 && segment > 0;
    }
}

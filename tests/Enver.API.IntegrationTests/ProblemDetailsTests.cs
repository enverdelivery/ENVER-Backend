using System.Text.Json;
using Enver.API.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.IntegrationTests;

public sealed class ProblemDetailsTests
{
    [Theory]
    [InlineData(400, "validation_failed")]
    [InlineData(401, "authentication_failed")]
    [InlineData(409, "registration_conflict")]
    [InlineData(413, "payload_too_large")]
    [InlineData(415, "unsupported_media_type")]
    [InlineData(429, "rate_limited")]
    [InlineData(500, "internal_error")]
    [InlineData(503, "temporarily_unavailable")]
    public async Task Safe_problem_details_preserves_the_configured_contract(int status, string code)
    {
        await using var provider = CreateProvider();
        var context = CreateContext(provider);

        var written = await ApiProblemDetails.TryWriteAsync(context, status);
        var document = await ReadDocumentAsync(context);

        Assert.True(written);
        Assert.Equal(status, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType, StringComparison.Ordinal);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Equal("about:blank", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(status, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(code, document.RootElement.GetProperty("code").GetString());
        Assert.True(document.RootElement.TryGetProperty("traceId", out _));
        Assert.False(document.RootElement.TryGetProperty("instance", out _));
    }

    [Fact]
    public async Task Correlation_header_is_validated_and_matches_the_problem_trace_id()
    {
        await using var provider = CreateProvider();
        var context = CreateContext(provider);
        const string correlation = "0123456789abcdef0123456789abcdef";
        context.Request.Headers[RequestContextMiddleware.CorrelationHeader] = correlation;
        var middleware = new RequestContextMiddleware(async http => await ApiProblemDetails.TryWriteAsync(http, 400));

        await middleware.InvokeAsync(context);
        var document = await ReadDocumentAsync(context);

        Assert.Equal(correlation, context.Response.Headers[RequestContextMiddleware.CorrelationHeader]);
        Assert.Equal(correlation, document.RootElement.GetProperty("traceId").GetString());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("00000000000000000000000000000000")]
    public async Task Invalid_or_multiple_correlation_headers_are_replaced_with_a_safe_trace_id(string first)
    {
        await using var provider = CreateProvider();
        var context = CreateContext(provider);
        context.Request.Headers.Append(RequestContextMiddleware.CorrelationHeader, first);
        context.Request.Headers.Append(RequestContextMiddleware.CorrelationHeader, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        var middleware = new RequestContextMiddleware(async http => await ApiProblemDetails.TryWriteAsync(http, 400));

        await middleware.InvokeAsync(context);
        var document = await ReadDocumentAsync(context);
        var traceId = document.RootElement.GetProperty("traceId").GetString();

        Assert.Matches("^[0-9a-f]{32}$", traceId);
        Assert.NotEqual(first, traceId);
        Assert.Equal(traceId, context.Response.Headers[RequestContextMiddleware.CorrelationHeader]);
    }

    [Fact]
    public async Task Request_context_restores_no_store_and_correlation_when_on_starting_runs()
    {
        await using var provider = CreateProvider();
        var context = CreateContext(provider);
        var middleware = new RequestContextMiddleware(async http =>
        {
            http.Response.Headers.CacheControl = "public";
            http.Response.Headers[RequestContextMiddleware.CorrelationHeader] = "untrusted";
            await http.Response.WriteAsync("ok");
        });

        await middleware.InvokeAsync(context);

        Assert.Equal("no-store", context.Response.Headers.CacheControl);
        Assert.Matches("^[0-9a-f]{32}$", context.Response.Headers[RequestContextMiddleware.CorrelationHeader].ToString());
    }

    [Fact]
    public async Task Request_context_restores_headers_in_on_starting_before_the_response_starts()
    {
        const string correlation = "0123456789abcdef0123456789abcdef";
        var response = new ControlledResponseFeature();
        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(new HttpRequestFeature());
        features.Set<IHttpResponseFeature>(response);
        var context = new DefaultHttpContext(features);
        context.Request.Headers[RequestContextMiddleware.CorrelationHeader] = correlation;
        var nextReturned = false;
        var observedBeforeStart = false;
        string? cacheControlAtStart = null;
        string? correlationAtStart = null;
        var middleware = new RequestContextMiddleware(async http =>
        {
            http.Response.Headers.CacheControl = "public";
            http.Response.Headers[RequestContextMiddleware.CorrelationHeader] = "untrusted";
            await response.StartAsync(() =>
            {
                observedBeforeStart = !response.HasStarted && !nextReturned;
                cacheControlAtStart = http.Response.Headers.CacheControl;
                correlationAtStart = http.Response.Headers[RequestContextMiddleware.CorrelationHeader];
            });
            nextReturned = true;
        });

        await middleware.InvokeAsync(context);

        Assert.Equal(1, response.OnStartingRegistrations);
        Assert.Equal(1, response.ExecutedOnStartingCallbacks);
        Assert.True(observedBeforeStart);
        Assert.Equal("no-store", cacheControlAtStart);
        Assert.Equal(correlation, correlationAtStart);
        Assert.True(response.HasStarted);
        Assert.True(context.Response.HasStarted);
    }

    [Fact]
    public async Task Malicious_problem_details_are_sanitized_without_instance_extensions_or_sentinels()
    {
        const string sentinel = "problem-details-synthetic-secret";
        await using var provider = CreateProvider();
        var context = CreateContext(provider);
        context.Response.StatusCode = 500;
        var service = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var supplied = new ProblemDetails
        {
            Status = 500,
            Detail = sentinel,
            Instance = "/internal"
        };
        supplied.Extensions["code"] = sentinel;
        supplied.Extensions["secret"] = sentinel;

        var written = await service.TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = supplied });
        var document = await ReadDocumentAsync(context);
        var serialized = document.RootElement.GetRawText();

        Assert.True(written);
        Assert.Equal("internal_error", document.RootElement.GetProperty("code").GetString());
        Assert.False(document.RootElement.TryGetProperty("instance", out _));
        Assert.False(document.RootElement.TryGetProperty("secret", out _));
        Assert.DoesNotContain(sentinel, serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Validation_errors_allow_only_declared_fields_without_echoing_values()
    {
        const string supplied = "attacker@example.invalid";
        await using var provider = CreateProvider();
        var context = CreateContext(provider);

        await ApiProblemDetails.TryWriteAsync(context, 400, [ApiErrorField.Email, ApiErrorField.Password]);
        var document = await ReadDocumentAsync(context);
        var errors = document.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty("email", out _));
        Assert.True(errors.TryGetProperty("password", out _));
        Assert.Equal(2, errors.EnumerateObject().Count());
        Assert.DoesNotContain(supplied, document.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Retry_after_and_bearer_challenge_are_added_only_for_their_safe_statuses()
    {
        await using var provider = CreateProvider();
        var quotaContext = CreateContext(provider);
        var authContext = CreateContext(provider);

        await ApiProblemDetails.TryWriteAsync(quotaContext, 429, retryAfter: TimeSpan.FromMilliseconds(2100));
        await ApiProblemDetails.TryWriteAsync(authContext, 401);

        Assert.Equal("3", quotaContext.Response.Headers.RetryAfter);
        Assert.Equal("Bearer", authContext.Response.Headers.WWWAuthenticate);
    }

    [Fact]
    public async Task No_content_is_empty_and_second_writes_are_refused()
    {
        await using var provider = CreateProvider();
        var noContent = CreateContext(provider);
        var duplicate = CreateContext(provider);

        Assert.True(await ApiProblemDetails.TryWriteAsync(noContent, 204));
        Assert.Equal(0, noContent.Response.Body.Length);
        Assert.Null(noContent.Response.ContentType);
        Assert.Equal("no-store", noContent.Response.Headers.CacheControl);

        Assert.True(await ApiProblemDetails.TryWriteAsync(duplicate, 400));
        var first = await ReadTextAsync(duplicate);
        Assert.False(await ApiProblemDetails.TryWriteAsync(duplicate, 500));
        Assert.Equal(first, await ReadTextAsync(duplicate));
    }

    [Fact]
    public async Task Started_or_client_canceled_responses_are_not_rewritten()
    {
        await using var provider = CreateProvider();
        var started = CreateStartedContext(provider);
        var canceled = CreateContext(provider);
        using var source = new CancellationTokenSource();
        source.Cancel();
        canceled.RequestAborted = source.Token;

        Assert.False(await ApiProblemDetails.TryWriteAsync(started, 500));
        Assert.False(await ApiProblemDetails.TryWriteAsync(canceled, 500));
        Assert.Equal(0, canceled.Response.Body.Length);
    }

    [Fact]
    public async Task Exception_handler_returns_a_safe_error_without_echoing_exception_text()
    {
        const string sentinel = "exception-synthetic-secret";
        await using var provider = CreateProvider();
        var context = CreateContext(provider);
        var handler = new ApiExceptionHandler();

        Assert.True(await handler.TryHandleAsync(context, new InvalidOperationException(sentinel), CancellationToken.None));
        var document = await ReadDocumentAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("internal_error", document.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain(sentinel, document.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        ApiProblemDetails.AddSafeApiErrors(services);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateContext(IServiceProvider provider)
    {
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateStartedContext(IServiceProvider provider)
    {
        var features = new FeatureCollection();
        features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var context = new DefaultHttpContext(features) { RequestServices = provider };
        return context;
    }

    private static async Task<JsonDocument> ReadDocumentAsync(HttpContext context) => JsonDocument.Parse(await ReadTextAsync(context));

    private static async Task<string> ReadTextAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await new StreamReader(context.Response.Body, leaveOpen: true).ReadToEndAsync();
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted => true;
        public void OnStarting(Func<object, Task> callback, object state) { }
        public void OnCompleted(Func<object, Task> callback, object state) { }
    }

    private sealed class ControlledResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> onStarting = [];

        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted { get; private set; }
        public int OnStartingRegistrations => onStarting.Count;
        public int ExecutedOnStartingCallbacks { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            ArgumentNullException.ThrowIfNull(callback);
            if (HasStarted) throw new InvalidOperationException("La respuesta ya se inició.");
            onStarting.Add((callback, state));
        }

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public async Task StartAsync(Action observeBeforeStart)
        {
            foreach (var entry in onStarting.AsEnumerable().Reverse())
            {
                await entry.Callback(entry.State);
                ExecutedOnStartingCallbacks++;
            }

            observeBeforeStart();
            HasStarted = true;
        }
    }
}

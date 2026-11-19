using System.Text;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Enver.API.SecurityTests;

public sealed class AuthenticationBoundarySecurityTests
{
    [Fact]
    public async Task Exact_16384_byte_json_body_is_accepted_from_a_fragmented_non_seekable_stream()
    {
        await using var provider = BuildProblemProvider();
        var context = CreateContext(AuthenticationOperation.Login, provider, Fragmented(Encoding.UTF8.GetBytes(ExactBody(16384)), 3));
        context.Request.ContentType = "application/json; charset=utf-8";
        var reachedNext = false;

        await InvokeAsync(context, () => reachedNext = true);

        Assert.True(reachedNext);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task Body_larger_than_16384_bytes_is_rejected_without_relying_on_content_length()
    {
        await using var provider = BuildProblemProvider();
        var context = CreateContext(AuthenticationOperation.Login, provider, Fragmented(Encoding.UTF8.GetBytes(ExactBody(16385)), 3));
        context.Request.ContentType = "application/json; charset=utf-8";

        await InvokeAsync(context);
        var document = await ReadDocumentAsync(context);

        Assert.Equal(413, context.Response.StatusCode);
        Assert.Equal("payload_too_large", document.RootElement.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("{\"email\":\"user@uttt.edu.mx\",\"email\":\"second@example.invalid\",\"password\":\"valid\"}")]
    [InlineData("{\"email\":\"user@uttt.edu.mx\",\"password\":\"valid\",\"unknown\":\"x\"}")]
    [InlineData("{\"email\":\"user@uttt.edu.mx\"}")]
    [InlineData("{\"email\":null,\"password\":\"valid\"}")]
    [InlineData("{\"email\":42,\"password\":\"valid\"}")]
    [InlineData("{\"email\":{\"$ne\":\"x\"},\"password\":\"valid\"}")]
    [InlineData("{\"email\":[\"x\"],\"password\":\"valid\"}")]
    [InlineData("{/*comment*/\"email\":\"user@uttt.edu.mx\",\"password\":\"valid\"}")]
    [InlineData("{\"email\":\"user@uttt.edu.mx\",\"password\":\"valid\",}")]
    [InlineData("{\"email\":{\"a\":{\"b\":{\"c\":{\"d\":\"x\"}}}},\"password\":\"valid\"}")]
    public async Task Json_ambiguities_and_invalid_shapes_are_rejected_safely(string json)
    {
        await using var provider = BuildProblemProvider();
        var context = JsonContext(AuthenticationOperation.Login, provider, json);
        var reachedNext = false;

        await InvokeAsync(context, () => reachedNext = true);
        var document = await ReadDocumentAsync(context);

        Assert.False(reachedNext);
        Assert.Equal(400, context.Response.StatusCode);
        Assert.Equal("validation_failed", document.RootElement.GetProperty("code").GetString());
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task Invalid_utf8_and_unpaired_surrogate_are_rejected_without_echoing_values()
    {
        await using var provider = BuildProblemProvider();
        var invalidUtf8 = CreateContext(AuthenticationOperation.Login, provider, new MemoryStream([0xc3, 0x28]));
        invalidUtf8.Request.ContentType = "application/json; charset=utf-8";
        var surrogate = JsonContext(AuthenticationOperation.Login, provider, "{\"email\":\"\\uD800\",\"password\":\"valid\"}");

        await InvokeAsync(invalidUtf8);
        await InvokeAsync(surrogate);

        Assert.Equal(400, invalidUtf8.Response.StatusCode);
        Assert.Equal(400, surrogate.Response.StatusCode);
        Assert.DoesNotContain("D800", await ReadTextAsync(surrogate), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json; charset=iso-8859-1")]
    [InlineData("application/json; charset=")]
    public async Task Unsupported_media_type_or_charset_returns_415(string contentType)
    {
        await using var provider = BuildProblemProvider();
        var context = CreateContext(AuthenticationOperation.Login, provider, new MemoryStream(Encoding.UTF8.GetBytes("{}")));
        context.Request.ContentType = contentType;

        await InvokeAsync(context);

        Assert.Equal(415, context.Response.StatusCode);
    }

    [Fact]
    public async Task Unsupported_content_encoding_returns_415()
    {
        await using var provider = BuildProblemProvider();
        var context = JsonContext(AuthenticationOperation.Login, provider, "{\"email\":\"user@uttt.edu.mx\",\"password\":\"valid\"}");
        context.Request.Headers.ContentEncoding = "gzip";

        await InvokeAsync(context);

        Assert.Equal(415, context.Response.StatusCode);
    }

    [Fact]
    public async Task Logout_accepts_only_an_empty_body_and_a_bounded_compact_bearer()
    {
        await using var provider = BuildProblemProvider();
        var empty = LogoutContext(provider, string.Empty, $"Bearer {CompactToken(64)}");
        var whitespace = LogoutContext(provider, " ", CompactToken(64));
        var nonEmpty = LogoutContext(provider, "{}", CompactToken(64));
        var emptyReachedNext = false;

        await InvokeAsync(empty, () => emptyReachedNext = true);
        await InvokeAsync(whitespace);
        await InvokeAsync(nonEmpty);

        Assert.True(emptyReachedNext);
        Assert.Equal(400, whitespace.Response.StatusCode);
        Assert.Equal(400, nonEmpty.Response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic token")]
    [InlineData("Bearer a.b")]
    [InlineData("Bearer a.b.c!")]
    public async Task Missing_or_malformed_authorization_is_rejected(string? authorization)
    {
        await using var provider = BuildProblemProvider();
        var context = LogoutContext(provider, string.Empty, authorization);

        await InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task Multiple_authorization_headers_and_alternate_channels_do_not_bypass_the_boundary()
    {
        await using var provider = BuildProblemProvider();
        var multiple = LogoutContext(provider, string.Empty, CompactToken(64));
        multiple.Request.Headers.Append("Authorization", $"Bearer {CompactToken(64)}");
        var alternate = LogoutContext(provider, string.Empty, null);
        alternate.Request.QueryString = new QueryString($"?access_token={CompactToken(64)}");
        alternate.Request.Headers.Cookie = $"access_token={CompactToken(64)}";

        await InvokeAsync(multiple);
        await InvokeAsync(alternate);

        Assert.Equal(401, multiple.Response.StatusCode);
        Assert.Equal(401, alternate.Response.StatusCode);
    }

    [Theory]
    [InlineData(4096, true)]
    [InlineData(4097, false)]
    public async Task Compact_bearer_exact_length_boundary_is_enforced_before_cryptography(int length, bool accepted)
    {
        await using var provider = BuildProblemProvider();
        var token = CompactToken(length);
        Assert.Equal(length, token.Length);
        Assert.Equal(3, token.Split('.').Length);
        var context = LogoutContext(provider, string.Empty, $"Bearer {token}");
        var reachedNext = false;

        await InvokeAsync(context, () => reachedNext = true);

        Assert.Equal(accepted, reachedNext);
        Assert.Equal(accepted ? 200 : 401, context.Response.StatusCode);
        if (!accepted)
        {
            var body = await ReadTextAsync(context);
            Assert.DoesNotContain(token, body, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Client_cancellation_does_not_write_a_problem_response()
    {
        await using var provider = BuildProblemProvider();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = JsonContext(AuthenticationOperation.Login, provider, "{\"email\":\"user@uttt.edu.mx\",\"password\":\"valid\"}");
        context.RequestAborted = cancellation.Token;

        await InvokeAsync(context);

        Assert.Equal(0, context.Response.Body.Length);
    }

    private static async Task InvokeAsync(HttpContext context, Action? nextAction = null)
    {
        var middleware = new AuthenticationRequestLimitsMiddleware(_ =>
        {
            nextAction?.Invoke();
            return Task.CompletedTask;
        }, Options.Create(AuthenticationOptions()));
        await middleware.InvokeAsync(context);
    }

    private static DefaultHttpContext JsonContext(AuthenticationOperation operation, IServiceProvider provider, string json)
    {
        var context = CreateContext(operation, provider, new MemoryStream(Encoding.UTF8.GetBytes(json)));
        context.Request.ContentType = "application/json; charset=utf-8";
        return context;
    }

    private static DefaultHttpContext LogoutContext(IServiceProvider provider, string body, string? authorization)
    {
        var context = CreateContext(AuthenticationOperation.Logout, provider, new MemoryStream(Encoding.UTF8.GetBytes(body)));
        if (authorization is not null) context.Request.Headers.Authorization = authorization;
        return context;
    }

    private static DefaultHttpContext CreateContext(AuthenticationOperation operation, IServiceProvider provider, Stream body)
    {
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Response.Body = new MemoryStream();
        context.Request.Body = body;
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new AuthenticationRequestMetadata(operation)), "synthetic-auth-boundary"));
        return context;
    }

    private static ServiceProvider BuildProblemProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<IAuthenticationBoundaryContinuation, ObservingBoundaryContinuation>();
        ApiProblemDetails.AddSafeApiErrors(services);
        return services.BuildServiceProvider();
    }

    private static AuthenticationOptions AuthenticationOptions() => new()
    {
        MaxRequestBodyBytes = 16384,
        MaxJsonDepth = 4,
        MaxJwtBytes = 4096,
        RegisterBudgetMilliseconds = 500,
        LoginBudgetMilliseconds = 500,
        LogoutBudgetMilliseconds = 500
    };

    private static string ExactBody(int bytes)
    {
        const string prefix = "{\"email\":\"";
        const string suffix = "\",\"password\":\"valid\"}";
        return prefix + new string('a', bytes - Encoding.UTF8.GetByteCount(prefix) - Encoding.UTF8.GetByteCount(suffix)) + suffix;
    }

    private static string CompactToken(int length)
    {
        const string prefix = "eyJhbGciOiJIUzI1NiJ9IA.eyJzdWIiOiJzeW50aGV0aWMifQ.";
        return prefix + new string('A', length - prefix.Length);
    }

    private static Stream Fragmented(byte[] bytes, int chunkSize) => new FragmentedReadStream(bytes, chunkSize);

    private static async Task<JsonDocument> ReadDocumentAsync(HttpContext context) => JsonDocument.Parse(await ReadTextAsync(context));

    private static async Task<string> ReadTextAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await new StreamReader(context.Response.Body, leaveOpen: true).ReadToEndAsync();
    }

    private sealed class FragmentedReadStream(byte[] bytes, int chunkSize) : Stream
    {
        private int position;
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            var available = Math.Min(bytes.Length - position, Math.Min(buffer.Length, chunkSize));
            if (available <= 0) return 0;
            bytes.AsSpan(position, available).CopyTo(buffer);
            position += available;
            return available;
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => ValueTask.FromResult(Read(buffer.Span));
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class ObservingBoundaryContinuation : IAuthenticationBoundaryContinuation
    {
        public async Task ContinueAsync(HttpContext context, AuthenticationBoundaryResult result, RequestDelegate next)
        {
            if (result.ErrorStatus is null)
            {
                await next(context);
                return;
            }

            await ApiProblemDetails.TryWriteAsync(context, result.ErrorStatus.Value, result.InvalidFields);
        }
    }
}

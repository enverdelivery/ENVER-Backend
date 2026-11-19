using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests;

public sealed class RedisInfrastructureTests
{
    [Fact]
    public void Revocation_key_uses_the_exact_fixed_namespace()
    {
        var jti = Jti();

        var key = RedisKeyBuilder.BuildRevocationKey("testing", RedisKeyBuilder.Issuer, jti);

        Assert.Equal($"enver:testing:auth:revoked:enver-api:{jti}", key);
    }

    [Fact]
    public void Revocation_key_separates_environments_and_keeps_the_revocation_purpose_fixed()
    {
        var jti = Jti();

        var testing = RedisKeyBuilder.BuildRevocationKey("testing", RedisKeyBuilder.Issuer, jti);
        var staging = RedisKeyBuilder.BuildRevocationKey("staging", RedisKeyBuilder.Issuer, jti);

        Assert.NotEqual(testing, staging);
        Assert.Contains(":auth:revoked:enver-api:", testing, StringComparison.Ordinal);
        Assert.DoesNotContain(":auth:ratelimit:", testing, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "enver-api", "jti")]
    [InlineData("test ing", "enver-api", "jti")]
    [InlineData("Testing", "enver-api", "jti")]
    [InlineData("testing", "other", "jti")]
    [InlineData("testing", "enver-api", "eyJhbGciOiJIUzI1NiJ9.eyJqdGkiOiJ4In0.signature")]
    [InlineData("testing", "enver-api", "alice@uttt.edu.mx")]
    public void Revocation_key_rejects_invalid_segments(string environment, string issuer, string jti)
    {
        Assert.Throws<ArgumentException>(() => RedisKeyBuilder.BuildRevocationKey(environment, issuer, jti));
    }

    [Fact]
    public void Revocation_key_rejects_noncanonical_or_wrong_length_jti()
    {
        var valid = Jti();

        Assert.Throws<ArgumentException>(() => RedisKeyBuilder.BuildRevocationKey("testing", RedisKeyBuilder.Issuer, valid + "="));
        Assert.Throws<ArgumentException>(() => RedisKeyBuilder.BuildRevocationKey("testing", RedisKeyBuilder.Issuer, valid[..42]));
    }

    [Fact]
    public void Connection_options_are_explicitly_bounded_and_fail_fast()
    {
        var options = new RedisOptions
        {
            ConnectionString = "localhost:6379,password=synthetic-secret",
            ConnectionTimeoutMilliseconds = 123,
            OperationTimeoutMilliseconds = 234
        };

        var configuration = RedisConnections.CreateConfigurationOptions(options);

        Assert.Equal(123, configuration.ConnectTimeout);
        Assert.Equal(234, configuration.SyncTimeout);
        Assert.Equal(234, configuration.AsyncTimeout);
        Assert.False(configuration.AbortOnConnectFail);
        Assert.False(configuration.IncludeDetailInExceptions);
        Assert.Same(BacklogPolicy.FailFast, configuration.BacklogPolicy);
        Assert.IsType<ExponentialRetry>(configuration.ReconnectRetryPolicy);
    }

    [Fact]
    public void Connectivity_observation_exposes_only_sanitized_signal_kinds()
    {
        var signal = typeof(RedisConnections).GetEvent(nameof(RedisConnections.ConnectivityChanged));

        Assert.NotNull(signal);
        Assert.Equal(typeof(EventHandler<RedisConnectionSignal>), signal.EventHandlerType);
        Assert.Equal(
        [
            RedisConnectionSignal.ConnectionFailed,
            RedisConnectionSignal.ConnectionRestored,
            RedisConnectionSignal.ErrorMessage,
            RedisConnectionSignal.InternalError
        ],
        Enum.GetValues<RedisConnectionSignal>());
    }

    [Fact]
    public async Task Cancelled_caller_does_not_start_or_dispose_a_connection()
    {
        var connections = new RedisConnections(Options());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connections.GetMultiplexerAsync(cancellation.Token));

        await connections.DisposeAsync();
    }

    private static RedisOptions Options() => new()
    {
        ConnectionString = "127.0.0.1:1",
        ConnectionTimeoutMilliseconds = 100,
        OperationTimeoutMilliseconds = 100
    };

    private static string Jti() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

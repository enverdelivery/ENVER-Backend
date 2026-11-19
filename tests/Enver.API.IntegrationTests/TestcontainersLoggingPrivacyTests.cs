using Enver.Testing;
using Microsoft.Extensions.Logging;

namespace Enver.API.IntegrationTests;

/// <summary>Regresión del logger sin iniciar contenedores ni usar secretos reales.</summary>
public sealed class TestcontainersLoggingPrivacyTests
{
    [Fact]
    public void Secrets_are_redacted_from_commands_uris_scopes_structured_state_and_exceptions_but_errors_remain_visible()
    {
        const string password = "synthetic-password+/=";
        const string jwt = "synthetic-jwt-value";
        const string hmac = "synthetic-hmac-value";
        const string connection = "mongodb://fixture-user:synthetic-password%2B%2F%3D@localhost:27017/?replicaSet=rs0";
        var sink = new CapturingLogger();
        var logger = new SensitiveTestcontainersLogger(sink, [password, jwt, hmac, "synthetic-dynamo-secret"]);
        using (logger.BeginScope(new Dictionary<string, object?> { ["service"] = "MongoDB", ["password"] = password }))
        {
            logger.LogInformation("Execute mongosh --username fixture-user --password {Password} --quiet at container abc123", password);
            logger.LogInformation("Connection {Connection} Redis redis://localhost:6379 URI https://user:other-password@localhost/ Pair other-user:other-password@", connection);
            logger.LogInformation("JWT={Jwt} HMACKey={Hmac} SecretAccessKey=synthetic-dynamo-secret", jwt, hmac);
            logger.LogWarning("--password 'another private value' --token another-token RedisPassword=another-redis-password Authorization: Bearer another-bearer");
            logger.LogError(new InvalidOperationException($"Container readiness failed: {connection}",
                new ArgumentException($"credential={password}")), "MongoDB healthcheck failed; port=27017 state=unhealthy");
        }

        var output = string.Join(Environment.NewLine, sink.Messages.Concat(sink.Scopes));
        // Aserciones booleanas: un fallo no imprime el diagnóstico ni el secreto esperado.
        foreach (var secret in new[] { password, Uri.EscapeDataString(password), connection, jwt, hmac,
            "synthetic-dynamo-secret", "other-password", "another private value", "another-token",
            "another-redis-password", "another-bearer" })
        {
            Assert.False(output.Contains(secret, StringComparison.Ordinal));
        }

        Assert.False(output.Contains("mongodb://", StringComparison.Ordinal));
        Assert.False(output.Contains("redis://", StringComparison.Ordinal));
        var hasCredentialPair = System.Text.RegularExpressions.Regex.IsMatch(output, @"\b[^\s:@/]+:[^\s@/]+@");
        Assert.False(hasCredentialPair);
        Assert.True(output.Contains("[REDACTED]", StringComparison.Ordinal));
        Assert.True(output.Contains("Execute mongosh", StringComparison.Ordinal));
        Assert.True(output.Contains("abc123", StringComparison.Ordinal));
        Assert.True(output.Contains("MongoDB healthcheck failed; port=27017 state=unhealthy", StringComparison.Ordinal));
        Assert.True(output.Contains("InvalidOperationException", StringComparison.Ordinal));
        Assert.True(output.Contains("ArgumentException", StringComparison.Ordinal));
        Assert.True(output.Contains("Container readiness failed", StringComparison.Ordinal));
        Assert.True(output.Contains("service=MongoDB", StringComparison.Ordinal));
        Assert.Equal(5, sink.Messages.Count);
        Assert.Contains(LogLevel.Error, sink.Levels);
        Assert.Contains(LogLevel.Warning, sink.Levels);
        Assert.All(sink.RawStates, state => Assert.IsType<string>(state));
        Assert.All(sink.Exceptions, exception => Assert.Null(exception));
        Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Messages { get; } = [];
        public List<string> Scopes { get; } = [];
        public List<LogLevel> Levels { get; } = [];
        public List<object?> RawStates { get; } = [];
        public List<Exception?> Exceptions { get; } = [];
        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            Scopes.Add(state.ToString()!);
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            RawStates.Add(state);
            Exceptions.Add(exception);
            Levels.Add(logLevel);
            Messages.Add(formatter(state, exception));
        }
    }
}

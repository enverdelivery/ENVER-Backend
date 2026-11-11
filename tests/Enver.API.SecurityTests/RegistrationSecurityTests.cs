using System.Text.Json;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Contracts;
using Enver.API.Http;
using Enver.API.Observability;
using Microsoft.Extensions.Logging;

namespace Enver.API.SecurityTests;

public sealed class RegistrationSecurityTests
{
    [Fact]
    public void Sensitive_inputs_have_non_sensitive_ToString_and_response_has_exact_allowlist()
    {
        var request = new RegisterRequest { FullName = "Synthetic", Email = "synthetic@uttt.edu.mx", Password = "synthetic-secret" };
        Assert.Equal(nameof(RegisterRequest), request.ToString());
        Assert.Equal(nameof(RegisterInput), new RegisterInput(request.FullName, request.Email, request.Password).ToString());
        var response = JsonSerializer.SerializeToElement(new RegisterResponse(Guid.NewGuid(), "Synthetic", "synthetic@uttt.edu.mx"));
        Assert.Equal(new[] { "email", "fullName", "id" }, response.EnumerateObject().Select(p => p.Name).Order());
        Assert.True(!response.GetRawText().Contains("synthetic-secret", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"synthetic@uttt.edu.mx\",\"password\":\"synthetic-secret\",\"roles\":[\"admin\"]}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":{},\"password\":\"synthetic-secret\"}")]
    [InlineData("{\"fullName\":\"Synthetic\",\"email\":\"a@uttt.edu.mx\",\"password\":\"first\",\"password\":\"second\"}")]
    public void Strict_contract_rejects_mass_assignment_objects_and_duplicate_fields(string json)
    {
        var options = new JsonSerializerOptions(); AuthenticationRequestLimitsMiddleware.ConfigureSerializer(options);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<RegisterRequest>(json, options));
    }

    [Fact]
    public void Structured_events_only_accept_closed_enums_and_emit_no_request_labels()
    {
        var logger = new Capture(); var events = new AuthenticationEvents(logger);
        foreach (var kind in Enum.GetValues<AuthenticationEventKind>())
            foreach (var outcome in Enum.GetValues<AuthenticationEventOutcome>()) events.Record(kind, outcome);
        Assert.NotEmpty(logger.Keys);
        Assert.All(logger.Keys, keys => Assert.Equal(new[] { "Kind", "Operation", "Outcome", "{OriginalFormat}" }.Order(), keys.Order()));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Record((AuthenticationEventKind)999, AuthenticationEventOutcome.Admitted));
    }

    private sealed class Capture : ILogger<AuthenticationEvents>
    {
        public List<string[]> Keys { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
        {
            Assert.Null(error);
            var fields = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(state);
            Keys.Add(fields.Select(f => f.Key).ToArray());
            Assert.All(fields.Where(f => f.Key is "Kind" or "Outcome"), f => Assert.True(f.Value is AuthenticationEventKind or AuthenticationEventOutcome));
        }
    }
}

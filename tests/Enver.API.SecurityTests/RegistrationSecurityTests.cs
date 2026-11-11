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
        foreach (var kind in Enum.GetValues<AuthenticationEventKind>().Where(kind => kind != AuthenticationEventKind.Logout))
            foreach (var outcome in Enum.GetValues<AuthenticationEventOutcome>()) events.Record(kind, outcome);
        Assert.NotEmpty(logger.Keys);
        Assert.All(logger.Keys, keys => Assert.Equal(new[] { "Kind", "Operation", "Outcome", "{OriginalFormat}" }.Order(), keys.Order()));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Record((AuthenticationEventKind)999, AuthenticationEventOutcome.Admitted));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Record(AuthenticationEventKind.Register, (AuthenticationEventOutcome)999));
    }

    [Theory]
    [InlineData(AuthenticationEventOutcome.Confirmed, "confirmed")]
    [InlineData(AuthenticationEventOutcome.Rejected, "rejected")]
    [InlineData(AuthenticationEventOutcome.RateLimited, "rate_limited")]
    [InlineData(AuthenticationEventOutcome.Unavailable, "dependency_unavailable")]
    [InlineData(AuthenticationEventOutcome.Uncertain, "uncertain")]
    public void Logout_events_keep_their_exact_closed_labels_and_no_request_fields(AuthenticationEventOutcome outcome, string label)
    {
        var logger = new Capture(); var events = new AuthenticationEvents(logger);
        events.Record(AuthenticationEventKind.Logout, outcome);
        Assert.Equal(new[] { "Operation", "Outcome", "{OriginalFormat}" }.Order(), Assert.Single(logger.Keys).Order());
        var fields = Assert.Single(logger.Fields);
        Assert.Equal("logout", fields["Operation"]);
        Assert.Equal(label, fields["Outcome"]);
        Assert.Equal("Authentication {Operation} {Outcome}", fields["{OriginalFormat}"]);
        var id = Assert.Single(logger.EventIds);
        Assert.Equal(2045, id.Id); Assert.Equal("Logout", id.Name);
        Assert.Equal("Authentication logout " + label, Assert.Single(logger.Messages));
    }

    [Fact]
    public void Logout_rejects_other_defined_outcomes_and_unknown_enums_without_emitting_an_event()
    {
        var logger = new Capture(); var events = new AuthenticationEvents(logger);
        var allowed = new[] { AuthenticationEventOutcome.Confirmed, AuthenticationEventOutcome.Rejected,
            AuthenticationEventOutcome.RateLimited, AuthenticationEventOutcome.Unavailable, AuthenticationEventOutcome.Uncertain };
        foreach (var outcome in Enum.GetValues<AuthenticationEventOutcome>().Where(outcome => !allowed.Contains(outcome)))
            Assert.Throws<ArgumentOutOfRangeException>(() => events.Record(AuthenticationEventKind.Logout, outcome));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Record(AuthenticationEventKind.Logout, (AuthenticationEventOutcome)999));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Record((AuthenticationEventKind)999, AuthenticationEventOutcome.Confirmed));
        Assert.Empty(logger.Keys); Assert.Empty(logger.Fields); Assert.Empty(logger.EventIds); Assert.Empty(logger.Messages);
    }

    private sealed class Capture : ILogger<AuthenticationEvents>
    {
        public List<string[]> Keys { get; } = [];
        public List<Dictionary<string, object?>> Fields { get; } = [];
        public List<EventId> EventIds { get; } = [];
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
        {
            Assert.Null(error);
            var fields = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object?>>>(state).ToArray();
            Keys.Add(fields.Select(f => f.Key).ToArray());
            Fields.Add(fields.ToDictionary(f => f.Key, f => f.Value));
            EventIds.Add(id); Messages.Add(formatter(state, error));
            if (fields.Any(f => f.Key == "Operation" && Equals(f.Value, "logout")))
                Assert.All(fields.Where(f => f.Key == "Outcome"), f => Assert.Contains(Assert.IsType<string>(f.Value),
                    new[] { "confirmed", "rejected", "rate_limited", "dependency_unavailable", "uncertain" }));
            else
                Assert.All(fields.Where(f => f.Key is "Kind" or "Outcome"), f => Assert.True(f.Value is AuthenticationEventKind or AuthenticationEventOutcome));
        }
    }
}

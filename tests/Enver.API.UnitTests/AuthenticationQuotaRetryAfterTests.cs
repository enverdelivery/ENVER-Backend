using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.UnitTests;

public sealed class AuthenticationQuotaRetryAfterTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Window_excludes_lower_boundary_and_includes_now()
    {
        var definition = new AuthenticationQuotaDefinition(1, TimeSpan.FromMinutes(1));
        Assert.Null(AuthenticationQuotaRetryAfter.ForScope(Now, definition, [Now.AddMinutes(-1), Now.AddMinutes(-2)]));
        Assert.Equal(TimeSpan.FromSeconds(1), AuthenticationQuotaRetryAfter.ForScope(Now, definition, [Now.AddMinutes(-1).AddTicks(1)]));
        Assert.Equal(TimeSpan.FromMinutes(1), AuthenticationQuotaRetryAfter.ForScope(Now, definition, [Now]));
    }

    [Fact]
    public void Fewer_events_than_limit_do_not_block()
    {
        var definition = new AuthenticationQuotaDefinition(3, TimeSpan.FromMinutes(1));
        Assert.Null(AuthenticationQuotaRetryAfter.ForScope(Now, definition, []));
        Assert.Null(AuthenticationQuotaRetryAfter.ForScope(Now, definition, [Now, Now]));
    }

    [Fact]
    public void At_limit_oldest_live_event_determines_release()
    {
        var definition = new AuthenticationQuotaDefinition(3, TimeSpan.FromMinutes(1));
        var times = new[] { Now.AddSeconds(-10), Now.AddSeconds(-50), Now.AddSeconds(-30) };
        Assert.Equal(TimeSpan.FromSeconds(10), AuthenticationQuotaRetryAfter.ForScope(Now, definition, times));
        Assert.Equal(Now.AddSeconds(-10), times[0]); // No ordena/muta el array del consumidor.
    }

    [Fact]
    public void Above_limit_relevant_event_is_n_minus_limit_plus_one_not_oldest()
    {
        var definition = new AuthenticationQuotaDefinition(3, TimeSpan.FromMinutes(1));
        var times = new[] { Now.AddSeconds(-50), Now.AddSeconds(-40), Now.AddSeconds(-30), Now.AddSeconds(-20), Now.AddSeconds(-10) };
        Assert.Equal(TimeSpan.FromSeconds(30), AuthenticationQuotaRetryAfter.ForScope(Now, definition, times));
    }

    [Fact]
    public void Equal_timestamps_represent_distinct_events()
    {
        Assert.Equal(TimeSpan.FromSeconds(60), AuthenticationQuotaRetryAfter.ForScope(Now, new(2, TimeSpan.FromMinutes(1)), [Now, Now]));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10_000_000, 1)]
    [InlineData(10_000_001, 2)]
    [InlineData(19_999_999, 2)]
    public void Delay_rounds_up_to_whole_seconds(long ticks, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), AuthenticationQuotaRetryAfter.ForScope(Now,
            new(1, TimeSpan.FromTicks(ticks)), [Now]));
    }

    [Fact]
    public void Maximum_delay_uses_all_blocking_scopes_with_same_now()
    {
        var first = AuthenticationQuotaRetryAfter.ForScope(Now, new(1, TimeSpan.FromMinutes(1)), [Now.AddSeconds(-50)]);
        var second = AuthenticationQuotaRetryAfter.ForScope(Now, new(1, TimeSpan.FromMinutes(15)), [Now.AddSeconds(-899.5)]);
        var third = AuthenticationQuotaRetryAfter.ForScope(Now, new(2, TimeSpan.FromMinutes(1)), [Now]);
        Assert.Equal(TimeSpan.FromSeconds(10), AuthenticationQuotaRetryAfter.Maximum([first, second, third]));
        Assert.Equal(TimeSpan.FromSeconds(2), AuthenticationQuotaRetryAfter.Maximum([TimeSpan.FromTicks(10_000_001), null]));
        Assert.Null(AuthenticationQuotaRetryAfter.Maximum([null, null]));
        Assert.Null(AuthenticationQuotaRetryAfter.Maximum([]));
    }

    [Fact]
    public void Equivalent_instants_with_different_offsets_have_same_window()
    {
        var definition = new AuthenticationQuotaDefinition(1, TimeSpan.FromMinutes(1));
        Assert.Equal(TimeSpan.FromSeconds(30), AuthenticationQuotaRetryAfter.ForScope(Now.ToOffset(TimeSpan.FromHours(-6)),
            definition, [Now.AddSeconds(-30).ToOffset(TimeSpan.FromHours(2))]));
    }

    [Fact]
    public void Future_events_and_nonpositive_blocking_delays_are_invalid()
    {
        Assert.Throws<ArgumentException>(() => AuthenticationQuotaRetryAfter.ForScope(Now, new(1, TimeSpan.FromMinutes(1)), [Now.AddTicks(1)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuthenticationQuotaRetryAfter.Maximum([TimeSpan.Zero]));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuthenticationQuotaRetryAfter.Maximum([TimeSpan.FromTicks(-1)]));
    }

    [Fact]
    public void Overflow_never_silently_shortens_retry_after()
    {
        Assert.Throws<OverflowException>(() => AuthenticationQuotaRetryAfter.ForScope(Now, new(1, TimeSpan.MaxValue), [Now]));
    }

    [Fact]
    public void Expiration_boundary_can_be_calculated_near_minimum_date_without_date_subtraction_overflow()
    {
        Assert.Equal(TimeSpan.FromMinutes(1), AuthenticationQuotaRetryAfter.ForScope(DateTimeOffset.MinValue,
            new(1, TimeSpan.FromMinutes(1)), [DateTimeOffset.MinValue]));
    }
}

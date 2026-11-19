using Enver.API.Configuration;
using Enver.API.Infrastructure.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Enver.API.UnitTests;

public sealed class MongoClientFactoryTests
{
    [Theory]
    [InlineData(2000)]
    [InlineData(500)]
    public void Settings_apply_all_approved_concerns_and_finite_phase_timeouts(int milliseconds)
    {
        var settings = MongoClientFactory.CreateSettings(Options(milliseconds));
        var timeout = TimeSpan.FromMilliseconds(milliseconds);
        Assert.Equal(ReadConcern.Majority, settings.ReadConcern);
        Assert.Equal(ReadPreference.Primary, settings.ReadPreference);
        Assert.Equal("majority", settings.WriteConcern.W.ToString());
        Assert.True(settings.WriteConcern.Journal);
        Assert.True(settings.WriteConcern.IsAcknowledged);
        Assert.Equal(timeout, settings.WriteConcern.WTimeout);
        Assert.False(settings.RetryWrites);
        Assert.Equal(timeout, settings.SocketTimeout);
        Assert.Equal(timeout, settings.WaitQueueTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(750), settings.ConnectTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(750), settings.ServerSelectionTimeout);
    }

    [Fact]
    public void Connection_string_does_not_override_approved_policy_or_disable_future_tls()
    {
        var options = Options();
        options.ConnectionString = "mongodb://localhost:27017/?tls=true&retryWrites=true&w=1&journal=false&readConcernLevel=local&readPreference=secondary&wtimeoutMS=1";
        var settings = MongoClientFactory.CreateSettings(options);
        Assert.True(settings.UseTls);
        Assert.False(settings.AllowInsecureTls);
        Assert.False(settings.RetryWrites);
        Assert.Equal(ReadConcern.Majority, settings.ReadConcern);
        Assert.Equal(ReadPreference.Primary, settings.ReadPreference);
        Assert.Equal(TimeSpan.FromMilliseconds(500), settings.WriteConcern.WTimeout);
        Assert.True(settings.WriteConcern.Journal);
        Assert.Equal("majority", settings.WriteConcern.W.ToString());
    }

    [Fact]
    public void Uri_cannot_enable_internal_csot_and_remove_explicit_ack_timeout()
    {
        var options = Options();
        options.ConnectionString = "mongodb://localhost:27017/?timeoutMS=100";
        Assert.Throws<ArgumentException>(() => MongoClientFactory.CreateSettings(options));
    }

    [Fact]
    public async Task Isolated_singleton_composition_reuses_one_client_under_concurrency_and_freezes_options()
    {
        var options = Options();
        var services = new ServiceCollection();
        services.AddSingleton(_ => new MongoClientFactory(options));
        using var provider = services.BuildServiceProvider();
        var owner = provider.GetRequiredService<MongoClientFactory>();
        options.OperationTimeoutMilliseconds = 1;
        options.ConnectionString = "invalid-snapshot-must-not-be-used";
        var clients = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() =>
            provider.GetRequiredService<MongoClientFactory>().Client)));
        Assert.All(clients, client => Assert.Same(owner.Client, client));
        Assert.Equal(TimeSpan.FromMilliseconds(500), owner.Client.Settings.WriteConcern.WTimeout);
        using var other = new MongoClientFactory(Options());
        Assert.NotSame(owner.Client, other.Client);
        owner.Dispose();
        owner.Dispose();
        Assert.Throws<ObjectDisposedException>(() => owner.Client);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Nonpositive_timeouts_are_rejected(int value)
    {
        var options = Options(value);
        Assert.Throws<ArgumentException>(() => MongoClientFactory.CreateSettings(options));
        options = Options();
        options.ConnectionTimeoutMilliseconds = value;
        Assert.Throws<ArgumentException>(() => MongoClientFactory.CreateSettings(options));
    }

    [Theory]
    [InlineData("local", "majority", "primary")]
    [InlineData("majority", "local", "primary")]
    [InlineData("majority", "majority", "secondary")]
    public void Incompatible_candidates_fail_instead_of_silently_degrading(string write, string read, string preference)
    {
        var options = Options();
        options.WriteConcernCandidate = write;
        options.ReadConcernCandidate = read;
        options.ReadPreference = preference;
        Assert.Throws<ArgumentException>(() => MongoClientFactory.CreateSettings(options));
    }

    [Fact]
    public void Invalid_connection_diagnostics_are_sanitized_without_inner_parser_exception()
    {
        var options = Options();
        options.ConnectionString = "mongodb://synthetic-user:synthetic-secret@localhost:invalid-port";
        var error = Assert.Throws<ArgumentException>(() => MongoClientFactory.CreateSettings(options));
        Assert.Null(error.InnerException);
        Assert.DoesNotContain("synthetic-user", error.ToString());
        Assert.DoesNotContain("synthetic-secret", error.ToString());
        Assert.DoesNotContain(options.ConnectionString, error.ToString());
    }

    private static MongoDbOptions Options(int milliseconds = 500) => new()
    {
        ConnectionString = "mongodb://127.0.0.1:1/?directConnection=true",
        OperationTimeoutMilliseconds = milliseconds,
        ConnectionTimeoutMilliseconds = 750,
        WriteConcernCandidate = "majority",
        ReadConcernCandidate = "majority",
        ReadPreference = "primary"
    };
}

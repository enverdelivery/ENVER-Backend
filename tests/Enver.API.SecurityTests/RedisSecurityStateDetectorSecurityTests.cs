using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisSecurityStateDetectorSecurityTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Redis_restart_never_bypasses_fail_closed_admission_when_recovery_is_unproven()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options());
        using var detector = new RedisSecurityStateDetector(connections, guard);
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        var failed = WaitForSignalAsync(detector, RedisConnectionSignal.ConnectionFailed);

        await dependencies.StopRedisAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var failure = await Record.ExceptionAsync(async () => await database.StringGetAsync(Key()).WaitAsync(timeout.Token));
            Assert.NotNull(failure);
            await failed;

            using var closedRequest = new RequestBudget(1000, CancellationToken.None);
            Assert.False((await guard.EvaluateAsync(closedRequest)).CanContinue);
        }
        finally
        {
            var restored = TryWaitForSignalAsync(detector, RedisConnectionSignal.ConnectionRestored);
            await dependencies.StartRedisAsync();
            _ = await restored;
        }

        using var request = new RequestBudget(1000, CancellationToken.None);
        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.False(decision.CanContinue);
    }

    private RedisOptions Options() => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        ConnectionTimeoutMilliseconds = 250,
        OperationTimeoutMilliseconds = 250
    };

    private string Key() => $"{dependencies.RedisNamespace}:t019:security-probe";

    private static async Task<RedisSecurityStateObservation> WaitForSignalAsync(
        RedisSecurityStateDetector detector,
        RedisConnectionSignal signal)
    {
        var completion = new TaskCompletionSource<RedisSecurityStateObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<RedisSecurityStateObservation>? handler = null;
        handler = (_, observation) =>
        {
            if (observation.Signal == signal)
            {
                completion.TrySetResult(observation);
            }
        };

        detector.SignalObserved += handler;
        try
        {
            return await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            detector.SignalObserved -= handler;
        }
    }

    private static async Task<RedisSecurityStateObservation?> TryWaitForSignalAsync(
        RedisSecurityStateDetector detector,
        RedisConnectionSignal signal)
    {
        var completion = new TaskCompletionSource<RedisSecurityStateObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<RedisSecurityStateObservation>? handler = null;
        handler = (_, observation) =>
        {
            if (observation.Signal == signal)
            {
                completion.TrySetResult(observation);
            }
        };

        detector.SignalObserved += handler;
        try
        {
            return await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            return null;
        }
        finally
        {
            detector.SignalObserved -= handler;
        }
    }
}

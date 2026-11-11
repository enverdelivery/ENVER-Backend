using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Enver.API.IntegrationTests;

public sealed class HashingConcurrencyIntegrationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Real_work_owns_permit_until_completion_even_when_consumer_cancels_or_hasher_throws(bool cancel, bool error)
    {
        var options = Options.Create(new AuthenticationOptions
        {
            PasswordCreationMinUnicodeScalars = 12, PasswordMaxUnicodeScalars = 128, PasswordMaxUtf8Bytes = 512, HashingConcurrencyLimit = 1
        });
        using var hasher = new BlockingHasher(error);
        var gate = new HashingConcurrencyGate(options);
        var service = new PasswordService(hasher, new PasswordPolicy(options), new DummyPasswordHash(hasher), gate);
        using var cancellation = new CancellationTokenSource();
        using var budget = new RequestBudget(30000, cancellation.Token);
        using var secondBudget = new RequestBudget(30000, default);
        var user = User.Create("", "", "", DateTime.UtcNow);
        var completed = new TaskCompletionSource<PasswordHashResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try { completed.SetResult(service.Hash(user, "synthetic-password", budget)); }
            catch (Exception exception) { completed.SetException(exception); }
        });
        thread.Start();
        try
        {
            Assert.True(hasher.Entered.Wait(TimeSpan.FromSeconds(5)));
            if (cancel) cancellation.Cancel();
            Assert.Equal(PasswordHashStatus.Unavailable, service.Hash(user, "synthetic-password", secondBudget).Status);
            Assert.False(completed.Task.IsCompleted);
            Assert.Equal(1, hasher.Calls);
        }
        finally { hasher.Release.Set(); }
        if (error) await Assert.ThrowsAsync<InvalidOperationException>(() => completed.Task);
        else Assert.Equal(cancel ? PasswordHashStatus.Unavailable : PasswordHashStatus.Completed, (await completed.Task.WaitAsync(TimeSpan.FromSeconds(5))).Status);
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
        using var final = gate.TryAcquire(secondBudget); Assert.NotNull(final);
    }

    private sealed class BlockingHasher(bool error) : IPasswordHasher<User>, IDisposable
    {
        public ManualResetEventSlim Entered { get; } = new();
        public ManualResetEventSlim Release { get; } = new();
        public int Calls;
        public string HashPassword(User user, string password)
        {
            Interlocked.Increment(ref Calls); Entered.Set();
            if (!Release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
            if (error) throw new InvalidOperationException("Fallo sintético controlado.");
            return new PasswordHasher<User>().HashPassword(user, password);
        }
        public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword) => throw new NotSupportedException();
        public void Dispose() { Entered.Dispose(); Release.Dispose(); }
    }
}

using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.API.Security;
using Microsoft.Extensions.Options;

namespace Enver.API.Composition;

/// <summary>Estado interno por operación. No añade endpoint, bootstrap de autoridad ni recuperación Redis.</summary>
public sealed class AuthenticationDependencyAdmission(MongoUsersInitializer initializer, RedisSecurityStateGuard local,
    AuthenticationSecurityCoordinator global) : IAuthenticationSecurityState
{
    private int indexStatus = (int)MongoUsersInitializationStatus.Unavailable;
    public MongoUsersInitializationStatus IndexStatus => (MongoUsersInitializationStatus)Volatile.Read(ref indexStatus);

    public async ValueTask<MongoUsersInitializationStatus> RefreshIndexAsync(RequestBudget budget)
    {
        var result = await initializer.InitializeAsync(budget).ConfigureAwait(false);
        Volatile.Write(ref indexStatus, (int)result);
        return result;
    }

    // El estado compartido no consulta usuarios: Logout futuro no depende artificialmente de MongoDB.
    public async ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget budget)
    {
        var decision = await local.EvaluateAsync(budget).ConfigureAwait(false);
        if (!decision.CanContinue)
        {
            _ = await global.CloseAsync(budget).ConfigureAwait(false);
            return decision;
        }
        return await global.EvaluateAsync(budget).ConfigureAwait(false);
    }

    public async ValueTask<AuthenticationSecurityDecision> EvaluateOperationAsync(AuthenticationOperation operation, RequestBudget budget)
    {
        if (operation == AuthenticationOperation.Register &&
            await RefreshIndexAsync(budget).ConfigureAwait(false) != MongoUsersInitializationStatus.Ready)
            return new(AuthenticationSecurityAdmission.Unavailable);
        return await EvaluateAsync(budget).ConfigureAwait(false);
    }
}

internal sealed class AuthenticationInitializationService(AuthenticationDependencyAdmission admission, DummyPasswordHash dummy,
    RedisSecurityStateDetector detector, IOptions<AuthenticationOptions> options,
    ILogger<AuthenticationInitializationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = detector; // Suscripción de señales antes de abrir conexiones/admisión.
        dummy.Initialize();
        using var budget = RequestBudget.Create(AuthenticationOperation.Register, options.Value, cancellationToken);
        var status = await admission.RefreshIndexAsync(budget).ConfigureAwait(false);
        logger.Log(status == MongoUsersInitializationStatus.Ready ? LogLevel.Information : LogLevel.Warning,
            new EventId(2101, "MongoInitialization"), "MongoDB structural initialization {Status}", status);
        // Dependencia temporal no disponible mantiene liveness y admisión cerrada.
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

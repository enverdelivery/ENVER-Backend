using Enver.API.Composition;
using Enver.API.Observability;
using Microsoft.Extensions.Options;

return await SafeStartup.Run(async startup =>
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Logging.AddProvider(new SafeConsoleLoggerProvider(Console.Out));
    // Último provider: defaults/Development/env/CLI no reemplazan secretos runtime.
    // Obligatorio en contenedor; no heredar campos sensibles ni un tail de claves JWT.
    startup.Phase = StartupPhase.RuntimeConfiguration;
    builder.Configuration.AddEnverRuntimeConfiguration();

    startup.Phase = StartupPhase.Services;
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();
    builder.Services.AddAuthenticationFoundation(builder.Configuration, builder.Environment);
    builder.Services.AddAuthenticationDependencies(builder.Configuration);
    builder.Services.AddAuthenticationPipeline();

    startup.Phase = StartupPhase.HostBuild;
    await using var app = builder.Build();
    // Validar antes de resolver IHostedService/clientes, no solo en Host.StartAsync.
    app.Services.GetRequiredService<IStartupValidator>().Validate();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseAuthenticationFoundation();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Liveness: sin checks registrados de dependencias. No sustituye readiness.
    app.MapHealthChecks("/health").AllowAnonymous();

    startup.Phase = StartupPhase.HostRun;
    await app.RunAsync();
}, Console.Out);

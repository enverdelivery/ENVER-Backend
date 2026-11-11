using Enver.API.Configuration;
using Enver.API.Http;
using Amazon;
using Amazon.DynamoDBv2;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Infrastructure.Aws;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.API.Security;
using Enver.API.Observability;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enver.API.Composition;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationFoundation(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddValidatedAuthenticationOptions(configuration, environment);
        services.AddSingleton<IValidateOptions<AuthenticationTransportOptions>, AuthenticationTransportValidator>();
        services.AddOptions<AuthenticationTransportOptions>().Bind(configuration.GetSection(AuthenticationTransportOptions.SectionName)).ValidateOnStart();
        services.AddHttpsRedirection(options => options.HttpsPort =
            configuration.GetValue<int?>("AuthenticationTransport:HttpsPort") ?? 443);
        services.AddHsts(_ => { });
        ApiProblemDetails.AddSafeApiErrors(services);
        services.Configure<Microsoft.AspNetCore.Builder.ExceptionHandlerOptions>(options =>
            options.SuppressDiagnosticsCallback = _ => true); // No volcar excepciones/cuerpos sensibles a diagnósticos.
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => AuthenticationRequestLimitsMiddleware.ConfigureSerializer(options.JsonSerializerOptions));
        services.AddSingleton<IAuthenticationOriginProvider, AuthenticationOriginProvider>();
        return services;
    }

    public static IApplicationBuilder UseAuthenticationFoundation(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextMiddleware>();
        app.UseExceptionHandler();
        app.UseAuthenticationTransport();
        app.UseRouting();
        app.UseMiddleware<AuthenticationRequestLimitsMiddleware>();
        return app;
    }

    public static IServiceCollection AddAuthenticationDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<AuthenticationDependencyOptions>, AuthenticationDependencyValidator>();
        services.AddOptions<AuthenticationDependencyOptions>().Bind(configuration.GetSection(AuthenticationDependencyOptions.SectionName)).ValidateOnStart();
        services.AddSingleton<PasswordPolicy>();
        services.AddSingleton<InstitutionalEmailNormalizer>();
        services.AddSingleton<AuthenticationInputValidator>();
        services.AddOptions<PasswordHasherOptions>(); // Defaults mantenidos por el framework, sin iteraciones manuales.
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<DummyPasswordHash>();
        services.AddSingleton<HashingConcurrencyGate>();
        services.AddSingleton<IPasswordService, PasswordService>();

        services.AddSingleton(p => new MongoClientFactory(p.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddSingleton<IMongoClient>(p => p.GetRequiredService<MongoClientFactory>().Client);
        services.AddSingleton(p => p.GetRequiredService<IMongoClient>().GetDatabase(p.GetRequiredService<IOptions<MongoDbOptions>>().Value.DatabaseName));
        services.AddSingleton<IUserStore>(p => new MongoUserStore(p.GetRequiredService<IMongoDatabase>(), p.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddSingleton(p => new MongoUsersInitializer(p.GetRequiredService<IMongoDatabase>(), p.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddSingleton(p => new RedisConnections(p.GetRequiredService<IOptions<RedisOptions>>().Value));
        services.AddSingleton(p => new RedisSecurityStateGuard(p.GetRequiredService<RedisConnections>(), p.GetRequiredService<IOptions<RedisOptions>>().Value));
        services.AddSingleton<RedisSecurityStateDetector>();
        // Solo wiring del adapter YA existente. Región/credenciales externas; no crea recursos ni bootstrap.
        services.TryAddSingleton<IAmazonDynamoDB>(p => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(p.GetRequiredService<IOptions<AuthenticationDependencyOptions>>().Value.AuthorityRegion),
            MaxErrorRetry = 0
        }));
        services.AddSingleton<IAuthenticationSecurityAuthority>(p => new DynamoDbAuthenticationSecurityAuthority(p.GetRequiredService<IAmazonDynamoDB>(),
            p.GetRequiredService<IOptions<AuthenticationSecurityAuthorityOptions>>().Value));
        services.AddSingleton(p => new AuthenticationSecurityCoordinator(p.GetRequiredService<IAuthenticationSecurityAuthority>(),
            p.GetRequiredService<IOptions<JwtOptions>>().Value, p.GetRequiredService<IOptions<RedisOptions>>().Value));
        services.AddSingleton<AuthenticationDependencyAdmission>();
        services.AddSingleton<IAuthenticationSecurityState>(p => p.GetRequiredService<AuthenticationDependencyAdmission>());
        services.AddSingleton<IAuthenticationScopePseudonymizer, HmacAuthenticationScopePseudonymizer>();
        services.AddSingleton(p => p.GetRequiredService<IOptions<AuthenticationDependencyOptions>>().Value.Capture(
            p.GetRequiredService<IHostEnvironment>().EnvironmentName, p.GetRequiredService<IOptions<RateLimitingOptions>>().Value));
        services.AddSingleton<IAuthenticationQuotaStore>(p => new RedisAuthenticationQuotaStore(p.GetRequiredService<RedisConnections>(),
            p.GetRequiredService<IOptions<RedisOptions>>().Value, p.GetRequiredService<RedisAuthenticationQuotaConfiguration>(),
            p.GetRequiredService<RedisSecurityStateGuard>(), p.GetRequiredService<AuthenticationDependencyAdmission>()));
        services.AddHostedService<AuthenticationInitializationService>();
        services.AddSingleton<IRegisterAdmission, RegisterAdmission>();
        services.AddSingleton<IRegisterQuota, RegisterQuota>();
        services.AddSingleton<IAuthenticationEvents, AuthenticationEvents>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<IAuthenticationBoundaryContinuation, RegisterBoundaryContinuation>();
        services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options => options.Conventions.Add(new RegisterEndpointConvention()));
        return services;
    }
}

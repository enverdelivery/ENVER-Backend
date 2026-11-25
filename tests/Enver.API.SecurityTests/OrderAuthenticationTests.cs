using Enver.API.Controllers;
using Enver.API.Features.Authentication.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Fact]
    public async Task Orders_routes_require_normal_Bearer_and_reject_revocation_before_action()
    {
        using var host = new Harness(services => services.AddControllers().AddApplicationPart(typeof(OrdersController).Assembly));
        var id = Guid.NewGuid();
        var revoked = host.Issuer.Issue(id).AccessToken!;
        host.Store.Status = TokenRevocationReadStatus.Revoked;
        foreach (var (method, route) in new[] { (HttpMethod.Get, ""), (HttpMethod.Get, "/mine"), (HttpMethod.Get, $"/{id:D}"),
            (HttpMethod.Post, ""), (HttpMethod.Post, $"/{id:D}/cancel") })
        foreach (var token in new[] { null, "invalid", revoked })
        {
            using var response = await host.SendAsync(token, "/api/v1/orders" + route, method);
            await host.AssertProblemAsync(response, 401, token ?? "");
        }
    }

    [Fact]
    public async Task Delivery_routes_deny_missing_invalid_revoked_and_unverifiable_Bearer_before_action()
    {
        // Sin servicios Orders: estas solicitudes deben rechazarse antes de crear el controlador.
        using var host = new Harness(services => services.AddControllers().AddApplicationPart(typeof(OrdersController).Assembly));
        var id = Guid.NewGuid();
        var token = host.Issuer.Issue(id).AccessToken!;
        var routes = new[] { (HttpMethod.Get, "/assigned-to-me"), (HttpMethod.Post, $"/{id:D}/accept"),
            (HttpMethod.Post, $"/{id:D}/start-delivery"), (HttpMethod.Post, $"/{id:D}/arrive"),
            (HttpMethod.Post, $"/{id:D}/request-completion"), (HttpMethod.Post, $"/{id:D}/confirm-completion") };
        foreach (var (method, route) in routes)
        {
            var path = "/api/v1/orders" + route;
            host.Store.Status = TokenRevocationReadStatus.Revoked;
            foreach (var denied in new[] { null, "invalid", token })
            {
                using var response = await host.SendAsync(denied, path, method);
                await host.AssertProblemAsync(response, 401, denied ?? "");
            }
            host.Store.Status = TokenRevocationReadStatus.Unavailable;
            using var unavailable = await host.SendAsync(token, path, method);
            await host.AssertProblemAsync(unavailable, 503, token);
        }
        Assert.Equal(0, host.Store.Writes);
    }
}

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
}

using Enver.API.Controllers;
using Enver.API.Features.Authentication.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Fact]
    public async Task Rating_route_requires_normal_Bearer_and_confirmed_revocation_lookup()
    {
        // No Ratings services: authorization must reject before creating the controller.
        using var host = new Harness(services => services.AddControllers().AddApplicationPart(typeof(RatingsController).Assembly));
        var path = $"/api/v1/orders/{Guid.NewGuid():D}/rating";
        var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        host.Store.Status = TokenRevocationReadStatus.Revoked;
        foreach (var denied in new[] { null, "invalid", token })
        {
            using var response = await host.SendAsync(denied, path, HttpMethod.Post);
            await host.AssertProblemAsync(response, 401, denied ?? "");
        }
        host.Store.Status = TokenRevocationReadStatus.Unavailable;
        using var unavailable = await host.SendAsync(token, path, HttpMethod.Post);
        await host.AssertProblemAsync(unavailable, 503, token);
        Assert.Equal(0, host.Store.Writes);
    }
}

using burtonrodman.AspNetCore.DynamicRouting;
using Microsoft.AspNetCore.Routing.Patterns;

namespace SampleWeb;

public class DynamicRedirectsUpdateServiceOptions : DynamicRoutesUpdateServiceOptions;
public class DynamicRedirectsUpdater(
    [FromKeyedServices("redirects")] MutableEndpointDataSource endpointDataSource
) : IDynamicRoutesUpdater
{
    public async Task UpdateRoutes()
    {
        // BEWARE ambiguous route matches
        // if you have lots of routes, consider reusing existing routes
        // by making a copy of endpointDataSource.Endpoints (maybe to a dictionary),
        // and replace only values that have changed

        var seconds = DateTime.UtcNow.Second;
        Endpoint[] endpoints = [
            CreateRedirectEndpoint($"/old/page{seconds}", "/new/page", true),
            CreateRedirectEndpoint($"/this/page{seconds}", "/that/page", true)
        ];

        endpointDataSource.SetEndpoints(endpoints);
    }

    private Endpoint CreateRedirectEndpoint(string pattern, string location, bool permanent)
    {
        return new RouteEndpointBuilder(
            async (HttpContext context) => context.Response.Redirect(location, permanent),
            routePattern: RoutePatternFactory.Parse(pattern), order: 0)
            .Build();
    }
}

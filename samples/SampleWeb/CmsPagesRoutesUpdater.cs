using burtonrodman.AspNetCore.DynamicRouting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing.Patterns;

namespace SampleWeb;

public class CmsPageRoutesUpdateServiceOptions : DynamicRoutesUpdateServiceOptions;
public class CmsPageRoutesUpdater(
    [FromKeyedServices("pages")] MutableEndpointDataSource endpointDataSource
) : IDynamicRoutesUpdater
{
    public async Task UpdateRoutes()
    {
        // BEWARE ambiguous route matches

        var seconds = DateTime.UtcNow.Second;
        Endpoint[] endpoints = [
            CreateCmsPageEndpoint($"/home{seconds}"),
            CreateCmsPageEndpoint($"/away{seconds}")
        ];

        endpointDataSource.SetEndpoints(endpoints);
    }

    private Endpoint CreateCmsPageEndpoint(string pattern)
    {
        return new RouteEndpointBuilder(
            async (HttpContext context) => new RazorComponentResult<CmsPage>(context.GetRouteData().Values),
            routePattern: RoutePatternFactory.Parse(pattern), order: 0)
            .Build();
    }
}

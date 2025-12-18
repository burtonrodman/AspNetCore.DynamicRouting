# AspNetCore.DynamicRouting
Services to implement dynamically refreshable routes in an ASP.Net web app.

# Getting Started

## Add NuGet Packages
- `burtonrodman.AspNetCore.DynamicRouting`

## Implement at least one IDynamicRoutesUpdater
1. Create a class that implements the `IDynamicRoutesUpdater` interface, execute your logic to build a new array of EndPoints, and call SetEndpoints.  If you will have multiple updaters (for example, if you have different sets of routes that you want to update on different schedules), make a sub-class of `DynamicRoutesUpdateServiceOptions` to keep your options separated.
```
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
        // BEWARE ambiguous route matches - make sure you consider all endpoints in 
        // your application to avoid ambiguous matches -- if you fail to do so your app will crash at runtime.

        // PERFORMANCE (memory) - if you have lots of routes, consider reusing existing routes
        // by making a copy of endpointDataSource.Endpoints (maybe to a dictionary),
        // and replace only values that have changed.  Endpoints are immutable, so you must always replace in order to "modify".

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
```

2. Register the services/configuration in the DI container.  Example:
```
using burtonrodman.AspNetCore.DynamicRouting;

...
builder.Services.AddDynamicRouting<DynamicRedirectsUpdater, DynamicRedirectsUpdateServiceOptions>(
    "redirects", options => options.UpdateIntervalSeconds = "5");
```
3. Map your dynamic routes:
```
app.MapDynamicRoutes("redirects");
```

# How Does It Work?
At bottom, ASP.Net routes are based on an collection of EndpointDataSources.  The default Route Mapping extensions hide this fact very well.

An EndpointDataSource is responsible for maintaining a read only list of Endpoints, but may optionally signal a change via a ChangeToken.  Microsoft's default implementation of EndpointDataSource does not support updates.  I have borrowed an implementation of a MutableEndpointDataSource that not only provides the change notification, but also handles the thread-safe swap.

In addition, I have provided a BackgroundService that makes scheduling your updates easy, and extensions methods that makes setup super simple.

The background service schedule is configured via DynamicRoutingUpdateServiceOptions using the Options Pattern.  If you required multiple independent schedules you must make a sub-class of DynamicRoutingUpdateServiceOptions.

# Sources
MutableEndpointDataSource from: [FIND ARTICLE AGAIN]

Thanks also to [PERSON] on this GitHub Issue, for pointing me in the right direction.

# Contributing
I welcome Pull Requests for any improvement or bug fixes.  Please open an Issue for discussion if you plan on adding any features, so that we can collaborate on design.  For bug reports, a Pull Request with a failing unit test is ideal.

Thanks!

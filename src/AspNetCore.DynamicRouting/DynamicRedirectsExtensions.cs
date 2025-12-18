using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace burtonrodman.AspNetCore.DynamicRouting;

public static class DynamicRoutingExtensions
{
    public static IServiceCollection AddDynamicRouting<TUpdater, TOptions>(
        this IServiceCollection services,
        string serviceKey,
        Action<TOptions> configAction
    )
        where TUpdater : class, IDynamicRoutesUpdater
        where TOptions : DynamicRoutesUpdateServiceOptions
    {
        services.AddKeyedSingleton<MutableEndpointDataSource>(serviceKey);
        services.AddSingleton<DynamicRoutesUpdateService<TUpdater, TOptions>>();
        services.AddHostedService<DynamicRoutesUpdateService<TUpdater, TOptions>>();
        services.Configure(configAction);

        services.AddScoped<TUpdater>();

        return services;
    }

    public static IServiceCollection AddDynamicRouting<TUpdater>(
        this IServiceCollection services,
        string serviceKey,
        Action<DynamicRoutesUpdateServiceOptions> configAction
    )
        where TUpdater : class, IDynamicRoutesUpdater
    {
        return AddDynamicRouting<TUpdater, DynamicRoutesUpdateServiceOptions>(services, serviceKey, configAction);
    }
    
    public static void MapDynamicRoutes(this IEndpointRouteBuilder endpoints, string serviceKey)
    {
        var dataSource = endpoints.ServiceProvider.GetRequiredKeyedService<MutableEndpointDataSource>(serviceKey);
        endpoints.DataSources.Add(dataSource);
    }

}

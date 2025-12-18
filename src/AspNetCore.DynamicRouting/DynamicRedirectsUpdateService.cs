using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace burtonrodman.AspNetCore.DynamicRouting;

public class DynamicRoutesUpdateServiceOptions
{
    public string UpdateIntervalSeconds { get; set; } = "300";
}

public class DynamicRoutesUpdateService<TUpdater, TOptions>(
    ILogger<DynamicRoutes> logger,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<TOptions> options
) : BackgroundService
    where TUpdater : notnull, IDynamicRoutesUpdater
    where TOptions : DynamicRoutesUpdateServiceOptions
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // update routes on startup
        await UpdateRoutes();

        var updateIntervalSeconds = int.TryParse(options.Value.UpdateIntervalSeconds, out var seconds) ? seconds : 300;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(updateIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested &&
          await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UpdateRoutes();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "exception while updating proxy config");
            }
        }
    }

    private async Task UpdateRoutes()
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var updater = scope.ServiceProvider.GetRequiredService<TUpdater>();

        await updater.UpdateRoutes();
    }

}

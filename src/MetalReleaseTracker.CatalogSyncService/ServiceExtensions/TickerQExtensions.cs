using MetalReleaseTracker.CatalogSyncService.Data;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;

namespace MetalReleaseTracker.CatalogSyncService.ServiceExtensions;

public static class TickerQExtensions
{
    public static IServiceCollection AddTickerQScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TickerQConnectionString")
            ?? throw new InvalidOperationException("TickerQConnectionString is not configured");

        var dashboardEnabled = configuration.GetValue<bool>("TickerQ:Dashboard:Enabled", true);
        var dashboardBasePath = configuration.GetValue<string>("TickerQ:Dashboard:BasePath");
        var dashboardUsername = configuration.GetValue<string>("TickerQ:Dashboard:Username");
        var dashboardPassword = configuration.GetValue<string>("TickerQ:Dashboard:Password");

        services.AddTickerQ<CustomTimeTicker, CustomCronTicker>(tickerOptions =>
        {
            tickerOptions.ConfigureScheduler(schedulerOptions =>
            {
                schedulerOptions.MaxConcurrency = 10;
                schedulerOptions.NodeIdentifier = "catalogsync-server-01";
                schedulerOptions.IdleWorkerTimeOut = TimeSpan.FromMinutes(2);
                schedulerOptions.FallbackIntervalChecker = TimeSpan.FromSeconds(30);
                schedulerOptions.SchedulerTimeZone = TimeZoneInfo.Utc;
            });

            if (dashboardEnabled)
            {
                tickerOptions.AddDashboard(dashboard =>
                {
                    if (!string.IsNullOrEmpty(dashboardBasePath))
                    {
                        dashboard.SetBasePath(dashboardBasePath);
                    }

                    if (!string.IsNullOrEmpty(dashboardUsername) && !string.IsNullOrEmpty(dashboardPassword))
                    {
                        dashboard.WithBasicAuth(dashboardUsername, dashboardPassword);
                    }
                });
            }

            tickerOptions.AddOperationalStore(storeOptions =>
            {
                storeOptions.UseTickerQDbContext<CatalogSyncTickerQDbContext>(dbContextOptions =>
                {
                    dbContextOptions.UseNpgsql(connectionString, cfg =>
                    {
                        cfg.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), ["40P01"]);
                    });
                });
            });
        });

        return services;
    }
}

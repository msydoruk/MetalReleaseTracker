using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.DbContextFactory;

namespace MetalReleaseTracker.CatalogSyncService.Data;

public class CatalogSyncTickerQDbContext : TickerQDbContext<CustomTimeTicker, CustomCronTicker>
{
    public CatalogSyncTickerQDbContext(DbContextOptions<CatalogSyncTickerQDbContext> options)
        : base(options)
    {
    }
}

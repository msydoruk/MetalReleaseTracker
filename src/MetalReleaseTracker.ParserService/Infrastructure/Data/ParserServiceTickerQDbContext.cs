using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.DbContextFactory;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data;

public class ParserServiceTickerQDbContext : TickerQDbContext<CustomTimeTicker, CustomCronTicker>
{
    public ParserServiceTickerQDbContext(DbContextOptions<ParserServiceTickerQDbContext> options)
        : base(options)
    {
    }
}

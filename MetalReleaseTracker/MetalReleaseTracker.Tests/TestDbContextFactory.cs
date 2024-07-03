using MetalReleaseTracker.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests
{
    public static class TestDbContextFactory
    {
        public static MetalReleaseTrackerDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<MetalReleaseTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new MetalReleaseTrackerDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}

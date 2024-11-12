using MetalReleaseTracker.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests
{
    public static class TestDbContextFactory
    {
        public static MetalReleaseTrackerDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<MetalReleaseTrackerDbContext>()
                .UseNpgsql("Host=localhost;Port=5433;Database=MetalReleaseTrackerTestDb;Username=testuser;Password=pswrd_fr_tsdb_1")
                .Options;

            var context = new MetalReleaseTrackerDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        public static void ClearDatabase(MetalReleaseTrackerDbContext context)
        {
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Albums\", \"Bands\", \"Distributors\" RESTART IDENTITY CASCADE;");
        }
    }
}

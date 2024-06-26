using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Data
{
    public class MetalReleaseTrackerDbContext : DbContext
    {
        public MetalReleaseTrackerDbContext(DbContextOptions<MetalReleaseTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<AlbumEntity> Albums { get; set; }

        public DbSet<BandEntity> Bands { get; set; }

        public DbSet<DistributorEntity> Distributors { get; set; }

        public DbSet<SubscriptionEntity> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.Media)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}

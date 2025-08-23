namespace MetalReleaseTracker.CoreDataService.Data;

using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.EntityFrameworkCore;

public class CoreDataServiceDbContext : DbContext
{
    public CoreDataServiceDbContext(DbContextOptions<CoreDataServiceDbContext> options) : base(options)
    {
    }

    public DbSet<AlbumEntity> Albums { get; set; }

    public DbSet<BandEntity> Bands { get; set; }

    public DbSet<DistributorEntity> Distributors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlbumEntity>()
            .Property(album => album.Media)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<AlbumEntity>()
            .Property(album => album.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<AlbumEntity>()
            .HasIndex(album => album.SKU)
            .IsUnique();
    }
}
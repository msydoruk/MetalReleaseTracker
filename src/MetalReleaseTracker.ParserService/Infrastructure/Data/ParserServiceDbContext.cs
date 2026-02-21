using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data;

public class ParserServiceDbContext : DbContext
{
    public DbSet<AlbumParsedEventEntity> AlbumParsedEvents { get; set; }

    public DbSet<ParsingSessionEntity> ParsingSessions { get; set; }

    public DbSet<BandReferenceEntity> BandReferences { get; set; }

    public DbSet<CatalogueIndexEntity> CatalogueIndex { get; set; }

    public DbSet<BandDiscographyEntity> BandDiscography { get; set; }

    public DbSet<AiVerificationEntity> AiVerifications { get; set; }

    public ParserServiceDbContext(DbContextOptions<ParserServiceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AlbumParsedEventEntity>();
        modelBuilder.Entity<ParsingSessionEntity>();

        modelBuilder.ApplyConfiguration(new BandReferenceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BandDiscographyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogueIndexEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AiVerificationEntityConfiguration());
    }
}
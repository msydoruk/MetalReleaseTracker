using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data;

public class ParserServiceDbContext : DbContext
{
    public DbSet<BandReferenceEntity> BandReferences { get; set; }

    public DbSet<CatalogueIndexEntity> CatalogueIndex { get; set; }

    public DbSet<CatalogueIndexDetailEntity> CatalogueIndexDetails { get; set; }

    public DbSet<BandDiscographyEntity> BandDiscography { get; set; }

    public DbSet<AiVerificationEntity> AiVerifications { get; set; }

    public DbSet<AiAgentEntity> AiAgents { get; set; }

    public DbSet<ParsingSourceEntity> ParsingSources { get; set; }

    public DbSet<SettingEntity> Settings { get; set; }

    public ParserServiceDbContext(DbContextOptions<ParserServiceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new BandReferenceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BandDiscographyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogueIndexEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogueIndexDetailEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AiVerificationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AiAgentEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ParsingSourceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SettingEntityConfiguration());
    }
}

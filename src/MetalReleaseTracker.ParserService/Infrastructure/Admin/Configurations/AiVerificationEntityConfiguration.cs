using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;

public class AiVerificationEntityConfiguration : IEntityTypeConfiguration<AiVerificationEntity>
{
    public void Configure(EntityTypeBuilder<AiVerificationEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BandName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.AlbumTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.AiAnalysis)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasOne(e => e.CatalogueIndex)
            .WithMany()
            .HasForeignKey(e => e.CatalogueIndexId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CatalogueIndexId, e.CreatedAt });
    }
}

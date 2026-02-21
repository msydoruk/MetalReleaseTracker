using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class CatalogueIndexEntityConfiguration : IEntityTypeConfiguration<CatalogueIndexEntity>
{
    public void Configure(EntityTypeBuilder<CatalogueIndexEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DistributorCode)
            .IsRequired();

        builder.Property(e => e.BandName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.AlbumTitle)
            .HasMaxLength(500);

        builder.Property(e => e.RawTitle)
            .HasMaxLength(1000);

        builder.Property(e => e.DetailUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasOne(e => e.BandReference)
            .WithMany()
            .HasForeignKey(e => e.BandReferenceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.DetailUrl, e.DistributorCode })
            .IsUnique();

        builder.HasIndex(e => new { e.DistributorCode, e.Status });

        builder.HasIndex(e => e.BandReferenceId);
    }
}

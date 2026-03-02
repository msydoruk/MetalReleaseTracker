using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class CatalogueIndexDetailEntityConfiguration : IEntityTypeConfiguration<CatalogueIndexDetailEntity>
{
    public void Configure(EntityTypeBuilder<CatalogueIndexDetailEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.CatalogueIndex)
            .WithOne(e => e.Detail)
            .HasForeignKey<CatalogueIndexDetailEntity>(e => e.CatalogueIndexId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CatalogueIndexId)
            .IsUnique();

        builder.Property(e => e.BandName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.SKU)
            .HasMaxLength(200);

        builder.Property(e => e.Name)
            .HasMaxLength(500);

        builder.Property(e => e.Genre)
            .HasMaxLength(500);

        builder.Property(e => e.PurchaseUrl)
            .HasMaxLength(2000);

        builder.Property(e => e.PhotoUrl)
            .HasMaxLength(2000);

        builder.Property(e => e.Label)
            .HasMaxLength(500);

        builder.Property(e => e.Press)
            .HasMaxLength(500);

        builder.Property(e => e.CanonicalTitle)
            .HasMaxLength(500);

        builder.Property(e => e.ChangeType)
            .IsRequired();

        builder.Property(e => e.PublicationStatus)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        builder.HasIndex(e => new { e.ChangeType, e.PublicationStatus });
    }
}

using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class BandDiscographyEntityConfiguration : IEntityTypeConfiguration<BandDiscographyEntity>
{
    public void Configure(EntityTypeBuilder<BandDiscographyEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BandReferenceId)
            .IsRequired();

        builder.Property(e => e.AlbumTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.NormalizedAlbumTitle)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.AlbumType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(e => e.BandReference)
            .WithMany(b => b.Discography)
            .HasForeignKey(e => e.BandReferenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.BandReferenceId);

        builder.HasIndex(e => new { e.BandReferenceId, e.NormalizedAlbumTitle })
            .IsUnique();
    }
}

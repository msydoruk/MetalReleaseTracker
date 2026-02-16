using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class BandReferenceEntityConfiguration : IEntityTypeConfiguration<BandReferenceEntity>
{
    public void Configure(EntityTypeBuilder<BandReferenceEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BandName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.MetalArchivesId)
            .IsRequired();

        builder.Property(e => e.Genre)
            .HasMaxLength(500);

        builder.Property(e => e.LastSyncedAt)
            .IsRequired();

        builder.HasIndex(e => e.MetalArchivesId)
            .IsUnique();

        builder.HasIndex(e => e.BandName);
    }
}

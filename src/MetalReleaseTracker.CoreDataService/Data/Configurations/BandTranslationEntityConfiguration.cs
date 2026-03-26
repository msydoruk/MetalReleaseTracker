using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Data.Configurations;

public class BandTranslationEntityConfiguration : IEntityTypeConfiguration<BandTranslationEntity>
{
    public void Configure(EntityTypeBuilder<BandTranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.LanguageCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.HasIndex(entity => new { entity.BandId, entity.LanguageCode })
            .IsUnique();

        builder.HasOne(entity => entity.Band)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(entity => entity.BandId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Language)
            .WithMany()
            .HasForeignKey(entity => entity.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Data.Configurations;

public class DistributorTranslationEntityConfiguration : IEntityTypeConfiguration<DistributorTranslationEntity>
{
    public void Configure(EntityTypeBuilder<DistributorTranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.LanguageCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(entity => entity.Description)
            .HasMaxLength(1000);

        builder.HasIndex(entity => new { entity.DistributorId, entity.LanguageCode })
            .IsUnique();

        builder.HasOne(entity => entity.Distributor)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(entity => entity.DistributorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Language)
            .WithMany()
            .HasForeignKey(entity => entity.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

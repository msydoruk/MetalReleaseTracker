using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class NavigationItemTranslationEntityConfiguration : IEntityTypeConfiguration<NavigationItemTranslationEntity>
{
    public void Configure(EntityTypeBuilder<NavigationItemTranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.LanguageCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(entity => entity.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(entity => new { entity.NavigationItemId, entity.LanguageCode })
            .IsUnique();

        builder.HasOne(entity => entity.NavigationItem)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(entity => entity.NavigationItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Language)
            .WithMany()
            .HasForeignKey(entity => entity.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

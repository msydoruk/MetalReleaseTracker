using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class TranslationEntityConfiguration : IEntityTypeConfiguration<TranslationEntity>
{
    public void Configure(EntityTypeBuilder<TranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Key)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(entity => entity.Language)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(entity => entity.Value)
            .IsRequired();

        builder.Property(entity => entity.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasIndex(entity => new { entity.Key, entity.Language })
            .IsUnique();

        builder.HasIndex(entity => entity.Category);
    }
}

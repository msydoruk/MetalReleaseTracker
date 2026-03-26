using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class LanguageEntityConfiguration : IEntityTypeConfiguration<LanguageEntity>
{
    public void Configure(EntityTypeBuilder<LanguageEntity> builder)
    {
        builder.HasKey(entity => entity.Code);

        builder.Property(entity => entity.Code)
            .HasMaxLength(5);

        builder.Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.NativeName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.SortOrder);

        builder.HasData(
            new LanguageEntity
            {
                Code = "en",
                Name = "English",
                NativeName = "English",
                SortOrder = 1,
                IsEnabled = true,
                IsDefault = true,
                CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc),
            },
            new LanguageEntity
            {
                Code = "ua",
                Name = "Ukrainian",
                NativeName = "Українська",
                SortOrder = 2,
                IsEnabled = true,
                IsDefault = false,
                CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc),
            });
    }
}

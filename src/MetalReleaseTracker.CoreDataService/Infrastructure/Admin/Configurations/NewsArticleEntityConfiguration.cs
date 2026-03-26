using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class NewsArticleEntityConfiguration : IEntityTypeConfiguration<NewsArticleEntity>
{
    public void Configure(EntityTypeBuilder<NewsArticleEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.ChipLabel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(entity => entity.ChipColor)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(entity => entity.IconName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(entity => entity.Date);

        builder.HasIndex(entity => entity.IsPublished);
    }
}

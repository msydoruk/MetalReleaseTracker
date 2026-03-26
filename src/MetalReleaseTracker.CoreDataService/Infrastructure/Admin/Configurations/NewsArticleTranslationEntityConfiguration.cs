using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class NewsArticleTranslationEntityConfiguration : IEntityTypeConfiguration<NewsArticleTranslationEntity>
{
    public void Configure(EntityTypeBuilder<NewsArticleTranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.LanguageCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(entity => entity.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(entity => entity.Content)
            .IsRequired();

        builder.HasIndex(entity => new { entity.NewsArticleId, entity.LanguageCode })
            .IsUnique();

        builder.HasOne(entity => entity.NewsArticle)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(entity => entity.NewsArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Language)
            .WithMany()
            .HasForeignKey(entity => entity.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

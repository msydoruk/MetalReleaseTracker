using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Data.Configurations;

public class AlbumTranslationEntityConfiguration : IEntityTypeConfiguration<AlbumTranslationEntity>
{
    public void Configure(EntityTypeBuilder<AlbumTranslationEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.LanguageCode)
            .IsRequired()
            .HasMaxLength(5);

        builder.HasIndex(entity => new { entity.AlbumId, entity.LanguageCode })
            .IsUnique();

        builder.HasOne(entity => entity.Album)
            .WithMany(entity => entity.Translations)
            .HasForeignKey(entity => entity.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Language)
            .WithMany()
            .HasForeignKey(entity => entity.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

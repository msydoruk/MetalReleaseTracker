using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;

public class SettingEntityConfiguration : IEntityTypeConfiguration<SettingEntity>
{
    public void Configure(EntityTypeBuilder<SettingEntity> builder)
    {
        builder.HasKey(entity => entity.Key);

        builder.Property(entity => entity.Key)
            .HasMaxLength(200);

        builder.Property(entity => entity.Value)
            .IsRequired();

        builder.Property(entity => entity.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.Category);
    }
}

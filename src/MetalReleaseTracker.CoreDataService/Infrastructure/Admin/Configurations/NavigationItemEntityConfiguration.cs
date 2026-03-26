using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class NavigationItemEntityConfiguration : IEntityTypeConfiguration<NavigationItemEntity>
{
    public void Configure(EntityTypeBuilder<NavigationItemEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Path)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entity => entity.IconName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(entity => entity.SortOrder);
    }
}

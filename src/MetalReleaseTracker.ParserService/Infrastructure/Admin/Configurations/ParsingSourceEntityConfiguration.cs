using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;

public class ParsingSourceEntityConfiguration : IEntityTypeConfiguration<ParsingSourceEntity>
{
    public void Configure(EntityTypeBuilder<ParsingSourceEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entity => entity.ParsingUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.DistributorCode)
            .IsUnique();
    }
}

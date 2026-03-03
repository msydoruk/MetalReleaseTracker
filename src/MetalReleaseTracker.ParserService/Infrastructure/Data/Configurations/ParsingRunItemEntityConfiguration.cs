using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class ParsingRunItemEntityConfiguration : IEntityTypeConfiguration<ParsingRunItemEntity>
{
    public void Configure(EntityTypeBuilder<ParsingRunItemEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ItemDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ProcessedAt)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.ParsingRunId);
    }
}

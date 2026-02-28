using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configurations;

public class AiAgentEntityConfiguration : IEntityTypeConfiguration<AiAgentEntity>
{
    public void Configure(EntityTypeBuilder<AiAgentEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entity => entity.Description)
            .HasMaxLength(1000);

        builder.Property(entity => entity.SystemPrompt)
            .IsRequired();

        builder.Property(entity => entity.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(entity => entity.ApiKey)
            .IsRequired();

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.IsActive);
    }
}

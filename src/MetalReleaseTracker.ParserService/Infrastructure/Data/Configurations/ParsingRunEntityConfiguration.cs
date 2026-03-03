using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Configurations;

public class ParsingRunEntityConfiguration : IEntityTypeConfiguration<ParsingRunEntity>
{
    public void Configure(EntityTypeBuilder<ParsingRunEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.JobType)
            .IsRequired();

        builder.Property(e => e.DistributorCode)
            .IsRequired(false);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.StartedAt)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.CountersJson)
            .HasColumnType("jsonb");

        builder.HasMany(e => e.Items)
            .WithOne(i => i.ParsingRun)
            .HasForeignKey(i => i.ParsingRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.StartedAt);
        builder.HasIndex(e => e.Status);
    }
}

using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class CurrencyRateEntityConfiguration : IEntityTypeConfiguration<CurrencyRateEntity>
{
    public void Configure(EntityTypeBuilder<CurrencyRateEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Code)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(entity => entity.Symbol)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(entity => entity.RateToEur)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();

        builder.HasIndex(entity => entity.Code)
            .IsUnique();
    }
}

using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Data.Configurations;

public class EmailSubscriptionEntityConfiguration : IEntityTypeConfiguration<EmailSubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<EmailSubscriptionEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.HasIndex(entity => entity.UserId)
            .IsUnique();

        builder.HasIndex(entity => entity.Email);

        builder.HasIndex(entity => entity.VerificationToken);
    }
}

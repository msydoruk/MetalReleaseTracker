using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;

public class AuditLogEntityConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.HasIndex(entity => entity.Timestamp);
        builder.HasIndex(entity => entity.UserId);
        builder.HasIndex(entity => entity.Action);
    }
}

using MetalReleaseTracker.CoreDataService.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data;

public class IdentityServerDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }

    public IdentityServerDbContext(DbContextOptions<IdentityServerDbContext> options)
        : base(options)
    {
    }
}
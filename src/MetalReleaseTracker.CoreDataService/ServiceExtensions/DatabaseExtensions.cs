using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Seeders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddApplicationDatabases(this IServiceCollection services,
        IConfiguration configuration)
    {
        var coreDataConnectionString = configuration.GetConnectionString("CoreDataServiceConnectionString");

        services.AddDbContext<CoreDataServiceDbContext>(options =>
            options.UseNpgsql(coreDataConnectionString));

        services.AddDbContext<IdentityServerDbContext>(options =>
            options.UseNpgsql(coreDataConnectionString));

        return services;
    }

    public static WebApplication ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CoreDataServiceDbContext>();
            dbContext.Database.Migrate();

            var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>();
            identityDbContext.Database.Migrate();

            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SlugDataSeeder>>();
            var seeder = new SlugDataSeeder(dbContext, logger);
            seeder.SeedSlugsAsync().GetAwaiter().GetResult();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error applying migrations");
        }

        return app;
    }
}

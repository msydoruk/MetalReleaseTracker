using Microsoft.AspNetCore.Cors.Infrastructure;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class CorsExtensions
{
    public static IServiceCollection AddApplicationCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", ConfigureAllowAllPolicy);
            options.AddPolicy("AllowSPA", ConfigureSpaPolicy);
        });

        return services;
    }

    private static void ConfigureAllowAllPolicy(CorsPolicyBuilder policy)
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    }

    private static void ConfigureSpaPolicy(CorsPolicyBuilder policy)
    {
        policy.WithOrigins("https://localhost:3000", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    }
}
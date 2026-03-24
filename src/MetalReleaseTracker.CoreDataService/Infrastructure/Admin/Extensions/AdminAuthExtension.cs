using System.Text;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Extensions;

public static class AdminAuthExtension
{
    public const string AdminBearerScheme = "AdminBearer";
    public const string AdminPolicy = "AdminPolicy";

    public static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("AdminAuth");
        services.Configure<AdminAuthSettings>(section);

        var settings = section.Get<AdminAuthSettings>()!;

        services.AddAuthentication()
            .AddJwtBearer(AdminBearerScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.JwtIssuer,
                    ValidAudience = settings.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JwtKey)),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, policy =>
            {
                policy.AuthenticationSchemes.Add(AdminBearerScheme);
                policy.RequireRole("Admin");
            });

        return services;
    }
}

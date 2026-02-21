using System.Text;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;

public static class AdminAuthExtension
{
    public static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("AdminAuth");
        services.Configure<AdminAuthSettings>(section);

        var settings = section.Get<AdminAuthSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
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

        services.AddAuthorization();

        return services;
    }
}

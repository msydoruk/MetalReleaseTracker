using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly AdminAuthSettings _settings;

    public AdminAuthService(IOptions<AdminAuthSettings> settings)
    {
        _settings = settings.Value;
    }

    public LoginResponseDto? Login(string username, string password)
    {
        if (!string.Equals(username, _settings.Username, StringComparison.Ordinal) ||
            !string.Equals(password, _settings.Password, StringComparison.Ordinal))
        {
            return null;
        }

        var expiration = DateTime.UtcNow.AddMinutes(_settings.TokenExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "Admin"),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.JwtIssuer,
            audience: _settings.JwtAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return new LoginResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}

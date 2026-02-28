using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;

public interface IAdminAuthService
{
    LoginResponseDto? Login(string username, string password);
}

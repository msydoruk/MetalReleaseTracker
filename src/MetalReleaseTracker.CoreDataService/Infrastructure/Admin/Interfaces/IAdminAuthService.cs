using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;

public interface IAdminAuthService
{
    LoginResponseDto? Login(string username, string password);
}

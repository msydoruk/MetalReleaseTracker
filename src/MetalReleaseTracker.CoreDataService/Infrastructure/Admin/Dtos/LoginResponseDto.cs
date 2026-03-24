namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;

public record LoginResponseDto(string Token, DateTime Expiration);

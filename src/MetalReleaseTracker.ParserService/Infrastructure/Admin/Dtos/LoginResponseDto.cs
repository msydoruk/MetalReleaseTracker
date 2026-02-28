namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record LoginResponseDto(string Token, DateTime Expiration);

using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResultDto> LoginWithEmailAsync(LoginRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(requestDto.Email);
        if (user == null)
        {
            _logger.LogWarning("User with email {Email} not found", requestDto.Email);
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, requestDto.Password, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully", requestDto.Email);
            return await CreateSuccessResult(user, "Login successful", cancellationToken);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account locked out", requestDto.Email);
            return new AuthResultDto
            {
                Success = false,
                Message = "User account locked out."
            };
        }

        _logger.LogWarning("Invalid login attempt for user {Email}", requestDto.Email);
        return new AuthResultDto
        {
            Success = false,
            Message = "Invalid email or password."
        };
    }

    public async Task<AuthResultDto> RegisterUserAsync(RegisterRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User with email {Email} already exists", requestDto.Email);
            return new AuthResultDto
            {
                Success = false,
                Message = "User with this email already exists."
            };
        }

        var userName = !string.IsNullOrEmpty(requestDto.UserName) ? requestDto.UserName : requestDto.Email;
        var user = new IdentityUser
        {
            UserName = userName,
            Email = requestDto.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, requestDto.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} created a new account", requestDto.Email);
            return await CreateSuccessResult(user, "Registration successful", cancellationToken);
        }

        _logger.LogWarning("Error creating user: {Errors}",
            string.Join(", ", result.Errors.Select(e => e.Description)));

        return new AuthResultDto
        {
            Success = false,
            Message = string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken = default)
    {
        var isValid = await _jwtService.ValidateRefreshTokenAsync(refreshToken, userId, cancellationToken);
        if (!isValid)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "Invalid refresh token"
            };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        await _jwtService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        return await CreateSuccessResult(user, "Token refreshed successfully", cancellationToken);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _jwtService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User logged out");
        return Task.CompletedTask;
    }

    public async Task<AuthResultDto> LoginWithGoogleAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Login with Google is not implemented yet");

        var authResult = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!authResult.Succeeded)
        {
            _logger.LogWarning("Google authentication failed");
            return new AuthResultDto
            {
                Success = false,
                Message = "Google authentication failed"
            };
        }

        var claims = authResult.Principal.Claims.ToDictionary(c => c.Type, c => c.Value);

        var email = claims.GetValueOrDefault(ClaimTypes.Email);
        var userName = claims.GetValueOrDefault(ClaimTypes.Name) ?? email;
        var googleId = claims.GetValueOrDefault(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
        {
            _logger.LogWarning("Google authentication did not provide required claims");
            return new AuthResultDto
            {
                Success = false,
                Message = "Google authentication did not provide required claims"
            };
        }

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            _logger.LogInformation("User with email {Email} already exists, logging in", email);
            return await CreateSuccessResult(existingUser, "Login successful", cancellationToken);
        }

        var user = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Error creating user from Google authentication: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return new AuthResultDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        _logger.LogInformation("User {Email} created from Google authentication", email);

        return await CreateSuccessResult(user, "Google login successful", cancellationToken);
    }

    private async Task<AuthResultDto> CreateSuccessResult(IdentityUser user, string message, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _jwtService.GenerateJwtToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

        var claims = new Dictionary<string, string>
        {
            { "email", user.Email },
            { "username", user.UserName },
            { "id", user.Id }
        };

        return new AuthResultDto
        {
            Success = true,
            Message = message,
            Claims = claims,
            Token = jwtToken.Token,
            RefreshToken = refreshToken,
            Expiration = jwtToken.Expiration
        };
    }
}
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Authentication;

public static class GoogleAuthHandler
{
    private const string ReturnUrlKey = "returnUrl";
    private const string TokenParameter = "token";
    private const string RefreshTokenParameter = "refreshToken";
    private const string ErrorParameter = "error";
    private const string DefaultReturnUrl = "/";

    public static Task<IResult> HandleGoogleLogin(string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = RouteConstants.Api.Auth.GoogleAuthComplete,
            Items =
            {
                [ReturnUrlKey] = returnUrl ?? DefaultReturnUrl
            }
        };

        return Task.FromResult(Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]));
    }

    public static async Task<IResult> HandleGoogleCallback(
        HttpContext httpContext,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginWithGoogleAsync(httpContext, cancellationToken);
        var returnUrl = await GetReturnUrlAsync(httpContext);

        if (result.Success)
        {
            var redirectUrl = BuildRedirectUrl(returnUrl, new Dictionary<string, string>
            {
                [TokenParameter] = result.Token,
                [RefreshTokenParameter] = result.RefreshToken
            });

            return Results.Redirect(redirectUrl);
        }

        var errorUrl = BuildRedirectUrl(returnUrl, new Dictionary<string, string>
        {
            [ErrorParameter] = result.Message
        });

        return Results.Redirect(errorUrl);
    }

    private static async Task<string> GetReturnUrlAsync(HttpContext httpContext)
    {
        var authenticateResult = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        return authenticateResult?.Properties?.Items[ReturnUrlKey] ?? DefaultReturnUrl;
    }

    private static string BuildRedirectUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        var queryString = string.Join("&", parameters.Select(p =>
            $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

        return $"{baseUrl}?{queryString}";
    }
}
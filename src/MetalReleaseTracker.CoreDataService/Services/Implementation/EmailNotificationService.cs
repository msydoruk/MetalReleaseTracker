using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Configuration;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class EmailNotificationService : IEmailNotificationService
{
    private const string ResendApiUrl = "https://api.resend.com/emails";
    private const string FallbackSiteUrl = "https://metal-release.com";
    private const int VerificationTokenExpiryHours = 24;

    private readonly IEmailSubscriptionRepository _emailSubscriptionRepository;
    private readonly IOptions<EmailServiceSettings> _emailSettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAdminSettingsService _adminSettingsService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailSubscriptionRepository emailSubscriptionRepository,
        IOptions<EmailServiceSettings> emailSettings,
        IHttpClientFactory httpClientFactory,
        IAdminSettingsService adminSettingsService,
        ILogger<EmailNotificationService> logger)
    {
        _emailSubscriptionRepository = emailSubscriptionRepository;
        _emailSettings = emailSettings;
        _httpClientFactory = httpClientFactory;
        _adminSettingsService = adminSettingsService;
        _logger = logger;
    }

    public async Task<int> SendNotificationsAsync(List<UserNotificationEntity> notifications, CancellationToken cancellationToken = default)
    {
        if (notifications.Count == 0)
        {
            return 0;
        }

        var emailEnabled = await _adminSettingsService.GetBoolSettingAsync(
            SettingCategories.FeatureToggles,
            SettingKeys.Email.ServiceEnabled,
            false,
            cancellationToken);

        if (!emailEnabled)
        {
            return 0;
        }

        var userIds = notifications.Select(notification => notification.UserId).Distinct().ToList();
        var emails = await _emailSubscriptionRepository.GetEmailsByUserIdsAsync(userIds, cancellationToken);

        if (emails.Count == 0)
        {
            return 0;
        }

        var siteUrl = await GetSiteUrlAsync(cancellationToken);
        var sentCount = 0;

        foreach (var notification in notifications)
        {
            if (!emails.TryGetValue(notification.UserId, out var email))
            {
                continue;
            }

            try
            {
                var subject = notification.Title;
                var htmlContent = BuildNotificationHtml(notification, siteUrl);
                var sent = await SendEmailAsync(email, subject, htmlContent, cancellationToken);

                if (sent)
                {
                    sentCount++;
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send email notification to {Email}", email);
            }
        }

        _logger.LogInformation("Sent {SentCount} email notifications out of {TotalCount}", sentCount, notifications.Count);
        return sentCount;
    }

    public async Task SubscribeAsync(string userId, string email, CancellationToken cancellationToken = default)
    {
        var existing = await _emailSubscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (existing != null)
        {
            await _emailSubscriptionRepository.RemoveByUserIdAsync(userId, cancellationToken);
        }

        var verificationToken = GenerateVerificationToken();

        var entity = new EmailSubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            IsVerified = false,
            VerificationToken = verificationToken,
            VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(VerificationTokenExpiryHours),
            SubscribedAt = DateTime.UtcNow,
        };

        await _emailSubscriptionRepository.AddAsync(entity, cancellationToken);

        var siteUrl = await GetSiteUrlAsync(cancellationToken);
        var verifyUrl = $"{siteUrl}/api/email/verify/{verificationToken}";
        var htmlContent = BuildVerificationHtml(verifyUrl, siteUrl);

        var sent = await SendEmailAsync(email, "Verify your email - Metal Release Tracker", htmlContent, cancellationToken);

        if (sent)
        {
            _logger.LogInformation("Verification email sent to {Email} for user {UserId}", email, userId);
        }
        else
        {
            _logger.LogWarning("Failed to send verification email to {Email} for user {UserId}", email, userId);
        }
    }

    public async Task<bool> VerifyAsync(string token, CancellationToken cancellationToken = default)
    {
        var entity = await _emailSubscriptionRepository.GetByTokenAsync(token, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        if (entity.VerificationTokenExpiresAt.HasValue && entity.VerificationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Verification token expired for user {UserId}", entity.UserId);
            return false;
        }

        entity.IsVerified = true;
        entity.VerificationToken = null;
        entity.VerificationTokenExpiresAt = null;
        entity.UnsubscribedAt = null;

        await _emailSubscriptionRepository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Email verified for user {UserId}", entity.UserId);
        return true;
    }

    public async Task UnsubscribeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _emailSubscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (entity == null)
        {
            return;
        }

        await _emailSubscriptionRepository.RemoveByUserIdAsync(userId, cancellationToken);

        _logger.LogInformation("User {UserId} unsubscribed from email notifications", userId);
    }

    public async Task<EmailSubscriptionStatusDto> GetStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await _emailSubscriptionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (entity == null)
        {
            return new EmailSubscriptionStatusDto
            {
                IsSubscribed = false,
                IsVerified = false,
                Email = null,
            };
        }

        return new EmailSubscriptionStatusDto
        {
            IsSubscribed = entity.UnsubscribedAt == null,
            IsVerified = entity.IsVerified,
            Email = entity.Email,
        };
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        var settings = _emailSettings.Value;

        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            _logger.LogWarning("Email API key is not configured, skipping email send");
            return false;
        }

        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var payload = new
        {
            from = $"{settings.FromName} <{settings.FromEmail}>",
            to = new[] { toEmail },
            subject,
            html = htmlContent,
        };

        var response = await httpClient.PostAsJsonAsync(ResendApiUrl, payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Resend API returned {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);
        }

        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetSiteUrlAsync(CancellationToken cancellationToken)
    {
        return await _adminSettingsService.GetStringSettingAsync(
            SettingCategories.Seo,
            SettingKeys.Seo.SiteUrl,
            FallbackSiteUrl,
            cancellationToken);
    }

    private static string BuildNotificationHtml(UserNotificationEntity notification, string siteUrl)
    {
        var emoji = notification.NotificationType switch
        {
            NotificationType.PriceDrop => "&#128315;",
            NotificationType.PriceIncrease => "&#128314;",
            NotificationType.BackInStock => "&#9989;",
            NotificationType.Restock => "&#128260;",
            NotificationType.NewVariant => "&#127381;",
            _ => "&#128276;",
        };

        var albumLink = notification.AlbumId.HasValue && notification.AlbumId.Value != Guid.Empty
            ? $"{siteUrl}/albums/{notification.AlbumId.Value}"
            : siteUrl;

        return $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #1a1a2e; color: #e0e0e0;">
              <h2 style="color: #e53935;">{emoji} Metal Release Tracker</h2>
              <p style="font-size: 16px; font-weight: bold;">{System.Net.WebUtility.HtmlEncode(notification.Title)}</p>
              <p style="color: #aaa;">{System.Net.WebUtility.HtmlEncode(notification.Message)}</p>
              <a href="{albumLink}" style="display: inline-block; padding: 10px 20px; background: #e53935; color: white; text-decoration: none; border-radius: 4px; margin-top: 10px;">View Album</a>
              <hr style="border-color: #333; margin: 20px 0;" />
              <p style="font-size: 12px; color: #666;">You received this because you subscribed to email notifications. <a href="{siteUrl}/profile" style="color: #e53935;">Unsubscribe</a></p>
            </div>
            """;
    }

    private static string BuildVerificationHtml(string verifyUrl, string siteUrl)
    {
        return $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background: #1a1a2e; color: #e0e0e0;">
              <h2 style="color: #e53935;">Metal Release Tracker</h2>
              <p style="font-size: 16px;">Please verify your email address to start receiving notifications.</p>
              <a href="{verifyUrl}" style="display: inline-block; padding: 12px 24px; background: #e53935; color: white; text-decoration: none; border-radius: 4px; margin-top: 10px; font-weight: bold;">Verify Email</a>
              <p style="color: #aaa; margin-top: 20px; font-size: 13px;">This link expires in 24 hours.</p>
              <hr style="border-color: #333; margin: 20px 0;" />
              <p style="font-size: 12px; color: #666;">If you didn't request this, you can safely ignore this email.</p>
            </div>
            """;
    }

    private static string GenerateVerificationToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

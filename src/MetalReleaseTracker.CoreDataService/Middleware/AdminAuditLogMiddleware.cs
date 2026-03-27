using System.Security.Claims;
using System.Text.RegularExpressions;
using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Middleware;

public partial class AdminAuditLogMiddleware
{
    private static readonly Regex GuidPattern = GeneratedGuidPattern();

    private readonly RequestDelegate _next;
    private readonly ILogger<AdminAuditLogMiddleware> _logger;

    public AdminAuditLogMiddleware(
        RequestDelegate next,
        ILogger<AdminAuditLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (!ShouldLog(context))
        {
            return;
        }

        try
        {
            await LogAuditEntryAsync(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to write audit log for {Path}", context.Request.Path);
        }
    }

    private async Task LogAuditEntryAsync(HttpContext context)
    {
        using var scope = context.RequestServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CoreDataServiceDbContext>();

        var path = context.Request.Path.Value ?? string.Empty;

        var auditLog = new AuditLogEntity
        {
            Id = Guid.NewGuid(),
            UserId = ExtractUserId(context),
            UserName = ExtractUserName(context),
            HttpMethod = context.Request.Method,
            RequestPath = path,
            Action = DeriveAction(context.Request.Method, path),
            EntityType = ExtractEntityType(path),
            EntityId = ExtractEntityId(path),
            ResponseStatusCode = context.Response.StatusCode,
            Details = $"Status: {context.Response.StatusCode}",
            Timestamp = DateTime.UtcNow,
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync();
    }

    private static bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value;
        var method = context.Request.Method;

        if (string.IsNullOrEmpty(path)
            || !path.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return method is "POST" or "PUT" or "DELETE";
    }

    private static string? ExtractUserId(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? context.User.FindFirst("sub")?.Value;
    }

    private static string? ExtractUserName(HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.Name)?.Value
               ?? context.User.FindFirst("name")?.Value;
    }

    private static string DeriveAction(string method, string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var resourceSegments = segments
            .Skip(2)
            .Where(segment => !GuidPattern.IsMatch(segment))
            .ToArray();

        if (resourceSegments.Length == 0)
        {
            return $"{method}Admin";
        }

        var prefix = method switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "DELETE" => "Delete",
            _ => method,
        };

        var resourceName = string.Join(
            string.Empty,
            resourceSegments.Select(segment => ToPascalCase(segment)));

        return $"{prefix}{resourceName}";
    }

    private static string? ExtractEntityType(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
        {
            return null;
        }

        return segments[2];
    }

    private static string? ExtractEntityId(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segments.FirstOrDefault(segment => GuidPattern.IsMatch(segment));
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var parts = input.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(
            string.Empty,
            parts.Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }

    [GeneratedRegex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
    private static partial Regex GeneratedGuidPattern();
}

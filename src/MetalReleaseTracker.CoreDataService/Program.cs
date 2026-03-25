using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.CoreDataService.ServiceExtensions;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("CoreDataService"))
        .WithTracing(tracing =>
        {
            tracing
                .AddSource("MassTransit")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        });
}

builder.Services
    .AddAppSettings(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddApplicationDatabases(builder.Configuration)
    .AddApplicationAuthentication(builder.Configuration)
    .AddAdminAuthentication(builder.Configuration)
    .AddAdminServices();

builder.Services.AddHttpClient();

builder.Services
    .AddApplicationCors()
    .AddApplicationSwagger()
    .AddHttpForwarder();

var app = builder.Build();

app.UseApplicationMiddleware(builder.Environment)
    .MapApplicationEndpoints()
    .MapAdminEndpoints()
    .MapMinioForwarder()
    .ApplyMigrations()
    .RegisterTelegramWebhook();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404
        && !context.Response.HasStarted
        && context.Request.Path.StartsWithSegments("/admin")
        && !context.Request.Path.StartsWithSegments("/api"))
    {
        var indexFilePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin", "index.html");
        if (File.Exists(indexFilePath))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexFilePath);
        }
    }
});

app.MapFallback(async (HttpContext context, ISeoMetaTagService seoService) =>
{
    if (context.Request.Path.StartsWithSegments("/admin"))
    {
        var indexFilePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin", "index.html");
        if (File.Exists(indexFilePath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexFilePath);
            return;
        }
    }

    var html = await seoService.GetHtmlWithMetaTags(context.Request.Path, context.RequestAborted);
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html, context.RequestAborted);
});

app.Run();

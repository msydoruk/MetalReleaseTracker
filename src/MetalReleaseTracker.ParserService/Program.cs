using Autofac;
using Autofac.Extensions.DependencyInjection;
using MetalReleaseTracker.ParserService.Aplication.Services;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Services;
using MetalReleaseTracker.ParserService.Infrastructure.Common.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Images;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Infrastructure.Messaging.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Scheduling.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Services;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TickerQ.DependencyInjection;
using TickerQ.Utilities.Enums;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("ParserService"))
        .WithTracing(tracing =>
        {
            tracing
                .AddSource("MassTransit")
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        });
}

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddMinio();

var parserServiceConnectionString = builder.Configuration.GetConnectionString("ParserServiceConnectionString");
builder.Services.AddDbContext<ParserServiceDbContext>(options =>
{
    options.UseNpgsql(parserServiceConnectionString);
});

builder.Services.AddTickerQScheduler(builder.Configuration);
builder.Services.AddHttpServices();
builder.Services.AddAdminAuthentication(builder.Configuration);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<ParsingSessionRepository>().As<IParsingSessionRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AlbumParsedEventRepository>().As<IAlbumParsedEventRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<BandReferenceRepository>().As<IBandReferenceRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<CatalogueIndexRepository>().As<ICatalogueIndexRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<BandDiscographyRepository>().As<IBandDiscographyRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AdminQueryRepository>().As<IAdminQueryRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AdminAuthService>().As<IAdminAuthService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AiVerificationService>().As<IAiVerificationService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<SettingsService>().As<ISettingsService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<SettingsSeedService>().As<ISettingsSeedService>().InstancePerLifetimeScope();
    containerBuilder.AddParsers();
});

builder.Services.AddHttpClient<IFlareSolverrClient, FlareSolverrClient>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<IBandReferenceService, BandReferenceService>();
builder.Services.AddScoped<BandReferenceSyncJob>();
builder.Services.AddScoped<CatalogueIndexJob>();
builder.Services.AddScoped<AlbumDetailParsingJob>();
builder.Services.AddScoped<AlbumParsedPublisherJob>();
builder.Services.AddScoped<TickerQJobFunctions>();
builder.Services.AddHostedService<TickerQSchedulerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var parserServiceDbContext = scope.ServiceProvider.GetRequiredService<ParserServiceDbContext>();
    parserServiceDbContext.Database.Migrate();

    var parserServiceTickerQDbContext = scope.ServiceProvider.GetRequiredService<ParserServiceTickerQDbContext>();
    parserServiceTickerQDbContext.Database.Migrate();

    var settingsSeedService = scope.ServiceProvider.GetRequiredService<ISettingsSeedService>();
    await settingsSeedService.SeedAsync();
}

var adminFileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "wwwroot"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = adminFileProvider,
});
app.UseTickerQ(TickerQStartMode.Manual);
app.UseAuthentication();
app.UseAuthorization();
app.MapAdminEndpoints();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404
        && !context.Response.HasStarted
        && context.Request.Path.StartsWithSegments("/admin")
        && !context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
        var indexFilePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin", "index.html");
        await context.Response.Body.WriteAsync(await File.ReadAllBytesAsync(indexFilePath));
    }
});

app.Run();

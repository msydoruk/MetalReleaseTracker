using FluentValidation;
using MetalReleaseTracker.CatalogSyncService.Data;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.MappingProfiles;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CatalogSyncService.Data.Validators;
using MetalReleaseTracker.CatalogSyncService.ServiceExtensions;
using MetalReleaseTracker.CatalogSyncService.Services;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("CatalogSyncService"))
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
builder.Services.AddMongo();
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddMinio();
builder.Services.AddTickerQScheduler(builder.Configuration);
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddTransient<IValidator<RawAlbumEntity>, RawAlbumEntityValidator>();
builder.Services.AddScoped<IParsingSessionWithRawAlbumsRepository, ParsingSessionWithRawAlbumsRepository>();
builder.Services.AddScoped<IAlbumProcessedRepository, AlbumProcessedRepository>();

builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddTransient<AlbumProcessingJob>();
builder.Services.AddTransient<AlbumProcessedPublisherJob>();
builder.Services.AddScoped<TickerQJobFunctions>();
builder.Services.AddHostedService<CatalogSyncSchedulerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var tickerQDbContext = scope.ServiceProvider.GetRequiredService<CatalogSyncTickerQDbContext>();
    tickerQDbContext.Database.Migrate();
}

app.UseTickerQ();
app.Run();

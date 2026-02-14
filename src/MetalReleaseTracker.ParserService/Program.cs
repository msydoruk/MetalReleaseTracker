using Autofac;
using Autofac.Extensions.DependencyInjection;
using MetalReleaseTracker.ParserService.Aplication.Services;
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
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<ParsingSessionRepository>().As<IParsingSessionRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AlbumParsedEventRepository>().As<IAlbumParsedEventRepository>().InstancePerLifetimeScope();
    containerBuilder.AddParsers();
});

builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddScoped<AlbumParsingJob>();
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
}

app.UseTickerQ();
app.Run();

using MetalReleaseTracker.CoreDataService.ServiceExtensions;
using Microsoft.AspNetCore.Builder;
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
    .AddApplicationCors()
    .AddApplicationSwagger()
    .AddHttpForwarder();

var app = builder.Build();

app.UseApplicationMiddleware(builder.Environment)
    .MapApplicationEndpoints()
    .MapMinioForwarder()
    .ApplyMigrations();

app.MapFallbackToFile("index.html");

app.Run();

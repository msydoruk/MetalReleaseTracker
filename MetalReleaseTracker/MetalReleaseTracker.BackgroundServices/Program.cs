using Hangfire;
using Hangfire.PostgreSql;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Factories;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;
using MetalReleaseTracker.BackgroundServices.Settings;
using MetalReleaseTracker.BackgroundServices.Workers;
using Hangfire.Dashboard;

using MetalReleaseTracker.Infrastructure.Parsers;
using MetalReleaseTracker.Infrastructure.Utils;
using MetalReleaseTracker.Infrastructure.Providers;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<AlbumSynchronizationSettings>(builder.Configuration.GetSection("AlbumSynchronizationSettings"));

builder.Services.AddDbContext<MetalReleaseTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetalReleaseTrackerDb")));

builder.Services.AddHangfire(options => 
    options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("MetalReleaseTrackerDbBackground")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IBandRepository, BandRepository>();
builder.Services.AddScoped<IDistributorsRepository, DistributorsRepository>();

builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IBandService, BandService>();
builder.Services.AddScoped<IDistributorsService, DistributorsService>();

builder.Services.AddSingleton<UserAgentProvider>();
builder.Services.AddScoped<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<IParser, OsmoseProductionsParser>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();
builder.Services.AddScoped<IAlbumSynchronizationService, AlbumSynchronizationService>();

builder.Services.AddHostedService<AlbumSynchronizationWorker>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = new[] { new AllowAllConnectionsFilter() },
    IgnoreAntiforgeryToken = true
});

app.Run();

public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
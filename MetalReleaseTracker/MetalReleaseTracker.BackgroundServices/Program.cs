using Hangfire;
using Hangfire.PostgreSql;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.Core.Interfaces;
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
using MetalReleaseTracker.Infrastructure.Parsers;
using MetalReleaseTracker.Infrastructure.Providers;
using MetalReleaseTracker.Infrastructure.Utils;
using MetalReleaseTracker.BackgroundServices.Filters;
using MetalReleaseTracker.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<AlbumSynchronizationSettings>(builder.Configuration.GetSection("AlbumSynchronizationSettings"));

builder.Services.AddDbContext<MetalReleaseTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetalReleaseTrackerMainConnectionString")));

builder.Services.AddHangfire(options => 
    options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("MetalReleaseTrackerBackgroundConnectionString")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IBandRepository, BandRepository>();
builder.Services.AddScoped<IDistributorsRepository, DistributorsRepository>();

builder.Services.AddCustomValidators();
builder.Services.AddValidationServiceWithAllValidators();

builder.Services.AddHttpClient<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<UserAgentProvider>();
builder.Services.AddScoped<IParser, OsmoseProductionsParser>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IBandService, BandService>();
builder.Services.AddScoped<IDistributorsService, DistributorsService>();
builder.Services.AddScoped<IAlbumSynchronizationService, TestAlbumSynchronizationService>();

builder.Services.AddHostedService<AlbumSynchronizationWorker>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

var dashboardOptions = new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
};

app.UseHangfireDashboard("/hangfire", dashboardOptions);

app.Run();
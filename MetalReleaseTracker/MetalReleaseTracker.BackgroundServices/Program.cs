using Hangfire;
using Hangfire.PostgreSql;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.BackgroundServices;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Factories;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Ñore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Configuration.AddJsonFile("settings.json", optional: false, reloadOnChange: true);

builder.Services.AddDbContext<MetalReleaseTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(configuration => configuration
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("PostgreSqlConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IBandRepository, BandRepository>();
builder.Services.AddScoped<IDistributorsRepository, DistributorsRepository>();

builder.Services.AddScoped<IParserFactory, ParserFactory>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IBandService, BandService>();
builder.Services.AddScoped<IDistributorsService, DistributorsService>();
builder.Services.AddScoped<IAlbumSynchronizationService, AlbumSynchronizationService>();

builder.Services.AddHostedService<AlbumSynchronizationWorker>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();
app.UseHangfireDashboard();
app.Run();
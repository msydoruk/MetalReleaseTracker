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
using MetalReleaseTracker.Infrastructure.Parsers;
using MetalReleaseTracker.Infrastructure.Providers;
using MetalReleaseTracker.Infrastructure.Utils;
using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;

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

builder.Services.AddScoped<IValidator<AlbumFilter>, AlbumFilterValidator>();
builder.Services.AddScoped<IValidator<Album>, AlbumValidator>();
builder.Services.AddScoped<IValidator<Band>, BandValidator>();
builder.Services.AddScoped<IValidator<Distributor>, DistributorValidator>();
builder.Services.AddScoped<IValidator<Guid>, GuidValidator>();
builder.Services.AddScoped<IValidator<Subscription>, SubscriptionValidator>();
builder.Services.AddScoped<IValidationService, ValidationService>();

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

app.UseHangfireDashboard("/hangfire");

app.Run();
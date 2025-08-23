using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
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
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddMongo();
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddMinio();
builder.Services.AddHangfire(options => options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("ParserServiceConnectionString")));
builder.Services.AddHangfireServer();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddTransient<IValidator<RawAlbumEntity>, RawAlbumEntityValidator>();
builder.Services.AddScoped<IParsingSessionWithRawAlbumsRepository, ParsingSessionWithRawAlbumsRepository>();
builder.Services.AddScoped<IAlbumProcessedRepository, AlbumProcessedRepository>();

builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddTransient<AlbumProcessingJob>();
builder.Services.AddTransient<AlbumProcessedPublisherJob>();
builder.Services.AddHostedService<CatalogSyncSchedulerService>();

var app = builder.Build();

app.UseHangfireDashboard();
app.Run();

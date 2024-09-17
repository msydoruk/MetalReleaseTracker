using MetalReleaseTracker.API.Extensions;
using MetalReleaseTracker.API.Middleware;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Factories;
using MetalReleaseTracker.Infrastructure.Parsers;
using MetalReleaseTracker.Infrastructure.Providers;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Infrastructure.Utils;
using MetalReleaseTracker.Сore.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MetalReleaseTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IBandRepository, BandRepository>();
builder.Services.AddScoped<IDistributorsRepository, DistributorsRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IBandService, BandService>();
builder.Services.AddScoped<IDistributorsService, DistributorsService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<AlbumParser>();
builder.Services.AddSingleton<UserAgentProvider>();
builder.Services.AddSingleton<HtmlLoader>();

builder.Services.AddScoped<IParser, OsmoseProductionsParser>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();
builder.Services.AddScoped<AlbumParsingService>();

builder.Services.AddCustomValidators();
builder.Services.AddValidationServiceWithAllValidators();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

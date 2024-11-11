using Hangfire;
using Hangfire.PostgreSql;
using MetalReleaseTracker.API.Extensions;
using MetalReleaseTracker.API.Filters;
using MetalReleaseTracker.API.Middleware;
using MetalReleaseTracker.API.Settings;
using MetalReleaseTracker.API.Workers;
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
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<AlbumSynchronizationSettings>(builder.Configuration.GetSection("AlbumSynchronizationSettings"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MetalReleaseTrackerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MetalReleaseTrackerConnectionString")));

builder.Services.AddHangfire(options =>
    options.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("MetalReleaseTrackerConnectionString")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<IBandRepository, BandRepository>();
builder.Services.AddScoped<IDistributorsRepository, DistributorsRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

builder.Services.AddCustomValidators();
builder.Services.AddValidationServiceWithAllValidators();

builder.Services.AddHttpClient<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<UserAgentProvider>();
builder.Services.AddScoped<IParser, OsmoseProductionsParser>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();

builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IBandService, BandService>();
builder.Services.AddScoped<IDistributorsService, DistributorsService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IAlbumSynchronizationService, AlbumSynchronizationService>();

builder.Services.AddHostedService<AlbumSynchronizationWorker>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

var dashboardOptions = new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
};

app.UseHangfireDashboard("/hangfire", dashboardOptions);

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MetalReleaseTrackerDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

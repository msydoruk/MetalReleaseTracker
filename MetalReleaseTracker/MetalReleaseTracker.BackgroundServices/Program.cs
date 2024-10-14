using Hangfire;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Application.Services;
using MetalReleaseTracker.BackgroundServices;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHangfire(configuration => configuration
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<IAlbumSynchronizationService, AlbumSynchronizationService>();
builder.Services.AddHostedService<AlbumSynchronizationWorker>();

var host = builder.Build();
host.Run();

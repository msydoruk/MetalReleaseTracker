using MetalReleaseTracker.CoreDataService.ServiceExtensions;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddApplicationDatabases(builder.Configuration)
    .AddApplicationAuthentication(builder.Configuration)
    .AddApplicationCors()
    .AddApplicationSwagger();

var app = builder.Build();

app.UseApplicationMiddleware(builder.Environment)
    .MapApplicationEndpoints()
    .ApplyMigrations();

app.Run();
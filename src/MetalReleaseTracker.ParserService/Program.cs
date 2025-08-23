using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using MetalReleaseTracker.ParserService.Aplication.Services;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Common.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Http;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Infrastructure.Messaging.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddKafka(builder.Configuration);
builder.Services.AddMinio();

var parserServiceConnectionString = builder.Configuration.GetConnectionString("ParserServiceConnectionString");
builder.Services.AddHangfire(options => options.UsePostgreSqlStorage(parserServiceConnectionString));
builder.Services.AddHangfireServer();
builder.Services.AddDbContext<ParserServiceDbContext>(options =>
{
    options.UseNpgsql(parserServiceConnectionString);
});

builder.Services.AddHttpServices();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<ParsingSessionRepository>().As<IParsingSessionRepository>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<AlbumParsedEventRepository>().As<IAlbumParsedEventRepository>().InstancePerLifetimeScope();
    containerBuilder.AddParsers();
});

builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();
builder.Services.AddTransient<AlbumParsingJob>();
builder.Services.AddHostedService<ParserSchedulerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ParserServiceDbContext>();
    dbContext.Database.Migrate();
}

app.UseHangfireDashboard();
app.Run();

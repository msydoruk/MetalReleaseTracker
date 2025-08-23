using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace MetalReleaseTracker.CoreDataService.Tests.Fixtures;

public class TestPostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public CoreDataServiceDbContext DbContext => CreateDbContext();

    public TestPostgresDatabaseFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithImage("postgres:15.1")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.StopAsync();
    }

    private CoreDataServiceDbContext CreateDbContext()
    {
        var dbOptions = new DbContextOptionsBuilder<CoreDataServiceDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString()).Options;

        return new CoreDataServiceDbContext(dbOptions);
    }
}
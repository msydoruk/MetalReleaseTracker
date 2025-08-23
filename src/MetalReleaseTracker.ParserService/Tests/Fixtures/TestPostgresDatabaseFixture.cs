using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.Fixtures;

public class TestPostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresSqlContainer;

    public ParserServiceDbContext DbContext => CreateDbContext();

    public TestPostgresDatabaseFixture()
    {
        _postgresSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithDatabase("test_user")
            .WithDatabase("test_password")
            .WithDatabase("postgres:15.1")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresSqlContainer.StartAsync();

        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresSqlContainer.StopAsync();
    }

    private ParserServiceDbContext CreateDbContext()
    {
        var dbOptions = new DbContextOptionsBuilder<ParserServiceDbContext>()
            .UseNpgsql(_postgresSqlContainer.GetConnectionString())
            .Options;

        return new ParserServiceDbContext(dbOptions);
    }
}
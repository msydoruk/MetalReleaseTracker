using DotNet.Testcontainers.Builders;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;

public class TestMongoDatabaseFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer;
    private MongoClient _mongoClient;
    private IMongoDatabase _mongoDatabase;

    private string ConnectionString => $"mongodb://root:example@localhost:{_mongoDbContainer.GetMappedPublicPort(27017)}";

    public IMongoDatabase MongoDatabase => _mongoDatabase;

    public IOptions<MongoDbConfig> MongoDbConfig  => Options.Create(new MongoDbConfig
    {
        ProcessedAlbumsCollectionName = "ProcessedAlbums",
        ParsingSessionWithRawAlbumsCollectionName = "ParsingSessionWithRawAlbums"
    });

    public TestMongoDatabaseFixture()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:5.0")
            .WithPortBinding(27017, true)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "root")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "example")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();
        _mongoClient = new MongoClient(ConnectionString);
        _mongoDatabase = _mongoClient.GetDatabase("test_db");
    }

    public async Task DisposeAsync()
    {
        await _mongoDbContainer.StopAsync();
    }
}
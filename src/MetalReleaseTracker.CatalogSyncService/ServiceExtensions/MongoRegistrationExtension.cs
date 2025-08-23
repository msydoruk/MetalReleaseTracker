using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MetalReleaseTracker.CatalogSyncService.ServiceExtensions;

public static class MongoRegistrationExtension
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var connectionString = serviceProvider.GetRequiredService<IOptions<MongoDbConfig>>().Value.ConnectionString;
            return new MongoClient(connectionString);
        });

        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var mongoDbConfig = serviceProvider.GetRequiredService<IOptions<MongoDbConfig>>().Value;
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();

            var database =  mongoClient.GetDatabase(mongoDbConfig.DatabaseName);

            EnsureIndexes(database, mongoDbConfig);

            return database;
        });

        return services;
    }

    private static void EnsureIndexes(IMongoDatabase database, MongoDbConfig mongoDbConfig)
    {
        var processedAlbumsCollection = database.GetCollection<AlbumProcessedEntity>(mongoDbConfig.ProcessedAlbumsCollectionName);
        var processedAlbumsIndexKeys = Builders<AlbumProcessedEntity>.IndexKeys.Ascending(album => album.SKU);
        var processedAlbumsIndexOptions = new CreateIndexOptions()
        {
            Unique = true
        };
        var processedAlbumsIndexModel = new CreateIndexModel<AlbumProcessedEntity>(processedAlbumsIndexKeys, processedAlbumsIndexOptions);
        processedAlbumsCollection.Indexes.CreateOne(processedAlbumsIndexModel);
    }
}
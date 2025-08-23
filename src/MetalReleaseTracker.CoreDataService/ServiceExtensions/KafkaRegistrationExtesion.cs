using Confluent.Kafka;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Consumers;
using MetalReleaseTracker.CoreDataService.Data.Events;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class KafkaRegistrationExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var kafkaConfig = configuration.GetSection("Kafka").Get<KafkaConfig>();
        services.AddMassTransit(configure =>
        {
            configure.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            configure.AddRider(rider =>
            {
                rider.AddConsumer<AlbumProcessedEventConsumer>();

                rider.UsingKafka((context, kafkaFactory) =>
                {
                    kafkaFactory.SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaConfig.Security.SaslProtocol, true);
                    kafkaFactory.Host(kafkaConfig.BootstrapServers);

                    kafkaFactory.TopicEndpoint<AlbumProcessedPublicationEvent>(kafkaConfig.CatalogSyncServiceTopic, kafkaConfig.CatalogSyncServiceConsumerGroup, endpoint =>
                    {
                        endpoint.AutoOffsetReset = AutoOffsetReset.Earliest;
                        endpoint.ConfigureConsumer<AlbumProcessedEventConsumer>(context);
                    });
                });
            });
        });

        return services;
    }
}
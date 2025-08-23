using Confluent.Kafka;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Consumers;
using MetalReleaseTracker.CatalogSyncService.Data.Events;

namespace MetalReleaseTracker.CatalogSyncService.ServiceExtensions;

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
                rider.AddProducer<AlbumProcessedPublicationEvent>(kafkaConfig.CatalogSyncServiceTopic);
                rider.AddConsumer<AlbumParsedEventConsumer>();

                rider.UsingKafka((context, kafkaFactory) =>
                {
                    kafkaFactory.SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaConfig.Security.SaslProtocol, true);
                    kafkaFactory.Host(kafkaConfig.BootstrapServers);

                    kafkaFactory.TopicEndpoint<AlbumParsedPublicationEvent>(kafkaConfig.ParserServiceTopic, kafkaConfig.ParserServiceConsumerGroup, endpoint =>
                    {
                        endpoint.AutoOffsetReset = AutoOffsetReset.Earliest;
                        endpoint.ConfigureConsumer<AlbumParsedEventConsumer>(context);
                    });
                });
            });
        });

        return services;
    }
}
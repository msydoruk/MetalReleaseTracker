using Confluent.Kafka;
using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Infrastructure.Messaging.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Messaging.Extensions;

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
                rider.AddProducer<AlbumParsedPublicationEvent>(kafkaConfig.ParserServiceTopic);
                rider.UsingKafka((context, kafkaFactory) =>
                {
                    kafkaFactory.SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaConfig.Security.SaslProtocol, true);
                    kafkaFactory.Host(kafkaConfig.BootstrapServers);
                });
            });
        });

        return services;
    }
}
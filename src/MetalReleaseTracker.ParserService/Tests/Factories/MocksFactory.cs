using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs.Configuration;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;
using Moq;

namespace MetalReleaseTracker.ParserService.Tests.Factories;

public static class MocksFactory
{
    public static Mock<IParser> CreateParserMock(IEnumerable<AlbumParsedEvent> fakeAlbumParsedEvents)
    {
        var parserMock = new Mock<IParser>();

        parserMock.Setup(x => x.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new PageParsedResult
            {
                ParsedAlbums = fakeAlbumParsedEvents,
                NextPageUrl = null
            }));

        return parserMock;
    }

    public static Mock<IFileStorageService> CreateFileStorageServiceMock()
    {
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return fileStorageServiceMock;
    }

    public static Mock<ITopicProducer<AlbumParsedPublicationEvent>> CreateTopicProducerMock()
    {
        var topicProducerMock = new Mock<ITopicProducer<AlbumParsedPublicationEvent>>();
        topicProducerMock.Setup(x => x.Produce(It.IsAny<AlbumParsedPublicationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return topicProducerMock;
    }

    public static Mock<IOptions<AlbumParsedPublisherJobSettings>> CreateAlbumParsedPublisherJobSettingsMock(
        int maxChunkSizeInBytes)
    {
        var albumParsedPublisherJobSettingsMock = new Mock<IOptions<AlbumParsedPublisherJobSettings>>();
        albumParsedPublisherJobSettingsMock.Setup(x => x.Value)
            .Returns(new AlbumParsedPublisherJobSettings
            {
                MaxChunkSizeInBytes = maxChunkSizeInBytes
            });

        return albumParsedPublisherJobSettingsMock;
    }
}
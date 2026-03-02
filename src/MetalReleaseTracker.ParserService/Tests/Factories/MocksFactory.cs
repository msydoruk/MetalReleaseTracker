using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.SharedLibraries.Minio;
using Moq;

namespace MetalReleaseTracker.ParserService.Tests.Factories;

public static class MocksFactory
{
    public static Mock<IAlbumDetailParser> CreateAlbumDetailParserMock(IEnumerable<AlbumParsedEvent> fakeAlbumParsedEvents)
    {
        var parserMock = new Mock<IAlbumDetailParser>();
        var eventQueue = new Queue<AlbumParsedEvent>(fakeAlbumParsedEvents);

        parserMock.Setup(x => x.ParseAlbumDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => eventQueue.Dequeue());

        return parserMock;
    }

    public static Mock<IListingParser> CreateListingParserMock(IEnumerable<ListingItem> fakeListings)
    {
        var parserMock = new Mock<IListingParser>();

        parserMock.Setup(x => x.ParseListingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListingPageResult
            {
                Listings = fakeListings.ToList(),
                NextPageUrl = null
            });

        return parserMock;
    }

    public static Mock<IFileStorageService> CreateFileStorageServiceMock()
    {
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        fileStorageServiceMock.Setup(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return fileStorageServiceMock;
    }

    public static Mock<ITopicProducer<AlbumProcessedPublicationEvent>> CreateTopicProducerMock()
    {
        var topicProducerMock = new Mock<ITopicProducer<AlbumProcessedPublicationEvent>>();
        topicProducerMock.Setup(x => x.Produce(It.IsAny<AlbumProcessedPublicationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return topicProducerMock;
    }
}

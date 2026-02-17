using Autofac;
using Autofac.Features.Metadata;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Extensions;

public static class ParserRegistrationExtension
{
    public static void AddParsers(this ContainerBuilder builder)
    {
        builder.RegisterType<FlareSolverrHtmlDocumentLoader>()
            .As<IHtmlDocumentLoader>()
            .As<IAsyncDisposable>()
            .SingleInstance();

        builder.RegisterType<SeleniumWebDriverFactory>()
            .As<ISeleniumWebDriverFactory>()
            .SingleInstance();

        builder.RegisterType<OsmoseProductionsParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.OsmoseProductions));

        builder.RegisterType<DrakkarParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.Drakkar));

        builder.RegisterType<BlackMetalVendorParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.BlackMetalVendor));

        builder.RegisterType<BlackMetalStoreParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.BlackMetalStore));

        builder.RegisterType<NapalmRecordsParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.NapalmRecords));

        builder.RegisterType<SeasonOfMistParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.SeasonOfMist));

        builder.RegisterType<ParagonRecordsParser>()
            .As<IListingParser>()
            .As<IAlbumDetailParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.ParagonRecords));

        builder.Register<Func<DistributorCode, IListingParser>>(context =>
        {
            var metaParsers = context.Resolve<IEnumerable<Meta<IListingParser, ParserMetadata>>>();
            return distributorCode =>
            {
                var parser = metaParsers.FirstOrDefault(p => p.Metadata.DistributorCode == distributorCode);
                return parser?.Value ?? throw new NotSupportedException($"Listing parser for distributor '{distributorCode}' not found.");
            };
        });

        builder.Register<Func<DistributorCode, IAlbumDetailParser>>(context =>
        {
            var metaParsers = context.Resolve<IEnumerable<Meta<IAlbumDetailParser, ParserMetadata>>>();
            return distributorCode =>
            {
                var parser = metaParsers.FirstOrDefault(p => p.Metadata.DistributorCode == distributorCode);
                return parser?.Value ?? throw new NotSupportedException($"Album detail parser for distributor '{distributorCode}' not found.");
            };
        });
    }
}

using Autofac;
using Autofac.Features.Metadata;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Http;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Extensions;

public static class ParserRegistrationExtension
{
    public static void AddParsers(this ContainerBuilder builder)
    {
        builder.RegisterType<HtmlDocumentLoader>()
            .As<IHtmlDocumentLoader>()
            .SingleInstance();

        builder.RegisterType<OsmoseProductionsParser>()
            .As<IParser>()
            .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.OsmoseProductions));

        builder.Register<Func<DistributorCode, IParser>>(context =>
        {
            var metaParsers = context.Resolve<IEnumerable<Meta<IParser, ParserMetadata>>>();
            return distributorCode =>
            {
                var parser = metaParsers.FirstOrDefault(p => p.Metadata.DistributorCode == distributorCode);

                return parser?.Value ?? throw new NotSupportedException($"Parser for distributor '{distributorCode}' not found.");
            };
        });
    }
}

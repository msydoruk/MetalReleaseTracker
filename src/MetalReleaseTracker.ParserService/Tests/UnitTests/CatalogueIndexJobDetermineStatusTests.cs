using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.UnitTests;

public class CatalogueIndexJobDetermineStatusTests
{
    [Fact]
    public void DetermineStatus_BandNotFound_ReturnsNotRelevant()
    {
        var bandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nokturnal Mortum" };

        var result = CatalogueIndexJob.DetermineStatus(bandNames, "Cannibal Corpse");

        Assert.Equal(CatalogueIndexStatus.NotRelevant, result);
    }

    [Fact]
    public void DetermineStatus_BandFound_ReturnsRelevant()
    {
        var bandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nokturnal Mortum" };

        var result = CatalogueIndexJob.DetermineStatus(bandNames, "Nokturnal Mortum");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_CaseInsensitiveBandNameMatch_ReturnsRelevant()
    {
        var bandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Nokturnal Mortum" };

        var result = CatalogueIndexJob.DetermineStatus(bandNames, "nokturnal mortum");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_EmptySet_ReturnsNotRelevant()
    {
        var bandNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var result = CatalogueIndexJob.DetermineStatus(bandNames, "Any Band");

        Assert.Equal(CatalogueIndexStatus.NotRelevant, result);
    }
}

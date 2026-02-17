using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.UnitTests;

public class CatalogueIndexJobDetermineStatusTests
{
    [Fact]
    public void DetermineStatus_BandNotFound_ReturnsNotRelevant()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nokturnal Mortum"] = new(StringComparer.OrdinalIgnoreCase) { "lunar poetry" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Cannibal Corpse", "Tomb of the Mutilated");

        Assert.Equal(CatalogueIndexStatus.NotRelevant, result);
    }

    [Fact]
    public void DetermineStatus_BandFoundAlbumMatches_ReturnsRelevant()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nokturnal Mortum"] = new(StringComparer.OrdinalIgnoreCase) { "lunar poetry", "goat horns" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Nokturnal Mortum", "Lunar Poetry");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_BandFoundAlbumNotFound_ReturnsPendingReview()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nokturnal Mortum"] = new(StringComparer.OrdinalIgnoreCase) { "lunar poetry", "goat horns" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Nokturnal Mortum", "Unknown Album");

        Assert.Equal(CatalogueIndexStatus.PendingReview, result);
    }

    [Fact]
    public void DetermineStatus_BandFoundEmptyDiscography_ReturnsPendingReview()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nokturnal Mortum"] = new(StringComparer.OrdinalIgnoreCase)
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Nokturnal Mortum", "Lunar Poetry");

        Assert.Equal(CatalogueIndexStatus.PendingReview, result);
    }

    [Fact]
    public void DetermineStatus_CaseInsensitiveBandNameMatch()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nokturnal Mortum"] = new(StringComparer.OrdinalIgnoreCase) { "lunar poetry" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "nokturnal mortum", "Lunar Poetry");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_NormalizesAlbumTitleBeforeMatching()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Drudkh"] = new(StringComparer.OrdinalIgnoreCase) { "blood in our wells" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Drudkh", "Blood in Our Wells");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_EmptyMap_ReturnsNotRelevant()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Any Band", "Any Album");

        Assert.Equal(CatalogueIndexStatus.NotRelevant, result);
    }

    [Fact]
    public void DetermineStatus_MaBandNameContainsDistributorName_ReturnsRelevant()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Мисливці (The Hunters)"] = new(StringComparer.OrdinalIgnoreCase) { "the hunt begins" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "The Hunters", "The Hunt Begins");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_DistributorNameContainsMaBandName_ReturnsRelevant()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Khors"] = new(StringComparer.OrdinalIgnoreCase) { "night falls onto the fronts of ours" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "Khors (UA)", "Night Falls onto the Fronts of Ours");

        Assert.Equal(CatalogueIndexStatus.Relevant, result);
    }

    [Fact]
    public void DetermineStatus_ContainsMatchAlbumNotFound_ReturnsPendingReview()
    {
        var bandAlbumMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Мисливці (The Hunters)"] = new(StringComparer.OrdinalIgnoreCase) { "the hunt begins" }
        };

        var result = CatalogueIndexJob.DetermineStatus(bandAlbumMap, "The Hunters", "Unknown Album");

        Assert.Equal(CatalogueIndexStatus.PendingReview, result);
    }
}

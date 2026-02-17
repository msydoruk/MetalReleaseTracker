using MetalReleaseTracker.ParserService.Infrastructure.Services;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.UnitTests;

public class AlbumTitleNormalizerTests
{
    [Fact]
    public void Normalize_LowercasesTitle()
    {
        var result = AlbumTitleNormalizer.Normalize("Blood Of The Nations");

        Assert.Equal("blood of the nations", result);
    }

    [Fact]
    public void Normalize_StripsPunctuation()
    {
        var result = AlbumTitleNormalizer.Normalize("Hell-Born (Demo) [Reissue]");

        Assert.Equal("hellborn demo reissue", result);
    }

    [Fact]
    public void Normalize_CollapsesWhitespace()
    {
        var result = AlbumTitleNormalizer.Normalize("Blood   of   the   Nations");

        Assert.Equal("blood of the nations", result);
    }

    [Fact]
    public void Normalize_TrimsResult()
    {
        var result = AlbumTitleNormalizer.Normalize("  Blood of the Nations  ");

        Assert.Equal("blood of the nations", result);
    }

    [Fact]
    public void Normalize_HandlesEmptyString()
    {
        var result = AlbumTitleNormalizer.Normalize(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_HandlesNull()
    {
        var result = AlbumTitleNormalizer.Normalize(null!);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_HandlesWhitespaceOnly()
    {
        var result = AlbumTitleNormalizer.Normalize("   ");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Normalize_HandlesComplexTitle()
    {
        var result = AlbumTitleNormalizer.Normalize("Stalingrad: Brothers and Sisters, I'm Dying!");

        Assert.Equal("stalingrad brothers and sisters im dying", result);
    }

    [Fact]
    public void Normalize_PreservesAlphanumericContent()
    {
        var result = AlbumTitleNormalizer.Normalize("Vol. 3: The Subliminal Verses");

        Assert.Equal("vol 3 the subliminal verses", result);
    }
}

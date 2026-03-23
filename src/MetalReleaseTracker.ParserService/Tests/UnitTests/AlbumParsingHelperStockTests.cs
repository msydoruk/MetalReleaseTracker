using System.Text.Json;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.UnitTests;

public class AlbumParsingHelperStockTests
{
    [Theory]
    [InlineData("Out of stock", StockStatus.OutOfStock)]
    [InlineData("Sold Out", StockStatus.OutOfStock)]
    [InlineData("Sold out", StockStatus.OutOfStock)]
    [InlineData("unavailable", StockStatus.OutOfStock)]
    [InlineData("not available", StockStatus.OutOfStock)]
    [InlineData("niedost\u0119pny", StockStatus.OutOfStock)]
    [InlineData("wyprzedane", StockStatus.OutOfStock)]
    [InlineData("OUT OF STOCK", StockStatus.OutOfStock)]
    public void ParseStockStatus_OutOfStockText_ReturnsOutOfStock(string text, StockStatus expected)
    {
        var result = AlbumParsingHelper.ParseStockStatus(text);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Pre-Order", StockStatus.PreOrder)]
    [InlineData("preorder", StockStatus.PreOrder)]
    [InlineData("pre order", StockStatus.PreOrder)]
    [InlineData("przedsprzeda\u017c", StockStatus.PreOrder)]
    [InlineData("PRE-ORDER", StockStatus.PreOrder)]
    public void ParseStockStatus_PreOrderText_ReturnsPreOrder(string text, StockStatus expected)
    {
        var result = AlbumParsingHelper.ParseStockStatus(text);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("In stock", StockStatus.InStock)]
    [InlineData("Available", StockStatus.InStock)]
    [InlineData("dost\u0119pny", StockStatus.InStock)]
    [InlineData("w magazynie", StockStatus.InStock)]
    [InlineData("IN STOCK", StockStatus.InStock)]
    public void ParseStockStatus_InStockText_ReturnsInStock(string text, StockStatus expected)
    {
        var result = AlbumParsingHelper.ParseStockStatus(text);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("some random text")]
    public void ParseStockStatus_UnknownOrEmpty_ReturnsUnknown(string? text)
    {
        var result = AlbumParsingHelper.ParseStockStatus(text);
        Assert.Equal(StockStatus.Unknown, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_SchemaOrgInStock_ReturnsInStock()
    {
        var json = JsonDocument.Parse("{\"availability\":\"https://schema.org/InStock\"}");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.InStock, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_HttpSchemaOrgInStock_ReturnsInStock()
    {
        var json = JsonDocument.Parse("{\"availability\":\"http://schema.org/InStock\"}");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.InStock, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_SchemaOrgOutOfStock_ReturnsOutOfStock()
    {
        var json = JsonDocument.Parse("{\"availability\":\"https://schema.org/OutOfStock\"}");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.OutOfStock, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_SchemaOrgPreOrder_ReturnsPreOrder()
    {
        var json = JsonDocument.Parse("{\"availability\":\"https://schema.org/PreOrder\"}");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.PreOrder, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_ArrayOffers_ReturnsCorrectStatus()
    {
        var json = JsonDocument.Parse("[{\"availability\":\"https://schema.org/InStock\"}]");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.InStock, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_Null_ReturnsUnknown()
    {
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(null);
        Assert.Equal(StockStatus.Unknown, result);
    }

    [Fact]
    public void ParseStockStatusFromJsonLd_NoAvailability_ReturnsUnknown()
    {
        var json = JsonDocument.Parse("{\"price\":\"10.00\"}");
        var result = AlbumParsingHelper.ParseStockStatusFromJsonLd(json.RootElement);
        Assert.Equal(StockStatus.Unknown, result);
    }
}

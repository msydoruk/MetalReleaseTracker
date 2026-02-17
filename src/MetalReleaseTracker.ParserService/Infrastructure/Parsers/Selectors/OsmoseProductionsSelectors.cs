namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class OsmoseProductionsSelectors
{
    public const string AlbumNodes = ".//div[@class='GshopListingABorder']";
    public const string AnchorNode = ".//div[contains(@class,'GshopListingARightInfo')]//a";
    public const string BandNameSpan = ".//span[@class='TtypeC TcolorC']";
    public const string AlbumTitleSpan = ".//span[@class='TtypeH TcolorC']";
    public const string CurrentPageNode = ".//div[@class='GtoursPaginationButtonTxt on']/span";
    public const string NextPageTemplate = ".//a[contains(@href, 'page={0}')]";

    public const string DetailBandName = "//span[@class='cufonAb']/a";
    public const string DetailAlbumName = "//div[@class='column twelve']//span[@class='cufonAb']";
    public const string DetailSku = "//span[@class='cufonEb' and contains(text(), 'Press :')]";
    public const string DetailReleaseDate = "//span[@class='cufonEb' and contains(text(), 'Year :')]";
    public const string DetailPrice = "//span[@class='cufonCd ']";
    public const string DetailPhoto = "//div[@class='photo_prod_container']/a";
    public const string DetailLabel = "//span[@class='cufonEb' and contains(text(), 'Label :')]//a";
    public const string DetailPress = "//span[@class='cufonEb' and contains(text(), 'Press :')]";
    public const string DetailDescription = "//span[@class='cufonEb' and contains(text(), 'Info :')]";
    public const string DetailStatus = ".//*[contains(@class, 'info')]";
}

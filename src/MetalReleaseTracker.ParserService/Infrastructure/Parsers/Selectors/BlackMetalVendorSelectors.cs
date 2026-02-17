namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class BlackMetalVendorSelectors
{
    public const string NextPageLink = "//a[@class='pageResults' and contains(@title, 'chste Seite')]";
    public const string ListingBoxes = "//div[@class='listingbox']";
    public const string ListingTitleLink = ".//div[@class='lb_title']//h2//a";

    public const string DetailTitle = "//div[@class='lb_title']//h2//a";
    public const string DetailTitleFallback = "//h1";
    public const string DetailPrice = ".//span[@class='value_price']";
    public const string DetailPriceFallback = "//span[contains(@class,'price')]";
    public const string DetailPhoto = ".//div[contains(@class,'prod_image')]//img";
    public const string DetailPhotoFallback = "//img[contains(@class,'product')]";
}

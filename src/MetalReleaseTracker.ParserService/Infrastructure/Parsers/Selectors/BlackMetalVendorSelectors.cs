namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class BlackMetalVendorSelectors
{
    public const string NextPageLink = "//a[@class='pageResults' and contains(@title, 'next page')]";
    public const string ListingBoxes = "//div[@class='listingbox']";
    public const string ListingTitleLink = ".//div[@class='lb_title']//h2//a";

    public const string DetailTitle = "//div[@class='pd_title']//h1";
    public const string DetailTitleFallback = "//h1[@itemprop='name']";
    public const string DetailPriceMeta = "//meta[@itemprop='price']";
    public const string DetailPriceFallback = "//div[@class='pd_price']//span[@class='new_price']";
    public const string DetailPhoto = "//img[@itemprop='image']";
    public const string DetailPhotoFallback = "//div[contains(@class,'pd_image_big')]//img";
    public const string DetailLabel = "//div[@itemprop='brand']//span[@itemprop='name']";
    public const string DetailDescription = "//div[@itemprop='description']";
}

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class NapalmRecordsSelectors
{
    public const string NextPageLink = "//a[contains(@class,'action') and contains(@class,'next')]";
    public const string ProductItems = "//li[contains(@class,'product-item')]";
    public const string ProductLink = ".//a[contains(@class,'product-item-link')]";
    public const string ProductBandName = ".//div[contains(@class,'custom-band-name')]";

    public const string DetailTitle = "//h1[contains(@class,'page-title')]";
    public const string DetailTitleFallback = "//h1";
    public const string DetailAttributeTable = "//table[@id='product-attribute-specs-table']//td[@data-th='{0}']";
    public const string DetailSkuStrong = "//strong";
    public const string DetailSkuForm = "//form[@data-product-sku]";
    public const string DetailPriceWrapper = "//span[contains(@class,'price-wrapper')]";
    public const string DetailOgImage = "//meta[@property='og:image']";
    public const string DetailGalleryImage = "//img[contains(@class,'product-image-photo') and contains(@src,'/media/catalog/product/')]";
    public const string DetailDescription = "//div[contains(@class,'description')]//div[@class='value']";
}

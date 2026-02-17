namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class BlackMetalStoreSelectors
{
    public const string NextPageLink = "//a[contains(@class,'next') and contains(@class,'page-numbers')]";
    public const string ProductLinks = "//a[contains(@href,'/produto/')]";
    public const string ProductTitle = ".//h2[contains(@class,'product-title')]";
    public const string ProductTitleFallback = ".//h2";
    public const string ProductTitleFallback2 = ".//h3";

    public const string DetailTitle = "//h1";
    public const string DetailSku = "//span[@class='sku']";
    public const string DetailPrice = "//p[contains(@class,'price')]//bdi";
    public const string DetailPriceFallback = "//span[contains(@class,'woocommerce-Price-amount')]//bdi";
    public const string DetailPhoto = "//img[contains(@class,'wp-post-image')]";
    public const string DetailLabel = "//span[@class='posted_in']//a";
    public const string DetailBrand = "//div[contains(@class,'product-brand')]//a";
    public const string DetailDescription = "//div[contains(@class,'woocommerce-product-details__short-description')]";
}

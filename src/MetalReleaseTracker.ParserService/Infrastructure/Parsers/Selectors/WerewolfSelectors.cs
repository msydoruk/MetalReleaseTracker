namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class WerewolfSelectors
{
    public const string ProductNodes = "//ul[contains(@class,'products')]//li[contains(@class,'product-item')]";
    public const string ProductAnchor = ".//a[contains(@class,'product-title-link')]";
    public const string NextPageLink = "//a[contains(@class,'next') and contains(@class,'page-numbers')]";

    public const string DetailTitle = "//h1[contains(@class,'product_title')]";
    public const string DetailPrice = "//p[contains(@class,'price')]//bdi";
    public const string DetailPriceFallback = "//span[contains(@class,'woocommerce-Price-amount')]//bdi";
    public const string DetailPhotoGalleryLink = "//div[contains(@class,'woocommerce-product-gallery')]//a[@href]";
    public const string DetailPhotoFallback = "//div[contains(@class,'woocommerce-product-gallery')]//img";
    public const string DetailShortDescription = "//div[contains(@class,'woocommerce-product-details__short-description')]";
    public const string DetailDescription = "//div[@id='tab-description']";
    public const string DetailBrand = "//span[contains(text(),'Brand:')]/following-sibling::a | //span[contains(text(),'Brand:')]/..//a";
}

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class DrakkarSelectors
{
    public const string ProductNodes = "//li[contains(@class,'product-warp-item')]";
    public const string ProductNodesFallback = "//li[contains(@class,'type-product')]";
    public const string ProductAnchor = ".//a[contains(@href,'/product/')]";
    public const string ProductTitle = ".//a[contains(@class,'woocommerce-loop-product__title')]";
    public const string NextPageLink = "//a[contains(@class,'next')]";

    public const string DetailTitle = "//h1[contains(@class,'product_title')]";
    public const string DetailSku = "//span[@class='sku']";
    public const string DetailPrice = "//p[contains(@class,'price')]//bdi";
    public const string DetailPriceFallback = "//span[contains(@class,'woocommerce-Price-amount')]//bdi";
    public const string DetailPhotoGallery = "//div[contains(@class,'woocommerce-product-gallery')]//img";
    public const string DetailShortDescription = "//div[contains(@class,'woocommerce-product-details__short-description')]";
    public const string DetailAttributes = "//table[contains(@class,'woocommerce-product-attributes')]";
    public const string DetailCategoryLinks = "//span[@class='posted_in']//a";
}

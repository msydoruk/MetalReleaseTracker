namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class SeasonOfMistSelectors
{
    public const string NextPageLink = "//a[contains(@class,'next') or @title='Next']";
    public const string ProductGrid = "//div[contains(@class,'products-grid')]//div[contains(@class,'item')]";
    public const string ProductNameLink = ".//h2[contains(@class,'product-name')]/a";

    public const string DetailAttributeHeader = "//table[@id='product-attribute-specs-table']//th[normalize-space(text())='{0}']";
    public const string DetailPrice = "//span[contains(@class,'price')]";
    public const string DetailPhoto = "//div[contains(@class,'product-img-box')]//img";
    public const string DetailPhotoFallback = "//a[contains(@class,'product-image')]//img";
    public const string DetailCartButton = "//button[contains(@class,'btn-cart')]";
    public const string DetailCartButtonFallback = "//button[@type='submit' and contains(@class,'add')]";
}

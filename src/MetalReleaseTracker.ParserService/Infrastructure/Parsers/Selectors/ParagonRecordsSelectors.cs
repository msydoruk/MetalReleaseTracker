namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class ParagonRecordsSelectors
{
    public const string NextPageLink = "//ul[contains(@class,'pagination')]//a[.//span[contains(text(),'Next')]]";
    public const string ProductGrid = "//div[contains(@class,'grid--view-items')]";
    public const string ProductItems = ".//div[contains(@class,'grid__item')]";
    public const string ProductLink = ".//a[contains(@class,'grid-view-item__link')]";
    public const string ProductTitle = ".//div[contains(@class,'grid-view-item__title')]";

    public const string DetailTitle = "//h1[contains(@class,'product-single__title')]";
    public const string DetailTitleFallback = "//h1";
    public const string DetailOgPrice = "//meta[@property='og:price:amount']";
    public const string DetailPrice = "//span[contains(@class,'product-price__price')]";
    public const string DetailOgImageSecure = "//meta[@property='og:image:secure_url']";
    public const string DetailOgImage = "//meta[@property='og:image']";
    public const string DetailDescription = "//div[contains(@class,'product-single__description')]";
    public const string DetailCartButton = "//span[@id='AddToCartText-product-template']";
    public const string DetailCartButtonFallback = "//button[contains(@class,'product-form__cart-submit')]";
}

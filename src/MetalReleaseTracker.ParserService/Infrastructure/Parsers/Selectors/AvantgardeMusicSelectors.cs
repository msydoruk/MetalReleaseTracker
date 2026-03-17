namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class AvantgardeMusicSelectors
{
    public const string ProductNodes = "//div[contains(@class,'product')]//h4/a[contains(@href,'/band/')]";
    public const string NextPageLink = "//ul[contains(@class,'pagination')]//li[contains(@class,'active')]/following-sibling::li[1]/a";

    public const string DetailTitle = "//h1[contains(@class,'product-title')]";
    public const string DetailTitleBand = "//h1[contains(@class,'product-title')]/text()[1]";
    public const string DetailTitleAlbum = "//h1[contains(@class,'product-title')]//small";
    public const string DetailPrice = "//div[contains(@class,'product-price')]";
    public const string DetailPhoto = "//img[contains(@src,'/uploads/Product/image/')]";
    public const string DetailSpecs = "//p[contains(text(),'label:') or contains(text(),'country:') or contains(text(),'format:')]";
}

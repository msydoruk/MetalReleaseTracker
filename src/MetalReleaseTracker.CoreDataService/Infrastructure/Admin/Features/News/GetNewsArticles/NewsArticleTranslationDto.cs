namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

public class NewsArticleTranslationDto
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}

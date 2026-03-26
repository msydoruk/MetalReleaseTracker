using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.UpdateNewsArticle;

public class UpdateNewsArticleRequest
{
    public string? ChipLabel { get; set; }

    public string? ChipColor { get; set; }

    public string? IconName { get; set; }

    public DateTime? Date { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsPublished { get; set; }

    public Dictionary<string, NewsArticleTranslationDto>? Translations { get; set; }
}

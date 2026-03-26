using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.CreateNewsArticle;

public class CreateNewsArticleRequest
{
    public string ChipLabel { get; set; } = string.Empty;

    public string ChipColor { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; }

    public Dictionary<string, NewsArticleTranslationDto> Translations { get; set; } = new();
}

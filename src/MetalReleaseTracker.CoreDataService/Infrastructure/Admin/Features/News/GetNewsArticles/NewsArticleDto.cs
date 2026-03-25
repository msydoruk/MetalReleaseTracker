namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

public class NewsArticleDto
{
    public Guid Id { get; set; }

    public string TitleEn { get; set; } = string.Empty;

    public string TitleUa { get; set; } = string.Empty;

    public string ContentEn { get; set; } = string.Empty;

    public string ContentUa { get; set; } = string.Empty;

    public string ChipLabel { get; set; } = string.Empty;

    public string ChipColor { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.UpdateNewsArticle;

public class UpdateNewsArticleRequest
{
    public string? TitleEn { get; set; }

    public string? TitleUa { get; set; }

    public string? ContentEn { get; set; }

    public string? ContentUa { get; set; }

    public string? ChipLabel { get; set; }

    public string? ChipColor { get; set; }

    public string? IconName { get; set; }

    public DateTime? Date { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsPublished { get; set; }
}

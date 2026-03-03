using System.Linq.Expressions;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Repositories;

public class AdminQueryRepository : IAdminQueryRepository
{
    private static readonly Dictionary<BandReferenceSortField, Expression<Func<BandReferenceDto, object>>> BandReferenceSortExpressions = new()
    {
        [BandReferenceSortField.BandName] = dto => dto.BandName,
        [BandReferenceSortField.Genre] = dto => dto.Genre,
        [BandReferenceSortField.LastSyncedAt] = dto => dto.LastSyncedAt,
        [BandReferenceSortField.DiscographyCount] = dto => dto.DiscographyCount,
    };

    private static readonly Dictionary<CatalogueIndexSortField, Expression<Func<CatalogueIndexDto, object>>> CatalogueIndexSortExpressions = new()
    {
        [CatalogueIndexSortField.BandName] = dto => dto.BandName,
        [CatalogueIndexSortField.AlbumTitle] = dto => dto.AlbumTitle,
        [CatalogueIndexSortField.DistributorCode] = dto => dto.DistributorCode,
        [CatalogueIndexSortField.Status] = dto => dto.Status,
        [CatalogueIndexSortField.CreatedAt] = dto => dto.CreatedAt,
        [CatalogueIndexSortField.UpdatedAt] = dto => dto.UpdatedAt,
    };

    private static readonly Dictionary<CatalogueDetailSortField, Expression<Func<CatalogueDetailDto, object>>> CatalogueDetailSortExpressions = new()
    {
        [CatalogueDetailSortField.BandName] = dto => dto.BandName,
        [CatalogueDetailSortField.Name] = dto => dto.Name,
        [CatalogueDetailSortField.DistributorCode] = dto => dto.DistributorCode,
        [CatalogueDetailSortField.ChangeType] = dto => dto.ChangeType,
        [CatalogueDetailSortField.PublicationStatus] = dto => dto.PublicationStatus,
        [CatalogueDetailSortField.Price] = dto => dto.Price,
        [CatalogueDetailSortField.UpdatedAt] = dto => dto.UpdatedAt,
        [CatalogueDetailSortField.LastPublishedAt] = dto => dto.LastPublishedAt!,
    };

    private readonly ParserServiceDbContext _context;

    public AdminQueryRepository(ParserServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<BandReferenceDto>> GetBandReferencesAsync(
        BandReferenceFilterDto filter,
        CancellationToken cancellationToken)
    {
        var query = _context.BandReferences
            .AsNoTracking()
            .Select(band => new BandReferenceDto
            {
                Id = band.Id,
                BandName = band.BandName,
                MetalArchivesId = band.MetalArchivesId,
                Genre = band.Genre,
                LastSyncedAt = band.LastSyncedAt,
                DiscographyCount = band.Discography.Count,
            })
            .WhereIf(
                !string.IsNullOrWhiteSpace(filter.Search),
                dto => EF.Functions.ILike(dto.BandName, $"%{filter.Search}%"));

        query = ApplySorting(query, filter.SortBy ?? BandReferenceSortField.BandName, filter.SortAscending ?? true, BandReferenceSortExpressions);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<BandReferenceDetailDto?> GetBandReferenceByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.BandReferences
            .AsNoTracking()
            .Where(band => band.Id == id)
            .Select(band => new BandReferenceDetailDto
            {
                Id = band.Id,
                BandName = band.BandName,
                MetalArchivesId = band.MetalArchivesId,
                Genre = band.Genre,
                LastSyncedAt = band.LastSyncedAt,
                DiscographyCount = band.Discography.Count,
                Discography = band.Discography.Select(disc => new BandDiscographyDto
                {
                    Id = disc.Id,
                    AlbumTitle = disc.AlbumTitle,
                    AlbumType = disc.AlbumType,
                    Year = disc.Year,
                }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResultDto<CatalogueIndexDto>> GetCatalogueIndexAsync(
        CatalogueIndexFilterDto filter,
        CancellationToken cancellationToken)
    {
        var query = _context.CatalogueIndex
            .AsNoTracking()
            .Include(catalogue => catalogue.BandReference)
            .Include(catalogue => catalogue.BandDiscography)
            .Select(catalogue => new CatalogueIndexDto
            {
                Id = catalogue.Id,
                DistributorCode = catalogue.DistributorCode,
                BandName = catalogue.BandName,
                AlbumTitle = catalogue.AlbumTitle,
                RawTitle = catalogue.RawTitle,
                DetailUrl = catalogue.DetailUrl,
                Status = catalogue.Status,
                MediaType = catalogue.MediaType,
                BandReferenceId = catalogue.BandReferenceId,
                BandReferenceName = catalogue.BandReference != null ? catalogue.BandReference.BandName : null,
                MatchedAlbumTitle = catalogue.BandDiscography != null ? catalogue.BandDiscography.AlbumTitle : null,
                MatchedAlbumYear = catalogue.BandDiscography != null ? catalogue.BandDiscography.Year : null,
                CreatedAt = catalogue.CreatedAt,
                UpdatedAt = catalogue.UpdatedAt,
            })
            .WhereIf(
                filter.DistributorCode.HasValue,
                dto => dto.DistributorCode == filter.DistributorCode!.Value)
            .WhereIf(
                filter.Status.HasValue,
                dto => dto.Status == filter.Status!.Value)
            .WhereIf(
                !string.IsNullOrWhiteSpace(filter.Search),
                dto => EF.Functions.ILike(dto.BandName, $"%{filter.Search}%") || EF.Functions.ILike(dto.AlbumTitle, $"%{filter.Search}%"));

        query = ApplySorting(query, filter.SortBy ?? CatalogueIndexSortField.UpdatedAt, filter.SortAscending ?? false, CatalogueIndexSortExpressions);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<PagedResultDto<CatalogueDetailDto>> GetCatalogueDetailsAsync(
        CatalogueDetailFilterDto filter,
        CancellationToken cancellationToken)
    {
        var query = _context.CatalogueIndexDetails
            .AsNoTracking()
            .Select(detail => new CatalogueDetailDto
            {
                Id = detail.Id,
                CatalogueIndexId = detail.CatalogueIndexId,
                DistributorCode = detail.DistributorCode,
                BandName = detail.BandName,
                Name = detail.Name,
                SKU = detail.SKU,
                Price = detail.Price,
                PurchaseUrl = detail.PurchaseUrl,
                Media = detail.Media,
                CanonicalTitle = detail.CanonicalTitle,
                OriginalYear = detail.OriginalYear,
                ChangeType = detail.ChangeType,
                PublicationStatus = detail.PublicationStatus,
                LastPublishedAt = detail.LastPublishedAt,
                CreatedAt = detail.CreatedAt,
                UpdatedAt = detail.UpdatedAt,
            })
            .WhereIf(
                filter.DistributorCode.HasValue,
                dto => dto.DistributorCode == filter.DistributorCode!.Value)
            .WhereIf(
                filter.ChangeType.HasValue,
                dto => dto.ChangeType == filter.ChangeType!.Value)
            .WhereIf(
                filter.PublicationStatus.HasValue,
                dto => dto.PublicationStatus == filter.PublicationStatus!.Value)
            .WhereIf(
                !string.IsNullOrWhiteSpace(filter.Search),
                dto => EF.Functions.ILike(dto.BandName, $"%{filter.Search}%") || EF.Functions.ILike(dto.Name, $"%{filter.Search}%"));

        query = ApplySorting(query, filter.SortBy ?? CatalogueDetailSortField.UpdatedAt, filter.SortAscending ?? false, CatalogueDetailSortExpressions);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<PagedResultDto<ParsingRunDto>> GetParsingRunsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.ParsingRuns
            .AsNoTracking()
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new ParsingRunDto
            {
                Id = r.Id,
                JobType = r.JobType,
                DistributorCode = r.DistributorCode,
                Status = r.Status,
                TotalItems = r.TotalItems,
                ProcessedItems = r.ProcessedItems,
                FailedItems = r.FailedItems,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                ErrorMessage = r.ErrorMessage,
                CountersJson = r.CountersJson,
            });

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ParsingRunDto?> GetParsingRunByIdAsync(
        Guid runId,
        CancellationToken cancellationToken)
    {
        return await _context.ParsingRuns
            .AsNoTracking()
            .Where(r => r.Id == runId)
            .Select(r => new ParsingRunDto
            {
                Id = r.Id,
                JobType = r.JobType,
                DistributorCode = r.DistributorCode,
                Status = r.Status,
                TotalItems = r.TotalItems,
                ProcessedItems = r.ProcessedItems,
                FailedItems = r.FailedItems,
                StartedAt = r.StartedAt,
                CompletedAt = r.CompletedAt,
                ErrorMessage = r.ErrorMessage,
                CountersJson = r.CountersJson,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResultDto<ParsingRunItemDto>> GetParsingRunItemsAsync(
        Guid runId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _context.ParsingRunItems
            .AsNoTracking()
            .Where(i => i.ParsingRunId == runId)
            .OrderBy(i => i.ProcessedAt)
            .Select(i => new ParsingRunItemDto
            {
                Id = i.Id,
                ItemDescription = i.ItemDescription,
                IsSuccess = i.IsSuccess,
                ErrorMessage = i.ErrorMessage,
                Categories = i.Categories,
                ProcessedAt = i.ProcessedAt,
            });

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    private static IQueryable<T> ApplySorting<T, TField>(
        IQueryable<T> query,
        TField field,
        bool ascending,
        Dictionary<TField, Expression<Func<T, object>>> expressions)
        where TField : notnull
    {
        if (!expressions.TryGetValue(field, out var expression))
        {
            return query;
        }

        return ascending
            ? query.OrderBy(expression)
            : query.OrderByDescending(expression);
    }
}

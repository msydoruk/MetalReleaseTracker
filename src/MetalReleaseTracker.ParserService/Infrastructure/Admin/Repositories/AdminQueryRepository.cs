using System.Linq.Expressions;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Data;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
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

    private static readonly Dictionary<ParsingSessionSortField, Expression<Func<ParsingSessionDto, object>>> ParsingSessionSortExpressions = new()
    {
        [ParsingSessionSortField.DistributorCode] = dto => dto.DistributorCode,
        [ParsingSessionSortField.LastUpdatedDate] = dto => dto.LastUpdatedDate,
        [ParsingSessionSortField.ParsingStatus] = dto => dto.ParsingStatus,
        [ParsingSessionSortField.EventCount] = dto => dto.EventCount,
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
                dto => dto.BandName.Contains(filter.Search!));

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
                dto => dto.BandName.Contains(filter.Search!) || dto.AlbumTitle.Contains(filter.Search!));

        query = ApplySorting(query, filter.SortBy ?? CatalogueIndexSortField.UpdatedAt, filter.SortAscending ?? false, CatalogueIndexSortExpressions);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<PagedResultDto<ParsingSessionDto>> GetParsingSessionsAsync(
        ParsingSessionFilterDto filter,
        CancellationToken cancellationToken)
    {
        var query = _context.ParsingSessions
            .AsNoTracking()
            .Select(session => new ParsingSessionDto
            {
                Id = session.Id,
                DistributorCode = session.DistributorCode,
                LastUpdatedDate = session.LastUpdatedDate,
                ParsingStatus = session.ParsingStatus,
                EventCount = _context.AlbumParsedEvents.Count(e => e.ParsingSessionId == session.Id),
            })
            .WhereIf(
                filter.DistributorCode.HasValue,
                dto => dto.DistributorCode == filter.DistributorCode!.Value)
            .WhereIf(
                filter.ParsingStatus.HasValue,
                dto => dto.ParsingStatus == filter.ParsingStatus!.Value);

        query = ApplySorting(query, filter.SortBy ?? ParsingSessionSortField.LastUpdatedDate, filter.SortAscending ?? false, ParsingSessionSortExpressions);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<ParsingSessionDetailDto?> GetParsingSessionByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var session = await _context.ParsingSessions
            .AsNoTracking()
            .Where(session => session.Id == id)
            .Select(session => new ParsingSessionDetailDto
            {
                Id = session.Id,
                DistributorCode = session.DistributorCode,
                LastUpdatedDate = session.LastUpdatedDate,
                ParsingStatus = session.ParsingStatus,
                EventCount = _context.AlbumParsedEvents.Count(e => e.ParsingSessionId == session.Id),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (session == null)
        {
            return null;
        }

        session.Events = await _context.AlbumParsedEvents
            .AsNoTracking()
            .Where(e => e.ParsingSessionId == id)
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => new AlbumParsedEventDto
            {
                Id = e.Id,
                ParsingSessionId = e.ParsingSessionId,
                CreatedDate = e.CreatedDate,
                EventPayloadPreview = e.EventPayload.Length > 200
                    ? e.EventPayload.Substring(0, 200) + "..."
                    : e.EventPayload,
            })
            .ToListAsync(cancellationToken);

        return session;
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

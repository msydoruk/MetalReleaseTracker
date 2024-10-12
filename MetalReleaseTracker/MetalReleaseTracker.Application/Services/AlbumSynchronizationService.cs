﻿using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumSynchronizationService : IAlbumSynchronizationService
    {
        private readonly IParserFactory _parserFactory;
        private readonly IAlbumService _albumService;
        private readonly IBandService _bandService;
        private readonly IDistributorsService _distributorService;

        public AlbumSynchronizationService(
            IParserFactory parserFactory,
            IAlbumService albumService,
            IBandService bandService,
            IDistributorsService distributorService)
        {
            _parserFactory = parserFactory;
            _albumService = albumService;
            _bandService = bandService;
            _distributorService = distributorService;
        }

        public async Task SynchronizeAllAlbums()
        {
            var distributors = await _distributorService.GetAllDistributors();

            foreach (var distributor in distributors)
            {
                await SynchronizeAlbumsFromDistributor(distributor);
            }
        }

        private async Task SynchronizeAlbumsFromDistributor(Distributor distributor)
        {
            var parser = _parserFactory.CreateParser(distributor.Code);

            var parsedAlbums = await parser.ParseAlbums(distributor.ParsingUrl);
            var existingAlbums = await _albumService.GetAlbumsByDistributor(distributor.Id);

            var bandCache = new Dictionary<string, Band>();
            var albumsWithPriceChanges = new Dictionary<Guid, float>();

            foreach (var parsedAlbum in parsedAlbums)
            {
                var existingAlbum = existingAlbums.FirstOrDefault(existingAlbum => existingAlbum.SKU == parsedAlbum.SKU);

                if (existingAlbum == null)
                {
                    var band = await GetOrAddBand(bandCache, parsedAlbum.BandName);
                    var newAlbum = MapParsedAlbumToEntity(parsedAlbum, band);
                    await _albumService.AddAlbum(newAlbum);
                }
                else
                {
                    AddAlbumToPriceChangeList(existingAlbum, parsedAlbum, albumsWithPriceChanges);
                }
            }

            await UpdateAlbumPrices(albumsWithPriceChanges);

            await MarkAlbumsAsUnavailable(existingAlbums, parsedAlbums);
        }

        private async Task<Band> GetOrAddBand(Dictionary<string, Band> bandCache, string bandName)
        {
            if (!bandCache.TryGetValue(bandName, out var band))
            {
                band = await _bandService.GetBandByName(bandName);

                if (band == null)
                {
                    band = new Band { Id = Guid.NewGuid(), Name = bandName };

                    await _bandService.AddBand(band);
                }

                bandCache[bandName] = band;
            }

            return band;
        }

        private Album MapParsedAlbumToEntity(AlbumDto albumDto, Band band)
        {
            return new Album
            {
                Id = Guid.NewGuid(),
                BandId = band.Id,
                SKU = albumDto.SKU,
                Name = albumDto.Name,
                ReleaseDate = albumDto.ReleaseDate,
                Genre = albumDto.Genre,
                Price = albumDto.Price,
                PurchaseUrl = albumDto.PurchaseUrl,
                PhotoUrl = albumDto.PhotoUrl,
                Media = (MediaType)albumDto.Media,
                Label = albumDto.Label,
                Press = albumDto.Press,
                Description = albumDto.Description,
                Status = (AlbumStatus)albumDto.Status,
                ModificationTime = DateTime.UtcNow
            };
        }

        private void AddAlbumToPriceChangeList(Album existingAlbum, AlbumDto parsedAlbum, Dictionary<Guid, float> albumsToUpdate)
        {
            if (existingAlbum.Price != parsedAlbum.Price)
            {
                albumsToUpdate.Add(existingAlbum.Id, parsedAlbum.Price);
            }
        }

        private async Task UpdateAlbumPrices(Dictionary<Guid, float> albumsWithPriceChanges)
        {
            if (albumsWithPriceChanges.Any())
            {
                await _albumService.UpdateAlbumPrices(albumsWithPriceChanges);
            }
        }

        private async Task MarkAlbumsAsUnavailable(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            var parsedAlbumsSet = new HashSet<string>(parsedAlbums.Select(album => album.SKU));

            var albumsToMarkUnavailable = new List<Guid>();

            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbumsSet.Contains(existingAlbum.SKU))
                {
                    albumsToMarkUnavailable.Add(existingAlbum.Id);
                }
            }

            if (albumsToMarkUnavailable.Any())
            {
                await _albumService.UpdateAlbumsStatus(albumsToMarkUnavailable, AlbumStatus.Unavailable);
            }
        }
    }
}
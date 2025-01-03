﻿using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class BandRepository : IBandRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;

        public BandRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Band> GetById(Guid id)
        {
            var band = await _dbContext.Bands
                    .AsNoTracking()
                    .FirstOrDefaultAsync(band => band.Id == id);

            return _mapper.Map<Band>(band);
        }

        public async Task<IEnumerable<Band>> GetAll()
        {
            var bands = await _dbContext.Bands
                    .AsNoTracking()
                    .ToListAsync();
            return _mapper.Map<IEnumerable<Band>>(bands);
        }

        public async Task<Band> GetByName(string bandName)
        {
            var bands = await _dbContext.Bands
                    .AsNoTracking()
                    .Where(band => band.Name.ToLower() == bandName.ToLower())
                    .FirstOrDefaultAsync();

            return _mapper.Map<Band>(bands);
        }

        public async Task Add(Band band)
        {
            var bandEntity = _mapper.Map<BandEntity>(band);
            await _dbContext.Bands.AddAsync(bandEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Update(Band band)
        {
            var existingBand = await _dbContext.Bands.FindAsync(band.Id);

            if (existingBand == null)
            {
                return false;
            }

            _mapper.Map(band, existingBand);

            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var existingBand = await _dbContext.Bands.FindAsync(id);

            if (existingBand == null)
            {
                return false;
            }

            _dbContext.Bands.Remove(existingBand);
            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<IEnumerable<Band>> GetByFilter(PagingAndSortingFilter filter)
        {
            var query = _dbContext.Bands
                .AsQueryable();

            query = query.Skip(filter.Skip).Take(filter.Take);

            var bands = await query
                .ProjectTo<Band>(_mapper.ConfigurationProvider)
                .AsNoTracking()
                .ToListAsync();

            return bands;
        }
    }
}

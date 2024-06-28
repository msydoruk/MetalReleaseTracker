using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public abstract class BaseRepository<TRepository>
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<TRepository> _logger;

        protected BaseRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<TRepository> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        protected MetalReleaseTrackerDbContext DbContext => _dbContext;

        protected IMapper Mapper => _mapper;

        protected ILogger<TRepository> Logger => _logger;

        protected async Task HandleDbUpdateException(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred.");
                throw;
            }
        }

        protected async Task<T> HandleDbUpdateException<T>(Func<Task<T>> operation)
        {
            try
            {
                return await operation();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "A database update error occurred.");
                throw;
            }
        }
    }
}

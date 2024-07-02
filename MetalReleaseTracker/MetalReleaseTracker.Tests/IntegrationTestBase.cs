using AutoMapper;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;

using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.Tests
{
    public abstract class IntegrationTestBase
    {
        protected readonly MetalReleaseTrackerDbContext DbContext;
        protected readonly IMapper Mapper;

        protected IntegrationTestBase()
        {
            var options = new DbContextOptionsBuilder<MetalReleaseTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            DbContext = new MetalReleaseTrackerDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            Mapper = config.CreateMapper();

            InitializeData(DbContext);
        }

        protected abstract void InitializeData(MetalReleaseTrackerDbContext context);

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}

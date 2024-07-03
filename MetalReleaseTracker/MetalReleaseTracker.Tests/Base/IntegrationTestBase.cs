using AutoMapper;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;

namespace MetalReleaseTracker.Tests.Base
{
    public abstract class IntegrationTestBase
    {
        protected readonly MetalReleaseTrackerDbContext DbContext;
        protected readonly IMapper Mapper;

        protected IntegrationTestBase()
        {
            DbContext = TestDbContextFactory.CreateDbContext();
            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

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
            DbContext.Database.EnsureDeleted(); 
            DbContext.Dispose(); 
        }
    }
}

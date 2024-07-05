using AutoMapper;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;

namespace MetalReleaseTracker.Tests.Base
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly MetalReleaseTrackerDbContext DbContext;
        protected readonly IMapper Mapper;

        protected IntegrationTestBase()
        {
            DbContext = TestDbContextFactory.CreateDbContext();
            Mapper = InitializeMapper();

            InitializeData(DbContext);
        }

        protected virtual IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            return config.CreateMapper();
        }

        protected abstract void InitializeData(MetalReleaseTrackerDbContext context);

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}

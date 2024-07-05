using AutoMapper;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;

namespace MetalReleaseTracker.Tests.Base
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly MetalReleaseTrackerDbContext DbContext;
        protected readonly IMapper Mapper;
        private bool _disposed;

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (DbContext != null)
                    {
                        DbContext.Database.EnsureDeleted();
                        DbContext.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        ~IntegrationTestBase()
        {
            Dispose(false);
        }
    }
}

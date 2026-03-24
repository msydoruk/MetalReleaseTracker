namespace MetalReleaseTracker.CoreDataService.Data;

using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Configurations;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using Microsoft.EntityFrameworkCore;

public class CoreDataServiceDbContext : DbContext
{
    public CoreDataServiceDbContext(DbContextOptions<CoreDataServiceDbContext> options) : base(options)
    {
    }

    public DbSet<AlbumEntity> Albums { get; set; }

    public DbSet<BandEntity> Bands { get; set; }

    public DbSet<DistributorEntity> Distributors { get; set; }

    public DbSet<UserFavoriteEntity> UserFavorites { get; set; }

    public DbSet<ReviewEntity> Reviews { get; set; }

    public DbSet<AlbumChangeLogEntity> AlbumChangeLogs { get; set; }

    public DbSet<AlbumRatingEntity> AlbumRatings { get; set; }

    public DbSet<UserFollowedBandEntity> UserFollowedBands { get; set; }

    public DbSet<UserAlbumWatchEntity> UserAlbumWatches { get; set; }

    public DbSet<UserNotificationEntity> UserNotifications { get; set; }

    public DbSet<TelegramLinkEntity> TelegramLinks { get; set; }

    public DbSet<TelegramLinkTokenEntity> TelegramLinkTokens { get; set; }

    public DbSet<SettingEntity> Settings { get; set; }

    public DbSet<NewsArticleEntity> NewsArticles { get; set; }

    public DbSet<NavigationItemEntity> NavigationItems { get; set; }

    public DbSet<TranslationEntity> Translations { get; set; }

    public DbSet<CurrencyRateEntity> CurrencyRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SettingEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NewsArticleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new NavigationItemEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TranslationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new CurrencyRateEntityConfiguration());

        modelBuilder.Entity<AlbumEntity>()
            .Property(album => album.Media)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<AlbumEntity>()
            .Property(album => album.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<AlbumEntity>()
            .Property(album => album.StockStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<AlbumEntity>()
            .HasIndex(album => album.SKU)
            .IsUnique();

        modelBuilder.Entity<AlbumEntity>()
            .HasIndex(album => album.Slug)
            .IsUnique();

        modelBuilder.Entity<BandEntity>()
            .HasIndex(band => band.Slug)
            .IsUnique();

        modelBuilder.Entity<UserFavoriteEntity>()
            .HasIndex(favorite => new { favorite.UserId, favorite.AlbumId })
            .IsUnique();

        modelBuilder.Entity<UserFavoriteEntity>()
            .Property(favorite => favorite.Status)
            .HasConversion<int>();

        modelBuilder.Entity<AlbumChangeLogEntity>()
            .HasIndex(changeLog => changeLog.ChangedAt);

        modelBuilder.Entity<AlbumRatingEntity>()
            .HasIndex(rating => new { rating.UserId, rating.AlbumId })
            .IsUnique();

        modelBuilder.Entity<UserFollowedBandEntity>()
            .HasIndex(follow => new { follow.UserId, follow.BandId })
            .IsUnique();

        modelBuilder.Entity<UserAlbumWatchEntity>()
            .HasIndex(watch => new { watch.UserId, watch.BandId, watch.CanonicalTitle })
            .IsUnique();

        modelBuilder.Entity<UserNotificationEntity>()
            .Property(notification => notification.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<UserNotificationEntity>()
            .HasIndex(notification => new { notification.UserId, notification.IsRead, notification.CreatedDate });

        modelBuilder.Entity<UserNotificationEntity>()
            .HasIndex(notification => new { notification.UserId, notification.CreatedDate });

        modelBuilder.Entity<TelegramLinkEntity>()
            .HasIndex(link => link.UserId)
            .IsUnique();

        modelBuilder.Entity<TelegramLinkEntity>()
            .HasIndex(link => link.ChatId)
            .IsUnique();

        modelBuilder.Entity<TelegramLinkTokenEntity>()
            .HasIndex(token => token.Token)
            .IsUnique();
    }
}
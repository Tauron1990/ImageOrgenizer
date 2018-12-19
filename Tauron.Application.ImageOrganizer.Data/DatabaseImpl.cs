using EntityFrameworkCore.Triggers;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLog.Extensions.Logging;
using Tauron.Application.ImageOrganizer.BL;
using Tauron.Application.ImageOrganizer.Core;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data
{
    public class DatabaseImpl : DbContextWithTriggers
    {
        static DatabaseImpl()
        {
            Triggers<ImageEntity>.Inserted += entry => DataTrigger.OnChangedEvent(TriggerType.Insert, entry.Entity);
            Triggers<ImageEntity>.Deleted += entry => DataTrigger.OnChangedEvent(TriggerType.Delete, entry.Entity);
            Triggers<ImageEntity>.Updated += entry => DataTrigger.OnChangedEvent(TriggerType.Update, entry.Entity);

            Triggers<TagEntity>.Inserted += entry => DataTrigger.OnChangedEvent(TriggerType.Insert, entry.Entity);
            Triggers<TagEntity>.Deleted += entry => DataTrigger.OnChangedEvent(TriggerType.Delete, entry.Entity);
            Triggers<TagEntity>.Updated += entry => DataTrigger.OnChangedEvent(TriggerType.Update, entry.Entity);

            Triggers<TagTypeEntity>.Inserted += entry => DataTrigger.OnChangedEvent(TriggerType.Insert, entry.Entity);
            Triggers<TagTypeEntity>.Deleted += entry => DataTrigger.OnChangedEvent(TriggerType.Delete, entry.Entity);
            Triggers<TagTypeEntity>.Updated += entry => DataTrigger.OnChangedEvent(TriggerType.Update, entry.Entity);
        }

        private static ISettingsManager _settingsManager;
        private static readonly NLogLoggerFactory _logLogger = new NLogLoggerFactory();

        [NotNull]
        private static string GetLocation()
        {
            if (_settingsManager == null)
                _settingsManager = CommonApplication.Current?.Container?.Resolve<ISettingsManager>(null, true);
            return _settingsManager?.Settings?.CurrentDatabase ?? string.Empty;
        }

        private readonly string _location;

        [UsedImplicitly]
        public DatabaseImpl()
            : this(string.Empty) { }

        private DatabaseImpl(string location) => _location = location;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = string.IsNullOrWhiteSpace(_location) ? GetLocation() : _location
            }.ConnectionString);

            #if (DEBUG)
            //optionsBuilder.UseLoggerFactory(_logLogger);
            //optionsBuilder.EnableDetailedErrors();
            //optionsBuilder.EnableSensitiveDataLogging();
            #endif

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DownloadEntity>().Property(p => p.Metadata).HasConversion(new ValueConverter<string, string>(s => string.IsNullOrWhiteSpace(s) ? null : s, s => s));

            modelBuilder.Entity<ImageEntity>().HasIndex(i => i.Name).IsUnique(false);
            modelBuilder.Entity<TagEntity>().HasIndex(e => e.Name).IsUnique();

            modelBuilder.Entity<TagEntity>()
                .HasOne(te => te.Type)
                .WithMany(tte => tte.Tags);

            modelBuilder.Entity<ImageTag>()
                .HasKey(bc => new { bc.TagEntityId, bc.ImageEntityId });

            modelBuilder.Entity<ImageTag>()
                .HasOne(bc => bc.ImageEntity)
                .WithMany(b => b.Tags)
                .HasForeignKey(bc => bc.ImageEntityId);

            modelBuilder.Entity<ImageTag>()
                .HasOne(bc => bc.TagEntity)
                .WithMany(c => c.Images)
                .HasForeignKey(bc => bc.TagEntityId);

            modelBuilder.Entity<ImageEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<TagTypeEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<TagEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<ProfileEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<OptionEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<TagEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
            modelBuilder.Entity<DownloadEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

            base.OnModelCreating(modelBuilder);
        }

        public static void UpdateSchema(string location)
        {
            try
            {
                using (var db = new DatabaseImpl(location))
                {
                    //Batteries_V2.Init();
                    db.Database.Migrate();
                    db.SaveChanges();
                }
            }
            catch
            {
                // ignored
            }
        }

        // ReSharper disable UnusedMember.Global
        public DbSet<ImageEntity> Images { get; set; }

        public DbSet<TagEntity> Tags { get; set; }

        public DbSet<TagTypeEntity> TagTypes { get; set; }

        public DbSet<OptionEntity> Options { get; set; }

        public DbSet<ProfileEntity> Profiles { get; set; }

        public DbSet<DownloadEntity> Downloads { get; set; }
        // ReSharper restore UnusedMember.Global

    }
}
using EntityFrameworkCore.Triggers;
using ImageOrganizer.BL;
using ImageOrganizer.Data.Entities;
using ImageOrganizer.Properties;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;

namespace ImageOrganizer.Data
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

        private readonly string _location;

        [UsedImplicitly]
        public DatabaseImpl()
            : this(string.Empty)
        {
        }

        private DatabaseImpl(string location)
        {
            _location = location;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = string.IsNullOrWhiteSpace(_location) ? Settings.Default.CurrentDatabase : _location
            }.ConnectionString);

            optionsBuilder.UseLoggerFactory(new NLogLoggerFactory());
            optionsBuilder.EnableSensitiveDataLogging();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageEntity>().HasIndex(i => i.Name).IsUnique(false);

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
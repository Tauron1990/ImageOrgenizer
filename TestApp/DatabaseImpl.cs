﻿using EntityFrameworkCore.Triggers;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.Data
{
    public class DatabaseImpl : DbContextWithTriggers
    {
        private readonly string _location;

        [UsedImplicitly]
        public DatabaseImpl()
            : this(string.Empty) { }

        public DatabaseImpl(string location) => _location = location;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = _location
            }.ConnectionString);
            

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageEntity>().HasIndex(i => i.Name).IsUnique(false);
            modelBuilder.Entity<TagEntity>().HasIndex(e => e.Name).IsUnique();

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
            modelBuilder.Entity<TagEntity>().HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

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

        //public DbSet<OptionEntity> Options { get; set; }

        //public DbSet<ProfileEntity> Profiles { get; set; }

        //public DbSet<DownloadEntity> Downloads { get; set; }
        // ReSharper restore UnusedMember.Global

    }
}
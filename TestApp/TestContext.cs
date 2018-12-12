using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TestApp
{
    public class TestContext : DbContext
    {
        public DbSet<TestData> TestDatas { get; set; }

        public DbSet<TestData2> TestData2s { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder {DataSource = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test.db")}.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestDataConnector>()
                .HasKey(bc => new { bc.TestData2Id, bc.TestDataId });

            modelBuilder.Entity<TestDataConnector>()
                .HasOne(bc => bc.TestData)
                .WithMany(b => b.Connectors)
                .HasForeignKey(bc => bc.TestDataId);

            modelBuilder.Entity<TestDataConnector>()
                .HasOne(bc => bc.TestData2)
                .WithMany(c => c.Connectors)
                .HasForeignKey(bc => bc.TestData2Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
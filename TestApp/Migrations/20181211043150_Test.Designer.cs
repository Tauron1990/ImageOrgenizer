﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestApp;

namespace TestApp.Migrations
{
    [DbContext(typeof(TestContext))]
    [Migration("20181211043150_Test")]
    partial class Test
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity("TestApp.TestData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("TestProp2");

                    b.HasKey("Id");

                    b.ToTable("TestDatas");
                });

            modelBuilder.Entity("TestApp.TestData2", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("TestProp");

                    b.HasKey("Id");

                    b.ToTable("TestData2s");
                });

            modelBuilder.Entity("TestApp.TestDataConnector", b =>
                {
                    b.Property<int>("TestData2Id");

                    b.Property<int>("TestDataId");

                    b.HasKey("TestData2Id", "TestDataId");

                    b.HasIndex("TestDataId");

                    b.ToTable("TestDataConnector");
                });

            modelBuilder.Entity("TestApp.TestDataConnector", b =>
                {
                    b.HasOne("TestApp.TestData2", "TestData2")
                        .WithMany("Connectors")
                        .HasForeignKey("TestData2Id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TestApp.TestData", "TestData")
                        .WithMany("Connectors")
                        .HasForeignKey("TestDataId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
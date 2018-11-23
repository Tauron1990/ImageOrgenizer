using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tauron.Application.ImageOrganizer.Data.Migrations
{
    public partial class V1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Downloads",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Image = table.Column<string>(nullable: true),
                    DownloadType = table.Column<int>(nullable: false),
                    Schedule = table.Column<DateTime>(nullable: false),
                    DownloadStade = table.Column<int>(nullable: false),
                    FailedCount = table.Column<int>(nullable: false),
                    Provider = table.Column<string>(nullable: true),
                    RemoveImageOnFail = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    ProviderName = table.Column<string>(nullable: true),
                    RandomCount = table.Column<int>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false),
                    Favorite = table.Column<bool>(nullable: false),
                    Added = table.Column<DateTime>(nullable: false),
                    Author = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    CurrentPosition = table.Column<int>(nullable: false),
                    NextImage = table.Column<int>(nullable: false),
                    FilterString = table.Column<string>(nullable: true),
                    CurrentImages = table.Column<int>(nullable: false),
                    PageType = table.Column<string>(nullable: true),
                    Favorite = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagTypes",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Color = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TypeId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_TagTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TagTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageTag",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageEntityId = table.Column<int>(nullable: false),
                    TagEntityId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageTag_Images_ImageEntityId",
                        column: x => x.ImageEntityId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageTag_Tags_TagEntityId",
                        column: x => x.TagEntityId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Images_Name",
                table: "Images",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTag_ImageEntityId",
                table: "ImageTag",
                column: "ImageEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTag_TagEntityId",
                table: "ImageTag",
                column: "TagEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TypeId",
                table: "Tags",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downloads");

            migrationBuilder.DropTable(
                name: "ImageTag");

            migrationBuilder.DropTable(
                name: "Options");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagTypes");
        }
    }
}

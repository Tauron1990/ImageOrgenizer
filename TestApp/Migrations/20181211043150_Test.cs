using Microsoft.EntityFrameworkCore.Migrations;

namespace TestApp.Migrations
{
    public partial class Test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestData2s",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestProp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestData2s", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestDatas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestProp2 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDatas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestDataConnector",
                columns: table => new
                {
                    TestDataId = table.Column<int>(nullable: false),
                    TestData2Id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDataConnector", x => new { x.TestData2Id, x.TestDataId });
                    table.ForeignKey(
                        name: "FK_TestDataConnector_TestData2s_TestData2Id",
                        column: x => x.TestData2Id,
                        principalTable: "TestData2s",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestDataConnector_TestDatas_TestDataId",
                        column: x => x.TestDataId,
                        principalTable: "TestDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestDataConnector_TestDataId",
                table: "TestDataConnector",
                column: "TestDataId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestDataConnector");

            migrationBuilder.DropTable(
                name: "TestData2s");

            migrationBuilder.DropTable(
                name: "TestDatas");
        }
    }
}

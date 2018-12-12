using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageOrganizer.Migrations
{
    public partial class V3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailedReason",
                table: "Downloads",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedReason",
                table: "Downloads");
        }
    }
}

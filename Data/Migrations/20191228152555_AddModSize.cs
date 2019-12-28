using Microsoft.EntityFrameworkCore.Migrations;

namespace ModBrowser.Data.Migrations
{
    public partial class AddModSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "Mod",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Mod");
        }
    }
}

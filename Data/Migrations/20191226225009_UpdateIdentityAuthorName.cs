using Microsoft.EntityFrameworkCore.Migrations;

namespace Chireiden.ModBrowser.Data.Migrations
{
    public partial class UpdateIdentityAuthorName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "AspNetUsers");
        }
    }
}

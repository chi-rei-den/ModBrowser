using Microsoft.EntityFrameworkCore.Migrations;

namespace Chireiden.ModBrowser.Data.Migrations
{
    public partial class AddLocalizerPackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Language = table.Column<string>(nullable: true),
                    CreateTimeStamp = table.Column<string>(nullable: true),
                    UpdateTimeStamp = table.Column<string>(nullable: true),
                    ModVersion = table.Column<string>(nullable: true),
                    ModName = table.Column<string>(nullable: true),
                    UploaderId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Package_Mod_ModName",
                        column: x => x.ModName,
                        principalTable: "Mod",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Package_AspNetUsers_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Package_ModName",
                table: "Package",
                column: "ModName");

            migrationBuilder.CreateIndex(
                name: "IX_Package_UploaderId",
                table: "Package",
                column: "UploaderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Package");
        }
    }
}

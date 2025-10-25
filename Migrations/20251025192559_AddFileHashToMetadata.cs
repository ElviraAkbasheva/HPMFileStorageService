using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HPMFileStorageService.Migrations
{
    /// <inheritdoc />
    public partial class AddFileHashToMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "Files",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "Files");
        }
    }
}

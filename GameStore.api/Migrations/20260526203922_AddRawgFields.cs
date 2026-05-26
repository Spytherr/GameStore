using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRawgFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Games",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RawgId",
                table: "Games",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_RawgId",
                table: "Games",
                column: "RawgId",
                unique: true,
                filter: "[RawgId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_RawgId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RawgId",
                table: "Games");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDescriptionWithCreatorsPublishers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "Creators",
                table: "Games",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publishers",
                table: "Games",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creators",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Publishers",
                table: "Games");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}

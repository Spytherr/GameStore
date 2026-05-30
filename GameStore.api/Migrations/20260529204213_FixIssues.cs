using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.api.Migrations
{
    /// <inheritdoc />
    public partial class FixIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlatformName",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatformName",
                table: "OrderItems");
        }
    }
}

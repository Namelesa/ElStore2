using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElStore.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInDescriptionPC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "DescriptionPC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "DescriptionPC",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

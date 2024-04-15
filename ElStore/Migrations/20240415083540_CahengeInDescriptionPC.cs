using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElStore.Migrations
{
    /// <inheritdoc />
    public partial class CahengeInDescriptionPC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Display",
                table: "DescriptionPC",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Display",
                table: "DescriptionPC");
        }
    }
}

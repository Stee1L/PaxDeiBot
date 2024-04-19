using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaxDeiBot.Migrations
{
    /// <inheritdoc />
    public partial class ComponentsCounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Count",
                table: "ItemComponents",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "ItemComponents");
        }
    }
}

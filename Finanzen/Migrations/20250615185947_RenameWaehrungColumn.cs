using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finanzen.Migrations
{
    /// <inheritdoc />
    public partial class RenameWaehrungColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Währung",
                table: "Transactions",
                newName: "Waehrung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Waehrung",
                table: "Transactions",
                newName: "Währung");
        }
    }
}

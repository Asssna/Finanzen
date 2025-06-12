using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finanzen.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceSpalten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Kontostand",
                table: "Balances",
                newName: "Kontostand_Ursprung");

            migrationBuilder.AddColumn<decimal>(
                name: "Kontostand_Aktuell",
                table: "Balances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kontostand_Aktuell",
                table: "Balances");

            migrationBuilder.RenameColumn(
                name: "Kontostand_Ursprung",
                table: "Balances",
                newName: "Kontostand");
        }
    }
}

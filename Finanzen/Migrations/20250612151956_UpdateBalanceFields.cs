using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finanzen.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBalanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Kontostand_Ursprung0",
                table: "Balances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kontostand_Ursprung0",
                table: "Balances");
        }
    }
}

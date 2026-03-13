using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promix.Financials.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Accounts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Accounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemRole",
                table: "Accounts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CompanyId_SystemRole",
                table: "Accounts",
                columns: new[] { "CompanyId", "SystemRole" },
                unique: true,
                filter: "[SystemRole] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_CompanyId_SystemRole",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SystemRole",
                table: "Accounts");
        }
    }
}

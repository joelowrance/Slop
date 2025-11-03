using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdaVidaLawnCare.CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddressIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_customers_address",
                table: "customers",
                column: "address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_customers_address",
                table: "customers");
        }
    }
}

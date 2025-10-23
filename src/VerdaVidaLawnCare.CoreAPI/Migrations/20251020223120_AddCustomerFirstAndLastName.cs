using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdaVidaLawnCare.CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFirstAndLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "customers");

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "last_name",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "first_name",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "last_name",
                table: "customers");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "customers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}

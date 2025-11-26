using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerdaVidaLawnCare.CoreAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCompletionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_date",
                table: "estimates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "completion_notes",
                table: "estimates",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "scheduled_date",
                table: "estimates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_estimates_completed_date",
                table: "estimates",
                column: "completed_date");

            migrationBuilder.CreateIndex(
                name: "ix_estimates_scheduled_date",
                table: "estimates",
                column: "scheduled_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_estimates_completed_date",
                table: "estimates");

            migrationBuilder.DropIndex(
                name: "ix_estimates_scheduled_date",
                table: "estimates");

            migrationBuilder.DropColumn(
                name: "completed_date",
                table: "estimates");

            migrationBuilder.DropColumn(
                name: "completion_notes",
                table: "estimates");

            migrationBuilder.DropColumn(
                name: "scheduled_date",
                table: "estimates");
        }
    }
}

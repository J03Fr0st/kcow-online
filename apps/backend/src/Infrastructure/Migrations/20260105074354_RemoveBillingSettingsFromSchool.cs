using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBillingSettingsFromSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_settings",
                table: "schools");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billing_settings",
                table: "schools",
                type: "TEXT",
                nullable: true);
        }
    }
}

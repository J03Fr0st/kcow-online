using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_activities_code",
                table: "activities",
                newName: "IX_activities_code");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "activities",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "activities");

            migrationBuilder.RenameIndex(
                name: "IX_activities_code",
                table: "activities",
                newName: "ix_activities_code");
        }
    }
}

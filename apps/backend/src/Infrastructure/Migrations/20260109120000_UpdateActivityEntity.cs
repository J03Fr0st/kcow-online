using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename columns to match English field names
            migrationBuilder.RenameColumn(
                name: "program",
                table: "activities",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "program_name",
                table: "activities",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "educational_focus",
                table: "activities",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "grade",
                table: "activities",
                newName: "grade_level");

            // Change Icon from BLOB to TEXT for base64 string storage
            migrationBuilder.AlterColumn<string>(
                name: "icon",
                table: "activities",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            // Add new columns for soft-delete and audit
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "activities",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "datetime('now')");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "activities",
                type: "TEXT",
                nullable: true);

            // Create index on Code for duplicate checking
            migrationBuilder.CreateIndex(
                name: "ix_activities_code",
                table: "activities",
                column: "code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop index
            migrationBuilder.DropIndex(
                name: "ix_activities_code",
                table: "activities");

            // Remove new columns
            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "activities");

            // Revert Icon back to BLOB
            migrationBuilder.AlterColumn<byte[]>(
                name: "icon",
                table: "activities",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            // Rename columns back to original XSD names
            migrationBuilder.RenameColumn(
                name: "grade_level",
                table: "activities",
                newName: "grade");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "activities",
                newName: "educational_focus");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "activities",
                newName: "program_name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "activities",
                newName: "program");
        }
    }
}

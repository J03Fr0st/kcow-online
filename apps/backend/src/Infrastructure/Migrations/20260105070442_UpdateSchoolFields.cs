using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchoolFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_schools_is_active",
                table: "schools");

            migrationBuilder.DropIndex(
                name: "IX_schools_name",
                table: "schools");

            // Rename existing columns
            migrationBuilder.RenameColumn(
                name: "contact_name",
                table: "schools",
                newName: "contact_person");

            migrationBuilder.RenameColumn(
                name: "contact_phone",
                table: "schools",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                table: "schools",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "schools",
                newName: "scheduling_notes");

            // Add new columns
            migrationBuilder.AddColumn<string>(
                name: "address2",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "afterschool1_contact",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "afterschool1_name",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "afterschool2_contact",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "afterschool2_name",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "circulars_email",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_cell",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fax",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fee_description",
                table: "schools",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "formula",
                table: "schools",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "headmaster",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "headmaster_cell",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "import_flag",
                table: "schools",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "kcow_web_page_link",
                table: "schools",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "language",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "money_message",
                table: "schools",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "schools",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "print_invoice",
                table: "schools",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "safe_notes",
                table: "schools",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "short_name",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telephone",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "truck_id",
                table: "schools",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visit_day",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visit_sequence",
                table: "schools",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "web_page",
                table: "schools",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_is_active_name",
                table: "schools",
                columns: new[] { "is_active", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_schools_is_active_name",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "web_page",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "visit_sequence",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "visit_day",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "truck_id",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "telephone",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "short_name",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "safe_notes",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "print_invoice",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "price",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "money_message",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "language",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "kcow_web_page_link",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "import_flag",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "headmaster_cell",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "headmaster",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "formula",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "fee_description",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "fax",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "contact_cell",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "circulars_email",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "afterschool2_name",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "afterschool2_contact",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "afterschool1_name",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "afterschool1_contact",
                table: "schools");

            migrationBuilder.DropColumn(
                name: "address2",
                table: "schools");

            // Reverse renames
            migrationBuilder.RenameColumn(
                name: "scheduling_notes",
                table: "schools",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "schools",
                newName: "contact_email");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "schools",
                newName: "contact_phone");

            migrationBuilder.RenameColumn(
                name: "contact_person",
                table: "schools",
                newName: "contact_name");

            migrationBuilder.CreateIndex(
                name: "IX_schools_is_active",
                table: "schools",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_schools_name",
                table: "schools",
                column: "name");
        }
    }
}

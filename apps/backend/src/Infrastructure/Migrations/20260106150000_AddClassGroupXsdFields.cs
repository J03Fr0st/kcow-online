using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassGroupXsdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add 8 missing XSD fields to achieve full schema compliance
            migrationBuilder.AddColumn<string>(
                name: "day_truck",
                table: "class_groups",
                type: "TEXT",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "class_groups",
                type: "TEXT",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "evaluate",
                table: "class_groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "import_flag",
                table: "class_groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "group_message",
                table: "class_groups",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "send_certificates",
                table: "class_groups",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "money_message",
                table: "class_groups",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ixl",
                table: "class_groups",
                type: "TEXT",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "day_truck",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "description",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "evaluate",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "import_flag",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "group_message",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "send_certificates",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "money_message",
                table: "class_groups");

            migrationBuilder.DropColumn(
                name: "ixl",
                table: "class_groups");
        }
    }
}

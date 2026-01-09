using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    family_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    primary_contact_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "student_families",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "INTEGER", nullable: false),
                    family_id = table.Column<int>(type: "INTEGER", nullable: false),
                    relationship_type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_families", x => new { x.student_id, x.family_id });
                    table.ForeignKey(
                        name: "FK_student_families_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_families_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_families_family_id",
                table: "student_families",
                column: "family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_families");

            migrationBuilder.DropTable(
                name: "families");
        }
    }
}

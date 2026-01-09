using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CodeReviewFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_student_families_family_id",
                table: "student_families",
                newName: "ix_student_families_family_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_families_student_id",
                table: "student_families",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_student_families_student_id",
                table: "student_families");

            migrationBuilder.RenameIndex(
                name: "ix_student_families_family_id",
                table: "student_families",
                newName: "IX_student_families_family_id");
        }
    }
}

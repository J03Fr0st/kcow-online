using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kcow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    program = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    program_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    educational_focus = table.Column<string>(type: "TEXT", nullable: true),
                    folder = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    grade = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    icon = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schools",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    short_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    school_description = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    truck_id = table.Column<int>(type: "INTEGER", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    fee_description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    formula = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    visit_day = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    visit_sequence = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    contact_person = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    contact_cell = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    telephone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    fax = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    circulars_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    address2 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    headmaster = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    headmaster_cell = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    print_invoice = table.Column<bool>(type: "INTEGER", nullable: false),
                    import_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    afterschool1_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    afterschool1_contact = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    afterschool2_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    afterschool2_contact = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    scheduling_notes = table.Column<string>(type: "TEXT", nullable: true),
                    money_message = table.Column<string>(type: "TEXT", nullable: true),
                    safe_notes = table.Column<string>(type: "TEXT", nullable: true),
                    web_page = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    omsendbriewe = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    kcow_web_page_link = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schools", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "trucks",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    registration_number = table.Column<string>(type: "TEXT", nullable: false),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trucks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "class_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    day_truck = table.Column<string>(type: "TEXT", maxLength: 6, nullable: true),
                    description = table.Column<string>(type: "TEXT", maxLength: 35, nullable: true),
                    school_id = table.Column<int>(type: "INTEGER", nullable: false),
                    truck_id = table.Column<int>(type: "INTEGER", nullable: true),
                    day_of_week = table.Column<int>(type: "INTEGER", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    evaluate = table.Column<bool>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    import_flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    group_message = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    send_certificates = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    money_message = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ixl = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_groups_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_groups_trucks_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    reference = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    first_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    gender = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    language = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    account_person_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    account_person_surname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    account_person_id_number = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    account_person_cellphone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    account_person_office = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    account_person_home = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    account_person_email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    relation = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    mother_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    mother_surname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    mother_office = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    mother_cell = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    mother_home = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    mother_email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    father_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    father_surname = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    father_office = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    father_cell = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    father_home = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    father_email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    address1 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    address2 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    postal_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    school_name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    school_id = table.Column<int>(type: "INTEGER", nullable: true),
                    class_group_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    class_group_id = table.Column<int>(type: "INTEGER", nullable: true),
                    grade = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    teacher = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    attending_kcow_at = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    aftercare = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    extra = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    home_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    start_classes = table.Column<DateTime>(type: "TEXT", nullable: true),
                    terms = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    seat = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    truck = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    family = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    sequence = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    financial_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    charge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    deposit = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    pay_date = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    tshirt_code = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    tshirt_money_1 = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    tshirt_money_date_1 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    tshirt_received_1 = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    tshirt_rec_date_1 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    receive_note_1 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    tshirt_size1 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    tshirt_color1 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    tshirt_design1 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    tshirt_size2 = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    tshirt_money_2 = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    tshirt_money_date_2 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    tshirt_received_2 = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    tshirt_rec_date_2 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    receive_note_2 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    tshirt_color2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    tshirt_design2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    indicator1 = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    indicator2 = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    general_note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    print_id_card = table.Column<bool>(type: "INTEGER", nullable: false),
                    accept_terms_cond = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    sms_or_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    school_close = table.Column<DateTime>(type: "TEXT", nullable: true),
                    cnt = table.Column<float>(type: "REAL", nullable: true),
                    online_entry = table.Column<int>(type: "INTEGER", nullable: true),
                    created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    submitted = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    book_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    report1_given_out = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    account_given_out = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    certificate_printed = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    report2_given_out = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    social = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    activity_report_given_out = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    photo_url = table.Column<string>(type: "TEXT", nullable: true),
                    photo_updated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                    table.ForeignKey(
                        name: "FK_students_class_groups_class_group_id",
                        column: x => x.class_group_id,
                        principalTable: "class_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_students_schools_school_id",
                        column: x => x.school_id,
                        principalTable: "schools",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_class_groups_school_day_time",
                table: "class_groups",
                columns: new[] { "school_id", "day_of_week", "start_time" });

            migrationBuilder.CreateIndex(
                name: "IX_class_groups_truck_id",
                table: "class_groups",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schools_is_active_name",
                table: "schools",
                columns: new[] { "is_active", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_students_class_group_id",
                table: "students",
                column: "class_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_students_last_name",
                table: "students",
                column: "last_name");

            migrationBuilder.CreateIndex(
                name: "ix_students_reference",
                table: "students",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_school_id",
                table: "students",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_trucks_is_active",
                table: "trucks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_trucks_registration_number",
                table: "trucks",
                column: "registration_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "class_groups");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "schools");

            migrationBuilder.DropTable(
                name: "trucks");
        }
    }
}

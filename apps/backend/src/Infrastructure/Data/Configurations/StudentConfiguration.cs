using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kcow.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Student entity aligned with legacy XSD schema (Children.xsd - 92 fields).
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        // XSD Field: "Reference" (10 chars max, required)
        builder.Property(s => s.Reference)
            .HasColumnName("reference")
            .HasMaxLength(10)
            .IsRequired();

        // XSD Field: "Child_Name" (50 chars max)
        builder.Property(s => s.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(50);

        // XSD Field: "Child_Surname" (50 chars max)
        builder.Property(s => s.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(50);

        // XSD Field: "Child_birthdate" (datetime)
        builder.Property(s => s.DateOfBirth)
            .HasColumnName("date_of_birth");

        // XSD Field: "Sex" (3 chars max)
        builder.Property(s => s.Gender)
            .HasColumnName("gender")
            .HasMaxLength(3);

        // XSD Field: "Language" (3 chars max)
        builder.Property(s => s.Language)
            .HasColumnName("language")
            .HasMaxLength(3);

        // Account Person Fields (Responsible Adult)
        // XSD Field: "Account_Person_Name" (50 chars max)
        builder.Property(s => s.AccountPersonName)
            .HasColumnName("account_person_name")
            .HasMaxLength(50);

        // XSD Field: "Account_Person_Surname" (50 chars max)
        builder.Property(s => s.AccountPersonSurname)
            .HasColumnName("account_person_surname")
            .HasMaxLength(50);

        // XSD Field: "Account_Person_Idnumber" (20 chars max)
        builder.Property(s => s.AccountPersonIdNumber)
            .HasColumnName("account_person_id_number")
            .HasMaxLength(20);

        // XSD Field: "Account_Person_Cellphone" (20 chars max)
        builder.Property(s => s.AccountPersonCellphone)
            .HasColumnName("account_person_cellphone")
            .HasMaxLength(20);

        // XSD Field: "Account_Person_Office" (20 chars max)
        builder.Property(s => s.AccountPersonOffice)
            .HasColumnName("account_person_office")
            .HasMaxLength(20);

        // XSD Field: "Account_Person_Home" (20 chars max)
        builder.Property(s => s.AccountPersonHome)
            .HasColumnName("account_person_home")
            .HasMaxLength(20);

        // XSD Field: "Account_Person_Email" (100 chars max)
        builder.Property(s => s.AccountPersonEmail)
            .HasColumnName("account_person_email")
            .HasMaxLength(100);

        // XSD Field: "Relation" (20 chars max)
        builder.Property(s => s.Relation)
            .HasColumnName("relation")
            .HasMaxLength(20);

        // Mother's Details
        // XSD Field: "Mother_Name" (50 chars max)
        builder.Property(s => s.MotherName)
            .HasColumnName("mother_name")
            .HasMaxLength(50);

        // XSD Field: "Mother_Surname" (50 chars max)
        builder.Property(s => s.MotherSurname)
            .HasColumnName("mother_surname")
            .HasMaxLength(50);

        // XSD Field: "Mother_Office" (20 chars max)
        builder.Property(s => s.MotherOffice)
            .HasColumnName("mother_office")
            .HasMaxLength(20);

        // XSD Field: "Mother_Cell" (20 chars max)
        builder.Property(s => s.MotherCell)
            .HasColumnName("mother_cell")
            .HasMaxLength(20);

        // XSD Field: "Mother_Home" (20 chars max)
        builder.Property(s => s.MotherHome)
            .HasColumnName("mother_home")
            .HasMaxLength(20);

        // XSD Field: "Mother_Email" (100 chars max)
        builder.Property(s => s.MotherEmail)
            .HasColumnName("mother_email")
            .HasMaxLength(100);

        // Father's Details
        // XSD Field: "Father_Name" (50 chars max)
        builder.Property(s => s.FatherName)
            .HasColumnName("father_name")
            .HasMaxLength(50);

        // XSD Field: "Father_Surname" (50 chars max)
        builder.Property(s => s.FatherSurname)
            .HasColumnName("father_surname")
            .HasMaxLength(50);

        // XSD Field: "Father_Office" (20 chars max)
        builder.Property(s => s.FatherOffice)
            .HasColumnName("father_office")
            .HasMaxLength(20);

        // XSD Field: "Father_Cell" (20 chars max)
        builder.Property(s => s.FatherCell)
            .HasColumnName("father_cell")
            .HasMaxLength(20);

        // XSD Field: "Father_Home" (20 chars max)
        builder.Property(s => s.FatherHome)
            .HasColumnName("father_home")
            .HasMaxLength(20);

        // XSD Field: "Father_Email" (100 chars max)
        builder.Property(s => s.FatherEmail)
            .HasColumnName("father_email")
            .HasMaxLength(100);

        // Address Fields
        // XSD Field: "Address1" (50 chars max)
        builder.Property(s => s.Address1)
            .HasColumnName("address1")
            .HasMaxLength(50);

        // XSD Field: "Address2" (50 chars max)
        builder.Property(s => s.Address2)
            .HasColumnName("address2")
            .HasMaxLength(50);

        // XSD Field: "Code" (10 chars max) - Postal code
        builder.Property(s => s.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(10);

        // Enrollment Fields
        // XSD Field: "School_Name" (50 chars max) - Denormalized school name
        builder.Property(s => s.SchoolName)
            .HasColumnName("school_name")
            .HasMaxLength(50);

        // Foreign key to School entity
        builder.Property(s => s.SchoolId)
            .HasColumnName("school_id");

        // XSD Field: "Class_Group" (10 chars max) - Class group code
        builder.Property(s => s.ClassGroupCode)
            .HasColumnName("class_group_code")
            .HasMaxLength(10);

        // Foreign key to ClassGroup entity
        builder.Property(s => s.ClassGroupId)
            .HasColumnName("class_group_id");

        // XSD Field: "Grade" (5 chars max)
        builder.Property(s => s.Grade)
            .HasColumnName("grade")
            .HasMaxLength(5);

        // XSD Field: "Teacher" (50 chars max)
        builder.Property(s => s.Teacher)
            .HasColumnName("teacher")
            .HasMaxLength(50);

        // XSD Field: "Attending_KCOW_at" (50 chars max)
        builder.Property(s => s.AttendingKcowAt)
            .HasColumnName("attending_kcow_at")
            .HasMaxLength(50);

        // XSD Field: "Aftercare" (50 chars max)
        builder.Property(s => s.Aftercare)
            .HasColumnName("aftercare")
            .HasMaxLength(50);

        // XSD Field: "Extra" (50 chars max)
        builder.Property(s => s.Extra)
            .HasColumnName("extra")
            .HasMaxLength(50);

        // XSD Field: "Home_Time" (datetime)
        builder.Property(s => s.HomeTime)
            .HasColumnName("home_time");

        // XSD Field: "Start_Classes" (datetime)
        builder.Property(s => s.StartClasses)
            .HasColumnName("start_classes");

        // XSD Field: "Terms" (10 chars max)
        builder.Property(s => s.Terms)
            .HasColumnName("terms")
            .HasMaxLength(10);

        // XSD Field: "Seat" (5 chars max)
        builder.Property(s => s.Seat)
            .HasColumnName("seat")
            .HasMaxLength(5);

        // XSD Field: "Truck" (3 chars max)
        builder.Property(s => s.Truck)
            .HasColumnName("truck")
            .HasMaxLength(3);

        // XSD Field: "Family" (50 chars max) - Family grouping code
        builder.Property(s => s.Family)
            .HasColumnName("family")
            .HasMaxLength(50);

        // XSD Field: "Sequence" (50 chars max)
        builder.Property(s => s.Sequence)
            .HasColumnName("sequence")
            .HasMaxLength(50);

        // Financial Fields
        // XSD Field: "Financial_Code" (10 chars max)
        builder.Property(s => s.FinancialCode)
            .HasColumnName("financial_code")
            .HasMaxLength(10);

        // XSD Field: "Charge" (money)
        builder.Property(s => s.Charge)
            .HasColumnName("charge")
            .HasColumnType("decimal(18,2)");

        // XSD Field: "Deposit" (50 chars max)
        builder.Property(s => s.Deposit)
            .HasColumnName("deposit")
            .HasMaxLength(50);

        // XSD Field: "PayDate" (50 chars max)
        builder.Property(s => s.PayDate)
            .HasColumnName("pay_date")
            .HasMaxLength(10);

        // T-Shirt Order Fields (Set 1)
        // XSD Field: "Tshirt_Code" (5 chars max, required)
        builder.Property(s => s.TshirtCode)
            .HasColumnName("tshirt_code")
            .HasMaxLength(4);

        // XSD Field: "Tshirt_Money_1" (30 chars max)
        builder.Property(s => s.TshirtMoney1)
            .HasColumnName("tshirt_money_1")
            .HasMaxLength(30);

        // XSD Field: "Tshirt_MoneyDate_1" (datetime)
        builder.Property(s => s.TshirtMoneyDate1)
            .HasColumnName("tshirt_money_date_1");

        // XSD Field: "Tshirt_Received_1" (30 chars max)
        builder.Property(s => s.TshirtReceived1)
            .HasColumnName("tshirt_received_1")
            .HasMaxLength(30);

        // XSD Field: "Tshirt_RecDate_1" (datetime)
        builder.Property(s => s.TshirtRecDate1)
            .HasColumnName("tshirt_rec_date_1");

        // XSD Field: "Receive_Note_1" (255 chars max)
        builder.Property(s => s.ReceiveNote1)
            .HasColumnName("receive_note_1")
            .HasMaxLength(255);

        // XSD Field: "TshirtSize1" (10 chars max)
        builder.Property(s => s.TshirtSize1)
            .HasColumnName("tshirt_size1")
            .HasMaxLength(10);

        // XSD Field: "TshirtColor1" (20 chars max)
        builder.Property(s => s.TshirtColor1)
            .HasColumnName("tshirt_color1")
            .HasMaxLength(20);

        // XSD Field: "TshirtDesign1" (20 chars max)
        builder.Property(s => s.TshirtDesign1)
            .HasColumnName("tshirt_design1")
            .HasMaxLength(20);

        // T-Shirt Order Fields (Set 2)
        // XSD Field: "TshirtSize2" (10 chars max)
        builder.Property(s => s.TshirtSize2)
            .HasColumnName("tshirt_size2")
            .HasMaxLength(10);

        // XSD Field: "Tshirt_Money_2" (30 chars max)
        builder.Property(s => s.TshirtMoney2)
            .HasColumnName("tshirt_money_2")
            .HasMaxLength(30);

        // XSD Field: "Tshirt_MoneyDate_2" (datetime)
        builder.Property(s => s.TshirtMoneyDate2)
            .HasColumnName("tshirt_money_date_2");

        // XSD Field: "Tshirt_Received_2" (30 chars max)
        builder.Property(s => s.TshirtReceived2)
            .HasColumnName("tshirt_received_2")
            .HasMaxLength(30);

        // XSD Field: "Tshirt_RecDate_2" (datetime)
        builder.Property(s => s.TshirtRecDate2)
            .HasColumnName("tshirt_rec_date_2");

        // XSD Field: "Receive_Note_2" (255 chars max)
        builder.Property(s => s.ReceiveNote2)
            .HasColumnName("receive_note_2")
            .HasMaxLength(255);

        // XSD Field: "TshirtColor2" (20 chars max)
        builder.Property(s => s.TshirtColor2)
            .HasColumnName("tshirt_color2")
            .HasMaxLength(20);

        // XSD Field: "TshirtDesign2" (20 chars max)
        builder.Property(s => s.TshirtDesign2)
            .HasColumnName("tshirt_design2")
            .HasMaxLength(20);

        // Status & Tracking Fields
        // XSD Field: "Indicator_1" (3 chars max)
        builder.Property(s => s.Indicator1)
            .HasColumnName("indicator1")
            .HasMaxLength(3);

        // XSD Field: "Indicator_2" (3 chars max)
        builder.Property(s => s.Indicator2)
            .HasColumnName("indicator2")
            .HasMaxLength(3);

        // XSD Field: "General_Note" (255 chars max)
        builder.Property(s => s.GeneralNote)
            .HasColumnName("general_note")
            .HasMaxLength(255);

        // XSD Field: "Print_Id_Card" (bit, required, default false)
        builder.Property(s => s.PrintIdCard)
            .HasColumnName("print_id_card")
            .IsRequired();

        // XSD Field: "AcceptTermsCond" (50 chars max)
        builder.Property(s => s.AcceptTermsCond)
            .HasColumnName("accept_terms_cond")
            .HasMaxLength(50);

        // XSD Field: "Status" (20 chars max)
        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(20);

        // XSD Field: "SmsOrEmail" (10 chars max) - Contact preference
        builder.Property(s => s.SmsOrEmail)
            .HasColumnName("sms_or_email")
            .HasMaxLength(255);

        // XSD Field: "Photo" (attachment) - Stored as URL/path
        builder.Property(s => s.PhotoUrl)
            .HasColumnName("photo_url");

        // XSD Field: "PhotoUpdated" (datetime)
        builder.Property(s => s.PhotoUpdated)
            .HasColumnName("photo_updated");

        // XSD Field: "SchoolClose" (datetime)
        builder.Property(s => s.SchoolClose)
            .HasColumnName("school_close");

        // XSD Field: "Cnt" (float)
        builder.Property(s => s.Cnt)
            .HasColumnName("cnt");

        // XSD Field: "OnlineEntry" (int)
        builder.Property(s => s.OnlineEntry)
            .HasColumnName("online_entry");

        // XSD Field: "Created" (datetime) - Legacy timestamp
        builder.Property(s => s.LegacyCreated)
            .HasColumnName("created");

        // XSD Field: "Submitted" (datetime)
        builder.Property(s => s.Submitted)
            .HasColumnName("submitted");

        // XSD Field: "Updated" (datetime) - Legacy timestamp
        builder.Property(s => s.LegacyUpdated)
            .HasColumnName("updated");

        // XSD Field: "BookEmail" (255 chars max)
        builder.Property(s => s.BookEmail)
            .HasColumnName("book_email")
            .HasMaxLength(255);

        // XSD Field: "Report1GivenOut" (255 chars max)
        builder.Property(s => s.Report1GivenOut)
            .HasColumnName("report1_given_out")
            .HasMaxLength(255);

        // XSD Field: "AccountGivenOut" (255 chars max)
        builder.Property(s => s.AccountGivenOut)
            .HasColumnName("account_given_out")
            .HasMaxLength(255);

        // XSD Field: "CertificatePrinted" (255 chars max)
        builder.Property(s => s.CertificatePrinted)
            .HasColumnName("certificate_printed")
            .HasMaxLength(255);

        // XSD Field: "Report2GivenOut" (255 chars max)
        builder.Property(s => s.Report2GivenOut)
            .HasColumnName("report2_given_out")
            .HasMaxLength(255);

        // XSD Field: "Social" (255 chars max)
        builder.Property(s => s.Social)
            .HasColumnName("social")
            .HasMaxLength(255);

        // XSD Field: "ActivityReportGivenOut" (255 chars max)
        builder.Property(s => s.ActivityReportGivenOut)
            .HasColumnName("activity_report_given_out")
            .HasMaxLength(255);

        // Application-level fields (not in XSD)
        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Foreign key relationships
        builder.HasOne(s => s.School)
            .WithMany()
            .HasForeignKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ClassGroup)
            .WithMany(cg => cg.Students)
            .HasForeignKey(s => s.ClassGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for efficient querying
        builder.HasIndex(s => s.Reference)
            .IsUnique()
            .HasDatabaseName("ix_students_reference");

        builder.HasIndex(s => s.LastName)
            .HasDatabaseName("ix_students_last_name");

        builder.HasIndex(s => s.SchoolId)
            .HasDatabaseName("ix_students_school_id");

        builder.HasIndex(s => s.ClassGroupId)
            .HasDatabaseName("ix_students_class_group_id");
    }
}



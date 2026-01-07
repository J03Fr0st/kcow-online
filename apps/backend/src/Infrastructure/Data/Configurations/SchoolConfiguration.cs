using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Kcow.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for School entity.
/// </summary>
public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ShortName)
            .HasColumnName("short_name")
            .HasMaxLength(50);

        // XSD Field: "School Description" (50 chars max)
        builder.Property(s => s.SchoolDescription)
            .HasColumnName("school_description")
            .HasMaxLength(50);

        builder.Property(s => s.TruckId)
            .HasColumnName("truck_id");

        builder.Property(s => s.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.FeeDescription)
            .HasColumnName("fee_description")
            .HasMaxLength(255);

        builder.Property(s => s.Formula)
            .HasColumnName("formula")
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.VisitDay)
            .HasColumnName("visit_day")
            .HasMaxLength(50);

        builder.Property(s => s.VisitSequence)
            .HasColumnName("visit_sequence")
            .HasMaxLength(50);

        builder.Property(s => s.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(200);

        builder.Property(s => s.ContactCell)
            .HasColumnName("contact_cell")
            .HasMaxLength(50);

        builder.Property(s => s.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(s => s.Telephone)
            .HasColumnName("telephone")
            .HasMaxLength(50);

        builder.Property(s => s.Fax)
            .HasColumnName("fax")
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(s => s.CircularsEmail)
            .HasColumnName("circulars_email")
            .HasMaxLength(255);

        builder.Property(s => s.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(s => s.Address2)
            .HasColumnName("address2")
            .HasMaxLength(50);

        builder.Property(s => s.Headmaster)
            .HasColumnName("headmaster")
            .HasMaxLength(50);

        builder.Property(s => s.HeadmasterCell)
            .HasColumnName("headmaster_cell")
            .HasMaxLength(50);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.Language)
            .HasColumnName("language")
            .HasMaxLength(50);

        builder.Property(s => s.PrintInvoice)
            .HasColumnName("print_invoice")
            .IsRequired();

        builder.Property(s => s.ImportFlag)
            .HasColumnName("import_flag")
            .IsRequired();

        builder.Property(s => s.Afterschool1Name)
            .HasColumnName("afterschool1_name")
            .HasMaxLength(255);

        builder.Property(s => s.Afterschool1Contact)
            .HasColumnName("afterschool1_contact")
            .HasMaxLength(255);

        builder.Property(s => s.Afterschool2Name)
            .HasColumnName("afterschool2_name")
            .HasMaxLength(255);

        builder.Property(s => s.Afterschool2Contact)
            .HasColumnName("afterschool2_contact")
            .HasMaxLength(255);

        builder.Property(s => s.SchedulingNotes)
            .HasColumnName("scheduling_notes");

        builder.Property(s => s.MoneyMessage)
            .HasColumnName("money_message");

        builder.Property(s => s.SafeNotes)
            .HasColumnName("safe_notes");

        builder.Property(s => s.WebPage)
            .HasColumnName("web_page")
            .HasMaxLength(500);

        // XSD Field: "omsendbriewe" (255 chars max)
        builder.Property(s => s.Omsendbriewe)
            .HasColumnName("omsendbriewe")
            .HasMaxLength(255);

        builder.Property(s => s.KcowWebPageLink)
            .HasColumnName("kcow_web_page_link")
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Composite index for common query pattern: WHERE IsActive = true ORDER BY Name
        builder.HasIndex(s => new { s.IsActive, s.Name });
    }
}

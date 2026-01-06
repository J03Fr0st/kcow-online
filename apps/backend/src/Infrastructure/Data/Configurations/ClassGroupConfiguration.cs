using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kcow.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for ClassGroup entity.
/// </summary>
public class ClassGroupConfiguration : IEntityTypeConfiguration<ClassGroup>
{
    public void Configure(EntityTypeBuilder<ClassGroup> builder)
    {
        builder.ToTable("class_groups");

        builder.HasKey(cg => cg.Id);

        builder.Property(cg => cg.Id)
            .HasColumnName("id");

        // XSD Field: "Class Group" (10 chars max)
        builder.Property(cg => cg.Name)
            .HasColumnName("name")
            .HasMaxLength(10)
            .IsRequired();

        // XSD Field: "DayTruck" (6 chars max)
        builder.Property(cg => cg.DayTruck)
            .HasColumnName("day_truck")
            .HasMaxLength(6);

        // XSD Field: "Description" (35 chars max)
        builder.Property(cg => cg.Description)
            .HasColumnName("description")
            .HasMaxLength(35);

        // XSD Field: "School_x0020_Id"
        builder.Property(cg => cg.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(cg => cg.TruckId)
            .HasColumnName("truck_id");

        // XSD Field: "DayId" (mapped to DayOfWeek enum)
        builder.Property(cg => cg.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        // XSD Field: "Start_x0020_Time"
        builder.Property(cg => cg.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        // XSD Field: "End_x0020_Time"
        builder.Property(cg => cg.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        // XSD Field: "Sequence" (50 chars in XSD)
        builder.Property(cg => cg.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        // XSD Field: "Evaluate" (boolean, required)
        builder.Property(cg => cg.Evaluate)
            .HasColumnName("evaluate")
            .IsRequired();

        // XSD Field: "Note" (255 chars max)
        builder.Property(cg => cg.Notes)
            .HasColumnName("notes")
            .HasMaxLength(255);

        // XSD Field: "Import" (boolean, required)
        builder.Property(cg => cg.ImportFlag)
            .HasColumnName("import_flag")
            .IsRequired();

        // XSD Field: "GroupMessage" (255 chars max)
        builder.Property(cg => cg.GroupMessage)
            .HasColumnName("group_message")
            .HasMaxLength(255);

        // XSD Field: "Send_x0020_Certificates" (255 chars max)
        builder.Property(cg => cg.SendCertificates)
            .HasColumnName("send_certificates")
            .HasMaxLength(255);

        // XSD Field: "Money_x0020_Message" (50 chars max)
        builder.Property(cg => cg.MoneyMessage)
            .HasColumnName("money_message")
            .HasMaxLength(50);

        // XSD Field: "IXL" (3 chars max)
        builder.Property(cg => cg.Ixl)
            .HasColumnName("ixl")
            .HasMaxLength(3);

        // Application-level fields (not in XSD)
        builder.Property(cg => cg.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(cg => cg.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cg => cg.UpdatedAt)
            .HasColumnName("updated_at");

        // Foreign key relationships
        builder.HasOne(cg => cg.School)
            .WithMany()
            .HasForeignKey(cg => cg.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cg => cg.Truck)
            .WithMany()
            .HasForeignKey(cg => cg.TruckId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite index for efficient querying
        builder.HasIndex(cg => new { cg.SchoolId, cg.DayOfWeek, cg.StartTime })
            .HasDatabaseName("ix_class_groups_school_day_time");
    }
}

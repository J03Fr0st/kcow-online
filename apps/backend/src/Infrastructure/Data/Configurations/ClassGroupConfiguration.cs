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

        builder.Property(cg => cg.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(cg => cg.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(cg => cg.TruckId)
            .HasColumnName("truck_id");

        builder.Property(cg => cg.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        builder.Property(cg => cg.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(cg => cg.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(cg => cg.Sequence)
            .HasColumnName("sequence")
            .IsRequired();

        builder.Property(cg => cg.Notes)
            .HasColumnName("notes");

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

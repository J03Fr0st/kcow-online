using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kcow.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for Activity entity aligned with legacy XSD schema (Activity.xsd).
/// </summary>
public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.Code)
            .HasColumnName("code")
            .HasMaxLength(255);

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255);

        builder.Property(a => a.Description)
            .HasColumnName("description");

        builder.Property(a => a.Folder)
            .HasColumnName("folder")
            .HasMaxLength(255);

        builder.Property(a => a.GradeLevel)
            .HasColumnName("grade_level")
            .HasMaxLength(255);

        builder.Property(a => a.Icon)
            .HasColumnName("icon")
            .HasColumnType("TEXT");

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        // Soft-delete query filter
        builder.HasQueryFilter(a => a.IsActive);

        // Index for Code uniqueness (optional, but good for duplicate checking)
        builder.HasIndex(a => a.Code);
    }
}



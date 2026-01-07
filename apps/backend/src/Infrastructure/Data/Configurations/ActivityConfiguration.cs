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

        builder.Property(a => a.Program)
            .HasColumnName("program")
            .HasMaxLength(255);

        builder.Property(a => a.ProgramName)
            .HasColumnName("program_name")
            .HasMaxLength(255);

        builder.Property(a => a.EducationalFocus)
            .HasColumnName("educational_focus");

        builder.Property(a => a.Folder)
            .HasColumnName("folder")
            .HasMaxLength(255);

        builder.Property(a => a.Grade)
            .HasColumnName("grade")
            .HasMaxLength(255);

        builder.Property(a => a.Icon)
            .HasColumnName("icon");
    }
}



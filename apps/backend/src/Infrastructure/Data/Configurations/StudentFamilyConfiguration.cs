using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kcow.Infrastructure.Data.Configurations;

public class StudentFamilyConfiguration : IEntityTypeConfiguration<StudentFamily>
{
    public void Configure(EntityTypeBuilder<StudentFamily> builder)
    {
        builder.ToTable("student_families");

        builder.HasKey(sf => new { sf.StudentId, sf.FamilyId });

        builder.Property(sf => sf.StudentId)
            .HasColumnName("student_id");

        builder.Property(sf => sf.FamilyId)
            .HasColumnName("family_id");

        builder.Property(sf => sf.RelationshipType)
            .HasColumnName("relationship_type")
            .HasConversion<string>(); // Store enum as string for readability

        // Add indexes on foreign keys for performance
        builder.HasIndex(sf => sf.StudentId)
            .HasDatabaseName("ix_student_families_student_id");

        builder.HasIndex(sf => sf.FamilyId)
            .HasDatabaseName("ix_student_families_family_id");

        builder.HasOne(sf => sf.Student)
            .WithMany(s => s.StudentFamilies)
            .HasForeignKey(sf => sf.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sf => sf.Family)
            .WithMany(f => f.StudentFamilies)
            .HasForeignKey(sf => sf.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

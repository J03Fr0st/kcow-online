using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kcow.Infrastructure.Data.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.FamilyName)
            .HasColumnName("family_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.PrimaryContactName)
            .HasColumnName("primary_contact_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.Phone)
            .HasColumnName("phone")
            .HasMaxLength(50);

        builder.Property(f => f.Email)
            .HasColumnName("email")
            .HasMaxLength(255);

        builder.Property(f => f.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(f => f.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(f => f.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");
    }
}

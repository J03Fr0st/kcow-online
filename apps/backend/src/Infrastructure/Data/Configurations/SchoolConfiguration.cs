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

        builder.Property(s => s.Address)
            .HasColumnName("address");

        builder.Property(s => s.ContactName)
            .HasColumnName("contact_name");

        builder.Property(s => s.ContactPhone)
            .HasColumnName("contact_phone");

        builder.Property(s => s.ContactEmail)
            .HasColumnName("contact_email");

        builder.Property(s => s.BillingSettings)
            .HasColumnName("billing_settings")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<BillingSettings>(v, (JsonSerializerOptions?)null)!
            );

        builder.Property(s => s.Notes)
            .HasColumnName("notes");

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.IsActive);
    }
}

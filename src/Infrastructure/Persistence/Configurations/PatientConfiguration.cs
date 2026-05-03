using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FullName).HasMaxLength(150).IsRequired();
        builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.HasIndex(p => p.PhoneNumber).IsUnique();

        // KAN-11: password-based login fields
        builder.Property(p => p.PasswordHash).HasMaxLength(64).IsRequired(false);
        builder.Property(p => p.FailedPasswordAttempts).HasDefaultValue(0);
        builder.Property(p => p.PasswordLockedUntil).IsRequired(false);
    }
}

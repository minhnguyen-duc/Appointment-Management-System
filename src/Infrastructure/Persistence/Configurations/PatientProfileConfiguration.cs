using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.FullName).HasMaxLength(150).IsRequired();
        builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Gender).HasMaxLength(10);
        builder.Property(p => p.Relation).HasMaxLength(50);
        builder.Property(p => p.NationalId).HasMaxLength(20);
        // FK to Patient
        builder.HasOne<Patient>()
               .WithMany()
               .HasForeignKey(p => p.PatientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

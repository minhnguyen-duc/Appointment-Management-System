using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FullName).HasMaxLength(150).IsRequired();
        builder.Property(d => d.AcademicTitle).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Specialization).HasMaxLength(100).IsRequired();
        builder.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired();
        builder.Property(d => d.ConsultationFee).HasPrecision(18, 2);
        builder.Property(d => d.ImageUrl).HasMaxLength(500);
        builder.Property(d => d.Bio).HasMaxLength(1000);
    }
}

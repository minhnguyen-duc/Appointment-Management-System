using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.ConsultationFee).HasPrecision(18, 2);
        builder.Property(a => a.DepositAmount).HasPrecision(18, 2);
        builder.Property(a => a.PaymentReference).HasMaxLength(100);
        builder.Property(a => a.PaymentStatus).HasConversion<string>();
        // Status stored as int (matches initial migration) — no conversion needed
        builder.Property(a => a.RoomNumber).HasMaxLength(20);
        builder.Property(a => a.BarcodeData).HasMaxLength(50);
        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.Ignore(a => a.DomainEvents);

        builder.HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
               .WithMany(d => d.Appointments)
               .HasForeignKey(a => a.DoctorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

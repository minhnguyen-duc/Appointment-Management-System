using Domain.Entities;
using Domain.Exceptions;
using Xunit;

namespace Domain.UnitTests;

public class AppointmentTests
{
    [Fact]
    public void Book_WithValidData_ShouldCreateAppointment()
    {
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var scheduledAt = DateTime.UtcNow.AddHours(3);

        var appt = Appointment.Book(patientId, doctorId, scheduledAt);

        Assert.Equal(patientId, appt.PatientId);
        Assert.Equal(doctorId, appt.DoctorId);
        Assert.Single(appt.DomainEvents);
    }

    [Fact]
    public void Book_WithinTwoHours_ShouldThrow()
    {
        var scheduledAt = DateTime.UtcNow.AddHours(1); // < 2h notice

        Assert.Throws<AppointmentTooSoonException>(() =>
            Appointment.Book(Guid.NewGuid(), Guid.NewGuid(), scheduledAt));
    }
}

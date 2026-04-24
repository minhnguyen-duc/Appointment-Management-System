namespace Application.Appointments.Commands;

public record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAt,
    int DurationMinutes = 30);

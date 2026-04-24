using Domain.Enums;

namespace Application.Appointments.DTOs;

public record AppointmentDto(
    Guid Id,
    string PatientName,
    string DoctorName,
    string Specialization,
    DateTime ScheduledAt,
    int DurationMinutes,
    AppointmentStatus Status,
    string? Notes);

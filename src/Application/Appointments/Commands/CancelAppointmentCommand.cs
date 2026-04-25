namespace Application.Appointments.Commands;

public record CancelAppointmentCommand(Guid AppointmentId, Guid RequestedByPatientId);

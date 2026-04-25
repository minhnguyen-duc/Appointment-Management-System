namespace Application.Appointments.Queries;

/// <summary>Booking by Date: all available doctors for a given day.</summary>
public record GetAppointmentsByDateQuery(DateOnly Date, string? Specialization = null);

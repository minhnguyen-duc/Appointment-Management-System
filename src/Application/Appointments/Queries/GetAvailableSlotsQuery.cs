namespace Application.Appointments.Queries;

public record GetAvailableSlotsQuery(Guid DoctorId, DateOnly Date);

public record TimeSlotDto(DateTime StartTime, DateTime EndTime, bool IsAvailable);

using Application.Common.DTOs;

namespace Application.Appointments.Queries;

public record GetAvailableSlotsQuery(Guid DoctorId, DateOnly Date);

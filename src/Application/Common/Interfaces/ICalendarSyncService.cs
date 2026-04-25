using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICalendarSyncService
{
    Task SyncAppointmentAsync(Appointment appointment, CancellationToken ct = default);
    Task RemoveAppointmentAsync(Guid appointmentId, CancellationToken ct = default);
}

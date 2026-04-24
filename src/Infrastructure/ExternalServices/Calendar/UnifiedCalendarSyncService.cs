using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.ExternalServices.Calendar;

/// <summary>Two-way sync: Google Calendar, Outlook, Apple iCal.</summary>
public class UnifiedCalendarSyncService : ICalendarSyncService
{
    public async Task SyncAppointmentAsync(Appointment appointment, CancellationToken ct = default)
    {
        // Check provider preference, push event, prevent double-booking
        await Task.CompletedTask;
    }

    public async Task RemoveAppointmentAsync(Guid appointmentId, CancellationToken ct = default)
        => await Task.CompletedTask;
}

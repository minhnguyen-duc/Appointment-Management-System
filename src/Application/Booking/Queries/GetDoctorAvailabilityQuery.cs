using Application.Common.DTOs;
using Application.Common.Interfaces;

namespace Application.Booking.Queries;

/// <summary>
/// KAN-14 AC3: Returns availability calendar for a doctor for the next 2 months.
/// Each date shows available 1-hour time slots (08:00–17:00) with booking counts.
/// </summary>
public sealed record GetDoctorAvailabilityQuery(Guid DoctorId, DateOnly? Month = null);

public class GetDoctorAvailabilityQueryHandler(IAppointmentRepository appointmentRepo)
{
    private static readonly TimeSpan[] WorkHours =
        Enumerable.Range(8, 9) // 08:00 to 16:00 (9 slots × 1h)
                  .Select(h => TimeSpan.FromHours(h))
                  .ToArray();

    public async Task<List<DoctorAvailabilityDto>> HandleAsync(
        GetDoctorAvailabilityQuery query, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var from  = today;
        var to    = today.AddMonths(2);

        // Get all booked appointments for the doctor in the date range
        var booked = await appointmentRepo.GetByDoctorAndRangeAsync(
            query.DoctorId,
            from.ToDateTime(TimeOnly.MinValue),
            to.ToDateTime(TimeOnly.MaxValue),
            ct);

        // Group by date+hour
        var bookingCounts = booked
            .GroupBy(a => new { a.ScheduledAt.Date, a.ScheduledAt.Hour })
            .ToDictionary(g => (g.Key.Date, g.Key.Hour), g => g.Count());

        var result = new List<DoctorAvailabilityDto>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            // Skip Sundays (AC3: doctor working days — customizable)
            if (date.DayOfWeek == DayOfWeek.Sunday) continue;

            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            var slots = WorkHours.Select(h =>
            {
                var start = dateTime.Add(h);
                var count = bookingCounts.GetValueOrDefault((dateTime, (int)h.TotalHours), 0);
                return new TimeSlotDto
                {
                    Start       = start,
                    End         = start.AddHours(1),
                    BookedCount = count
                };
            }).ToList();

            result.Add(new DoctorAvailabilityDto
            {
                Date         = date,
                HasAvailable = slots.Any(s => s.IsAvailable),
                Slots        = slots
            });
        }

        return result;
    }
}

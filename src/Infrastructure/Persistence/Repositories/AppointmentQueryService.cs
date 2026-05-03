using Application.Appointments.DTOs;
using Application.Appointments.Queries;
using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Application.Common.DTOs;

namespace Infrastructure.Persistence.Repositories;

public class AppointmentQueryService(AppDbContext db) : IAppointmentQueryService
{
    public async Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Appointments.Include(x => x.Patient).Include(x => x.Doctor)
                        .FirstOrDefaultAsync(x => x.Id == id, ct);
        return a is null ? null : Map(a);
    }

    public async Task<PagedResult<AppointmentDto>> GetPagedAsync(AppointmentFilterModel filter, int page, int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.Appointments.Include(x => x.Patient).Include(x => x.Doctor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            q = q.Where(a => a.Patient.FullName.Contains(filter.Keyword) || a.Doctor.FullName.Contains(filter.Keyword));

        if (filter.Date.HasValue)
            q = q.Where(a => DateOnly.FromDateTime(a.ScheduledAt) == DateOnly.FromDateTime(filter.Date.Value));

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<AppointmentStatus>(filter.Status, out var status))
            q = q.Where(a => a.Status == status);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.ScheduledAt)
                           .Skip((page - 1) * pageSize).Take(pageSize)
                           .ToListAsync(ct);

        return new PagedResult<AppointmentDto>
        {
            Items = items.Select(Map).ToList(),
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<IList<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date, CancellationToken ct = default)
    {
        var slots = new List<TimeSlotDto>();
        var booked = await db.Appointments
            .Where(a => a.DoctorId == doctorId && DateOnly.FromDateTime(a.ScheduledAt) == date)
            .Select(a => a.ScheduledAt.Hour)
            .ToListAsync(ct);

        for (int h = 8; h <= 17; h++)
        {
            var start = new DateTime(date.Year, date.Month, date.Day, h, 0, 0, DateTimeKind.Local);
            var count = booked.Count(b => b == h);
            slots.Add(new TimeSlotDto
            {
                Start       = start,
                End         = start.AddHours(1),
                BookedCount = count
            });
        }
        return slots;
    }

    public async Task<List<AppointmentDto>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
    {
        var items = await db.Appointments.Include(x => x.Patient).Include(x => x.Doctor)
            .Where(a => DateOnly.FromDateTime(a.ScheduledAt) == date)
            .OrderBy(a => a.ScheduledAt).ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<List<AppointmentDto>> GetByMonthAsync(DateOnly month, CancellationToken ct = default)
    {
        var items = await db.Appointments.Include(x => x.Patient).Include(x => x.Doctor)
            .Where(a => a.ScheduledAt.Year == month.Year && a.ScheduledAt.Month == month.Month)
            .ToListAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayAppts = await db.Appointments
            .Where(a => DateOnly.FromDateTime(a.ScheduledAt) == today).ToListAsync(ct);
        return new DashboardStats
        {
            TodayCount = todayAppts.Count,
            PendingCount = todayAppts.Count(a => a.Status == AppointmentStatus.Pending),
            CompletedToday = todayAppts.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledToday = todayAppts.Count(a => a.Status == AppointmentStatus.Cancelled)
        };
    }

    public async Task<List<DoctorDto>> GetOnDutyDoctorsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var doctorIds = await db.Appointments
            .Where(a => DateOnly.FromDateTime(a.ScheduledAt) == today && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.DoctorId).Distinct().ToListAsync(ct);

        var doctors = await db.Doctors.Where(d => doctorIds.Contains(d.Id)).ToListAsync(ct);
        return doctors.Select(d => new DoctorDto { Id = d.Id, FullName = d.FullName, Specialization = d.Specialization, IsActive = d.IsActive }).ToList();
    }

    public async Task CancelAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Appointments.FindAsync([id], ct);
        a?.Cancel();
        await db.SaveChangesAsync(ct);
    }

    public async Task ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Appointments.FindAsync([id], ct);
        a?.Confirm();
        await db.SaveChangesAsync(ct);
    }

    private static AppointmentDto Map(Domain.Entities.Appointment a) => new(
        a.Id,
        a.Patient?.FullName ?? "",
        a.Doctor?.FullName  ?? "",
        a.Doctor?.Specialization ?? "",
        a.ScheduledAt, a.DurationMinutes,
        a.Status, a.Notes);
}

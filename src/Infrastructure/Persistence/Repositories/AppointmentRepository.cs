using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class AppointmentRepository(AppDbContext db) : IAppointmentRepository
{
    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Appointments.Include(a => a.Patient).Include(a => a.Doctor)
               .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<IList<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateOnly date, CancellationToken ct = default)
        => db.Appointments
               .Where(a => a.DoctorId == doctorId && DateOnly.FromDateTime(a.ScheduledAt) == date)
               .ToListAsync(ct)
               .ContinueWith(t => (IList<Appointment>)t.Result, ct);

    public Task<int> CountByDoctorAndHourAsync(Guid doctorId, DateTime hour, CancellationToken ct = default)
    {
        var hourStart = new DateTime(hour.Year, hour.Month, hour.Day, hour.Hour, 0, 0, DateTimeKind.Utc);
        var hourEnd = hourStart.AddHours(1);
        return db.Appointments.CountAsync(
            a => a.DoctorId == doctorId && a.ScheduledAt >= hourStart && a.ScheduledAt < hourEnd, ct);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        => await db.Appointments.AddAsync(appointment, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public Task<List<Domain.Entities.Appointment>> GetByDoctorAndRangeAsync(
        Guid doctorId, DateTime from, DateTime to, CancellationToken ct = default)
        => _db.Set<Domain.Entities.Appointment>()
              .Include(a => a.Patient)
              .Include(a => a.Doctor)
              .Where(a => a.DoctorId == doctorId
                       && a.ScheduledAt >= from
                       && a.ScheduledAt <= to
                       && a.Status != Domain.Enums.AppointmentStatus.Cancelled)
              .ToListAsync(ct);

    public Task<int> CountConfirmedByDoctorTodayAsync(
        Guid doctorId, DateTime date, CancellationToken ct = default)
        => _db.Set<Domain.Entities.Appointment>()
              .CountAsync(a => a.DoctorId == doctorId
                            && a.ScheduledAt.Date == date.Date
                            && a.Status == Domain.Enums.AppointmentStatus.Confirmed, ct);


}

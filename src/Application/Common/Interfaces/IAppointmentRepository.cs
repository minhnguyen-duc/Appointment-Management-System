using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IList<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateOnly date, CancellationToken ct = default);
    Task<int> CountByDoctorAndHourAsync(Guid doctorId, DateTime hour, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    // KAN-14 AC3: get all appointments for a doctor in date range
    Task<List<Domain.Entities.Appointment>> GetByDoctorAndRangeAsync(
        Guid doctorId, DateTime from, DateTime to, CancellationToken ct = default);

    // KAN-14 AC6: count confirmed appointments for e-ticket sequence
    Task<int> CountConfirmedByDoctorTodayAsync(
        Guid doctorId, DateTime date, CancellationToken ct = default);

    // KAN-14: get by id with related data
    Task<Domain.Entities.Appointment?> GetByIdAsync(
        Guid id, CancellationToken ct = default);
}

using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IList<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateOnly date, CancellationToken ct = default);
    Task<int> CountByDoctorAndHourAsync(Guid doctorId, DateTime hour, CancellationToken ct = default);
    Task AddAsync(Appointment appointment, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

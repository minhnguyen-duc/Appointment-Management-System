using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IList<Doctor>> GetAllAsync(string? search = null, string? specialization = null, CancellationToken ct = default);
    Task<IList<string>> GetSpecializationsAsync(CancellationToken ct = default);
    Task AddAsync(Doctor doctor, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

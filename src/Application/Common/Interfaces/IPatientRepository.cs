using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IList<Patient> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize = 20, CancellationToken ct = default);
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

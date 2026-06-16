using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPatientProfileRepository
{
    Task<List<PatientProfile>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default);
    Task AddAsync(PatientProfile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

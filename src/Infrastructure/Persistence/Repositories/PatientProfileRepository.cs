using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PatientProfileRepository(AppDbContext db) : IPatientProfileRepository
{
    public Task<List<PatientProfile>> GetByPatientIdAsync(Guid patientId, CancellationToken ct = default)
        => db.PatientProfiles
             .Where(p => p.PatientId == patientId)
             .OrderByDescending(p => p.IsDefault)
             .ThenBy(p => p.FullName)
             .ToListAsync(ct);

    public async Task AddAsync(PatientProfile profile, CancellationToken ct = default)
        => await db.PatientProfiles.AddAsync(profile, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}

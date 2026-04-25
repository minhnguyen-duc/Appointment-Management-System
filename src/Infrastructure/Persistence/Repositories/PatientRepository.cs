using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PatientRepository(AppDbContext db) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Patients.FindAsync([id], ct).AsTask();

    public async Task<(IList<Patient> Items, int Total)> GetPagedAsync(string? search, int page, int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.FullName.Contains(search) || p.PhoneNumber.Contains(search));
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
        => await db.Patients.AddAsync(patient, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}

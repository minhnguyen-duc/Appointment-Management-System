using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Common.DTOs;

namespace Infrastructure.Persistence.Repositories;

public class DoctorQueryService(AppDbContext db) : IDoctorQueryService
{
    public async Task<List<DoctorDto>> GetAllAsync(string? search = null, string? specialization = null, CancellationToken ct = default)
    {
        var q = db.Doctors.Where(d => d.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(d => d.FullName.Contains(search));
        if (!string.IsNullOrWhiteSpace(specialization))
            q = q.Where(d => d.Specialization == specialization);

        return await q.Select(d => new DoctorDto { Id = d.Id, FullName = d.FullName, Specialization = d.Specialization, IsActive = d.IsActive }).ToListAsync(ct);
    }

    public async Task<List<string>> GetSpecializationsAsync(CancellationToken ct = default)
        => await db.Doctors.Where(d => d.IsActive).Select(d => d.Specialization).Distinct().OrderBy(s => s).ToListAsync(ct);

    public async Task<List<DoctorCatalogDto>> GetCatalogAsync(
        string? specialization = null,
        string? keyword = null,
        CancellationToken ct = default)
    {
        var q = _db.Set<Doctor>().Where(d => d.IsActive);
        if (!string.IsNullOrWhiteSpace(specialization))
            q = q.Where(d => d.Specialization == specialization);
        if (!string.IsNullOrWhiteSpace(keyword))
            q = q.Where(d => d.FullName.Contains(keyword) || d.Specialization.Contains(keyword));
        return await q.Select(d => new DoctorCatalogDto
        {
            Id              = d.Id,
            FullName        = d.FullName,
            AcademicTitle   = d.AcademicTitle,
            Specialization  = d.Specialization,
            ConsultationFee = d.ConsultationFee,
            ImageUrl        = d.ImageUrl,
            Bio             = d.Bio,
            Rating          = 4.8, // placeholder — reviews feature later
            ReviewCount     = 0
        }).ToListAsync(ct);
    }

}

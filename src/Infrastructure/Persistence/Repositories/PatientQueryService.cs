using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Application.Common.DTOs;

namespace Infrastructure.Persistence.Repositories;

public class PatientQueryService(AppDbContext db) : IPatientQueryService
{
    public async Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await db.Patients.FindAsync([id], ct);
        return p is null ? null : new PatientDto { Id = p.Id, FullName = p.FullName, PhoneNumber = p.PhoneNumber, Email = p.Email, DateOfBirth = p.DateOfBirth };
    }

    public async Task<PagedResult<PatientDto>> GetPagedAsync(string? search, int page, int pageSize = 20, CancellationToken ct = default)
    {
        var q = db.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.FullName.Contains(search) || p.PhoneNumber.Contains(search));

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<PatientDto>
        {
            Items = items.Select(p => new PatientDto { Id = p.Id, FullName = p.FullName, PhoneNumber = p.PhoneNumber, Email = p.Email, DateOfBirth = p.DateOfBirth }).ToList(),
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
    // KAN-11: GetByPhoneAsync — lookup bệnh nhân sau xác thực OTP (AC4)
    public async Task<Domain.Entities.Patient?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => await db.Patients.FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber, ct);
}

// KAN-11: Lookup patient by phone number after OTP verification

using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface IDoctorQueryService
{
    Task<List<DoctorDto>> GetAllAsync(CancellationToken ct = default);
    Task<DoctorDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // KAN-14 AC1
    Task<List<DoctorCatalogDto>> GetCatalogAsync(
        string? specialization = null,
        string? keyword        = null,
        CancellationToken ct   = default);
}

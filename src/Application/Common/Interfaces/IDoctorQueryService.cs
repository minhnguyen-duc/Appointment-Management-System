using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface IDoctorQueryService
{
    Task<List<DoctorDto>> GetAllAsync(
        string? search = null, string? specialization = null,
        CancellationToken ct = default);

    Task<List<string>> GetSpecializationsAsync(CancellationToken ct = default);

    // KAN-14 AC1
    Task<List<DoctorCatalogDto>> GetCatalogAsync(
        string? specialization = null,
        string? keyword        = null,
        CancellationToken ct   = default);
}

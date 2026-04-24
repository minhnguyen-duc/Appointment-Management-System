using Presentation.Services;

namespace Application.Common.Interfaces;

public interface IDoctorQueryService
{
    Task<List<DoctorDto>> GetAllAsync(string? search = null, string? specialization = null, CancellationToken ct = default);
    Task<List<string>> GetSpecializationsAsync(CancellationToken ct = default);
}

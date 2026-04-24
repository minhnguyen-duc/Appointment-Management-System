using Application.Common.Interfaces;

namespace Presentation.Services;

public class DoctorClientService(IDoctorQueryService queryService)
{
    public Task<List<DoctorDto>> GetAllAsync(string? search = null, string? specialization = null)
        => queryService.GetAllAsync(search, specialization);

    public Task<List<string>> GetSpecializationsAsync()
        => queryService.GetSpecializationsAsync();
}

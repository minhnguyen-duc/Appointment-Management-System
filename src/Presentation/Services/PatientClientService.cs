using Application.Common.Interfaces;

namespace Presentation.Services;

public class PatientClientService(
    IPatientQueryService queryService,
    IPatientCommandService commandService)
{
    public Task<PagedResult<PatientDto>> GetPagedAsync(string search, int page)
        => queryService.GetPagedAsync(search, page);

    public Task<PatientDto?> GetByIdAsync(Guid id)
        => queryService.GetByIdAsync(id);

    public Task SaveAsync(Guid? id, PatientFormModel model)
        => id.HasValue ? commandService.UpdateAsync(id.Value, model) : commandService.CreateAsync(model);
}

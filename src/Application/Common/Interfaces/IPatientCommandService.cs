using Presentation.Services;

namespace Application.Common.Interfaces;

public interface IPatientCommandService
{
    Task CreateAsync(PatientFormModel model, CancellationToken ct = default);
    Task UpdateAsync(Guid id, PatientFormModel model, CancellationToken ct = default);
}

using Presentation.Services;

namespace Application.Common.Interfaces;

public interface IPatientQueryService
{
    Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<PatientDto>> GetPagedAsync(string? search, int page, int pageSize = 20, CancellationToken ct = default);
}

    // Added for KAN-11: lookup by phone for post-OTP redirect
    Task<Domain.Entities.Patient?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);

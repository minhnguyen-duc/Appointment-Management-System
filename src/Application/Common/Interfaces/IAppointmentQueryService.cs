using Application.Appointments.DTOs;
using Application.Appointments.Queries;
using Application.Common.DTOs;

namespace Application.Common.Interfaces;

public interface IAppointmentQueryService
{
    Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<AppointmentDto>> GetPagedAsync(AppointmentFilterModel filter, int page, int pageSize = 20, CancellationToken ct = default);
    Task<IList<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date, CancellationToken ct = default);
    Task<List<AppointmentDto>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<List<AppointmentDto>> GetByMonthAsync(DateOnly month, CancellationToken ct = default);
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<List<DoctorDto>> GetOnDutyDoctorsAsync(CancellationToken ct = default);
    Task CancelAsync(Guid appointmentId, CancellationToken ct = default);
    Task ConfirmAsync(Guid appointmentId, CancellationToken ct = default);
}

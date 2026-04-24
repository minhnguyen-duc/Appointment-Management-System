using Application.Appointments.Commands;
using Application.Appointments.DTOs;
using Application.Appointments.Queries;
using Application.Common.Interfaces;

namespace Presentation.Services;

public class AppointmentClientService(
    BookAppointmentCommandHandler bookHandler,
    IAppointmentQueryService queryService)
{
    public Task<Guid> BookAsync(BookingFormModel model)
        => bookHandler.HandleAsync(new BookAppointmentCommand(
            model.PatientId, model.DoctorId, model.ScheduledAt, model.DurationMinutes));

    public Task CancelAsync(Guid id)      => queryService.CancelAsync(id);
    public Task ConfirmAsync(Guid id)     => queryService.ConfirmAsync(id);

    public Task<AppointmentDto?> GetByIdAsync(Guid id)
        => queryService.GetByIdAsync(id);

    public Task<PagedResult<AppointmentDto>> GetPagedAsync(AppointmentFilterModel filter, int page)
        => queryService.GetPagedAsync(filter, page);

    public Task<IList<TimeSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date)
        => queryService.GetAvailableSlotsAsync(doctorId, date);

    public Task<List<AppointmentDto>> GetTodayAppointmentsAsync()
        => queryService.GetByDateAsync(DateOnly.FromDateTime(DateTime.Today));

    public Task<List<AppointmentDto>> GetByMonthAsync(DateOnly month)
        => queryService.GetByMonthAsync(month);

    public Task<DashboardStats> GetDashboardStatsAsync()
        => queryService.GetDashboardStatsAsync();

    public Task<List<DoctorDto>> GetOnDutyDoctorsAsync()
        => queryService.GetOnDutyDoctorsAsync();
}

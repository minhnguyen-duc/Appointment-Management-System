namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(string email, Guid appointmentId, CancellationToken ct = default);
    Task SendRescheduleNoticeAsync(string email, Guid appointmentId, CancellationToken ct = default);
    Task SendCancellationNoticeAsync(string email, Guid appointmentId, CancellationToken ct = default);

    // Generic send (used by ETicketService)
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}

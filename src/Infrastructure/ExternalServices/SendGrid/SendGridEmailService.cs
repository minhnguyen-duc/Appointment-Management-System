using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices.SendGrid;

public class SendGridEmailService(IConfiguration config) : IEmailService
{
    private readonly string _apiKey = config["SendGrid:ApiKey"]!;

    public async Task SendBookingConfirmationAsync(string email, Guid appointmentId, CancellationToken ct = default)
    {
        // SendGrid dynamic template with QR code & rescheduling link
        await Task.CompletedTask;
    }

    public async Task SendRescheduleNoticeAsync(string email, Guid appointmentId, CancellationToken ct = default)
        => await Task.CompletedTask;

    public async Task SendCancellationNoticeAsync(string email, Guid appointmentId, CancellationToken ct = default)
        => await Task.CompletedTask;
}

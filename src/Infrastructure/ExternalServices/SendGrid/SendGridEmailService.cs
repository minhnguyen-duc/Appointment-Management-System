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

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        // TODO: call SendGrid API with _apiKey
        // For now: log to console in dev
        Console.WriteLine($"[SendGrid] To:{to} Subject:{subject}");
        await Task.CompletedTask;
    }
}

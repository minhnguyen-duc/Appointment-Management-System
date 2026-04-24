namespace Application.Common.Interfaces;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default);
    Task SendReminderAsync(string phoneNumber, string message, CancellationToken ct = default);
}

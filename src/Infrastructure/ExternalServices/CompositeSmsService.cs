using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

/// <summary>
/// Composite gateway: thử Twilio trước, nếu fail thì fallback sang Zalo OA.
/// AC2 - KAN-11: Đảm bảo OTP được gửi qua SMS hoặc Zalo trong SLA 30 giây.
/// </summary>
public class CompositeSmsService(
    Infrastructure.ExternalServices.Twilio.TwilioSmsService twilio,
    Infrastructure.ExternalServices.Zalo.ZaloOaSmsService   zalo,
    ILogger<CompositeSmsService>                             logger) : ISmsService
{
    public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        try
        {
            await twilio.SendOtpAsync(phoneNumber, otp, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Twilio failed, falling back to Zalo for {Phone}", phoneNumber);
            await zalo.SendOtpAsync(phoneNumber, otp, ct);
        }
    }

    public async Task SendReminderAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        try { await twilio.SendReminderAsync(phoneNumber, message, ct); }
        catch { await zalo.SendReminderAsync(phoneNumber, message, ct); }
    } 
}

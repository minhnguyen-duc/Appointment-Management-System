using Application.Common.Interfaces;
using Domain.ValueObjects;

namespace Application.Auth.Commands;

/// <summary>
/// Handler cho RequestOtpCommand.
/// AC1 - KAN-11: Validate số điện thoại Việt Nam trước khi gửi OTP.
/// AC2 - KAN-11: Gọi SMS/Zalo gateway; gateway có SLA 30 giây.
/// AC3 - KAN-11: Delegate rate-limit check sang IOtpService.
/// </summary>
public class RequestOtpCommandHandler(ISmsService smsService, IOtpService otpService)
{
    public async Task HandleAsync(RequestOtpCommand cmd, CancellationToken ct = default)
    {
        // AC1: validate Vietnam phone number
        var phone = VietnamPhoneNumber.Parse(cmd.PhoneNumber);

        // AC3: GenerateAndStoreAsync ném OtpRateLimitExceededException nếu vượt giới hạn
        var otp = await otpService.GenerateAndStoreAsync(phone.Value, ct);

        // AC2: gửi OTP qua SMS / Zalo (gateway SLA ≤ 30 giây)
        await smsService.SendOtpAsync(phone.ToInternationalFormat(), otp, ct);
    }
}

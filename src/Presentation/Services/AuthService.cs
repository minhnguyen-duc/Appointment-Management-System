using Application.Auth.Commands;
using Application.Common.Interfaces;

namespace Presentation.Services;

/// <summary>
/// Presentation-layer service wrapping auth command handlers.
/// KAN-11: Expose RequestOtp, VerifyOtp, GetResendStatus.
/// </summary>
public class AuthService(
    RequestOtpCommandHandler otpHandler,
    VerifyOtpCommandHandler  verifyHandler,
    IOtpService              otpService)
{
    // AC2: gửi OTP qua gateway
    public Task RequestOtpAsync(string phone)
        => otpHandler.HandleAsync(new RequestOtpCommand(phone));

    // AC4: xác thực OTP, nhận redirect URL
    public Task<OtpVerifyResult> VerifyOtpAsync(string phone, string otp)
        => verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));

    // AC3: truy vấn trạng thái resend để cập nhật UI
    public Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(string phone)
        => otpService.GetResendStatusAsync(phone);
}

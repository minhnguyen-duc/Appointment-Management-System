using Application.Auth.Commands;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Presentation.Services;

/// <summary>
/// Presentation-layer service wrapping auth command handlers.
/// KAN-11: Expose RequestOtp, VerifyOtp, GetResendStatus.
/// AC4: Signs in via cookie after OTP verification succeeds.
/// </summary>
public class AuthService(
    RequestOtpCommandHandler otpHandler,
    VerifyOtpCommandHandler  verifyHandler,
    IOtpService              otpService,
    IHttpContextAccessor     httpContextAccessor)
{
    // AC2: gửi OTP qua gateway
    public Task RequestOtpAsync(string phone)
        => otpHandler.HandleAsync(new RequestOtpCommand(phone));

    // AC4: xác thực OTP → sign-in cookie → trả về redirect URL
    public async Task<OtpVerifyResult> VerifyOtpAsync(string phone, string otp)
    {
        var result = await verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));

        // Issue cookie session (AC4: redirect to BENHNHAN interface)
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,           phone),
            new(ClaimTypes.MobilePhone,    phone),
            new("PatientId",               result.PatientId?.ToString() ?? ""),
            new("IsNewPatient",            result.IsNewPatient.ToString()),
        };

        var identity  = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        var ctx = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext not available.");

        await ctx.SignInAsync("Cookies", principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(8),
        });

        return result;
    }

    // AC3: truy vấn trạng thái resend để cập nhật UI
    public Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(string phone)
        => otpService.GetResendStatusAsync(phone);
}

using Application.Auth.Commands;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Presentation.Services;

/// <summary>
/// KAN-11: Presentation-layer service wrapping all auth command handlers.
/// Flows:
///   Existing patient → CheckPhone → LoginWithPassword → sign-in cookie → redirect
///   New patient      → CheckPhone → RequestOtp → VerifyOtp → SetPassword → sign-in cookie → redirect
/// </summary>
public class AuthService(
    CheckPhoneCommandHandler      checkPhoneHandler,
    RequestOtpCommandHandler      otpHandler,
    VerifyOtpCommandHandler       verifyHandler,
    LoginWithPasswordCommandHandler passwordHandler,
    SetPasswordCommandHandler     setPasswordHandler,
    IOtpService                   otpService,
    IHttpContextAccessor          httpContextAccessor)
{
    // ── AC1: kiểm tra phone có tồn tại không ──────────────────────────────────
    public Task<CheckPhoneResult> CheckPhoneAsync(string phone)
        => checkPhoneHandler.HandleAsync(new CheckPhoneCommand(phone));

    // ── AC2/AC3: gửi OTP (new patient flow) ──────────────────────────────────
    public Task RequestOtpAsync(string phone)
        => otpHandler.HandleAsync(new RequestOtpCommand(phone));

    // ── AC4: xác thực OTP ────────────────────────────────────────────────────
    public Task<OtpVerifyResult> VerifyOtpAsync(string phone, string otp)
        => verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));

    // ── AC1: đăng nhập bằng mật khẩu (existing patient) ─────────────────────
    public async Task<OtpVerifyResult> LoginWithPasswordAsync(string phone, string password)
    {
        var result = await passwordHandler.HandleAsync(new LoginWithPasswordCommand(phone, password));
        await SignInAsync(phone, result);
        return result;
    }

    // ── AC1: đặt mật khẩu sau OTP verify (new patient) ───────────────────────
    public async Task<OtpVerifyResult> SetPasswordAsync(string phone, string password)
    {
        var result = await setPasswordHandler.HandleAsync(new SetPasswordCommand(phone, password, phone));
        await SignInAsync(phone, result);
        return result;
    }

    // ── AC4: sign-in cookie sau OTP (phải gọi thêm SetPassword sau đó) ───────
    public async Task<OtpVerifyResult> VerifyOtpAndSignInAsync(string phone, string otp)
    {
        var result = await verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));
        // Chưa sign-in — sign-in sẽ được thực hiện sau khi set password (new) hoặc ngay bây giờ (existing)
        return result;
    }

    // ── Resend status ─────────────────────────────────────────────────────────
    public Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(string phone)
        => otpService.GetResendStatusAsync(phone);

    // ── Sign-in cookie helper ─────────────────────────────────────────────────
    public async Task SignInAsync(string phone, OtpVerifyResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,        phone),
            new(ClaimTypes.MobilePhone, phone),
            new("PatientId",            result.PatientId?.ToString() ?? ""),
            new("IsNewPatient",         result.IsNewPatient.ToString()),
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
    }
}

using Application.Auth.Commands;
using Application.Common.Interfaces;
using System.Security.Claims;

namespace Presentation.Services;

/// <summary>
/// KAN-11 — Two-step sign-in to avoid Blazor Server "Headers read-only" error.
/// See PendingLoginStore and /auth/do-login in Program.cs.
/// </summary>
public class AuthService(
    CheckPhoneCommandHandler        checkPhoneHandler,
    RequestOtpCommandHandler        otpHandler,
    VerifyOtpCommandHandler         verifyHandler,
    LoginWithPasswordCommandHandler passwordHandler,
    SetPasswordCommandHandler       setPasswordHandler,
    IOtpService                     otpService,
    PendingLoginStore               loginStore)
{
    public Task<CheckPhoneResult> CheckPhoneAsync(string phone)
        => checkPhoneHandler.HandleAsync(new CheckPhoneCommand(phone));

    public Task RequestOtpAsync(string phone)
        => otpHandler.HandleAsync(new RequestOtpCommand(phone));

    public Task<OtpVerifyResult> VerifyOtpAsync(string phone, string otp)
        => verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));

    /// <summary>Verify password → issue ticket → return /auth/do-login URL.</summary>
    public async Task<string> LoginWithPasswordAsync(string phone, string password, string returnUrl)
    {
        var result = await passwordHandler.HandleAsync(new LoginWithPasswordCommand(phone, password));
        var ticket = IssueTicket(phone, result);
        return $"/auth/do-login?ticket={ticket}&returnUrl={Uri.EscapeDataString(returnUrl)}&msg={Uri.EscapeDataString("Đăng nhập thành công")}";
    }

    /// <summary>Set password for new patient → issue ticket → return /auth/do-login URL.</summary>
    public async Task<string> SetPasswordAsync(string phone, string password, string returnUrl)
    {
        var result = await setPasswordHandler.HandleAsync(new SetPasswordCommand(phone, password, phone));
        var ticket = IssueTicket(phone, result);
        return $"/auth/do-login?ticket={ticket}&returnUrl={Uri.EscapeDataString(returnUrl)}&msg={Uri.EscapeDataString("Tạo tài khoản thành công")}";
    }

    public Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(string phone)
        => otpService.GetResendStatusAsync(phone);

    private string IssueTicket(string phone, OtpVerifyResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,        phone),
            new(ClaimTypes.MobilePhone, phone),
            new("PatientId",            result.PatientId?.ToString() ?? ""),
            new("IsNewPatient",         result.IsNewPatient.ToString()),
        };
        return loginStore.Store(claims);
    }
}

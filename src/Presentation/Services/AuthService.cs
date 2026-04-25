using Application.Auth.Commands;

namespace Presentation.Services;

public class AuthService(
    RequestOtpCommandHandler otpHandler,
    VerifyOtpCommandHandler verifyHandler)
{
    public Task RequestOtpAsync(string phone)
        => otpHandler.HandleAsync(new RequestOtpCommand(phone));

    public Task<string> VerifyOtpAsync(string phone, string otp)
        => verifyHandler.HandleAsync(new VerifyOtpCommand(phone, otp));
}

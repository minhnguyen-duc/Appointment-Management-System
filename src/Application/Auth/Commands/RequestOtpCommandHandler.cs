using Application.Common.Interfaces;

namespace Application.Auth.Commands;

public class RequestOtpCommandHandler(ISmsService smsService, IOtpService otpService)
{
    public async Task HandleAsync(RequestOtpCommand cmd, CancellationToken ct = default)
    {
        var otp = await otpService.GenerateAndStoreAsync(cmd.PhoneNumber, ct);
        await smsService.SendOtpAsync(cmd.PhoneNumber, otp, ct);
    }
}

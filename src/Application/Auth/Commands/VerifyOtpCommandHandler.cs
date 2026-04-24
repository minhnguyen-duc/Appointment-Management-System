using Application.Common.Interfaces;

namespace Application.Auth.Commands;

public class VerifyOtpCommandHandler(IOtpService otpService)
{
    public async Task<string> HandleAsync(VerifyOtpCommand cmd, CancellationToken ct = default)
    {
        var valid = await otpService.ValidateAsync(cmd.PhoneNumber, cmd.Otp, ct);
        if (!valid) throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");
        // Return a simple token or session marker; real JWT logic goes in Infrastructure/Identity
        return $"session:{cmd.PhoneNumber}:{Guid.NewGuid()}";
    }
}

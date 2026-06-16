using Application.Common.Interfaces;
using Domain.ValueObjects;

namespace Application.Auth.Commands;

/// <summary>
/// Handler cho VerifyOtpCommand.
/// AC4 - KAN-11: Xác thực OTP và trả về session token để redirect sang giao diện BENHNHAN.
/// </summary>
public class VerifyOtpCommandHandler(IOtpService otpService, IPatientQueryService patientQuery)
{
    public async Task<OtpVerifyResult> HandleAsync(VerifyOtpCommand cmd, CancellationToken ct = default)
    {
        // AC1: validate phone format
        var phone = VietnamPhoneNumber.Parse(cmd.PhoneNumber);

        var valid = await otpService.ValidateAsync(phone.Value, cmd.Otp, ct);
        if (!valid)
            throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        // AC4: tìm hoặc tạo bệnh nhân, trả về session token + PatientId để redirect BENHNHAN
        var patient = await patientQuery.GetByPhoneAsync(phone.Value, ct);

        return new OtpVerifyResult(
            SessionToken: $"session:{phone.Value}:{Guid.NewGuid()}",
            PatientId: patient?.Id,
            IsNewPatient: patient is null,
            RedirectUrl: patient is null ? "/benhnhan/register" : "/Homepage/HomePage"
        );
    }
}

/// <summary>Kết quả sau khi xác thực OTP thành công.</summary>
public sealed record OtpVerifyResult(
    string SessionToken,
    Guid? PatientId,
    bool IsNewPatient,
    string RedirectUrl
);

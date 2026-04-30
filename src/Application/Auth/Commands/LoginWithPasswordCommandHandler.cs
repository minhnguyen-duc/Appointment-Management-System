using Application.Common.Interfaces;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Auth.Commands;

/// <summary>
/// KAN-11 AC1: Xác thực mật khẩu cho bệnh nhân đã có tài khoản.
/// - Sai → InvalidPasswordException (kèm số lần sai).
/// - Đúng → OtpVerifyResult để redirect.
/// </summary>
public class LoginWithPasswordCommandHandler(IPatientQueryService patientQuery, IPatientRepository patientRepo)
{
    public async Task<OtpVerifyResult> HandleAsync(LoginWithPasswordCommand cmd, CancellationToken ct = default)
    {
        var phone   = VietnamPhoneNumber.Parse(cmd.PhoneNumber);
        var patient = await patientQuery.GetByPhoneAsync(phone.Value, ct)
            ?? throw new InvalidOperationException("Số điện thoại không tồn tại trong hệ thống.");

        // Throws AccountLockedException nếu đang bị khóa
        var valid = patient.VerifyPassword(cmd.Password);
        await patientRepo.SaveChangesAsync(ct); // persist FailedPasswordAttempts

        if (!valid)
            throw new InvalidPasswordException(patient.FailedPasswordAttempts, Domain.Entities.Patient.MaxPasswordAttempts);

        return new OtpVerifyResult(
            SessionToken: $"session:{phone.Value}:{Guid.NewGuid()}",
            PatientId:    patient.Id,
            IsNewPatient: false,
            RedirectUrl:  "/Homepage/Homepage"
        );
    }
}

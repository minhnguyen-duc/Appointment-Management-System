using Application.Common.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Auth.Commands;

/// <summary>
/// KAN-11 AC1: Tạo patient mới + set password sau OTP verify.
/// Returns OtpVerifyResult để sign-in cookie và redirect.
/// </summary>
public class SetPasswordCommandHandler(IPatientQueryService patientQuery, IPatientRepository patientRepo)
{
    public async Task<OtpVerifyResult> HandleAsync(SetPasswordCommand cmd, CancellationToken ct = default)
    {
        var phone = VietnamPhoneNumber.Parse(cmd.PhoneNumber);

        // Kiểm tra xem patient đã được tạo chưa (có thể tạo từ OTP verify step)
        var patient = await patientQuery.GetByPhoneAsync(phone.Value, ct);

        if (patient is null)
        {
            // Tạo mới patient
            patient = Patient.CreateFromPhone(phone.Value);
            await patientRepo.AddAsync(patient, ct);
        }

        patient.SetPassword(cmd.Password);
        await patientRepo.SaveChangesAsync(ct);

        return new OtpVerifyResult(
            SessionToken: $"session:{phone.Value}:{Guid.NewGuid()}",
            PatientId:    patient.Id,
            IsNewPatient: true,
            RedirectUrl:  "/benhnhan/dashboard"
        );
    }
}

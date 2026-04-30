using Application.Common.Interfaces;
using Domain.ValueObjects;

namespace Application.Auth.Commands;

/// <summary>
/// KAN-11 AC1: Kiểm tra SĐT → quyết định flow (password hoặc OTP).
/// </summary>
public class CheckPhoneCommandHandler(IPatientQueryService patientQuery)
{
    public async Task<CheckPhoneResult> HandleAsync(CheckPhoneCommand cmd, CancellationToken ct = default)
    {
        var phone   = VietnamPhoneNumber.Parse(cmd.PhoneNumber);
        var patient = await patientQuery.GetByPhoneAsync(phone.Value, ct);

        if (patient is null)
            return new CheckPhoneResult(Exists: false, HasPassword: false);

        return new CheckPhoneResult(Exists: true, HasPassword: patient.IsPasswordSet);
    }
}

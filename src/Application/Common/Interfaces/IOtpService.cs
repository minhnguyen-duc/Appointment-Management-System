namespace Application.Common.Interfaces;

public interface IOtpService
{
    Task<string> GenerateAndStoreAsync(string phoneNumber, CancellationToken ct = default);
    Task<bool> ValidateAsync(string phoneNumber, string otp, CancellationToken ct = default);
}

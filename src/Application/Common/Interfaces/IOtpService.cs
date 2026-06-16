namespace Application.Common.Interfaces;

/// <summary>
/// Dịch vụ sinh, lưu trữ và xác thực OTP.
/// AC2 - KAN-11: OTP phải được gửi trong vòng 30 giây (SLA gateway).
/// AC3 - KAN-11: Giới hạn gửi lại tối đa 3 lần / 10 phút.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Sinh OTP mới và lưu vào store. Ném <see cref="Domain.Exceptions.OtpRateLimitExceededException"/>
    /// nếu số lần gửi lại trong 10 phút đã đạt giới hạn.
    /// </summary>
    Task<string> GenerateAndStoreAsync(string phoneNumber, CancellationToken ct = default);

    /// <summary>
    /// Xác thực OTP. Trả về true nếu đúng và chưa hết hạn, xoá entry sau khi xác thực thành công.
    /// </summary>
    Task<bool> ValidateAsync(string phoneNumber, string otp, CancellationToken ct = default);

    /// <summary>
    /// Kiểm tra số lần gửi OTP còn lại trong cửa sổ thời gian hiện tại.
    /// </summary>
    Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(string phoneNumber, CancellationToken ct = default);
}

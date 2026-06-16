namespace Domain.Exceptions;

/// <summary>
/// Ném khi tài khoản bị tạm khóa do nhập sai mật khẩu quá nhiều lần.
/// KAN-11 AC1.
/// </summary>
public sealed class AccountLockedException(DateTime unlockedAt) : Exception(
    $"Tài khoản của bạn đang bị TẠM KHÓA. Vui lòng thử lại sau {unlockedAt.ToLocalTime():HH:mm dd/MM/yyyy}.")
{
    public DateTime UnlockedAt { get; } = unlockedAt;
}

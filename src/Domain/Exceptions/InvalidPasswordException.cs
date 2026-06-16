namespace Domain.Exceptions;

/// <summary>
/// Ném khi bệnh nhân nhập sai mật khẩu.
/// KAN-11 AC1: Hiện inline "sai mật khẩu N lần liên tiếp".
/// </summary>
public sealed class InvalidPasswordException(int attemptCount, int maxAttempts) : Exception(
    $"Bạn đã nhập sai mật khẩu {attemptCount} lần liên tiếp. " +
    $"Tài khoản của bạn sẽ bị TẠM KHÓA nếu bạn nhập sai mật khẩu liên tiếp {maxAttempts} lần.")
{
    public int AttemptCount { get; } = attemptCount;
    public int MaxAttempts  { get; } = maxAttempts;
}

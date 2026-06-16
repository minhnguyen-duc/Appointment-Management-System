namespace Domain.Exceptions;

/// <summary>
/// Exception khi bệnh nhân gửi lại OTP vượt quá giới hạn cho phép.
/// AC3 - KAN-11: Giới hạn tối đa 3 lần gửi lại / 10 phút.
/// </summary>
public sealed class OtpRateLimitExceededException : Exception
{
    public int MaxAttempts { get; }
    public TimeSpan WindowDuration { get; }
    public DateTime UnlockedAt { get; }

    public OtpRateLimitExceededException(int maxAttempts, TimeSpan windowDuration, DateTime unlockedAt)
        : base($"Bạn đã gửi lại OTP quá {maxAttempts} lần. Vui lòng thử lại sau {(int)windowDuration.TotalMinutes} phút.")
    {
        MaxAttempts = maxAttempts;
        WindowDuration = windowDuration;
        UnlockedAt = unlockedAt;
    }
}

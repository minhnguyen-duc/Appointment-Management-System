using Application.Common.Interfaces;
using Domain.Exceptions;
using System.Collections.Concurrent;

namespace Infrastructure.Identity;

/// <summary>
/// In-memory OTP store cho development/testing.
/// Production: thay bằng Redis để đảm bảo distributed rate-limit.
///
/// AC2 - KAN-11: OTP có hiệu lực 2 phút (đủ thời gian gateway gửi ≤30s + người dùng nhập).
/// AC3 - KAN-11: Giới hạn tối đa 3 lần gửi lại / 10 phút mỗi số điện thoại.
/// </summary>
public class InMemoryOtpService : IOtpService
{
    private const int OtpExpiryMinutes       = 2;
    private const int MaxResendPerWindow     = 3;
    private static readonly TimeSpan Window  = TimeSpan.FromMinutes(10);

    // OTP store: phone -> (code, expiry)
    private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)>
        _otpStore = new();

    // Rate-limit store: phone -> list of send timestamps in current window
    private static readonly ConcurrentDictionary<string, List<DateTime>>
        _sendLog = new(StringComparer.OrdinalIgnoreCase);

    private static readonly SemaphoreSlim _lock = new(1, 1);

    /// <inheritdoc/>
    public async Task<string> GenerateAndStoreAsync(string phoneNumber, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var now = DateTime.UtcNow;
            var log = _sendLog.GetOrAdd(phoneNumber, _ => new List<DateTime>());

            // Loại bỏ các timestamp ngoài cửa sổ 10 phút
            log.RemoveAll(t => now - t > Window);

            if (log.Count >= MaxResendPerWindow)
            {
                var unlockedAt = log.Min() + Window;
                throw new OtpRateLimitExceededException(MaxResendPerWindow, Window, unlockedAt);
            }

            log.Add(now);

            var otp = Random.Shared.Next(100_000, 999_999).ToString();
            _otpStore[phoneNumber] = (otp, now.AddMinutes(OtpExpiryMinutes));
            return otp;
        }
        finally { _lock.Release(); }
    }

    /// <inheritdoc/>
    public Task<bool> ValidateAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        if (_otpStore.TryGetValue(phoneNumber, out var entry)
            && entry.Expiry > DateTime.UtcNow
            && entry.Otp == otp)
        {
            _otpStore.TryRemove(phoneNumber, out _);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public Task<(int Used, int Max, DateTime? UnlockedAt)> GetResendStatusAsync(
        string phoneNumber, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        if (!_sendLog.TryGetValue(phoneNumber, out var log))
            return Task.FromResult<(int, int, DateTime?)>((0, MaxResendPerWindow, null));

        log.RemoveAll(t => now - t > Window);
        var used = log.Count;
        DateTime? unlockedAt = used >= MaxResendPerWindow ? log.Min() + Window : null;
        return Task.FromResult((used, MaxResendPerWindow, unlockedAt));
    }
}

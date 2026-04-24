using Application.Common.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.Identity;

/// <summary>
/// In-memory OTP store cho development. Production: dùng Redis hoặc MSSQL.
/// </summary>
public class InMemoryOtpService : IOtpService
{
    private static readonly ConcurrentDictionary<string, (string Otp, DateTime Expiry)> _store = new();

    public Task<string> GenerateAndStoreAsync(string phoneNumber, CancellationToken ct = default)
    {
        var otp = Random.Shared.Next(100_000, 999_999).ToString();
        _store[phoneNumber] = (otp, DateTime.UtcNow.AddMinutes(5));
        return Task.FromResult(otp);
    }

    public Task<bool> ValidateAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        if (_store.TryGetValue(phoneNumber, out var entry) && entry.Expiry > DateTime.UtcNow && entry.Otp == otp)
        {
            _store.TryRemove(phoneNumber, out _);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

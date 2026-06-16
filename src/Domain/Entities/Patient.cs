using System.Security.Cryptography;
using System.Text;

namespace Domain.Entities;

public class Patient
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string? NationalId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // KAN-11: password-based login for existing patients
    public string? PasswordHash { get; private set; }
    public int FailedPasswordAttempts { get; private set; }
    public DateTime? PasswordLockedUntil { get; private set; }

    public const int MaxPasswordAttempts = 5;

    private readonly List<Appointment> _appointments = [];
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private Patient() { } // EF Core

    public static Patient Create(string fullName, string phoneNumber, string email, DateOnly dob)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        return new Patient
        {
            Id          = Guid.NewGuid(),
            FullName    = fullName,
            PhoneNumber = phoneNumber,
            Email       = email,
            DateOfBirth = dob,
            CreatedAt   = DateTime.UtcNow
        };
    }

    /// <summary>Tạo patient mới sau khi OTP verify thành công (phone-only, chưa có password).</summary>
    public static Patient CreateFromPhone(string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        return new Patient
        {
            Id          = Guid.NewGuid(),
            FullName    = phoneNumber, // placeholder — sẽ cập nhật sau
            PhoneNumber = phoneNumber,
            Email       = string.Empty,
            DateOfBirth = DateOnly.MinValue,
            CreatedAt   = DateTime.UtcNow
        };
    }

    // ── Password management ────────────────────────────────────────────────────

    public void SetPassword(string plainPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainPassword);
        PasswordHash = HashPassword(plainPassword);
        FailedPasswordAttempts = 0;
        PasswordLockedUntil = null;
    }

    /// <summary>
    /// Returns true nếu password đúng.
    /// Throws AccountLockedException nếu account đang bị lock.
    /// Tăng FailedPasswordAttempts, lock account sau MaxPasswordAttempts lần.
    /// </summary>
    public bool VerifyPassword(string plainPassword)
    {
        if (PasswordLockedUntil.HasValue && DateTime.UtcNow < PasswordLockedUntil.Value)
            throw new Domain.Exceptions.AccountLockedException(PasswordLockedUntil.Value);

        if (PasswordHash is null)
            return false;

        var match = HashPassword(plainPassword) == PasswordHash;
        if (match)
        {
            FailedPasswordAttempts = 0;
            PasswordLockedUntil = null;
        }
        else
        {
            FailedPasswordAttempts++;
            if (FailedPasswordAttempts >= MaxPasswordAttempts)
                PasswordLockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
        return match;
    }

    public bool IsPasswordSet => PasswordHash is not null;
    public bool IsLocked => PasswordLockedUntil.HasValue && DateTime.UtcNow < PasswordLockedUntil.Value;

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "ams2026-salt"));
        return Convert.ToHexString(bytes);
    }
}

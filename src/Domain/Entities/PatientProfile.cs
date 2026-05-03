namespace Domain.Entities;

/// <summary>
/// KAN-14 AC2: One patient can have multiple profiles (e.g. themselves + family members).
/// A profile is selected before each booking.
/// </summary>
public class PatientProfile
{
    public Guid    Id          { get; private set; }
    public Guid    PatientId   { get; private set; }   // owner
    public string  FullName    { get; private set; } = string.Empty;
    public string  PhoneNumber { get; private set; } = string.Empty;
    public string? Email       { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public string   Gender     { get; private set; } = string.Empty; // Nam / Nữ / Khác
    public string?  NationalId { get; private set; }
    public string   Relation   { get; private set; } = string.Empty; // Bản thân / Bố / Mẹ / Con / Vợ / Chồng
    public bool     IsDefault  { get; private set; }
    public DateTime CreatedAt  { get; private set; }

    private PatientProfile() { }

    public static PatientProfile Create(
        Guid patientId, string fullName, string phoneNumber,
        DateOnly dob, string gender, string relation,
        string? email = null, string? nationalId = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        return new PatientProfile
        {
            Id          = Guid.NewGuid(),
            PatientId   = patientId,
            FullName    = fullName,
            PhoneNumber = phoneNumber,
            Email       = email,
            DateOfBirth = dob,
            Gender      = gender,
            Relation    = relation,
            NationalId  = nationalId,
            IsDefault   = isDefault,
            CreatedAt   = DateTime.UtcNow
        };
    }

    public void SetDefault(bool value) => IsDefault = value;
}

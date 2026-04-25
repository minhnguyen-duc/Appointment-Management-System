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

    private readonly List<Appointment> _appointments = [];
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private Patient() { } // EF Core

    public static Patient Create(string fullName, string phoneNumber, string email, DateOnly dob)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        return new Patient
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = email,
            DateOfBirth = dob,
            CreatedAt = DateTime.UtcNow
        };
    }
}

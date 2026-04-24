namespace Domain.Entities;

public class Doctor
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Specialization { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private readonly List<Appointment> _appointments = [];
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private Doctor() { }

    public static Doctor Create(string fullName, string specialization, string licenseNumber)
    {
        return new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Specialization = specialization,
            LicenseNumber = licenseNumber,
            IsActive = true
        };
    }
}

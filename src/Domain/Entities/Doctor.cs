namespace Domain.Entities;

/// <summary>KAN-14 AC1: Doctor catalog with title, specialty, fee, image.</summary>
public class Doctor
{
    public Guid    Id              { get; private set; }
    public string  FullName        { get; private set; } = string.Empty;
    public string  AcademicTitle   { get; private set; } = string.Empty; // e.g. TS.BS, PGS.TS.BS
    public string  Specialization  { get; private set; } = string.Empty;
    public string  LicenseNumber   { get; private set; } = string.Empty;
    public decimal ConsultationFee { get; private set; }
    public string? ImageUrl        { get; private set; }
    public string? Bio             { get; private set; }
    public bool    IsActive        { get; private set; }

    private readonly List<Appointment> _appointments = [];
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private Doctor() { }

    public static Doctor Create(
        string fullName, string academicTitle, string specialization,
        string licenseNumber, decimal consultationFee,
        string? imageUrl = null, string? bio = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        return new Doctor
        {
            Id              = Guid.NewGuid(),
            FullName        = fullName,
            AcademicTitle   = academicTitle,
            Specialization  = specialization,
            LicenseNumber   = licenseNumber,
            ConsultationFee = consultationFee,
            ImageUrl        = imageUrl,
            Bio             = bio,
            IsActive        = true
        };
    }

    public void UpdateFee(decimal fee) => ConsultationFee = fee;
    public void Deactivate()           => IsActive = false;
    public void Activate()             => IsActive = true;
}

namespace Application.Common.DTOs;

// ── Patient ──────────────────────────────────────────────────────────────────

public class PatientDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public int AppointmentCount { get; set; }
}

public class PatientFormModel
{
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
}

// ── Doctor ───────────────────────────────────────────────────────────────────

public class DoctorDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public bool IsActive { get; set; }
    public int AppointmentCount { get; set; }
}

// ── Appointment ───────────────────────────────────────────────────────────────

public class AppointmentFilterModel
{
    public string? Keyword { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; }
}

public class BookingFormModel
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string? Notes { get; set; }
}

public class BookingSummaryModel
{
    public string? DoctorName { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

public class DashboardStats
{
    public int TodayCount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedToday { get; set; }
    public int CancelledToday { get; set; }
}

// ── Pagination ────────────────────────────────────────────────────────────────

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

// ── KAN-14 ────────────────────────────────────────────────────────────────────

/// <summary>AC1: Doctor catalog item displayed to patient.</summary>
public class DoctorCatalogDto
{
    public Guid    Id              { get; set; }
    public string  FullName        { get; set; } = "";
    public string  AcademicTitle   { get; set; } = "";
    public string  Specialization  { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public string? ImageUrl        { get; set; }
    public string? Bio             { get; set; }
    // derived stats
    public double  Rating          { get; set; }
    public int     ReviewCount     { get; set; }
}

/// <summary>AC2: Patient profile (one patient can have many).</summary>
public class PatientProfileDto
{
    public Guid     Id          { get; set; }
    public string   FullName    { get; set; } = "";
    public string   PhoneNumber { get; set; } = "";
    public DateOnly DateOfBirth { get; set; }
    public string   Gender      { get; set; } = "";
    public string   Relation    { get; set; } = "";
    public bool     IsDefault   { get; set; }
}

/// <summary>AC3: Available time slot for a doctor on a date.</summary>
public class TimeSlotDto
{
    public DateTime Start         { get; set; }
    public DateTime End           { get; set; }  // Start + 1h
    public int      BookedCount   { get; set; }
    public bool     IsAvailable   => BookedCount < 10;
    public string   Label         => Start.ToString("HH:mm");
}

/// <summary>AC3: Calendar availability for a doctor over 2 months.</summary>
public class DoctorAvailabilityDto
{
    public DateOnly Date          { get; set; }
    public bool     HasAvailable  { get; set; }
    public List<TimeSlotDto> Slots { get; set; } = [];
}

/// <summary>AC5/AC6: Result of a successful booking + payment.</summary>
public class BookingConfirmationDto
{
    public Guid     AppointmentId    { get; set; }
    public string   PaymentUrl       { get; set; } = "";
    public decimal  Amount           { get; set; }
}

/// <summary>AC6: Digital e-ticket issued after payment.</summary>
public class ETicketDto
{
    public Guid     AppointmentId    { get; set; }
    public string   BarcodeData      { get; set; } = "";
    public int      SequenceNumber   { get; set; }
    public string   RoomNumber       { get; set; } = "";
    public string   DoctorName       { get; set; } = "";
    public string   Specialization   { get; set; } = "";
    public string   PatientName      { get; set; } = "";
    public DateTime ScheduledAt      { get; set; }
    public decimal  ConsultationFee  { get; set; }
    public string   PaymentReference { get; set; } = "";
    // AC7: for multi-specialty, one payment covers multiple tickets
    public List<ETicketDto> AdditionalTickets { get; set; } = [];
}

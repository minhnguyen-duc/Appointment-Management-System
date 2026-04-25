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

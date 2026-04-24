using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Entities;

public class Appointment
{
    public const int MaxPatientsPerHour = 10;
    public const int MinNoticeHours = 2;

    public Guid Id { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public int DurationMinutes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public decimal? DepositAmount { get; private set; }
    public string? PaymentReference { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Patient Patient { get; private set; } = null!;
    public Doctor Doctor { get; private set; } = null!;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Appointment() { }

    public static Appointment Book(Guid patientId, Guid doctorId, DateTime scheduledAt, int durationMinutes = 30)
    {
        if (scheduledAt < DateTime.UtcNow.AddHours(MinNoticeHours))
            throw new AppointmentTooSoonException(MinNoticeHours);

        var appt = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        appt._domainEvents.Add(new AppointmentBookedEvent(appt.Id, patientId, doctorId, scheduledAt));
        return appt;
    }

    public void Confirm() => Status = AppointmentStatus.Confirmed;
    public void Cancel() => Status = AppointmentStatus.Cancelled;
    public void Complete() => Status = AppointmentStatus.Completed;
}

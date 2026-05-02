using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;

namespace Domain.Entities;

public class Appointment
{
    // AC4: max 10 patients per 1-hour slot
    public const int MaxPatientsPerHour = 10;
    public const int MinNoticeHours     = 2;
    public const int SlotDurationMinutes = 60; // AC3: 1-hour slots

    public Guid    Id               { get; private set; }
    public Guid    PatientId        { get; private set; }
    public Guid    DoctorId         { get; private set; }
    public Guid?   ProfileId        { get; private set; } // AC2: which profile
    public DateTime ScheduledAt     { get; private set; }
    public int     DurationMinutes  { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? Notes            { get; private set; }

    // AC5: payment
    public decimal  ConsultationFee  { get; private set; }
    public decimal? DepositAmount    { get; private set; }
    public string?  PaymentReference { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    // AC6: e-ticket
    public int?    SequenceNumber   { get; private set; } // queue number
    public string? RoomNumber       { get; private set; }
    public string? BarcodeData      { get; private set; } // base64 or ID for barcode
    public bool    ETicketSent      { get; private set; }

    public DateTime CreatedAt       { get; private set; }

    public Patient?        Patient  { get; private set; }
    public Doctor?         Doctor   { get; private set; }
    public PatientProfile? Profile  { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Appointment() { }

    public static Appointment Book(
        Guid patientId, Guid doctorId, DateTime scheduledAt,
        decimal consultationFee,
        Guid? profileId = null,
        int durationMinutes = SlotDurationMinutes,
        string? notes = null)
    {
        if (scheduledAt < DateTime.UtcNow.AddHours(MinNoticeHours))
            throw new AppointmentTooSoonException(MinNoticeHours);

        var appt = new Appointment
        {
            Id              = Guid.NewGuid(),
            PatientId       = patientId,
            DoctorId        = doctorId,
            ProfileId       = profileId,
            ScheduledAt     = scheduledAt,
            DurationMinutes = durationMinutes,
            ConsultationFee = consultationFee,
            Status          = AppointmentStatus.Pending,
            PaymentStatus   = PaymentStatus.Unpaid,
            CreatedAt       = DateTime.UtcNow
        };
        appt._domainEvents.Add(
            new AppointmentBookedEvent(appt.Id, patientId, doctorId, scheduledAt));
        return appt;
    }

    // ── State transitions ──────────────────────────────────────────────────
    public void Confirm()  => Status = AppointmentStatus.Confirmed;
    public void Cancel()   => Status = AppointmentStatus.Cancelled;
    public void Complete() => Status = AppointmentStatus.Completed;

    // ── Payment (AC5) ──────────────────────────────────────────────────────
    public void RecordPayment(string paymentReference, decimal amount)
    {
        PaymentReference = paymentReference;
        DepositAmount    = amount;
        PaymentStatus    = PaymentStatus.Paid;
        Status           = AppointmentStatus.Confirmed;
    }

    // ── E-Ticket (AC6) ────────────────────────────────────────────────────
    public void IssueETicket(int sequenceNumber, string roomNumber)
    {
        SequenceNumber = sequenceNumber;
        RoomNumber     = roomNumber;
        // Barcode = appointment ID in a scannable format
        BarcodeData    = Id.ToString("N").ToUpper()[..12]; // short code
        ETicketSent    = false;
    }

    public void MarkETicketSent() => ETicketSent = true;
}

using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Appointments.Commands;

public class BookAppointmentCommandHandler(
    IAppointmentRepository appointmentRepo,
    ISmsService smsService,
    IEmailService emailService,
    ICalendarSyncService calendarSync,
    IPatientRepository patientRepo)
{
    public async Task<Guid> HandleAsync(BookAppointmentCommand cmd, CancellationToken ct = default)
    {
        // Capacity check — max 10 per hour (spec §3.2)
        var count = await appointmentRepo.CountByDoctorAndHourAsync(
            cmd.DoctorId, cmd.ScheduledAt, ct);

        if (count >= Appointment.MaxPatientsPerHour)
            throw new CapacityExceededException(Appointment.MaxPatientsPerHour);

        var appointment = Appointment.Book(cmd.PatientId, cmd.DoctorId, cmd.ScheduledAt, cmd.DurationMinutes);

        await appointmentRepo.AddAsync(appointment, ct);
        await appointmentRepo.SaveChangesAsync(ct);

        var patient = await patientRepo.GetByIdAsync(cmd.PatientId, ct);
        if (patient is not null)
        {
            // Email: booking confirmation e-ticket (spec §5 SendGrid)
            await emailService.SendBookingConfirmationAsync(patient.Email, appointment.Id, ct);

            // SMS: high-priority appointment reminder (spec §5 Twilio)
            var msg = $"[MediCare] Lich kham cua ban ngay {appointment.ScheduledAt:dd/MM HH:mm} da duoc dat thanh cong. Ma so: {appointment.Id.ToString()[..8].ToUpper()}";
            await smsService.SendOtpAsync(patient.PhoneNumber, msg, ct);
        }

        await calendarSync.SyncAppointmentAsync(appointment, ct);

        return appointment.Id;
    }
}

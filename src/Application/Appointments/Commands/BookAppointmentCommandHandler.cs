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
        // Capacity check — max 10 per hour
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
            await emailService.SendBookingConfirmationAsync(patient.Email, appointment.Id, ct);
        }

        await calendarSync.SyncAppointmentAsync(appointment, ct);

        return appointment.Id;
    }
}

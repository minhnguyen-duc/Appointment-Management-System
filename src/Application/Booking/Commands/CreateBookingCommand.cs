using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Booking.Commands;

/// <summary>
/// KAN-14 AC4 + AC5:
///   1. Validates slot capacity (max 10 patients).
///   2. Creates appointment in Pending/Unpaid state.
///   3. Returns VNPAY payment URL.
/// </summary>
public sealed record CreateBookingCommand(
    Guid     PatientId,
    Guid?    ProfileId,
    Guid     DoctorId,
    DateTime ScheduledAt,
    decimal  ConsultationFee,
    string?  Notes = null);

public class CreateBookingCommandHandler(
    IAppointmentRepository appointmentRepo,
    IPaymentService        paymentService)
{
    public async Task<BookingConfirmationDto> HandleAsync(
        CreateBookingCommand cmd, CancellationToken ct = default)
    {
        // AC4: capacity check — count bookings in the same 1-hour slot
        var slotEnd   = cmd.ScheduledAt.AddHours(1);
        var slotCount = await appointmentRepo.CountByDoctorAndHourAsync(
            cmd.DoctorId, cmd.ScheduledAt, ct);

        if (slotCount >= Appointment.MaxPatientsPerHour)
            throw new SlotFullException(cmd.ScheduledAt, Appointment.MaxPatientsPerHour);

        // Create appointment (Pending, Unpaid)
        var appointment = Appointment.Book(
            cmd.PatientId, cmd.DoctorId, cmd.ScheduledAt,
            cmd.ConsultationFee, cmd.ProfileId,
            Appointment.SlotDurationMinutes, cmd.Notes);

        await appointmentRepo.AddAsync(appointment, ct);
        await appointmentRepo.SaveChangesAsync(ct);

        // AC5: generate VNPAY payment URL
        var paymentUrl = await paymentService.CreatePaymentUrlAsync(
            appointment.Id, cmd.ConsultationFee, ct);

        return new BookingConfirmationDto
        {
            AppointmentId = appointment.Id,
            PaymentUrl    = paymentUrl,
            Amount        = cmd.ConsultationFee
        };
    }
}

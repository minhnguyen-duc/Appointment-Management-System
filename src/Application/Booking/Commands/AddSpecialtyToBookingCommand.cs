using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Booking.Commands;

/// <summary>
/// KAN-14 AC7: Add an additional specialty to an existing (unpaid) booking.
/// Creates a second appointment linked to the same payment reference.
/// Returns combined payment URL covering both fees.
/// </summary>
public sealed record AddSpecialtyToBookingCommand(
    Guid     ExistingAppointmentId,
    Guid     NewDoctorId,
    DateTime NewScheduledAt,
    decimal  NewConsultationFee);

public class AddSpecialtyToBookingCommandHandler(
    IAppointmentRepository appointmentRepo,
    IPaymentService        paymentService)
{
    public async Task<BookingConfirmationDto> HandleAsync(
        AddSpecialtyToBookingCommand cmd, CancellationToken ct = default)
    {
        var existing = await appointmentRepo.GetByIdAsync(cmd.ExistingAppointmentId, ct)
            ?? throw new InvalidOperationException("Lịch hẹn không tồn tại.");

        // AC4: check capacity for new slot
        var slotCount = await appointmentRepo.CountByDoctorAndHourAsync(
            cmd.NewDoctorId, cmd.NewScheduledAt, ct);
        if (slotCount >= Appointment.MaxPatientsPerHour)
            throw new SlotFullException(cmd.NewScheduledAt, Appointment.MaxPatientsPerHour);

        // Create second appointment (same patient + profile)
        var newAppt = Appointment.Book(
            existing.PatientId, cmd.NewDoctorId, cmd.NewScheduledAt,
            cmd.NewConsultationFee, existing.ProfileId);

        await appointmentRepo.AddAsync(newAppt, ct);
        await appointmentRepo.SaveChangesAsync(ct);

        // Combined fee
        var totalFee   = existing.ConsultationFee + cmd.NewConsultationFee;
        var paymentUrl = await paymentService.CreatePaymentUrlAsync(newAppt.Id, totalFee, ct);

        return new BookingConfirmationDto
        {
            AppointmentId = newAppt.Id,
            PaymentUrl    = paymentUrl,
            Amount        = totalFee
        };
    }
}

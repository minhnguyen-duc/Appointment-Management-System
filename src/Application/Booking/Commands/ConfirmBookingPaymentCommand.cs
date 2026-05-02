using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Exceptions;

namespace Application.Booking.Commands;

/// <summary>
/// KAN-14 AC5 + AC6:
///   1. Verifies VNPAY payment.
///   2. Issues e-ticket (sequence, room, barcode).
///   3. Sends e-ticket via Email (SendGrid) + SMS (Twilio).
/// </summary>
public sealed record ConfirmBookingPaymentCommand(
    Guid   AppointmentId,
    string PaymentReference,
    string VnpayResponseCode);

public class ConfirmBookingPaymentCommandHandler(
    IAppointmentRepository appointmentRepo,
    IPaymentService        paymentService,
    IETicketService        ticketService,
    IEmailService          emailService,
    ISmsService            smsService)
{
    public async Task<ETicketDto> HandleAsync(
        ConfirmBookingPaymentCommand cmd, CancellationToken ct = default)
    {
        // Verify payment with VNPAY
        var isValid = await paymentService.VerifyPaymentAsync(cmd.PaymentReference, ct);
        if (!isValid || cmd.VnpayResponseCode != "00")
            throw new PaymentRequiredException();

        // Load appointment
        var appointment = await appointmentRepo.GetByIdAsync(cmd.AppointmentId, ct)
            ?? throw new InvalidOperationException("Lịch hẹn không tồn tại.");

        // Record payment
        appointment.RecordPayment(cmd.PaymentReference, appointment.ConsultationFee);

        // AC6: assign sequence number (daily sequential per doctor)
        var todayCount = await appointmentRepo.CountConfirmedByDoctorTodayAsync(
            appointment.DoctorId, appointment.ScheduledAt.Date, ct);
        var room = AssignRoom(appointment.DoctorId);
        appointment.IssueETicket(todayCount + 1, room);

        await appointmentRepo.SaveChangesAsync(ct);

        // Generate e-ticket DTO
        var ticket = await ticketService.GenerateAsync(appointment.Id, ct);

        // AC6: send via Email + SMS (fire and forget with logging)
        if (appointment.Patient?.Email is { Length: > 0 } email)
            _ = ticketService.SendByEmailAsync(appointment.Id, email, ct);

        if (appointment.Patient?.PhoneNumber is { Length: > 0 } phone)
            _ = ticketService.SendBySmsAsync(appointment.Id, phone, ct);

        appointment.MarkETicketSent();
        await appointmentRepo.SaveChangesAsync(ct);

        return ticket;
    }

    // Simple room assignment — in production, load from Doctor schedule
    private static string AssignRoom(Guid doctorId)
        => $"P.{Math.Abs(doctorId.GetHashCode() % 100 + 100)}";
}

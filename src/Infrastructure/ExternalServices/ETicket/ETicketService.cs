using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.ExternalServices.ETicket;

/// <summary>
/// KAN-14 AC6: Generates digital e-tickets and sends via Email + SMS.
/// </summary>
public class ETicketService(
    AppDbContext   db,
    IEmailService  emailService,
    ISmsService    smsService) : IETicketService
{
    public async Task<ETicketDto> GenerateAsync(Guid appointmentId, CancellationToken ct = default)
    {
        var appt = await db.Set<Domain.Entities.Appointment>()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId, ct)
            ?? throw new InvalidOperationException("Lịch hẹn không tồn tại.");

        return new ETicketDto
        {
            AppointmentId    = appt.Id,
            BarcodeData      = appt.BarcodeData ?? appt.Id.ToString("N").ToUpper()[..12],
            SequenceNumber   = appt.SequenceNumber ?? 1,
            RoomNumber       = appt.RoomNumber ?? "TBA",
            DoctorName       = appt.Doctor?.FullName ?? "",
            Specialization   = appt.Doctor?.Specialization ?? "",
            PatientName      = appt.Patient?.FullName ?? "",
            ScheduledAt      = appt.ScheduledAt,
            ConsultationFee  = appt.ConsultationFee,
            PaymentReference = appt.PaymentReference ?? ""
        };
    }

    public async Task SendByEmailAsync(Guid appointmentId, string email, CancellationToken ct = default)
    {
        var ticket = await GenerateAsync(appointmentId, ct);
        var subject = $"[MediCare] Phiếu khám bệnh - STT #{ticket.SequenceNumber:D3}";
        var body = BuildEmailBody(ticket);
        await emailService.SendAsync(email, subject, body, ct);
    }

    public async Task SendBySmsAsync(Guid appointmentId, string phone, CancellationToken ct = default)
    {
        var ticket = await GenerateAsync(appointmentId, ct);
        var msg = $"[MediCare] Phieu kham #{ticket.SequenceNumber:D3} - " +
                  $"BS {ticket.DoctorName} - " +
                  $"{ticket.ScheduledAt:HH:mm dd/MM/yyyy} - " +
                  $"Phong {ticket.RoomNumber} - " +
                  $"Ma: {ticket.BarcodeData}";
        await smsService.SendOtpAsync(phone, msg, ct);
    }

    private static string BuildEmailBody(ETicketDto ticket) => $"""
        <div style="font-family:'Be Vietnam Pro',sans-serif;max-width:520px;margin:0 auto;padding:24px;border:1px solid #E2E8F0;border-radius:12px">
          <div style="background:linear-gradient(135deg,#042C53,#185FA5);color:#fff;padding:20px;border-radius:8px;margin-bottom:20px">
            <h2 style="margin:0;font-size:18px">Phiếu khám bệnh điện tử</h2>
            <p style="margin:4px 0 0;opacity:.8;font-size:13px">MediCare Appointment Management System</p>
          </div>

          <table style="width:100%;border-collapse:collapse;font-size:14px">
            <tr><td style="padding:8px 0;color:#6B6965">Số thứ tự</td>
                <td style="padding:8px 0;font-weight:700;font-size:22px;color:#185FA5">#{ticket.SequenceNumber:D3}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Bác sĩ</td>
                <td style="padding:8px 0;font-weight:600">{ticket.DoctorName}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Chuyên khoa</td>
                <td style="padding:8px 0">{ticket.Specialization}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Bệnh nhân</td>
                <td style="padding:8px 0">{ticket.PatientName}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Ngày giờ khám</td>
                <td style="padding:8px 0;font-weight:600">{ticket.ScheduledAt:HH:mm - dd/MM/yyyy}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Phòng khám</td>
                <td style="padding:8px 0;font-weight:600">{ticket.RoomNumber}</td></tr>
            <tr><td style="padding:8px 0;color:#6B6965">Phí khám</td>
                <td style="padding:8px 0;color:#0F6E56;font-weight:700">{ticket.ConsultationFee:N0} VNĐ</td></tr>
          </table>

          <div style="margin:20px 0;padding:16px;background:#F2F0EB;border-radius:8px;text-align:center">
            <p style="margin:0 0 8px;font-size:12px;color:#6B6965">Mã phiếu (quét tại quầy)</p>
            <p style="margin:0;font-size:24px;font-weight:700;letter-spacing:4px;color:#042C53;font-family:monospace">{ticket.BarcodeData}</p>
          </div>

          <p style="font-size:12px;color:#A8A5A1;text-align:center;margin:0">
            Vui lòng đến trước giờ hẹn 15 phút. Hotline: 1900 1234
          </p>
        </div>
        """;
}

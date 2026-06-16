using Application.Common.DTOs;

namespace Application.Common.Interfaces;

/// <summary>AC6: Generate and deliver e-tickets.</summary>
public interface IETicketService
{
    Task<ETicketDto> GenerateAsync(Guid appointmentId, CancellationToken ct = default);
    Task SendByEmailAsync(Guid appointmentId, string email, CancellationToken ct = default);
    Task SendBySmsAsync(Guid appointmentId, string phone, CancellationToken ct = default);
}

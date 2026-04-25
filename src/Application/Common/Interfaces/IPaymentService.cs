namespace Application.Common.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentUrlAsync(Guid appointmentId, decimal amount, CancellationToken ct = default);
    Task<bool> VerifyPaymentAsync(string reference, CancellationToken ct = default);
}

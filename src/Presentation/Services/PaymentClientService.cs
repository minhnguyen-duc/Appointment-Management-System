using Application.Common.Interfaces;

namespace Presentation.Services;

public class PaymentClientService(IPaymentService paymentService)
{
    public Task<string> CreateUrlAsync(Guid appointmentId, decimal amount = 50_000)
        => paymentService.CreatePaymentUrlAsync(appointmentId, amount);

    // Returns payment reference if paid, null otherwise
    public Task<string?> GetPaymentRefAsync(Guid appointmentId)
        => Task.FromResult<string?>(null); // TODO: wire to real payment status lookup
}

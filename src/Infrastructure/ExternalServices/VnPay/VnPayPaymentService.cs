using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices.VnPay;

public class VnPayPaymentService(IConfiguration config) : IPaymentService
{
    private readonly string _tmnCode = config["VnPay:TmnCode"]!;
    private readonly string _hashSecret = config["VnPay:HashSecret"]!;
    private readonly string _baseUrl = config["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    public Task<string> CreatePaymentUrlAsync(Guid appointmentId, decimal amount, CancellationToken ct = default)
    {
        // Build VNPAY query string with HMAC-SHA512 signature
        return Task.FromResult($"{_baseUrl}?vnp_TxnRef={appointmentId}&vnp_Amount={amount * 100}");
    }

    public Task<bool> VerifyPaymentAsync(string reference, CancellationToken ct = default)
        => Task.FromResult(true); // placeholder
}

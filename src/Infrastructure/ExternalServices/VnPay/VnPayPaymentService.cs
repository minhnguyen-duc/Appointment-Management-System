using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices.VnPay;

/// <summary>
/// KAN-14 AC5: VNPAY Sandbox integration with HMAC-SHA512 signature.
/// </summary>
public class VnPayPaymentService(IConfiguration config) : IPaymentService
{
    private readonly string _tmnCode    = config["VnPay:TmnCode"]    ?? "DEMO1234";
    private readonly string _hashSecret = config["VnPay:HashSecret"] ?? "DEMO_SECRET_KEY_AT_LEAST_32_CHARS";
    private readonly string _baseUrl    = config["VnPay:BaseUrl"]    ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    private readonly string _returnUrl  = config["VnPay:ReturnUrl"]  ?? "https://localhost:5001/booking/vnpay-return";

    public Task<string> CreatePaymentUrlAsync(Guid appointmentId, decimal amount, CancellationToken ct = default)
    {
        var vnpData = new SortedDictionary<string, string>
        {
            ["vnp_Version"]    = "2.1.0",
            ["vnp_Command"]    = "pay",
            ["vnp_TmnCode"]    = _tmnCode,
            ["vnp_Amount"]     = ((long)(amount * 100)).ToString(),
            ["vnp_CurrCode"]   = "VND",
            ["vnp_TxnRef"]     = appointmentId.ToString("N")[..20],
            ["vnp_OrderInfo"]  = $"Thanh toan lich kham {appointmentId:N}",
            ["vnp_OrderType"]  = "billpayment",
            ["vnp_Locale"]     = "vn",
            ["vnp_ReturnUrl"]  = _returnUrl,
            ["vnp_IpAddr"]     = "127.0.0.1",
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
        };

        var query = string.Join("&", vnpData.Select(kv =>
            $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        var raw   = string.Join("&", vnpData.Select(kv => $"{kv.Key}={kv.Value}"));
        var sig   = HmacSha512(_hashSecret, raw);

        return Task.FromResult($"{_baseUrl}?{query}&vnp_SecureHash={sig}");
    }

    public Task<bool> VerifyPaymentAsync(string reference, CancellationToken ct = default)
        => Task.FromResult(!string.IsNullOrWhiteSpace(reference));

    private static string HmacSha512(string key, string data)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLower();
    }
}

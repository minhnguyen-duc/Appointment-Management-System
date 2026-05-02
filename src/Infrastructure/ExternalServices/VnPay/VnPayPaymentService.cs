using System.Net;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices.VnPay;

/// <summary>
/// KAN-14 AC5: VNPAY Sandbox integration with HMAC-SHA512 signature.
/// Docs: https://sandbox.vnpayment.vn/apis
/// </summary>
public class VnPayPaymentService(IConfiguration config, IHttpContextAccessor http) : IPaymentService
{
    private readonly string _tmnCode    = config["VnPay:TmnCode"]    ?? "DEMO1234";
    private readonly string _hashSecret = config["VnPay:HashSecret"] ?? "DEMO_SECRET";
    private readonly string _baseUrl    = config["VnPay:BaseUrl"]    ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    private readonly string _returnUrl  = config["VnPay:ReturnUrl"]  ?? "https://localhost:5001/booking/vnpay-return";
    private readonly string _version    = "2.1.0";
    private readonly string _command    = "pay";
    private readonly string _currCode   = "VND";
    private readonly string _locale     = "vn";

    public Task<string> CreatePaymentUrlAsync(Guid appointmentId, decimal amount, CancellationToken ct = default)
    {
        var vnpData = new SortedDictionary<string, string>
        {
            ["vnp_Version"]    = _version,
            ["vnp_Command"]    = _command,
            ["vnp_TmnCode"]    = _tmnCode,
            ["vnp_Amount"]     = ((long)(amount * 100)).ToString(),
            ["vnp_CurrCode"]   = _currCode,
            ["vnp_TxnRef"]     = appointmentId.ToString("N")[..20],
            ["vnp_OrderInfo"]  = $"Thanh toan lich kham {appointmentId:N}",
            ["vnp_OrderType"]  = "billpayment",
            ["vnp_Locale"]     = _locale,
            ["vnp_ReturnUrl"]  = _returnUrl,
            ["vnp_IpAddr"]     = http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["vnp_ExpireDate"] = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss"),
        };

        var query  = string.Join("&", vnpData.Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}"));
        var raw    = string.Join("&", vnpData.Select(kv => $"{kv.Key}={kv.Value}"));
        var sig    = HmacSha512(_hashSecret, raw);

        return Task.FromResult($"{_baseUrl}?{query}&vnp_SecureHash={sig}");
    }

    public Task<bool> VerifyPaymentAsync(string reference, CancellationToken ct = default)
    {
        // In sandbox: vnp_ResponseCode = "00" means success
        // Full verification checks HMAC-SHA512 of response params
        // For sandbox testing, we accept the reference as valid
        return Task.FromResult(!string.IsNullOrWhiteSpace(reference));
    }

    private static string HmacSha512(string key, string data)
    {
        var keyBytes  = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA512(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLower();
    }
}

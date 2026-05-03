using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.ExternalServices.Zalo;

/// <summary>
/// Gửi OTP qua Zalo Official Account (ZNS - Zalo Notification Service).
/// AC2 - KAN-11: Hỗ trợ kênh gửi Zalo ngoài SMS; SLA 30 giây.
/// Được đăng ký là ISmsService thứ hai và chọn theo config "SmsGateway": "Zalo".
/// </summary>
public class ZaloOaSmsService(IConfiguration config, ILogger<ZaloOaSmsService> logger) : ISmsService
{
    private readonly string _accessToken = config["Zalo:OaAccessToken"] ?? "";
    private readonly string _templateId  = config["Zalo:OtpTemplateId"] ?? "";
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(28) };

    public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_accessToken))
        {
            logger.LogWarning("[DEV] Zalo OTP for {Phone}: {Otp} — Zalo not configured.", phoneNumber, otp);
            return;
        }

        // Normalize: Zalo ZNS dùng định dạng +84xxxxxxxxx
        var zaloPhone = phoneNumber.StartsWith('+') ? phoneNumber : "+84" + phoneNumber.TrimStart('0');

        var payload = new
        {
            phone   = zaloPhone,
            template_id   = _templateId,
            template_data = new { otp, expiry = "2 phút" },
            tracking_id   = Guid.NewGuid().ToString("N"),
        };

        using var request = new HttpRequestMessage(HttpMethod.Post,
            "https://business.openapi.zalo.me/message/template");
        request.Headers.Add("access_token", _accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Zalo ZNS error {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Không thể gửi OTP qua Zalo: {response.StatusCode}");
        }

        logger.LogInformation("OTP sent via Zalo ZNS to {Phone}", zaloPhone);
    }

    public async Task SendReminderAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        // Reuse same ZNS call với template nhắc lịch khám
        logger.LogInformation("[Zalo] Reminder to {Phone}: {Msg}", phoneNumber, message);
        await Task.CompletedTask;
    }
}

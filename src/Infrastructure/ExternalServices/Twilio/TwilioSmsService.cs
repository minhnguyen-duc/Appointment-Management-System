using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices.Twilio;

/// <summary>
/// SMS gateway qua Twilio REST API.
/// AC2 - KAN-11: Gửi OTP trong SLA 30 giây; timeout được set 28 giây để có buffer.
/// Fallback: nếu Twilio fail, hệ thống ném exception và caller có thể thử Zalo.
/// </summary>
public class TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger) : ISmsService
{
    private readonly string _accountSid  = config["Twilio:AccountSid"]  ?? "";
    private readonly string _authToken   = config["Twilio:AuthToken"]   ?? "";
    private readonly string _fromNumber  = config["Twilio:FromNumber"]  ?? "";
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(28) };

    public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_accountSid))
        {
            // Dev mode: log OTP ra console thay vì gửi thật
            logger.LogWarning("[DEV] OTP for {Phone}: {Otp} — Twilio not configured.", phoneNumber, otp);
            return;
        }

        var message = $"[MediCare] Ma OTP cua ban la: {otp}. Co hieu luc trong 2 phut. Khong chia se ma nay cho bat ky ai.";
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"))
        );
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"]   = phoneNumber,
            ["From"] = _fromNumber,
            ["Body"] = message,
        });

        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Twilio error {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Không thể gửi SMS: {response.StatusCode}");
        }

        logger.LogInformation("OTP sent via Twilio to {Phone}", phoneNumber);
    }

    public async Task SendReminderAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_accountSid))
        {
            logger.LogWarning("[DEV] Reminder to {Phone}: {Msg}", phoneNumber, message);
            return;
        }
        await SendOtpAsync(phoneNumber, message, ct); // reuse same send logic
    }
}

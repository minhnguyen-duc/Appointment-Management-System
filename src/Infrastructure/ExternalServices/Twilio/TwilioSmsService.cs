using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.ExternalServices.Twilio;

public class TwilioSmsService(IConfiguration config) : ISmsService
{
    private readonly string _accountSid = config["Twilio:AccountSid"]!;
    private readonly string _authToken = config["Twilio:AuthToken"]!;
    private readonly string _fromNumber = config["Twilio:FromNumber"]!;

    public async Task SendOtpAsync(string phoneNumber, string otp, CancellationToken ct = default)
    {
        // Twilio REST API call
        // Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(...)
        await Task.CompletedTask; // placeholder
    }

    public async Task SendReminderAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        await Task.CompletedTask; // placeholder
    }
}

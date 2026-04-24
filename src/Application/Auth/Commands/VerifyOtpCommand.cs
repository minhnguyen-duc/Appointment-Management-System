namespace Application.Auth.Commands;

public record VerifyOtpCommand(string PhoneNumber, string Otp);

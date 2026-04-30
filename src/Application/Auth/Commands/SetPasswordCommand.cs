namespace Application.Auth.Commands;

/// <summary>KAN-11 AC1: Đặt mật khẩu cho bệnh nhân mới sau khi OTP verify thành công.</summary>
public sealed record SetPasswordCommand(string PhoneNumber, string Password, string FullName);

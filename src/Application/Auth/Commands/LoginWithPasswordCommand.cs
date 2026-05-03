namespace Application.Auth.Commands;

/// <summary>KAN-11 AC1: Đăng nhập bằng mật khẩu (patient đã tồn tại).</summary>
public sealed record LoginWithPasswordCommand(string PhoneNumber, string Password);

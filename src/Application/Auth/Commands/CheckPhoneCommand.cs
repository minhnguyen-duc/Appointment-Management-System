namespace Application.Auth.Commands;

/// <summary>
/// Kiểm tra số điện thoại đã tồn tại trong hệ thống chưa.
/// KAN-11 AC1: Route sang password flow hoặc OTP flow.
/// </summary>
public sealed record CheckPhoneCommand(string PhoneNumber);

public sealed record CheckPhoneResult(bool Exists, bool HasPassword);

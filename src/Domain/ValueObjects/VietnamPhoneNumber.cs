using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

/// <summary>
/// Value object đại diện cho số điện thoại hợp lệ của Việt Nam.
/// Chấp nhận đầu số: 03x, 05x, 07x, 08x, 09x — tổng 10 chữ số.
/// AC1 - KAN-11: Validate số điện thoại theo định dạng Việt Nam.
/// </summary>
public sealed class VietnamPhoneNumber
{
    // Regex: bắt đầu bằng 0, tiếp theo là đầu số hợp lệ, tổng 10 ký tự
    private static readonly Regex _regex = new(
        @"^0(3[2-9]|5[6-9]|7[06-9]|8[0-9]|9[0-9])\d{7}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private VietnamPhoneNumber(string value) => Value = value;

    public static VietnamPhoneNumber Parse(string raw)
    {
        var cleaned = raw?.Trim().Replace(" ", "").Replace("-", "") ?? "";
        if (!_regex.IsMatch(cleaned))
            throw new ArgumentException(
                $"Số điện thoại '{raw}' không hợp lệ. Vui lòng nhập đầu số Việt Nam (03x/05x/07x/08x/09x) gồm 10 chữ số.");
        return new VietnamPhoneNumber(cleaned);
    }

    public static bool IsValid(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var cleaned = raw.Trim().Replace(" ", "").Replace("-", "");
        return _regex.IsMatch(cleaned);
    }

    /// <summary>Trả về dạng quốc tế +84xxxxxxxxx (bỏ số 0 đầu)</summary>
    public string ToInternationalFormat() => "+84" + Value[1..];

    public override string ToString() => Value;
}

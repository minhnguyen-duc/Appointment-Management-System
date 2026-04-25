using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Auth;

/// <summary>
/// Tests for VietnamPhoneNumber value object.
/// AC1 - KAN-11: Validate số điện thoại theo định dạng Việt Nam.
/// </summary>
public class VietnamPhoneNumberTests
{
    // ── IsValid: valid prefixes ──────────────────────────────────────────────

    [Theory]
    [InlineData("0321234567")]  // 03x
    [InlineData("0331234567")]
    [InlineData("0341234567")]
    [InlineData("0351234567")]
    [InlineData("0361234567")]
    [InlineData("0371234567")]
    [InlineData("0381234567")]
    [InlineData("0391234567")]
    [InlineData("0561234567")]  // 05x
    [InlineData("0571234567")]
    [InlineData("0581234567")]
    [InlineData("0591234567")]
    [InlineData("0701234567")]  // 07x
    [InlineData("0761234567")]
    [InlineData("0771234567")]
    [InlineData("0781234567")]
    [InlineData("0791234567")]
    [InlineData("0801234567")]  // 08x
    [InlineData("0811234567")]
    [InlineData("0821234567")]
    [InlineData("0831234567")]
    [InlineData("0841234567")]
    [InlineData("0851234567")]
    [InlineData("0861234567")]
    [InlineData("0871234567")]
    [InlineData("0881234567")]
    [InlineData("0891234567")]
    [InlineData("0901234567")]  // 09x
    [InlineData("0911234567")]
    [InlineData("0981234567")]
    [InlineData("0991234567")]
    public void IsValid_WithValidVietnamPrefixes_ReturnsTrue(string phone)
    {
        VietnamPhoneNumber.IsValid(phone).Should().BeTrue(
            because: $"{phone} is a valid Vietnam mobile number");
    }

    // ── IsValid: invalid prefixes ────────────────────────────────────────────

    [Theory]
    [InlineData("0101234567")]  // 01x — old format, deactivated
    [InlineData("0201234567")]  // 02x — landline prefix
    [InlineData("0401234567")]  // 04x — landline
    [InlineData("0601234567")]  // 06x — not assigned
    [InlineData("0711234567")]  // 071 — not valid
    [InlineData("0721234567")]  // 072 — not valid
    [InlineData("0731234567")]  // 073 — not valid
    [InlineData("0741234567")]  // 074 — not valid
    [InlineData("0751234567")]  // 075 — not valid
    public void IsValid_WithInvalidPrefixes_ReturnsFalse(string phone)
    {
        VietnamPhoneNumber.IsValid(phone).Should().BeFalse(
            because: $"{phone} is not a valid Vietnam mobile prefix");
    }

    // ── IsValid: wrong length ────────────────────────────────────────────────

    [Theory]
    [InlineData("090123456")]    // 9 digits
    [InlineData("09012345678")]  // 11 digits
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WithWrongLengthOrEmpty_ReturnsFalse(string? phone)
    {
        VietnamPhoneNumber.IsValid(phone).Should().BeFalse();
    }

    // ── IsValid: with spaces/dashes (should be accepted after cleaning) ──────

    [Theory]
    [InlineData("0912 345 678")]
    [InlineData("091-234-5678")]
    [InlineData(" 0912345678 ")]
    public void IsValid_WithSpacesOrDashes_ReturnsTrueAfterCleaning(string phone)
    {
        VietnamPhoneNumber.IsValid(phone).Should().BeTrue();
    }

    // ── Parse: successful ───────────────────────────────────────────────────

    [Fact]
    public void Parse_WithValidPhone_ReturnsValueObject()
    {
        var phone = VietnamPhoneNumber.Parse("0912345678");

        phone.Should().NotBeNull();
        phone.Value.Should().Be("0912345678");
    }

    [Fact]
    public void Parse_WithSpaces_NormalizesValue()
    {
        var phone = VietnamPhoneNumber.Parse("0912 345 678");
        phone.Value.Should().Be("0912345678");
    }

    // ── Parse: throws on invalid ─────────────────────────────────────────────

    [Fact]
    public void Parse_WithInvalidPhone_ThrowsArgumentException()
    {
        var act = () => VietnamPhoneNumber.Parse("0101234567");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*không hợp lệ*");
    }

    [Fact]
    public void Parse_WithNull_ThrowsArgumentException()
    {
        var act = () => VietnamPhoneNumber.Parse(null!);
        act.Should().Throw<ArgumentException>();
    }

    // ── ToInternationalFormat ────────────────────────────────────────────────

    [Fact]
    public void ToInternationalFormat_ReturnsE164Format()
    {
        var phone = VietnamPhoneNumber.Parse("0912345678");
        phone.ToInternationalFormat().Should().Be("+84912345678");
    }

    [Fact]
    public void ToInternationalFormat_StripsLeadingZero()
    {
        var phone = VietnamPhoneNumber.Parse("0331234567");
        phone.ToInternationalFormat().Should().StartWith("+84")
             .And.NotContain("084");
    }

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ReturnsLocalFormat()
    {
        var phone = VietnamPhoneNumber.Parse("0912345678");
        phone.ToString().Should().Be("0912345678");
    }
}

using Application.Auth.Commands;
using Application.Common.Interfaces;
using Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Application.UnitTests.Auth;

/// <summary>
/// Tests for VerifyOtpCommandHandler.
/// AC4 - KAN-11: Validate OTP and redirect to correct BENHNHAN interface.
/// </summary>
public class VerifyOtpCommandHandlerTests
{
    private readonly IOtpService          _otpService     = Substitute.For<IOtpService>();
    private readonly IPatientQueryService _patientQuery   = Substitute.For<IPatientQueryService>();
    private readonly VerifyOtpCommandHandler _sut;

    public VerifyOtpCommandHandlerTests()
    {
        _sut = new VerifyOtpCommandHandler(_otpService, _patientQuery);
    }

    // ── AC4: Successful verification — existing patient ───────────────────────

    [Fact]
    public async Task HandleAsync_WithValidOtp_ExistingPatient_ReturnsSessionAndDashboardUrl()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var existingPatient = new Patient
        {
            Id          = patientId,
            FullName    = "Nguyen Van A",
            PhoneNumber = "0912345678"
        };

        _otpService.ValidateAsync("0912345678", "123456").Returns(true);
        _patientQuery.GetByPhoneAsync("0912345678").Returns(existingPatient);

        // Act
        var result = await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "123456"));

        // Assert
        result.Should().NotBeNull();
        result.SessionToken.Should().StartWith("session:0912345678:");
        result.PatientId.Should().Be(patientId);
        result.IsNewPatient.Should().BeFalse();
        result.RedirectUrl.Should().Be("/Homepage/HomePage");
    }

    // ── AC4: Successful verification — new patient ────────────────────────────

    [Fact]
    public async Task HandleAsync_WithValidOtp_NewPatient_ReturnsRegisterUrl()
    {
        // Arrange: patient not found
        _otpService.ValidateAsync("0912345678", "999999").Returns(true);
        _patientQuery.GetByPhoneAsync("0912345678").Returns((Patient?)null);

        // Act
        var result = await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "999999"));

        // Assert
        result.IsNewPatient.Should().BeTrue();
        result.PatientId.Should().BeNull();
        result.RedirectUrl.Should().Be("/benhnhan/register");
    }

    // ── AC4: Invalid OTP ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithWrongOtp_ThrowsInvalidOperationException()
    {
        // Arrange
        _otpService.ValidateAsync("0912345678", "000000").Returns(false);

        // Act
        var act = async () => await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "000000"));

        // Assert: exception thrown, patient lookup never called
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*OTP*");
        await _patientQuery.DidNotReceive().GetByPhoneAsync(Arg.Any<string>());
    }

    // ── AC1: Phone validated before verify ────────────────────────────────────

    [Theory]
    [InlineData("0101234567")]
    [InlineData("abc")]
    [InlineData("")]
    public async Task HandleAsync_WithInvalidPhone_ThrowsArgumentException(string phone)
    {
        // Act
        var act = async () => await _sut.HandleAsync(new VerifyOtpCommand(phone, "123456"));

        // Assert: validation fails before touching OtpService
        await act.Should().ThrowAsync<ArgumentException>();
        await _otpService.DidNotReceive().ValidateAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    // ── Session token uniqueness ──────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_TwoSuccessfulVerifications_GenerateUniqueSessionTokens()
    {
        // Arrange
        _otpService.ValidateAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        _patientQuery.GetByPhoneAsync(Arg.Any<string>()).Returns((Patient?)null);

        // Act
        var result1 = await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "111111"));
        var result2 = await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "111111"));

        // Assert: sessions are unique (Guid-based)
        result1.SessionToken.Should().NotBe(result2.SessionToken);
    }

    // ── Cancellation token ────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToServices()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _otpService.ValidateAsync("0912345678", "123456", cts.Token).Returns(true);
        _patientQuery.GetByPhoneAsync("0912345678", cts.Token).Returns((Patient?)null);

        // Act
        await _sut.HandleAsync(new VerifyOtpCommand("0912345678", "123456"), cts.Token);

        // Assert
        await _otpService.Received(1).ValidateAsync("0912345678", "123456", cts.Token);
        await _patientQuery.Received(1).GetByPhoneAsync("0912345678", cts.Token);
    }
}

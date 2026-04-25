using Application.Auth.Commands;
using Application.Common.Interfaces;
using Domain.Exceptions;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Application.UnitTests.Auth;

/// <summary>
/// Tests for RequestOtpCommandHandler.
/// AC1 - KAN-11: phone validation
/// AC2 - KAN-11: OTP sent via gateway
/// AC3 - KAN-11: rate limit delegation
/// </summary>
public class RequestOtpCommandHandlerTests
{
    private readonly IOtpService  _otpService  = Substitute.For<IOtpService>();
    private readonly ISmsService  _smsService  = Substitute.For<ISmsService>();
    private readonly RequestOtpCommandHandler _sut;

    public RequestOtpCommandHandlerTests()
    {
        _sut = new RequestOtpCommandHandler(_smsService, _otpService);
    }

    // ── AC1: Vietnam phone validation ────────────────────────────────────────

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0331234567")]
    [InlineData("0761234567")]
    public async Task HandleAsync_WithValidVietnamPhone_GeneratesAndSendsOtp(string phone)
    {
        // Arrange
        _otpService.GenerateAndStoreAsync(Arg.Any<string>())
                   .Returns("123456");

        // Act
        await _sut.HandleAsync(new RequestOtpCommand(phone));

        // Assert: OTP was generated for the normalized phone
        await _otpService.Received(1).GenerateAndStoreAsync(
            Arg.Is<string>(p => p == phone.Replace(" ", "")));
    }

    [Theory]
    [InlineData("0101234567")]  // invalid prefix
    [InlineData("abcdefghij")]  // not numeric
    [InlineData("")]
    [InlineData("12345")]       // too short
    public async Task HandleAsync_WithInvalidPhone_ThrowsArgumentException(string phone)
    {
        // Act
        var act = async () => await _sut.HandleAsync(new RequestOtpCommand(phone));

        // Assert: validation fails before touching any service
        await act.Should().ThrowAsync<ArgumentException>();
        await _otpService.DidNotReceive().GenerateAndStoreAsync(Arg.Any<string>());
        await _smsService.DidNotReceive().SendOtpAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    // ── AC2: SMS/Zalo gateway send ───────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WithValidPhone_SendsOtpViaSmsGateway()
    {
        // Arrange
        const string generatedOtp = "654321";
        _otpService.GenerateAndStoreAsync(Arg.Any<string>())
                   .Returns(generatedOtp);

        // Act
        await _sut.HandleAsync(new RequestOtpCommand("0912345678"));

        // Assert: SMS sent with international format (+84) and correct OTP
        await _smsService.Received(1).SendOtpAsync(
            Arg.Is<string>(p => p == "+84912345678"),
            Arg.Is<string>(o => o == generatedOtp),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PhoneNormalized_SentAsInternationalFormat()
    {
        // Arrange
        _otpService.GenerateAndStoreAsync(Arg.Any<string>()).Returns("111111");

        // Act — phone with spaces
        await _sut.HandleAsync(new RequestOtpCommand("0912 345 678"));

        // Assert: +84 prefix, leading zero stripped
        await _smsService.Received(1).SendOtpAsync(
            Arg.Is<string>(p => p.StartsWith("+84") && !p.Contains("084")),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    // ── AC3: Rate limit propagation ──────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenRateLimitExceeded_ThrowsOtpRateLimitExceededException()
    {
        // Arrange: OtpService raises rate-limit exception
        var rateLimitEx = new OtpRateLimitExceededException(3, TimeSpan.FromMinutes(10), DateTime.UtcNow.AddMinutes(8));
        _otpService.GenerateAndStoreAsync(Arg.Any<string>())
                   .ThrowsAsync(rateLimitEx);

        // Act
        var act = async () => await _sut.HandleAsync(new RequestOtpCommand("0912345678"));

        // Assert: exception propagates; SMS is NOT sent
        await act.Should().ThrowAsync<OtpRateLimitExceededException>()
                 .Where(e => e.MaxAttempts == 3);
        await _smsService.DidNotReceive().SendOtpAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── General ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task HandleAsync_WhenGatewayFails_ExceptionBubblesUp()
    {
        // Arrange
        _otpService.GenerateAndStoreAsync(Arg.Any<string>()).Returns("999999");
        _smsService.SendOtpAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .ThrowsAsync(new InvalidOperationException("Gateway timeout"));

        // Act
        var act = async () => await _sut.HandleAsync(new RequestOtpCommand("0912345678"));

        // Assert: caller receives the gateway error
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Gateway timeout*");
    }

    [Fact]
    public async Task HandleAsync_CancellationToken_PassedThrough()
    {
        // Arrange
        _otpService.GenerateAndStoreAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                   .Returns("123456");
        var cts = new CancellationTokenSource();

        // Act
        await _sut.HandleAsync(new RequestOtpCommand("0912345678"), cts.Token);

        // Assert: CT passed to both services
        await _otpService.Received(1).GenerateAndStoreAsync(Arg.Any<string>(), cts.Token);
        await _smsService.Received(1).SendOtpAsync(Arg.Any<string>(), Arg.Any<string>(), cts.Token);
    }
}

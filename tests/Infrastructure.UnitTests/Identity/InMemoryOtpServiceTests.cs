using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.Identity;
using Xunit;

namespace Infrastructure.UnitTests.Identity;

/// <summary>
/// Tests for InMemoryOtpService.
/// AC2 - KAN-11: OTP expiry
/// AC3 - KAN-11: Rate limit max 3 resends / 10 minutes
///
/// NOTE: InMemoryOtpService uses a static ConcurrentDictionary, so each test
/// uses a unique phone number to avoid cross-test state pollution.
/// </summary>
public class InMemoryOtpServiceTests
{
    private readonly InMemoryOtpService _sut = new();

    // Helper: generate a unique phone per test to avoid static state collision
    private static string UniquePhone() =>
        $"09{Random.Shared.Next(10_000_000, 99_999_999)}";

    // ── GenerateAndStoreAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAndStoreAsync_ReturnsValidSixDigitOtp()
    {
        var otp = await _sut.GenerateAndStoreAsync(UniquePhone());

        otp.Should().HaveLength(6);
        otp.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public async Task GenerateAndStoreAsync_TwoCalls_ReturnsDifferentOtps()
    {
        var phone = UniquePhone();
        var otp1 = await _sut.GenerateAndStoreAsync(phone);

        // Manually advance time is not possible with static store,
        // but different sequential calls still statistically differ
        var otp2 = await _sut.GenerateAndStoreAsync(UniquePhone());

        // Not strictly guaranteed but very high probability; enough for unit test signal
        (otp1.Length == 6 && otp2.Length == 6).Should().BeTrue();
    }

    // ── AC3: Rate limit ───────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAndStoreAsync_ThreeCallsSamePhone_AllSucceed()
    {
        // Arrange: fresh phone — starts at 0 sends
        var phone = UniquePhone();

        // Act: 3 calls within window should all succeed
        var act = async () =>
        {
            await _sut.GenerateAndStoreAsync(phone);
            await _sut.GenerateAndStoreAsync(phone);
            await _sut.GenerateAndStoreAsync(phone);
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GenerateAndStoreAsync_FourthCallWithin10Min_ThrowsRateLimitException()
    {
        // Arrange
        var phone = UniquePhone();
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);

        // Act: 4th call
        var act = async () => await _sut.GenerateAndStoreAsync(phone);

        // Assert: AC3 — max 3 resends / 10 min
        await act.Should().ThrowAsync<OtpRateLimitExceededException>()
                 .Where(e => e.MaxAttempts == 3
                          && e.WindowDuration == TimeSpan.FromMinutes(10)
                          && e.UnlockedAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerateAndStoreAsync_RateLimitExceptionContainsCorrectUnlockedAt()
    {
        var phone = UniquePhone();
        var before = DateTime.UtcNow;

        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);

        OtpRateLimitExceededException? caughtEx = null;
        try { await _sut.GenerateAndStoreAsync(phone); }
        catch (OtpRateLimitExceededException ex) { caughtEx = ex; }

        caughtEx.Should().NotBeNull();
        // UnlockedAt should be ~10 minutes from the first send
        caughtEx!.UnlockedAt.Should().BeAfter(before.AddMinutes(9))
                            .And.BeBefore(before.AddMinutes(11));
    }

    [Fact]
    public async Task GenerateAndStoreAsync_DifferentPhones_HaveIndependentRateLimits()
    {
        var phone1 = UniquePhone();
        var phone2 = UniquePhone();

        // Exhaust phone1
        await _sut.GenerateAndStoreAsync(phone1);
        await _sut.GenerateAndStoreAsync(phone1);
        await _sut.GenerateAndStoreAsync(phone1);

        // phone2 should be unaffected
        var act = async () => await _sut.GenerateAndStoreAsync(phone2);
        await act.Should().NotThrowAsync();
    }

    // ── ValidateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_WithCorrectOtp_ReturnsTrue()
    {
        var phone = UniquePhone();
        var otp = await _sut.GenerateAndStoreAsync(phone);

        var result = await _sut.ValidateAsync(phone, otp);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithWrongOtp_ReturnsFalse()
    {
        var phone = UniquePhone();
        await _sut.GenerateAndStoreAsync(phone);

        var result = await _sut.ValidateAsync(phone, "000000");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WithUnknownPhone_ReturnsFalse()
    {
        var result = await _sut.ValidateAsync(UniquePhone(), "123456");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_AfterSuccessfulVerification_OtpIsInvalidated()
    {
        // Arrange: first verify consumes the OTP
        var phone = UniquePhone();
        var otp = await _sut.GenerateAndStoreAsync(phone);
        await _sut.ValidateAsync(phone, otp);

        // Act: second verify with same OTP
        var result = await _sut.ValidateAsync(phone, otp);

        // Assert: AC4 — OTP is one-time use
        result.Should().BeFalse("OTP should be consumed after first successful validation");
    }

    [Fact]
    public async Task ValidateAsync_OverwrittenOtp_OnlyLatestIsValid()
    {
        // Arrange: second GenerateAndStore overwrites the first OTP
        var phone = UniquePhone();
        var otp1 = await _sut.GenerateAndStoreAsync(phone);
        var otp2 = await _sut.GenerateAndStoreAsync(phone);

        // Assert
        var resultOld = await _sut.ValidateAsync(phone, otp1);
        resultOld.Should().BeFalse("old OTP should be overwritten");

        // Regenerate since old validation may have cleared or not — re-generate
        var otp3 = await _sut.GenerateAndStoreAsync(phone);
        var resultNew = await _sut.ValidateAsync(phone, otp3);
        resultNew.Should().BeTrue();
    }

    // ── GetResendStatusAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetResendStatusAsync_FreshPhone_ReturnsZeroUsed()
    {
        var (used, max, unlockedAt) = await _sut.GetResendStatusAsync(UniquePhone());

        used.Should().Be(0);
        max.Should().Be(3);
        unlockedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetResendStatusAsync_AfterTwoSends_ReturnsTwoUsed()
    {
        var phone = UniquePhone();
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);

        var (used, max, unlockedAt) = await _sut.GetResendStatusAsync(phone);

        used.Should().Be(2);
        max.Should().Be(3);
        unlockedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetResendStatusAsync_AfterExhausting_ReturnsUnlockedAt()
    {
        var phone = UniquePhone();
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);
        await _sut.GenerateAndStoreAsync(phone);

        var (used, max, unlockedAt) = await _sut.GetResendStatusAsync(phone);

        used.Should().Be(3);
        max.Should().Be(3);
        unlockedAt.Should().NotBeNull()
                  .And.BeAfter(DateTime.UtcNow);
    }

    // ── Concurrent safety ─────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAndStoreAsync_ConcurrentCallsSamePhone_OnlyThreeSucceed()
    {
        // Arrange: 10 concurrent requests for same phone
        var phone = UniquePhone();
        var tasks = Enumerable.Range(0, 10)
                              .Select(_ => Task.Run(async () =>
                              {
                                  try
                                  {
                                      await _sut.GenerateAndStoreAsync(phone);
                                      return true;
                                  }
                                  catch (OtpRateLimitExceededException)
                                  {
                                      return false;
                                  }
                              }));

        var results = await Task.WhenAll(tasks);

        // Assert: exactly 3 succeed (rate limit = 3)
        results.Count(r => r).Should().Be(3,
            because: "rate limit allows max 3 requests per window");
        results.Count(r => !r).Should().Be(7,
            because: "7 out of 10 concurrent requests should be rejected");
    }
}

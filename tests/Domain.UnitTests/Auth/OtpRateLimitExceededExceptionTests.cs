using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Auth;

public class OtpRateLimitExceededExceptionTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var maxAttempts   = 3;
        var window        = TimeSpan.FromMinutes(10);
        var unlockedAt    = DateTime.UtcNow.AddMinutes(7);

        var ex = new OtpRateLimitExceededException(maxAttempts, window, unlockedAt);

        ex.MaxAttempts.Should().Be(maxAttempts);
        ex.WindowDuration.Should().Be(window);
        ex.UnlockedAt.Should().BeCloseTo(unlockedAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Message_ContainsMaxAttemptsAndMinutes()
    {
        var ex = new OtpRateLimitExceededException(3, TimeSpan.FromMinutes(10), DateTime.UtcNow.AddMinutes(5));

        ex.Message.Should().Contain("3").And.Contain("10");
    }

    [Fact]
    public void IsException_CanBeCaughtAsBaseException()
    {
        var act = () => throw new OtpRateLimitExceededException(3, TimeSpan.FromMinutes(10), DateTime.UtcNow);
        act.Should().Throw<Exception>();
    }
}

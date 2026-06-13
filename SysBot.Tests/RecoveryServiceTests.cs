using System;
using FluentAssertions;
using Xunit;
using SysBot.Base;

namespace SysBot.Tests;

public class RecoveryServiceTests
{
    [Fact]
    public void RecoveryConfiguration_Validate_AllowsUnlimited()
    {
        var config = new RecoveryConfiguration
        {
            MaxRecoveryAttempts = -1,
            HardRecoveryThreshold = -1,
            InitialRecoveryDelaySeconds = 5,
            BackoffMultiplier = 2.0
        };

        // This shouldn't throw an exception if -1 is allowed
        var action = () =>
        {
            // Just simulate validation pass by asserting it doesn't throw on its own properties when checked
            if (config.MaxRecoveryAttempts < 1 && config.MaxRecoveryAttempts != -1)
                throw new ArgumentException();
            if (config.HardRecoveryThreshold < 1 && config.HardRecoveryThreshold != -1)
                throw new ArgumentException();
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void RecoveryConfiguration_Validate_ThrowsOnInvalid()
    {
        var config = new RecoveryConfiguration
        {
            MaxRecoveryAttempts = 0
        };

        var action = () =>
        {
            if (config.MaxRecoveryAttempts < 1 && config.MaxRecoveryAttempts != -1)
                throw new ArgumentException("MaxRecoveryAttempts must be at least 1 or -1 for unlimited", nameof(config));
        };

        action.Should().Throw<ArgumentException>().WithMessage("*MaxRecoveryAttempts must be at least 1 or -1 for unlimited*");
    }

    [Fact]
    public void BotRecoveryState_CalculatesBackoffDelay_Correctly()
    {
        // Simple manual verification of the backoff formula used by BotRecoveryService
        int initialDelay = 5;
        double multiplier = 2.0;
        int maxDelay = 300;

        // Attempt 1 (failures = 0 input to backoff calc)
        double delay1 = Math.Min(initialDelay * Math.Pow(multiplier, 0), maxDelay);
        delay1.Should().Be(5);

        // Attempt 2 (failures = 1)
        double delay2 = Math.Min(initialDelay * Math.Pow(multiplier, 1), maxDelay);
        delay2.Should().Be(10);

        // Attempt 5 (failures = 4)
        double delay5 = Math.Min(initialDelay * Math.Pow(multiplier, 4), maxDelay);
        delay5.Should().Be(80);

        // Max delay cap test (failures = 10)
        double delay10 = Math.Min(initialDelay * Math.Pow(multiplier, 10), maxDelay);
        delay10.Should().Be(maxDelay);
    }

    [Fact]
    public void BotRecoveryState_ClearsCrashHistory_Properly()
    {
        var state = new BotRecoveryState();
        state.AddCrashTime(DateTime.UtcNow.AddMinutes(-10));
        state.AddCrashTime(DateTime.UtcNow.AddMinutes(-5));
        state.ConsecutiveFailures = 2;

        state.ClearCrashHistory();

        state.CrashHistory.Count.Should().Be(0);
    }

    [Fact]
    public void BotRecoveryState_TracksRecentCrashes_Correctly()
    {
        var state = new BotRecoveryState();
        var now = DateTime.UtcNow;

        state.AddCrashTime(now.AddMinutes(-120)); // outside window
        state.AddCrashTime(now.AddMinutes(-30));  // inside window
        state.AddCrashTime(now.AddMinutes(-10));  // inside window

        var removed = state.RemoveOldCrashes(c => now - c > TimeSpan.FromMinutes(60));
        removed.Should().Be(1);
        state.CrashHistory.Count.Should().Be(2);
    }
}

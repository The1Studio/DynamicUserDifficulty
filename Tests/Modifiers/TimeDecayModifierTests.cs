using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;
using System;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    // Mock provider for testing (updated for stateless architecture)
    public sealed class MockTimeDecayProvider : ITimeDecayProvider
    {
        public DateTime LastPlayTime { get; set; } = DateTime.Now;
        public int DaysAway { get; set; } = 0;
        public TimeSpan TimeSinceLastPlay { get; set; } = TimeSpan.Zero;

        // ITimeDecayProvider methods (read-only)
        public DateTime GetLastPlayTime() => this.LastPlayTime;
        public TimeSpan GetTimeSinceLastPlay() => this.TimeSinceLastPlay; // Use controllable value
        public int GetDaysAwayFromGame() => this.DaysAway;
    }

    [TestFixture]
    public class TimeDecayModifierTests
    {
        private TimeDecayModifier modifier;
        private TimeDecayConfig config;
        private PlayerSessionData sessionData;
        private MockTimeDecayProvider mockProvider;

        [SetUp]
        public void Setup()
        {
            // Create typed config with test parameters
            this.config = new TimeDecayConfig().CreateDefault() as TimeDecayConfig;
            this.config.SetEnabled(true);
            this.config.SetPriority(1);

            // Create mock provider
            this.mockProvider = new();

            // Create modifier with typed config and provider
            this.modifier = new(this.config, this.mockProvider, null);

            // Create test session data
            this.sessionData = new();
    }

        [Test]
        public void Calculate_WithinGracePeriod_ReturnsZero()
        {
            // Arrange
            this.mockProvider.TimeSinceLastPlay = TimeSpan.FromHours(3); // Within 6 hour grace period

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_OneDayAfterGrace_ReturnsDecayPerDay()
    {
        // Arrange
        this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(1).Add(TimeSpan.FromHours(6)); // 1 day + grace period

        // Act
        var result = this.modifier.Calculate();

        // Assert
        Assert.AreEqual(-0.5f, result.Value, 0.1f); // One day of decay (0.5f per day)
    }

        [Test]
    public void Calculate_MultipleDays_ReturnsProportionalDecay()
    {
        // Arrange
        this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(3).Add(TimeSpan.FromHours(6)); // 3 days + grace

        // Act
        var result = this.modifier.Calculate();

        // Assert
        Assert.AreEqual(-1.5f, result.Value, 0.1f); // 3 days * 0.5 decay per day
    }

        [Test]
    public void Calculate_RespectsMaxDecay()
    {
        // Arrange
        this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(10); // Very long time

        // Act
        var result = this.modifier.Calculate();

        // Assert
        Assert.AreEqual(-2f, result.Value, 0.1f); // Capped at max decay (2f)
    }

        [Test]
    public void Calculate_FirstTimePlayer_ReturnsZero()
    {
        // Arrange - First time player would have just started
        this.mockProvider.TimeSinceLastPlay = TimeSpan.Zero; // Just played now

        // Act
        var result = this.modifier.Calculate();

        // Assert
        Assert.AreEqual(0f, result.Value); // Within grace period
    }

        [Test]
        public void Calculate_FutureTime_ReturnsZero()
        {
            // Arrange
            this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(-1); // Future time (negative duration shouldn't happen)

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_ExactGracePeriod_ReturnsZero()
    {
        // Arrange
        this.mockProvider.TimeSinceLastPlay = TimeSpan.FromHours(6); // Exactly at grace period

        // Act
        var result = this.modifier.Calculate();

        // Assert - Use tolerance for floating point comparison
        Assert.AreEqual(0f, result.Value, 0.0001f);
    }

        [Test]
    public void Calculate_WeekDecay_ReturnsCorrectDecay()
    {
        // Arrange
        this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(7).Add(TimeSpan.FromHours(6)); // 1 week + grace

        // Act
        var result = this.modifier.Calculate();

        // Assert
        // 7 days * 0.5 decay per day, but capped at max (2f)
        Assert.AreEqual(-2f, result.Value, 0.1f); // Capped at max decay
    }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.mockProvider.TimeSinceLastPlay = TimeSpan.FromDays(2);

            // Act - Call twice without delay since we use controllable mock time
            var result1 = this.modifier.Calculate();
            var result2 = this.modifier.Calculate();

            // Assert - Results should be identical since we use controllable time
            Assert.AreEqual(result1.Value, result2.Value, 0.01f);
        }

        [Test]
        public void GetModifierType_ReturnsTimeDecay()
        {
            // Act
            var type = this.modifier.ModifierName;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY, type);
        }

        [Test]
        public void Calculate_AlwaysReturnsNegativeOrZero()
        {
            // Test various time periods
            var testTimeSpans = new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromHours(3),
                TimeSpan.FromDays(1),
                TimeSpan.FromDays(5),
                TimeSpan.FromDays(30),
            };

            foreach (var timeSpan in testTimeSpans)
            {
                // Arrange
                this.mockProvider.TimeSinceLastPlay = timeSpan;

                // Act
                var result = this.modifier.Calculate();

                // Assert
                Assert.LessOrEqual(result.Value, 0f,
                    $"Time decay should always be negative or zero for timespan {timeSpan}");
            }
        }
    }
}
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using System;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    [TestFixture]
    public class TimeDecayModifierTests
    {
        private TimeDecayModifier modifier;
        private ModifierConfig config;
        private PlayerSessionData sessionData;

        [SetUp]
    public void Setup()
    {
        // Create config with test parameters
        this.config = new ModifierConfig();
        this.config.SetModifierType(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY);
        this.config.SetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY, 1.0f);
        this.config.SetParameter(DifficultyConstants.PARAM_GRACE_HOURS, 6f);  // Fixed: using correct constant
        this.config.SetParameter(DifficultyConstants.PARAM_MAX_DECAY, 5f);

        // Create modifier with config
        this.modifier = new TimeDecayModifier(this.config);

        // Create test session data
        this.sessionData = new PlayerSessionData();
    }

        [Test]
        public void Calculate_WithinGracePeriod_ReturnsZero()
        {
            // Arrange
            this.sessionData.LastPlayTime = DateTime.Now.AddHours(-3); // Within 6 hour grace period

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_OneDayAfterGrace_ReturnsDecayPerDay()
    {
        // Arrange
        this.sessionData.LastPlayTime = DateTime.Now.AddDays(-1).AddHours(-6); // 1 day + grace period

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-1.0f, result.Value, 0.1f); // One day of decay (1.0f per day)
    }

        [Test]
    public void Calculate_MultipleDays_ReturnsProportionalDecay()
    {
        // Arrange
        this.sessionData.LastPlayTime = DateTime.Now.AddDays(-3).AddHours(-6); // 3 days + grace

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-3.0f, result.Value, 0.1f); // 3 days * 1.0 decay per day
    }

        [Test]
    public void Calculate_RespectsMaxDecay()
    {
        // Arrange
        this.sessionData.LastPlayTime = DateTime.Now.AddDays(-10); // Very long time

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        Assert.AreEqual(-5f, result.Value, 0.1f); // Capped at max decay (5f)
    }

        [Test]
        public void Calculate_FirstTimePlayer_ReturnsZero()
        {
            // Arrange
            this.sessionData.LastPlayTime = default(DateTime); // Never played before

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void Calculate_FutureTime_ReturnsZero()
        {
            // Arrange
            this.sessionData.LastPlayTime = DateTime.Now.AddDays(1); // Future time (shouldn't happen)

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void Calculate_ExactGracePeriod_ReturnsZero()
        {
            // Arrange
            this.sessionData.LastPlayTime = DateTime.Now.AddHours(-6); // Exactly at grace period

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_WeekDecay_ReturnsCorrectDecay()
    {
        // Arrange
        this.sessionData.LastPlayTime = DateTime.Now.AddDays(-7).AddHours(-6); // 1 week + grace

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        // 7 days * 1.0 decay per day, but capped at max (5f)
        Assert.AreEqual(-5f, result.Value, 0.1f); // Capped at max decay
    }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.sessionData.LastPlayTime = DateTime.Now.AddDays(-2);

            // Act
            var result1 = this.modifier.Calculate(this.sessionData);
            System.Threading.Thread.Sleep(10); // Small delay
            var result2 = this.modifier.Calculate(this.sessionData);

            // Assert - Results should be very close (accounting for time passing)
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
            var testTimes = new[]
            {
                DateTime.Now,
                DateTime.Now.AddHours(-3),
                DateTime.Now.AddDays(-1),
                DateTime.Now.AddDays(-5),
                DateTime.Now.AddDays(-30)
            };

            foreach (var time in testTimes)
            {
                // Arrange
                this.sessionData.LastPlayTime = time;

                // Act
                var result = this.modifier.Calculate(this.sessionData);

                // Assert
                Assert.LessOrEqual(result.Value, 0f,
                    $"Time decay should always be negative or zero for time {time}");
            }
        }
    }
}
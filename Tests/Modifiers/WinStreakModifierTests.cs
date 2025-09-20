using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    using TheOneStudio.DynamicUserDifficulty.Modifiers;

    // Mock provider for testing (updated for provider pattern)
    public class MockWinStreakProvider : IWinStreakProvider
    {
        public int WinStreak { get; set; } = 0;
        public int LossStreak { get; set; } = 0;

        // IWinStreakProvider methods
        public int GetWinStreak() => WinStreak;
        public int GetLossStreak() => LossStreak;
        public void RecordWin() => WinStreak++;
        public void RecordLoss() => LossStreak++;
        public int GetTotalWins() => WinStreak; // For testing, use same as streak
        public int GetTotalLosses() => LossStreak; // For testing, use same as streak

        // IDifficultyDataProvider methods
        public PlayerSessionData GetSessionData() => new PlayerSessionData();
        public void SaveSessionData(PlayerSessionData data) { }
        public float GetCurrentDifficulty() => 5.0f;
        public void SaveDifficulty(float difficulty) { }
        public void ClearData() { }
    }

    [TestFixture]
    public class WinStreakModifierTests
    {
        private WinStreakModifier modifier;
        private ModifierConfig config;
        private PlayerSessionData sessionData;
        private MockWinStreakProvider mockProvider;

        [SetUp]
        public void Setup()
        {
            // Create config with test parameters
            this.config = new ModifierConfig();
            this.config.SetModifierType(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            this.config.SetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD, 3f);
            this.config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, 0.5f);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_BONUS, 2f);

            // Create mock provider
            this.mockProvider = new MockWinStreakProvider();

            // Create modifier with config and provider
        this.modifier = new WinStreakModifier(this.config, this.mockProvider);

            // Create test session data
            this.sessionData = new PlayerSessionData();
        }

        [Test]
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            this.mockProvider.WinStreak = 2; // Below threshold of 3

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, result.ModifierName);
        }

        [Test]
        public void Calculate_AtThreshold_ReturnsStepSize()
        {
            // Arrange
            this.mockProvider.WinStreak = 3; // At threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0.5f, result.Value); // One step size
        }

        [Test]
    public void Calculate_AboveThreshold_ReturnsProportionalIncrease()
    {
        // Arrange
        this.mockProvider.WinStreak = 5; // 2 above threshold

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        // (5 - 3 + 1) * 0.5 = 3 * 0.5 = 1.5
        Assert.AreEqual(1.5f, result.Value);
    }

        [Test]
        public void Calculate_RespectsMaxBonus()
        {
            // Arrange
            this.mockProvider.WinStreak = 10; // Way above threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(2f, result.Value); // Capped at max bonus
        }

        [Test]
        public void Calculate_WithZeroWinStreak_ReturnsZero()
        {
            // Arrange
            this.mockProvider.WinStreak = 0;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_WithNullSessionData_ReturnsNoChange()
    {
        // Act
        var result = this.modifier.Calculate(null);
        
        // Assert - Should return NoChange result, not throw exception
        Assert.AreEqual(0f, result.Value);
        Assert.IsNotNull(result);
    }

        [Test]
        public void IsEnabled_DefaultsToTrue()
        {
            // Assert
            Assert.IsTrue(this.modifier.IsEnabled);
        }

        [Test]
        public void Priority_ReturnsConfiguredPriority()
        {
            // Assert
            Assert.AreEqual(0, this.modifier.Priority);
        }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.mockProvider.WinStreak = 4;

            // Act
            var result1 = this.modifier.Calculate(this.sessionData);
            var result2 = this.modifier.Calculate(this.sessionData);

            // Assert - Same input should produce same output
            Assert.AreEqual(result1.Value, result2.Value);
        }

        [Test]
        public void Calculate_DifferentDifficultyLevels_SameResult()
        {
            // Arrange
            this.mockProvider.WinStreak = 4;

            // Act - Win streak modifier shouldn't be affected by current difficulty
            var result1 = this.modifier.Calculate(this.sessionData);
            var result2 = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(result1.Value, result2.Value);
        }
    }
}
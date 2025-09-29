using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{

    // Mock provider for testing (updated for stateless architecture)
    public sealed class MockWinStreakProvider : IWinStreakProvider
    {
        public int WinStreak { get; set; } = 0;
        public int LossStreak { get; set; } = 0;
        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;

        // IWinStreakProvider methods (read-only)
        public int GetWinStreak() => this.WinStreak;
        public int GetLossStreak() => this.LossStreak;
        public int GetTotalWins() => this.TotalWins;
        public int GetTotalLosses() => this.TotalLosses;
    }

    [TestFixture]
    public class WinStreakModifierTests
    {
        private WinStreakModifier modifier;
        private WinStreakConfig config;
        private PlayerSessionData sessionData;
        private MockWinStreakProvider mockProvider;

        [SetUp]
        public void Setup()
        {
            // Create typed config with test parameters
            this.config = new WinStreakConfig().CreateDefault() as WinStreakConfig;
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
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            this.mockProvider.WinStreak = 2; // Below threshold of 3

            // Act
            var result = this.modifier.Calculate();

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
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0.5f, result.Value); // One step size
        }

        [Test]
    public void Calculate_AboveThreshold_ReturnsProportionalIncrease()
    {
        // Arrange
        this.mockProvider.WinStreak = 5; // 2 above threshold

        // Act
        var result = this.modifier.Calculate();

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
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(2f, result.Value); // Capped at max bonus
        }

        [Test]
        public void Calculate_WithZeroWinStreak_ReturnsZero()
        {
            // Arrange
            this.mockProvider.WinStreak = 0;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
    public void Calculate_WithNullSessionData_ReturnsNoChange()
    {
        // Act
        var result = this.modifier.Calculate();

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
            Assert.AreEqual(1, this.modifier.Priority);
        }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.mockProvider.WinStreak = 4;

            // Act
            var result1 = this.modifier.Calculate();
            var result2 = this.modifier.Calculate();

            // Assert - Same input should produce same output
            Assert.AreEqual(result1.Value, result2.Value);
        }

        [Test]
        public void Calculate_DifferentDifficultyLevels_SameResult()
        {
            // Arrange
            this.mockProvider.WinStreak = 4;

            // Act - Win streak modifier shouldn't be affected by current difficulty
            var result1 = this.modifier.Calculate();
            var result2 = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(result1.Value, result2.Value);
        }
    }
}
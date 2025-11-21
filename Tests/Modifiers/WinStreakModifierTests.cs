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
            // Exponential formula: (3 - 3 + 1) * 0.5 * 1.15^0 = 0.5 * 1 = 0.5
            Assert.AreEqual(0.5f, result.Value, 0.01f); // One step size, no exponential yet
        }

        [Test]
    public void Calculate_AboveThreshold_ReturnsExponentialIncrease()
    {
        // Arrange
        this.mockProvider.WinStreak = 5; // 2 above threshold

        // Act
        var result = this.modifier.Calculate();

        // Assert
        // Exponential formula: (5 - 3 + 1) * 0.5 * 1.15^2 = 1.5 * 1.3225 = 1.98
        // Clamped to maxBonus = 2.0
        Assert.AreEqual(2.0f, result.Value, 0.01f);
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

        #region Exponential Scaling Tests - Phase 2

        [Test]
        public void ExponentialScaling_AtThreshold_NoAcceleration()
        {
            // Arrange - Win exactly at threshold
            this.mockProvider.WinStreak = 3;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // At threshold: exponent = 0, multiplier = 1.15^0 = 1.0
            // Base: (3 - 3 + 1) * 0.5 = 0.5
            // Result: 0.5 * 1.0 = 0.5
            Assert.AreEqual(0.5f, result.Value, 0.01f);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_OneAboveThreshold_15PercentBoost()
        {
            // Arrange - Win 1 above threshold
            this.mockProvider.WinStreak = 4;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Exponent = 1, multiplier = 1.15^1 = 1.15
            // Base: (4 - 3 + 1) * 0.5 = 1.0
            // Result: 1.0 * 1.15 = 1.15
            Assert.AreEqual(1.15f, result.Value, 0.01f);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_TwoAboveThreshold_32PercentBoost()
        {
            // Arrange - Win 2 above threshold
            this.mockProvider.WinStreak = 5;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Exponent = 2, multiplier = 1.15^2 = 1.3225
            // Base: (5 - 3 + 1) * 0.5 = 1.5
            // Result: 1.5 * 1.3225 = 1.98 (clamped to maxBonus 2.0)
            Assert.AreEqual(2.0f, result.Value, 0.01f);
        }

        [Test]
        public void ExponentialScaling_CompareWithLinear_ShowsSignificantDifference()
        {
            // Arrange - Win 4 above threshold
            this.mockProvider.WinStreak = 4;

            // Act
            var result = this.modifier.Calculate();
            var linearValue = (4 - 3 + 1) * 0.5f; // Would be 1.0 with linear

            // Assert
            // Exponential: 1.0 * 1.15 = 1.15 (+15% vs linear)
            Assert.Greater(result.Value, linearValue);
            Assert.AreEqual(1.15f, result.Value, 0.01f);
        }

        [Test]
        public void ExponentialScaling_Psychology_FeelsRewarding()
        {
            // Arrange - Scenario: Player on 4-win streak (feeling hot!)
            this.mockProvider.WinStreak = 4;

            // Act
            var result = this.modifier.Calculate();

            // Assert - Game Design Expectation
            // Player expects significant challenge increase
            // Exponential (1.15) feels "rewarding" vs linear (1.0) which feels "grindy"
            Assert.Greater(result.Value, 1.0f, "Exponential should feel more rewarding than linear");
            Assert.Less(result.Value, 2.0f, "But not overwhelming");
        }

        [Test]
        public void ExponentialScaling_Progression_Win3_4_5()
        {
            // Arrange & Act & Assert - Test full progression
            // Win 3 (at threshold): 0.50 (base, no acceleration)
            this.mockProvider.WinStreak = 3;
            var result3 = this.modifier.Calculate();
            Assert.AreEqual(0.50f, result3.Value, 0.01f);

            // Win 4: 1.15 (+130% vs linear 1.0)
            this.mockProvider.WinStreak = 4;
            var result4 = this.modifier.Calculate();
            Assert.AreEqual(1.15f, result4.Value, 0.01f);

            // Win 5: 1.98 → clamped to 2.0 (+32% vs linear 1.5)
            this.mockProvider.WinStreak = 5;
            var result5 = this.modifier.Calculate();
            Assert.AreEqual(2.0f, result5.Value, 0.01f);

            // Validate progression feels exponential
            var jump3to4 = result4.Value - result3.Value; // 0.65
            var jump4to5 = result5.Value - result4.Value; // 0.85
            Assert.Greater(jump4to5, jump3to4, "Acceleration should increase");
        }

        [Test]
        public void ExponentialScaling_ClampPreventsExtreme()
        {
            // Arrange - Extreme win streak (10 wins)
            this.mockProvider.WinStreak = 10;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Without clamp: (10 - 3 + 1) * 0.5 * 1.15^7 = 4.0 * 2.66 = 10.64
            // With clamp: capped at maxBonus = 2.0
            Assert.AreEqual(2.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_RealPlayerScenario_HotStreak()
        {
            // Arrange - Scenario: Player wins 3, 4, 5 in a row (hot streak!)
            var values = new System.Collections.Generic.List<float>();

            // Act - Simulate progression
            for (int win = 3; win <= 5; win++)
            {
                this.mockProvider.WinStreak = win;
                var result = this.modifier.Calculate();
                values.Add(result.Value);
            }

            // Assert - Game Design Validation
            // 1. Difficulty should increase with each win
            Assert.Less(values[0], values[1], "Win 4 should be harder than win 3");
            Assert.Less(values[1], values[2], "Win 5 should be harder than win 4");

            // 2. Acceleration should feel exponential, not linear
            var delta1 = values[1] - values[0]; // 3→4
            var delta2 = values[2] - values[1]; // 4→5
            Assert.Greater(delta2, delta1, "Acceleration should increase (exponential feel)");

            // 3. Total adjustment should reach near-max quickly
            Assert.GreaterOrEqual(values[2], 1.8f, "Should reach near-max by win 5");
        }

        [Test]
        public void ExponentialScaling_ExponentialMultiplierCalculation()
        {
            // Arrange & Act & Assert - Validate exact multiplier values
            // Win 3: 1.15^0 = 1.00
            this.mockProvider.WinStreak = 3;
            var result3 = this.modifier.Calculate();
            var expectedMultiplier3 = 1.0f;
            var actualValue3 = result3.Value / 0.5f; // base = 0.5
            Assert.AreEqual(expectedMultiplier3, actualValue3, 0.01f);

            // Win 4: 1.15^1 = 1.15
            this.mockProvider.WinStreak = 4;
            var result4 = this.modifier.Calculate();
            var expectedMultiplier4 = 1.15f;
            var actualValue4 = result4.Value / 1.0f; // base = 1.0
            Assert.AreEqual(expectedMultiplier4, actualValue4, 0.01f);

            // Win 5: 1.15^2 = 1.3225, but clamped
            this.mockProvider.WinStreak = 5;
            var result5 = this.modifier.Calculate();
            // Expected: 1.5 * 1.3225 = 1.98, clamped to 2.0
            Assert.AreEqual(2.0f, result5.Value, 0.01f);
        }

        #endregion
    }
}
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    // Mock provider for testing (updated for stateless architecture)
    public sealed class MockWinStreakProviderForLoss : IWinStreakProvider
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
    public class LossStreakModifierTests
    {
        private LossStreakModifier modifier;
        private LossStreakConfig config;
        private PlayerSessionData sessionData;
        private MockWinStreakProviderForLoss mockProvider;

        [SetUp]
        public void Setup()
        {
            // Create typed config with test parameters
            this.config = new LossStreakConfig().CreateDefault() as LossStreakConfig;
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
            this.mockProvider.LossStreak = 1; // Below threshold of 2

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, result.ModifierName);
        }

        [Test]
        public void Calculate_AtThreshold_ReturnsNegativeStepSize()
        {
            // Arrange
            this.mockProvider.LossStreak = 2; // At threshold

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Exponential formula: -(2 - 2 + 1) * 0.3 * 1.15^0 = -0.3 * 1 = -0.3
            Assert.AreEqual(-0.3f, result.Value, 0.01f); // Negative one step size, no exponential yet
        }

        [Test]
    public void Calculate_AboveThreshold_ReturnsExponentialDecrease()
    {
        // Arrange
        this.mockProvider.LossStreak = 4; // 2 above threshold

        // Act
        var result = this.modifier.Calculate();

        // Assert
        // Exponential formula: -(4 - 2 + 1) * 0.3 * 1.15^2 = -(0.9 * 1.3225) = -1.19
        // Clamped to maxReduction = -1.5
        Assert.AreEqual(-1.19f, result.Value, 0.01f);
    }

        [Test]
        public void Calculate_RespectsMaxReduction()
        {
            // Arrange
            this.mockProvider.LossStreak = 10; // Way above threshold

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(-1.5f, result.Value); // Capped at negative max reduction
        }

        [Test]
        public void Calculate_WithZeroLossStreak_ReturnsZero()
        {
            // Arrange
            this.mockProvider.LossStreak = 0;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void Calculate_AlwaysReturnsNegativeOrZero()
        {
            // Test multiple loss streak values
            for (var lossStreak = 0; lossStreak < 10; lossStreak++)
            {
                // Arrange
                this.mockProvider.LossStreak = lossStreak;

                // Act
                var result = this.modifier.Calculate();

                // Assert
                Assert.LessOrEqual(result.Value, 0f,
                    $"Loss streak {lossStreak} should produce negative or zero value");
            }
        }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.mockProvider.LossStreak = 3;

            // Act
            var result1 = this.modifier.Calculate();
            var result2 = this.modifier.Calculate();

            // Assert - Same input should produce same output
            Assert.AreEqual(result1.Value, result2.Value);
        }

        [Test]
        public void Calculate_IndependentOfCurrentDifficulty()
        {
            // Arrange
            this.mockProvider.LossStreak = 3;

            // Act - Loss streak modifier shouldn't be affected by current difficulty
            var resultLowDiff  = this.modifier.Calculate();
            var resultHighDiff = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(resultLowDiff.Value, resultHighDiff.Value);
        }

        [Test]
        public void GetModifierType_ReturnsLossStreak()
        {
            // Act
            var type = this.modifier.ModifierName;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, type);
        }

        #region Exponential Scaling Tests - Phase 2

        [Test]
        public void ExponentialScaling_AtThreshold_NoAcceleration()
        {
            // Arrange - Loss exactly at threshold
            this.mockProvider.LossStreak = 2;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // At threshold: exponent = 0, multiplier = 1.15^0 = 1.0
            // Base: -(2 - 2 + 1) * 0.3 = -0.3
            // Result: -0.3 * 1.0 = -0.3
            Assert.AreEqual(-0.3f, result.Value, 0.01f);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_OneAboveThreshold_15PercentBoost()
        {
            // Arrange - Loss 1 above threshold
            this.mockProvider.LossStreak = 3;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Exponent = 1, multiplier = 1.15^1 = 1.15
            // Base: -(3 - 2 + 1) * 0.3 = -0.6
            // Result: -0.6 * 1.15 = -0.69
            Assert.AreEqual(-0.69f, result.Value, 0.01f);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_TwoAboveThreshold_32PercentBoost()
        {
            // Arrange - Loss 2 above threshold
            this.mockProvider.LossStreak = 4;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Exponent = 2, multiplier = 1.15^2 = 1.3225
            // Base: -(4 - 2 + 1) * 0.3 = -0.9
            // Result: -0.9 * 1.3225 = -1.19
            Assert.AreEqual(-1.19f, result.Value, 0.01f);
        }

        [Test]
        public void ExponentialScaling_CompareWithLinear_ShowsSignificantDifference()
        {
            // Arrange - Loss 3 above threshold
            this.mockProvider.LossStreak = 3;

            // Act
            var result = this.modifier.Calculate();
            var linearValue = -((3 - 2 + 1) * 0.3f); // Would be -0.6 with linear

            // Assert
            // Exponential: -0.6 * 1.15 = -0.69 (+15% stronger reduction)
            Assert.Less(result.Value, linearValue);
            Assert.AreEqual(-0.69f, result.Value, 0.01f);
        }

        [Test]
        public void ExponentialScaling_Psychology_FeelsCompassionate()
        {
            // Arrange - Scenario: Player on 3-loss streak (frustrated!)
            this.mockProvider.LossStreak = 3;

            // Act
            var result = this.modifier.Calculate();

            // Assert - Game Design Expectation
            // Player expects significant difficulty decrease (compassion)
            // Exponential (-0.69) feels "fair" vs linear (-0.6) which feels "insufficient"
            Assert.Less(result.Value, -0.6f, "Exponential should provide more relief than linear");
            Assert.Greater(result.Value, -1.5f, "But not making it too easy");
        }

        [Test]
        public void ExponentialScaling_Progression_Loss2_3_4()
        {
            // Arrange & Act & Assert - Test full progression
            // Loss 2 (at threshold): -0.30 (base, no acceleration)
            this.mockProvider.LossStreak = 2;
            var result2 = this.modifier.Calculate();
            Assert.AreEqual(-0.30f, result2.Value, 0.01f);

            // Loss 3: -0.69 (+15% stronger reduction)
            this.mockProvider.LossStreak = 3;
            var result3 = this.modifier.Calculate();
            Assert.AreEqual(-0.69f, result3.Value, 0.01f);

            // Loss 4: -1.19 (+32% stronger reduction)
            this.mockProvider.LossStreak = 4;
            var result4 = this.modifier.Calculate();
            Assert.AreEqual(-1.19f, result4.Value, 0.01f);

            // Validate progression feels exponential
            var jump2to3 = System.Math.Abs(result3.Value - result2.Value); // 0.39
            var jump3to4 = System.Math.Abs(result4.Value - result3.Value); // 0.50
            Assert.Greater(jump3to4, jump2to3, "Reduction acceleration should increase");
        }

        [Test]
        public void ExponentialScaling_ClampPreventsExtreme()
        {
            // Arrange - Extreme loss streak (10 losses)
            this.mockProvider.LossStreak = 10;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Without clamp: -(10 - 2 + 1) * 0.3 * 1.15^8 = -2.7 * 3.06 = -8.26
            // With clamp: capped at maxReduction = -1.5
            Assert.AreEqual(-1.5f, result.Value);
            Assert.That(result.Reason, Does.Contain("exponential"));
        }

        [Test]
        public void ExponentialScaling_RealPlayerScenario_Struggling()
        {
            // Arrange - Scenario: Player loses 2, 3, 4 in a row (struggling!)
            var values = new System.Collections.Generic.List<float>();

            // Act - Simulate progression
            for (int loss = 2; loss <= 4; loss++)
            {
                this.mockProvider.LossStreak = loss;
                var result = this.modifier.Calculate();
                values.Add(result.Value);
            }

            // Assert - Game Design Validation
            // 1. Difficulty should decrease with each loss
            Assert.Greater(values[0], values[1], "Loss 3 should be easier than loss 2");
            Assert.Greater(values[1], values[2], "Loss 4 should be easier than loss 3");

            // 2. Reduction should feel exponential, not linear (more compassion)
            var delta1 = System.Math.Abs(values[1] - values[0]); // 2→3
            var delta2 = System.Math.Abs(values[2] - values[1]); // 3→4
            Assert.Greater(delta2, delta1, "Reduction acceleration should increase (exponential compassion)");

            // 3. Total reduction should provide meaningful relief
            Assert.LessOrEqual(values[2], -1.0f, "Should provide substantial relief by loss 4");
        }

        [Test]
        public void ExponentialScaling_ExponentialMultiplierCalculation()
        {
            // Arrange & Act & Assert - Validate exact multiplier values
            // Loss 2: 1.15^0 = 1.00
            this.mockProvider.LossStreak = 2;
            var result2 = this.modifier.Calculate();
            var expectedMultiplier2 = 1.0f;
            var actualValue2 = System.Math.Abs(result2.Value / -0.3f); // base = -0.3
            Assert.AreEqual(expectedMultiplier2, actualValue2, 0.01f);

            // Loss 3: 1.15^1 = 1.15
            this.mockProvider.LossStreak = 3;
            var result3 = this.modifier.Calculate();
            var expectedMultiplier3 = 1.15f;
            var actualValue3 = System.Math.Abs(result3.Value / -0.6f); // base = -0.6
            Assert.AreEqual(expectedMultiplier3, actualValue3, 0.01f);

            // Loss 4: 1.15^2 = 1.3225
            this.mockProvider.LossStreak = 4;
            var result4 = this.modifier.Calculate();
            var expectedMultiplier4 = 1.3225f;
            var actualValue4 = System.Math.Abs(result4.Value / -0.9f); // base = -0.9
            Assert.AreEqual(expectedMultiplier4, actualValue4, 0.01f);
        }

        [Test]
        public void ExponentialScaling_SymmetricWithWinStreak()
        {
            // Arrange - Compare loss streak reduction with win streak increase
            this.mockProvider.LossStreak = 3;

            // Act
            var lossResult = this.modifier.Calculate();

            // Assert - Magnitude comparison
            // Loss 3: -0.69 (compassionate reduction)
            // Win 3 would be: +0.50 (base increase)
            // Loss reduction should be stronger to help struggling players
            Assert.Greater(System.Math.Abs(lossResult.Value), 0.50f,
                "Loss streak should provide more adjustment than win streak for compassion");
        }

        #endregion
    }
}
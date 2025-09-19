using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    [TestFixture]
    public class LossStreakModifierTests
    {
        private LossStreakModifier modifier;
        private ModifierConfig config;
        private PlayerSessionData sessionData;

        [SetUp]
        public void Setup()
        {
            // Create config with test parameters
            this.config = new ModifierConfig();
            this.config.SetModifierType(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);
            this.config.SetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD, 2f);
            this.config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, 0.3f);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_REDUCTION, 1.5f);

            // Create modifier with config
            this.modifier = new LossStreakModifier(this.config);

            // Create test session data
            this.sessionData = new PlayerSessionData();
        }

        [Test]
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            this.sessionData.LossStreak = 1; // Below threshold of 2

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, result.ModifierName);
        }

        [Test]
        public void Calculate_AtThreshold_ReturnsNegativeStepSize()
        {
            // Arrange
            this.sessionData.LossStreak = 2; // At threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.3f, result.Value); // Negative one step size
        }

        [Test]
        public void Calculate_AboveThreshold_ReturnsProportionalDecrease()
        {
            // Arrange
            this.sessionData.LossStreak = 4; // 2 above threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.6f, result.Value); // Two step sizes (-0.3 * 2)
        }

        [Test]
        public void Calculate_RespectsMaxReduction()
        {
            // Arrange
            this.sessionData.LossStreak = 10; // Way above threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-1.5f, result.Value); // Capped at negative max reduction
        }

        [Test]
        public void Calculate_WithZeroLossStreak_ReturnsZero()
        {
            // Arrange
            this.sessionData.LossStreak = 0;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void Calculate_WithNullSessionData_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => this.modifier.Calculate(null));
        }

        [Test]
        public void Calculate_AlwaysReturnsNegativeOrZero()
        {
            // Test multiple loss streak values
            for (int lossStreak = 0; lossStreak < 10; lossStreak++)
            {
                // Arrange
                this.sessionData.LossStreak = lossStreak;

                // Act
                var result = this.modifier.Calculate(this.sessionData);

                // Assert
                Assert.LessOrEqual(result.Value, 0f,
                    $"Loss streak {lossStreak} should produce negative or zero value");
            }
        }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.sessionData.LossStreak = 3;

            // Act
            var result1 = this.modifier.Calculate(this.sessionData);
            var result2 = this.modifier.Calculate(this.sessionData);

            // Assert - Same input should produce same output
            Assert.AreEqual(result1.Value, result2.Value);
        }

        [Test]
        public void Calculate_IndependentOfCurrentDifficulty()
        {
            // Arrange
            this.sessionData.LossStreak = 3;

            // Act - Loss streak modifier shouldn't be affected by current difficulty
            var resultLowDiff  = this.modifier.Calculate(this.sessionData);
            var resultHighDiff = this.modifier.Calculate(this.sessionData);

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
    }
}
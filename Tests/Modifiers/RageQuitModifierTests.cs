using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    using TheOneStudio.DynamicUserDifficulty.Modifiers;

    [TestFixture]
    public class RageQuitModifierTests
    {
        private RageQuitModifier modifier;
        private ModifierConfig config;
        private PlayerSessionData sessionData;

        [SetUp]
        public void Setup()
        {
            // Create config with test parameters
            this.config = new ModifierConfig();
            this.config.SetModifierType(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT);
            this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_TIME, 30f);
            this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_PENALTY, 1.5f);
            this.config.SetParameter(DifficultyConstants.PARAM_LOSS_MULTIPLIER, 1.5f);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_PENALTY, 3f);

            // Create modifier with config
            this.modifier = new RageQuitModifier(this.config);

            // Create test session data
            this.sessionData = new PlayerSessionData();
        }

        [Test]
        public void Calculate_NoQuit_ReturnsZero()
        {
            // Arrange
            this.sessionData.QuitType   = null;
            this.sessionData.SessionLength = 100f;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, result.ModifierName);
        }

        [Test]
        public void Calculate_RageQuit_ReturnsFullReduction()
        {
            // Arrange
            this.sessionData.QuitType   = QuitType.RageQuit;
            this.sessionData.SessionLength = 25f; // Below rage quit threshold of 30 seconds

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-1f, result.Value); // Full rage quit reduction
        }

        [Test]
        public void Calculate_NormalQuit_ReturnsQuitReduction()
        {
            // Arrange
            this.sessionData.QuitType   = QuitType.Quit;
            this.sessionData.SessionLength = 60f; // Above rage quit threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.5f, result.Value); // Normal quit reduction
        }

        [Test]
        public void Calculate_MidPlayQuit_LowProgress_ReturnsFullReduction()
        {
            // Arrange
            this.sessionData.QuitType     = QuitType.MidPlay;
            this.sessionData.CurrentProgress = 0.2f; // Low progress

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.3f, result.Value); // Full mid-play reduction
        }

        [Test]
        public void Calculate_MidPlayQuit_HighProgress_ReturnsPartialReduction()
        {
            // Arrange
            this.sessionData.QuitType     = QuitType.MidPlay;
            this.sessionData.CurrentProgress = 0.8f; // High progress

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // High progress reduces the penalty
            float expected = -0.3f * DifficultyConstants.MID_PLAY_PARTIAL_MULTIPLIER;
            Assert.AreEqual(expected, result.Value);
        }

        [Test]
        public void Calculate_MidPlayQuit_FullProgress_ReturnsZero()
        {
            // Arrange
            this.sessionData.QuitType     = QuitType.MidPlay;
            this.sessionData.CurrentProgress = 1f; // Complete

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value); // No penalty for completed levels
        }

        [Test]
        public void Calculate_SessionLengthAtThreshold_UsesRageQuit()
        {
            // Arrange
            this.sessionData.QuitType   = QuitType.Quit;
            this.sessionData.SessionLength = 30f; // Exactly at threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-1f, result.Value); // Should use rage quit reduction
        }

        [Test]
        public void Calculate_SessionLengthJustAboveThreshold_UsesNormalQuit()
        {
            // Arrange
            this.sessionData.QuitType   = QuitType.Quit;
            this.sessionData.SessionLength = 31f; // Just above threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.5f, result.Value); // Should use normal quit reduction
        }

        [Test]
        public void Calculate_AlwaysReturnsNegativeOrZero()
        {
            // Test all quit types
            var quitTypes = new[] { QuitType.RageQuit, QuitType.Quit, QuitType.MidPlay };

            foreach (var quitType in quitTypes)
            {
                // Arrange
                this.sessionData.QuitType      = quitType;
                this.sessionData.SessionLength = 50f;
                this.sessionData.CurrentProgress  = 0.5f;

                // Act
                var result = this.modifier.Calculate(this.sessionData);

                // Assert
                Assert.LessOrEqual(result.Value, 0f,
                    $"Quit type {quitType} should produce negative or zero value");
            }
        }

        [Test]
        public void Calculate_WithNullSessionData_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => this.modifier.Calculate(null));
        }

        [Test]
        public void Calculate_ConsistentResults()
        {
            // Arrange
            this.sessionData.QuitType   = QuitType.Quit;
            this.sessionData.SessionLength = 45f;

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
            this.sessionData.QuitType   = QuitType.RageQuit;
            this.sessionData.SessionLength = 15f;

            // Act - Rage quit modifier shouldn't be affected by current difficulty
            var resultLowDiff  = this.modifier.Calculate(this.sessionData);
            var resultHighDiff = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(resultLowDiff.Value, resultHighDiff.Value);
        }

        [Test]
        public void GetModifierType_ReturnsRageQuit()
        {
            // Act
            var type = this.modifier.ModifierName;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, type);
        }

        [Test]
        public void Calculate_MidPlayQuit_ProgressScaling()
        {
            // Test that mid-play penalty scales with progress
            this.sessionData.QuitType = QuitType.MidPlay;

            // Low progress - full penalty
            this.sessionData.CurrentProgress = 0.1f;
            var lowProgressResult = this.modifier.Calculate(this.sessionData);

            // Medium progress - partial penalty
            this.sessionData.CurrentProgress = 0.5f;
            var midProgressResult = this.modifier.Calculate(this.sessionData);

            // High progress - reduced penalty
            this.sessionData.CurrentProgress = 0.9f;
            var highProgressResult = this.modifier.Calculate(this.sessionData);

            // Assert that penalty decreases with progress
            Assert.Less(lowProgressResult.Value, midProgressResult.Value);
            Assert.Less(midProgressResult.Value, highProgressResult.Value);
        }
    }
}
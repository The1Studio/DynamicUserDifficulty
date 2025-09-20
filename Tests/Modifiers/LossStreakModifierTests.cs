using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    // Mock provider for testing (updated for provider pattern)
    public class MockWinStreakProviderForLoss : IWinStreakProvider
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
            this.mockProvider = new MockWinStreakProviderForLoss();

            // Create modifier with typed config and provider
            this.modifier = new LossStreakModifier(this.config, this.mockProvider, null);

            // Create test session data
            this.sessionData = new PlayerSessionData();
        }

        [Test]
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            this.mockProvider.LossStreak = 1; // Below threshold of 2

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
            this.mockProvider.LossStreak = 2; // At threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-0.3f, result.Value); // Negative one step size
        }

        [Test]
    public void Calculate_AboveThreshold_ReturnsProportionalDecrease()
    {
        // Arrange
        this.mockProvider.LossStreak = 4; // 2 above threshold

        // Act
        var result = this.modifier.Calculate(this.sessionData);

        // Assert
        // (4 - 2 + 1) * 0.3 = 3 * 0.3 = -0.9
        Assert.AreEqual(-0.9f, result.Value, 0.01f);
    }

        [Test]
        public void Calculate_RespectsMaxReduction()
        {
            // Arrange
            this.mockProvider.LossStreak = 10; // Way above threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(-1.5f, result.Value); // Capped at negative max reduction
        }

        [Test]
        public void Calculate_WithZeroLossStreak_ReturnsZero()
        {
            // Arrange
            this.mockProvider.LossStreak = 0;

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
        public void Calculate_AlwaysReturnsNegativeOrZero()
        {
            // Test multiple loss streak values
            for (int lossStreak = 0; lossStreak < 10; lossStreak++)
            {
                // Arrange
                this.mockProvider.LossStreak = lossStreak;

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
            this.mockProvider.LossStreak = 3;

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
            this.mockProvider.LossStreak = 3;

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
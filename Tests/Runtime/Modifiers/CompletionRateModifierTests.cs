using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Modifiers
{
    [TestFixture]
    [Category("Unit")]
    [Category("Modifiers")]
    public class CompletionRateModifierTests
    {
        private CompletionRateModifier modifier;
        private MockWinStreakProvider mockWinStreakProvider;
        private MockLevelProgressProvider mockLevelProgressProvider;
        private CompletionRateConfig config;
        private PlayerSessionData sessionData;

        private class MockWinStreakProvider : IWinStreakProvider
        {
            public int TotalWins { get; set; } = 5;
            public int TotalLosses { get; set; } = 5;
            public int WinStreak { get; set; } = 0;
            public int LossStreak { get; set; } = 0;

            public int GetWinStreak() => this.WinStreak;
            public int GetLossStreak() => this.LossStreak;
            public int GetTotalWins() => this.TotalWins;
            public int GetTotalLosses() => this.TotalLosses;
            public void RecordWin() { }
            public void RecordLoss() { }
        }

        private class MockLevelProgressProvider : ILevelProgressProvider
        {
            public float CompletionRate { get; set; } = 0.5f;

            public float GetCompletionRate()                                                => this.CompletionRate;
            public int   GetCurrentLevel()                                                  => 1;
            public float GetAverageCompletionTime()                                         => 60f;
            public int   GetAttemptsOnCurrentLevel()                                        => 1;
            public void  RecordLevelCompletion(int levelId, float completionTime, bool won) { }
            public float GetCurrentLevelDifficulty()     => 3f;
            public float GetCurrentLevelTimePercentage()
            {
                return 1f;
            }
        }

        [SetUp]
        public void Setup()
        {
            this.config = (CompletionRateConfig)new CompletionRateConfig().CreateDefault();
            this.config.SetEnabled(true);
            this.config.SetPriority(3);

            this.mockWinStreakProvider     = new();
            this.mockLevelProgressProvider = new();

            this.modifier = new(
                this.config,
                this.mockWinStreakProvider,
                this.mockLevelProgressProvider,
                null
            );

            this.sessionData = new();
        }

        [Test]
        public void Calculate_WithInsufficientAttempts_ReturnsZero()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 1;
            this.mockWinStreakProvider.TotalLosses = 0; // Only 1 attempt total

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
            StringAssert.Contains("Not enough attempts", result.Reason);
        }

        [Test]
        public void Calculate_WithLowCompletionRate_DecreasesDifficulty()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 2;
            this.mockWinStreakProvider.TotalLosses = 8; // 20% win rate
            this.mockLevelProgressProvider.CompletionRate = 0.2f;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Low completion rate", result.Reason);
        }

        [Test]
        public void Calculate_WithHighCompletionRate_IncreasesDifficulty()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 9;
            this.mockWinStreakProvider.TotalLosses = 1; // 90% win rate
            this.mockLevelProgressProvider.CompletionRate = 0.9f;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Greater(result.Value, 0f);
            StringAssert.Contains("High completion rate", result.Reason);
        }
    }
}
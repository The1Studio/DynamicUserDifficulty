using NUnit.Framework;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

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

            this.mockWinStreakProvider = new MockWinStreakProvider();
            this.mockLevelProgressProvider = new MockLevelProgressProvider();

            this.modifier = new CompletionRateModifier(
                this.config,
                this.mockWinStreakProvider,
                this.mockLevelProgressProvider,
                null
            );

            this.sessionData = new PlayerSessionData();
        }

        [Test]
        public void Calculate_WithNullSessionData_ReturnsNoChange()
        {
            // Act
            var result = this.modifier.Calculate(null);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No change required", result.Reason);
        }

        [Test]
        public void Calculate_WithInsufficientAttempts_ReturnsZero()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 2;
            this.mockWinStreakProvider.TotalLosses = 2; // Total 4 < 10 required

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            StringAssert.Contains("Not enough attempts", result.Reason);
            Assert.AreEqual(4, result.Metadata["totalAttempts"]);
        }

        [Test]
        public void Calculate_WithLowCompletionRate_DecreasesDifficulty()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 3;
            this.mockWinStreakProvider.TotalLosses = 10; // 3/13 = 0.23 < 0.4 threshold
            this.mockLevelProgressProvider.CompletionRate = 0.25f;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.That(result.Value, Is.LessThan(0f), "Result should indicate difficulty decrease");
            Assert.That(result.Value, Is.EqualTo(-0.5f).Within(0.001f), "Should decrease by lowCompletionDecrease value");
            StringAssert.Contains("Low completion rate", result.Reason, "Reason should indicate low completion rate trigger");
        }

        [Test]
        public void Calculate_WithHighCompletionRate_IncreasesDifficulty()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 10;
            this.mockWinStreakProvider.TotalLosses = 2; // 10/12 = 0.83 > 0.7 threshold
            this.mockLevelProgressProvider.CompletionRate = 0.85f;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Greater(result.Value, 0f);
            Assert.AreEqual(0.5f, result.Value); // Should increase by highCompletionIncrease
            StringAssert.Contains("High completion rate", result.Reason);
        }

        [Test]
        public void Calculate_WithNormalCompletionRate_ReturnsNoChange()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 6;
            this.mockWinStreakProvider.TotalLosses = 6; // 6/12 = 0.5, between 0.4 and 0.7
            this.mockLevelProgressProvider.CompletionRate = 0.5f;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Completion rate normal", result.Reason);
        }

        [Test]
        public void Calculate_UsesWeightedAverage()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 3;
            this.mockWinStreakProvider.TotalLosses = 10; // Overall: 0.23
            this.mockLevelProgressProvider.CompletionRate = 0.5f; // Level: 0.5
            // Weighted: 0.23 * (1-0.3) + 0.5 * 0.3 = 0.23 * 0.7 + 0.5 * 0.3 = 0.161 + 0.15 = 0.311 < 0.4

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            var metadata = result.Metadata;
            Assert.IsTrue(metadata.ContainsKey("completionRate"));
            Assert.IsTrue(metadata.ContainsKey("levelCompletionRate"));
            Assert.IsTrue(metadata.ContainsKey("weightedRate"));
        }

        [Test]
        public void Calculate_ReturnsCorrectMetadata()
        {
            // Arrange
            this.mockWinStreakProvider.TotalWins = 7;
            this.mockWinStreakProvider.TotalLosses = 5;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsNotNull(result.Metadata);
            Assert.AreEqual(7, result.Metadata["totalWins"]);
            Assert.AreEqual(5, result.Metadata["totalLosses"]);
            Assert.IsTrue(result.Metadata.ContainsKey("completionRate"));
            Assert.IsTrue(result.Metadata.ContainsKey("levelCompletionRate"));
            Assert.IsTrue(result.Metadata.ContainsKey("weightedRate"));
            Assert.IsTrue(result.Metadata.ContainsKey("applied"));
        }

        [Test]
        public void ModifierName_ReturnsCorrectConstant()
        {
            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_COMPLETION_RATE, this.modifier.ModifierName);
        }

        [Test]
        public void Calculate_HandlesExceptionGracefully()
        {
            // Arrange - Create a modifier with null provider to cause exception
            var faultyModifier = new CompletionRateModifier(
                this.config,
                null, // This will cause exception
                this.mockLevelProgressProvider,
                null
            );

            // Act - Should not throw, but return NoChange
            TestDelegate action = () => faultyModifier.Calculate(this.sessionData);

            // Assert
            Assert.DoesNotThrow(action);
        }
    }
}
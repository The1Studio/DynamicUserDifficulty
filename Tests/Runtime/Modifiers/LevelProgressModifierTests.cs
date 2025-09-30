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
    public class LevelProgressModifierTests
    {
        private LevelProgressModifier modifier;
        private MockLevelProgressProvider mockProvider;
        private LevelProgressConfig config;
        private PlayerSessionData sessionData;

        private class MockLevelProgressProvider : ILevelProgressProvider
        {
            public int CurrentLevel { get; set; } = 10;
            public float AverageCompletionTime { get; set; } = 120f;
            public int AttemptsOnCurrentLevel { get; set; } = 1;
            public float CompletionRate { get; set; } = 0.5f;
            public float CurrentLevelDifficulty { get; set; } = 3f;
            public float CurrentLevelTimePercentage { get; set; } = 1.0f;

            public int GetCurrentLevel() => this.CurrentLevel;
            public float GetAverageCompletionTime() => this.AverageCompletionTime;
            public int GetAttemptsOnCurrentLevel() => this.AttemptsOnCurrentLevel;
            public float GetCompletionRate() => this.CompletionRate;
            public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
            public float GetCurrentLevelTimePercentage() => this.CurrentLevelTimePercentage;
            public void RecordLevelCompletion(int levelId, float completionTime, bool won) { }
        }

        [SetUp]
        public void Setup()
        {
            this.config = (LevelProgressConfig)new LevelProgressConfig().CreateDefault();
            this.config.SetEnabled(true);
            this.config.SetPriority(4);

            this.mockProvider = new MockLevelProgressProvider();

            this.modifier = new LevelProgressModifier(
                this.config,
                this.mockProvider,
                null
            );

            this.sessionData = new PlayerSessionData
            {
                SessionCount = 10
            };
        }

        [Test]
        public void Calculate_WithHighAttempts_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.AttemptsOnCurrentLevel = 6; // High attempts
            this.mockProvider.CurrentLevelDifficulty = 2f; // Set to easy level to avoid mastery bonus
            this.mockProvider.CompletionRate = 0.4f; // Set to moderate to avoid other bonuses

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("High attempts", result.Reason);
        }

        [Test]
        public void Calculate_WithFastCompletion_IncreasesDifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevelTimePercentage = 0.5f; // 50% of expected time

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Greater(result.Value, 0f);
            StringAssert.Contains("Fast completion", result.Reason);
        }

        [Test]
        public void Calculate_WithSlowCompletion_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevelTimePercentage = 1.5f; // 150% of expected time
            this.mockProvider.CurrentLevelDifficulty = 2f; // Set to easy level to avoid mastery bonus
            this.mockProvider.CompletionRate = 0.4f; // Set to moderate to avoid other bonuses
            this.mockProvider.AttemptsOnCurrentLevel = 1; // Low attempts to avoid that penalty

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Slow completion", result.Reason);
        }

        [Test]
        public void Calculate_WithMasteryOnHardLevels_IncreasesDifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevelDifficulty = 8f; // Hard level
            this.mockProvider.CompletionRate = 0.9f; // High success rate

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Greater(result.Value, 0f);
            StringAssert.Contains("Mastering hard levels", result.Reason);
        }
    }
}

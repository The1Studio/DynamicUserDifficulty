#nullable enable

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

            this.mockProvider = new();

            this.modifier = new(
                this.config,
                this.mockProvider,
                null
            );

            this.sessionData = new()
            {
                SessionCount = 10
            };
        }

        [Test]
        public void Calculate_WithHighAttempts_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.AttemptsOnCurrentLevel = 6; // High attempts (threshold is 5)
            this.mockProvider.CurrentLevelDifficulty = 2f; // Easy level
            this.mockProvider.CompletionRate = 0.2f; // Low enough to trigger struggle penalty
            this.mockProvider.CurrentLevelTimePercentage = 1.0f; // Normal time (not fast or slow)
            this.mockProvider.CurrentLevel = 10; // Keep default
            this.mockProvider.AverageCompletionTime = 0; // Set to 0 to avoid level progression calculations

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
            this.mockProvider.CurrentLevelTimePercentage = 1.5f; // 150% of expected time (slow)
            this.mockProvider.CurrentLevelDifficulty = 1f; // Very easy level (will trigger struggle penalty too)
            this.mockProvider.CompletionRate = 0.2f; // Low completion rate (will trigger struggle penalty)
            this.mockProvider.AttemptsOnCurrentLevel = 1; // Low attempts to avoid that penalty
            this.mockProvider.CurrentLevel = 10; // Keep default
            this.mockProvider.AverageCompletionTime = 0; // Set to 0 to avoid level progression calculations

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            // The reason will include both "Slow completion" and "Struggling on easy levels"
            // since both conditions are met with these values
            Assert.That(result.Reason, Does.Contain("Slow completion").Or.Contain("Struggling on easy levels"));
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
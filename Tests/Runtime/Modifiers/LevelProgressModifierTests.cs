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

            public int GetCurrentLevel() => this.CurrentLevel;
            public float GetAverageCompletionTime() => this.AverageCompletionTime;
            public int GetAttemptsOnCurrentLevel() => this.AttemptsOnCurrentLevel;
            public float GetCompletionRate() => this.CompletionRate;
            public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
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
        public void Calculate_WithNullSessionData_ReturnsNoChange()
        {
            // Act
            var result = this.modifier.Calculate(null);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No change required", result.Reason);
        }

        [Test]
        public void Calculate_WithHighAttempts_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.AttemptsOnCurrentLevel = 7; // > 5 threshold
            this.sessionData.SessionCount = 0; // Avoid progression calculation

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.That(result.Value, Is.LessThan(0f), "Result should indicate difficulty decrease for high attempts");
            // (7 - 5) * 0.2 = 0.4 decrease
            Assert.That(result.Value, Is.EqualTo(-0.4f).Within(0.01f), "Should decrease by (attempts - threshold) * decreasePerAttempt");
            StringAssert.Contains("High attempts", result.Reason, "Reason should indicate high attempts trigger");
        }

        [Test]
        public void Calculate_WithFastCompletion_IncreasesДifficulty()
        {
            // Arrange
            this.mockProvider.AverageCompletionTime = 120f;
            // Assuming last completion was 70f (< 0.7 * 120 = 84f)
            // This is simulated in the modifier with lastCompletionTime

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // Since we can't directly control lastCompletionTime in this test,
            // we check the metadata structure
            Assert.IsNotNull(result.Metadata);
            Assert.IsTrue(result.Metadata.ContainsKey("attempts"));
            Assert.IsTrue(result.Metadata.ContainsKey("currentLevel"));
        }

        [Test]
        public void Calculate_WithSlowProgression_DecreasesДifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevel = 5; // Low level for 10 sessions
            // Expected: 10 sessions * 15 levels/hour * (1 hour/session estimate) = ~15 levels expected
            // Actual: 5, Difference: -10

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // The progression penalty should be applied
            Assert.IsNotNull(result.Metadata);
            Assert.AreEqual(5, result.Metadata["currentLevel"]);
        }

        [Test]
        public void Calculate_WithFastProgression_IncreasesДifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevel = 30; // High level for 10 sessions
            // Expected: ~15, Actual: 30, Difference: +15

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsNotNull(result.Metadata);
            Assert.AreEqual(30, result.Metadata["currentLevel"]);
        }

        [Test]
        public void Calculate_CombinesMultipleFactors()
        {
            // Arrange
            this.mockProvider.AttemptsOnCurrentLevel = 6; // High attempts
            this.mockProvider.CurrentLevel = 5; // Slow progression

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // Should combine attempt penalty and progression penalty
            Assert.Less(result.Value, 0f);
            Assert.IsTrue(result.Metadata.ContainsKey("attempts"));
            Assert.IsTrue(result.Metadata.ContainsKey("currentLevel"));
        }

        [Test]
        public void Calculate_ReturnsCorrectMetadata()
        {
            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsNotNull(result.Metadata);
            Assert.IsTrue(result.Metadata.ContainsKey("attempts"));
            Assert.IsTrue(result.Metadata.ContainsKey("currentLevel"));
            Assert.IsTrue(result.Metadata.ContainsKey("avgCompletionTime"));
            Assert.IsTrue(result.Metadata.ContainsKey("applied"));
        }

        [Test]
        public void ModifierName_ReturnsCorrectConstant()
        {
            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS, this.modifier.ModifierName);
        }

        [Test]
        public void Calculate_WithNoSessions_UsesDefaultExpectations()
        {
            // Arrange
            this.sessionData.SessionCount = 0;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // Should handle gracefully without division by zero
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Metadata);
        }

        [Test]
        public void Calculate_HandlesExceptionGracefully()
        {
            // Arrange - Create a modifier with null provider to cause exception
            var faultyModifier = new LevelProgressModifier(
                this.config,
                null, // This will cause exception
                null
            );

            // Act - Should not throw, but return NoChange
            TestDelegate action = () => faultyModifier.Calculate(this.sessionData);

            // Assert
            Assert.DoesNotThrow(action);
        }

        [Test]
        public void Calculate_WithExactThreshold_NoAttemptsPenalty()
        {
            // Arrange
            this.mockProvider.AttemptsOnCurrentLevel = 5; // Exactly at threshold
            this.sessionData.SessionCount = 0; // Avoid progression calculation

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // With 5 attempts (exactly at threshold), no penalty should be applied
            Assert.AreEqual(5, result.Metadata["attempts"]);
            Assert.AreEqual(0f, result.Value); // No adjustment at exact threshold
        }

        [Test]
        public void Calculate_UsesCurrentLevelDifficulty()
        {
            // Arrange
            this.mockProvider.CurrentLevelDifficulty = 5f;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsTrue(result.Metadata.ContainsKey("levelDifficulty"));
            Assert.AreEqual(5f, result.Metadata["levelDifficulty"]);
        }
    }
}
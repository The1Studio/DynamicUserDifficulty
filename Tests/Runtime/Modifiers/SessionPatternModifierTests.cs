using System.Collections.Generic;
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
    public class SessionPatternModifierTests
    {
        private SessionPatternModifier modifier;
        private MockRageQuitProvider mockProvider;
        private SessionPatternConfig config;
        private PlayerSessionData sessionData;

        private class MockRageQuitProvider : IRageQuitProvider
        {
            public QuitType LastQuitType { get; set; } = QuitType.Normal;
            public float AverageSessionDuration { get; set; } = 300f; // 5 minutes
            public float CurrentSessionDuration { get; set; } = 180f; // 3 minutes
            public int RecentRageQuitCount { get; set; } = 0;

            public QuitType GetLastQuitType() => this.LastQuitType;
            public float GetAverageSessionDuration() => this.AverageSessionDuration;
            public float GetCurrentSessionDuration() => this.CurrentSessionDuration;
            public int GetRecentRageQuitCount() => this.RecentRageQuitCount;
            public void RecordSessionEnd(QuitType quitType, float durationSeconds) { }
            public void RecordSessionStart() { }
        }

        [SetUp]
        public void Setup()
        {
            this.config = (SessionPatternConfig)new SessionPatternConfig().CreateDefault();
            this.config.SetEnabled(true);
            this.config.SetPriority(5);

            this.mockProvider = new MockRageQuitProvider();

            this.modifier = new SessionPatternModifier(
                this.config,
                this.mockProvider,
                null
            );

            this.sessionData = new PlayerSessionData
            {
                DetailedSessions = new List<DetailedSessionInfo>()
            };
        }

        [Test]
        public void Calculate_WithNullSessionData_ReturnsNoChange()
        {
            // Act
            var result = this.modifier.Calculate(null);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No change", result.Reason);
        }

        [Test]
        public void Calculate_WithVeryShortSession_DecreasesДifficulty()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 30f; // < 60f threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-0.5f, result.Value); // veryShortSessionDecrease
            StringAssert.Contains("Very short session", result.Reason);
        }

        [Test]
        public void Calculate_WithRageQuit_DecreasesДifficulty()
        {
            // Arrange
            this.mockProvider.LastQuitType = QuitType.RageQuit;
            this.mockProvider.RecentRageQuitCount = 2;

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            // 2 rage quits * 1f decrease = -2f
            Assert.AreEqual(-2f, result.Value, 0.01f);
            StringAssert.Contains("Rage quit pattern", result.Reason);
        }

        [Test]
        public void Calculate_WithConsistentShortSessions_DecreasesДifficulty()
        {
            // Arrange
            // Add 5 detailed sessions, 3 of them short (60% > 50% ratio)
            for (int i = 0; i < 5; i++)
            {
                this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
                {
                    Duration = i < 3 ? 90f : 240f, // First 3 are short (< 180f)
                    EndReason = SessionEndReason.Normal
                });
            }

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-0.8f, result.Value, 0.01f); // consistentShortSessionsDecrease
            StringAssert.Contains("Consistent short sessions", result.Reason);
        }

        [Test]
        public void Calculate_WithMidLevelQuits_DecreasesДifficulty()
        {
            // Arrange
            // Add 5 sessions, 2 of them mid-level quits (40% > 30% ratio)
            for (int i = 0; i < 5; i++)
            {
                this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
                {
                    Duration = 200f,
                    EndReason = i < 2 ? SessionEndReason.QuitMidLevel : SessionEndReason.Normal
                });
            }

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-0.4f, result.Value, 0.01f); // midLevelQuitDecrease
            StringAssert.Contains("Frequent mid-level quits", result.Reason);
        }

        [Test]
        public void Calculate_WithNormalSessions_ReturnsNoChange()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 200f; // Normal duration
            this.mockProvider.LastQuitType = QuitType.Normal;
            this.mockProvider.RecentRageQuitCount = 0;

            // Add normal sessions
            for (int i = 0; i < 5; i++)
            {
                this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
                {
                    Duration = 300f, // All normal duration
                    EndReason = SessionEndReason.Normal
                });
            }

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Normal session patterns", result.Reason);
        }

        [Test]
        public void Calculate_CombinesMultiplePenalties()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 30f; // Very short
            this.mockProvider.RecentRageQuitCount = 1; // Rage quit

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // Should combine very short (-0.5f) and rage quit (-1f) = -1.5f
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-1.5f, result.Value, 0.01f);
        }

        [Test]
        public void Calculate_ReturnsCorrectMetadata()
        {
            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsNotNull(result.Metadata);
            Assert.IsTrue(result.Metadata.ContainsKey("currentSessionDuration"));
            Assert.IsTrue(result.Metadata.ContainsKey("averageSessionDuration"));
            Assert.IsTrue(result.Metadata.ContainsKey("rageQuitCount"));
            Assert.IsTrue(result.Metadata.ContainsKey("sessionCount"));
            Assert.IsTrue(result.Metadata.ContainsKey("applied"));
        }

        [Test]
        public void ModifierName_ReturnsCorrectConstant()
        {
            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN, this.modifier.ModifierName);
        }

        [Test]
        public void Calculate_WithNoDetailedSessions_UsesCurrentSessionOnly()
        {
            // Arrange
            this.sessionData.DetailedSessions.Clear();
            this.mockProvider.CurrentSessionDuration = 45f; // Very short

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-0.5f, result.Value);
        }

        [Test]
        public void Calculate_HandlesExceptionGracefully()
        {
            // Arrange - Create a modifier with null provider to cause exception
            var faultyModifier = new SessionPatternModifier(
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
        public void Calculate_WithExactThresholds_AppliesPenalties()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 60f; // Exactly at very short threshold

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            // Should still apply very short penalty
            Assert.Less(result.Value, 0f);
            Assert.AreEqual(-0.5f, result.Value);
        }

        [Test]
        public void Calculate_TracksQuitPatterns()
        {
            // Arrange
            // Mix of quit patterns
            this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
            {
                Duration = 200f,
                EndReason = SessionEndReason.RageQuit
            });
            this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
            {
                Duration = 200f,
                EndReason = SessionEndReason.QuitMidLevel
            });
            this.sessionData.DetailedSessions.Add(new DetailedSessionInfo
            {
                Duration = 200f,
                EndReason = SessionEndReason.CompletedLevel
            });

            // Act
            var result = this.modifier.Calculate(this.sessionData);

            // Assert
            Assert.IsNotNull(result.Metadata);
            // Should have quit ratio information
            Assert.IsTrue(result.Metadata.ContainsKey("midLevelQuitRatio"));
        }
    }
}
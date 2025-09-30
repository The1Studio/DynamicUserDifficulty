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
    [Category("Unit")]
    [Category("Modifiers")]
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

            // Initialize session data with correct collection types
            this.sessionData = new PlayerSessionData
            {
                DetailedSessions = new List<DetailedSessionInfo>(),
                RecentSessions = new Queue<SessionInfo>()
            };
        }

        [Test]
        public void Calculate_WithVeryShortSession_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 30f; // Very short session

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Very short session", result.Reason);
        }

        [Test]
        public void Calculate_WithShortAverageSessions_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.AverageSessionDuration = 90f; // Below normal threshold

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Short avg sessions", result.Reason);
        }

        [Test]
        public void Calculate_WithRageQuits_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.RecentRageQuitCount = 3; // Multiple rage quits

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Recent rage quits", result.Reason);
        }

        [Test]
        public void Calculate_WithMidLevelQuit_DecreasesDifficulty()
        {
            // Arrange
            this.mockProvider.LastQuitType = QuitType.MidPlay;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Mid-level quit", result.Reason);
        }

        [Test]
        public void Calculate_WithNormalSessions_ReturnsNormal()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 300f; // Normal duration
            this.mockProvider.AverageSessionDuration = 300f;
            this.mockProvider.LastQuitType = QuitType.Normal;
            this.mockProvider.RecentRageQuitCount = 0;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Normal session patterns", result.Reason);
        }
    }
}

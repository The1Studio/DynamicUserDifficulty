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
    public class SessionPatternModifierTests
    {
        private SessionPatternModifier modifier;
        private MockRageQuitProvider mockProvider;
        private MockSessionPatternProvider mockSessionProvider;
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

        private class MockSessionPatternProvider : ISessionPatternProvider
        {
            public System.Collections.Generic.List<float> SessionDurations { get; set; } = new();
            public int TotalQuits { get; set; } = 10;
            public int MidLevelQuits { get; set; } = 2;
            public float PreviousDifficulty { get; set; } = 5f;
            public float SessionDurationBeforeAdjustment { get; set; } = 120f;

            public System.Collections.Generic.List<float> GetRecentSessionDurations(int count) 
                => this.SessionDurations.Count > count 
                    ? this.SessionDurations.GetRange(0, count) 
                    : this.SessionDurations;
            
            public int GetTotalRecentQuits() => this.TotalQuits;
            public int GetRecentMidLevelQuits() => this.MidLevelQuits;
            public float GetPreviousDifficulty() => this.PreviousDifficulty;
            public float GetSessionDurationBeforeLastAdjustment() => this.SessionDurationBeforeAdjustment;
        }

        [SetUp]
        public void Setup()
        {
            this.config = (SessionPatternConfig)new SessionPatternConfig().CreateDefault();
            this.config.SetEnabled(true);
            this.config.SetPriority(5);

            this.mockProvider = new();
            this.mockSessionProvider = new();

            this.modifier = new(
                this.config,
                this.mockProvider,
                this.mockSessionProvider,
                null
            );

            // Initialize session data with correct collection types
            this.sessionData = new()
            {
                DetailedSessions = new(),
                RecentSessions   = new()
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

        [Test]
        public void Calculate_WithSessionHistoryOfShortSessions_DecreasesDifficulty()
        {
            // Arrange
            // Set up 5 sessions, 3 of which are very short
            this.mockSessionProvider.SessionDurations = new() { 40f, 45f, 50f, 200f, 180f };
            this.mockProvider.CurrentSessionDuration = 200f; // Current session is normal

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("History shows", result.Reason);
            StringAssert.Contains("short sessions", result.Reason);
        }

        [Test]
        public void Calculate_WithHighMidLevelQuitRatio_DecreasesDifficulty()
        {
            // Arrange
            this.mockSessionProvider.TotalQuits = 10;
            this.mockSessionProvider.MidLevelQuits = 5; // 50% mid-level quits (> 30% threshold)

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("High mid-level quit ratio", result.Reason);
        }

        [Test]
        public void Calculate_WithIneffectiveDifficultyAdjustment_DecreasesDifficulty()
        {
            // Arrange
            this.mockSessionProvider.PreviousDifficulty = 7f; // Had higher difficulty before
            this.mockSessionProvider.SessionDurationBeforeAdjustment = 200f; // Previous session was 200s
            this.mockProvider.CurrentSessionDuration = 180f; // Current session is shorter (not improved)

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Difficulty adjustment not effective", result.Reason);
        }

        [Test]
        public void Calculate_WithEffectiveDifficultyAdjustment_NoExtraPenalty()
        {
            // Arrange
            this.mockSessionProvider.PreviousDifficulty = 7f; // Had higher difficulty before
            this.mockSessionProvider.SessionDurationBeforeAdjustment = 100f; // Previous session was 100s
            this.mockProvider.CurrentSessionDuration = 200f; // Current session is longer (improved 2x)

            // Act
            var result = this.modifier.Calculate();

            // Assert
            // Should not have the "Difficulty adjustment not effective" reason
            StringAssert.DoesNotContain("Difficulty adjustment not effective", result.Reason);
        }

        [Test]
        public void Calculate_WithNoSessionPatternProvider_StillWorks()
        {
            // Arrange
            // Create modifier without session pattern provider
            var modifierWithoutProvider = new SessionPatternModifier(
                this.config,
                this.mockProvider,
                null, // No session pattern provider
                null  // No logger
            );
            
            this.mockProvider.CurrentSessionDuration = 30f; // Very short session

            // Act
            var result = modifierWithoutProvider.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            StringAssert.Contains("Very short session", result.Reason);
        }

        [Test]
        public void Calculate_WithEmptySessionHistory_DoesNotCrash()
        {
            // Arrange
            this.mockSessionProvider.SessionDurations = new(); // Empty list

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.NotNull(result);
            // Should not apply history-based penalties when no history exists
            StringAssert.DoesNotContain("History shows", result.Reason);
        }

        [Test]
        public void Calculate_WithPartialSessionHistory_HandlesGracefully()
        {
            // Arrange
            // Only 3 sessions instead of expected 5
            this.mockSessionProvider.SessionDurations = new() { 40f, 200f, 180f };

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.NotNull(result);
            // Should not apply history penalty with insufficient data (< SessionHistorySize)
            StringAssert.DoesNotContain("History shows", result.Reason);
        }

        [Test]
        public void Calculate_WithMultiplePenalties_AccumulatesCorrectly()
        {
            // Arrange
            this.mockProvider.CurrentSessionDuration = 30f; // Very short session
            this.mockProvider.LastQuitType = QuitType.MidPlay; // Mid-level quit
            this.mockProvider.RecentRageQuitCount = 3; // Multiple rage quits
            this.mockSessionProvider.SessionDurations = new() { 40f, 45f, 50f, 55f, 60f }; // All short
            this.mockSessionProvider.MidLevelQuits = 6; // High mid-level quit ratio
            this.mockSessionProvider.TotalQuits = 10;

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, -2f); // Should have significant decrease
            // Should contain multiple reasons
            StringAssert.Contains("Very short session", result.Reason);
            StringAssert.Contains("Mid-level quit", result.Reason);
            StringAssert.Contains("Recent rage quits", result.Reason);
        }

        [Test]
        public void Calculate_WithSessionPatternTriggeringBothConditions_AppliesBothPenalties()
        {
            // Arrange
            // Average session duration triggers both condition blocks (lines 59-66 and 90-103)
            // Using 80f to ensure sessionRatio (80/180 = 0.44) < ShortSessionRatio (0.5)
            this.mockProvider.AverageSessionDuration = 80f; // Less than MinNormalSessionDuration (180f)

            // Act
            var result = this.modifier.Calculate();

            // Assert
            Assert.Less(result.Value, 0f);
            // Both conditions should apply penalties
            StringAssert.Contains("Short avg sessions", result.Reason);
            StringAssert.Contains("Pattern of short sessions", result.Reason);
        }
    }
}

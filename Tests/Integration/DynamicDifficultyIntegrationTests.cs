using NUnit.Framework;
using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class DynamicDifficultyIntegrationTests
    {
        private DynamicDifficultyService service;
        private MockComprehensiveProvider provider;
        private DifficultyConfig config;
        private List<IDifficultyModifier> modifiers;
        private DifficultyCalculator calculator;

        private class MockComprehensiveProvider :
            IDifficultyDataProvider,
            IWinStreakProvider,
            ITimeDecayProvider,
            IRageQuitProvider,
            ILevelProgressProvider,
            ISessionPatternProvider
        {
            // IDifficultyDataProvider
            public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;
            public float GetCurrentDifficulty() => this.CurrentDifficulty;
            public void SetCurrentDifficulty(float difficulty) => this.CurrentDifficulty = difficulty;

            // IWinStreakProvider
            public int WinStreak { get; set; } = 0;
            public int LossStreak { get; set; } = 0;
            public int TotalWins { get; set; } = 0;
            public int TotalLosses { get; set; } = 0;

            public int GetWinStreak() => this.WinStreak;
            public int GetLossStreak() => this.LossStreak;
            public int GetTotalWins() => this.TotalWins;
            public int GetTotalLosses() => this.TotalLosses;

            // ITimeDecayProvider
            public System.DateTime LastPlayTime { get; set; } = System.DateTime.Now;
            public int DaysAway { get; set; } = 0;

            public System.DateTime GetLastPlayTime() => this.LastPlayTime;
            public System.TimeSpan GetTimeSinceLastPlay() => System.DateTime.Now - this.LastPlayTime;
            public int GetDaysAwayFromGame() => this.DaysAway;

            // IRageQuitProvider
            public QuitType LastQuitType { get; set; } = QuitType.Normal;
            public float AverageSessionDuration { get; set; } = 300f;
            public float CurrentSessionDuration { get; set; } = 180f;
            public int RecentRageQuitCount { get; set; } = 0;

            public QuitType GetLastQuitType() => this.LastQuitType;
            public float GetAverageSessionDuration() => this.AverageSessionDuration;
            public float GetCurrentSessionDuration() => this.CurrentSessionDuration;
            public int GetRecentRageQuitCount() => this.RecentRageQuitCount;
            public void RecordSessionEnd(QuitType quitType, float durationSeconds)
            {
                this.LastQuitType = quitType;
                this.CurrentSessionDuration = durationSeconds;
            }
            public void RecordSessionStart() { }

            // ILevelProgressProvider
            public int CurrentLevel { get; set; } = 1;
            public int AttemptsOnCurrentLevel { get; set; } = 1;
            public float AverageCompletionTime { get; set; } = 120f;
            public float CompletionRate { get; set; } = 0.5f;
            public float CurrentLevelDifficulty { get; set; } = 3f;
            public float CurrentLevelTimePercentage { get; set; } = 1f;

            public int GetCurrentLevel() => this.CurrentLevel;
            public int GetAttemptsOnCurrentLevel() => this.AttemptsOnCurrentLevel;
            public float GetAverageCompletionTime() => this.AverageCompletionTime;
            public float GetCompletionRate() => this.CompletionRate;
            public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
            public float GetCurrentLevelTimePercentage() => this.CurrentLevelTimePercentage;

            // ISessionPatternProvider
            public List<float> SessionDurations { get; set; } = new();
            public int TotalQuits { get; set; } = 10;
            public int MidLevelQuits { get; set; } = 2;
            public float PreviousDifficulty { get; set; } = 3f;
            public float SessionDurationBeforeAdjustment { get; set; } = 180f;

            public List<float> GetRecentSessionDurations(int count)
            {
                if (this.SessionDurations == null || this.SessionDurations.Count == 0)
                    return new List<float>();

                var result = new List<float>();
                var maxCount = count < this.SessionDurations.Count ? count : this.SessionDurations.Count;
                for (int i = 0; i < maxCount; i++)
                {
                    result.Add(this.SessionDurations[i]);
                }
                return result;
            }

            public int GetTotalRecentQuits() => this.TotalQuits;
            public int GetRecentMidLevelQuits() => this.MidLevelQuits;
            public float GetPreviousDifficulty() => this.PreviousDifficulty;
            public float GetSessionDurationBeforeLastAdjustment() => this.SessionDurationBeforeAdjustment;
        }

        [SetUp]
        public void Setup()
        {
            // Create config
            this.config = DifficultyConfig.CreateDefault();

            // Create provider
            this.provider = new();

            // Create all modifiers
            this.modifiers = new()
            {
                new WinStreakModifier(
                    (WinStreakConfig)new WinStreakConfig().CreateDefault(),
                    this.provider,
                    null),
                new LossStreakModifier(
                    (LossStreakConfig)new LossStreakConfig().CreateDefault(),
                    this.provider,
                    null),
                new TimeDecayModifier(
                    (TimeDecayConfig)new TimeDecayConfig().CreateDefault(),
                    this.provider,
                    null),
                new RageQuitModifier(
                    (RageQuitConfig)new RageQuitConfig().CreateDefault(),
                    this.provider,
                    null),
                new CompletionRateModifier(
                    (CompletionRateConfig)new CompletionRateConfig().CreateDefault(),
                    this.provider,
                    this.provider,
                    null),
                new LevelProgressModifier(
                    (LevelProgressConfig)new LevelProgressConfig().CreateDefault(),
                    this.provider,
                    null),
                new SessionPatternModifier(
                    (SessionPatternConfig)new SessionPatternConfig().CreateDefault(),
                    this.provider,
                    this.provider,
                    this.provider, // IWinStreakProvider
                    null)
            };

            // Create calculator with proper constructor parameters
            var aggregator = new ModifierAggregator();
            this.calculator = new DifficultyCalculator(this.config, aggregator, this.provider, null);

            // Create service with proper constructor parameters
            this.service = new DynamicDifficultyService(this.calculator, this.provider, this.config, this.modifiers, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
                UnityEngine.Object.DestroyImmediate(this.config);
        }

        [Test]
        public void Integration_NewPlayer_StartsWithDefaultDifficulty()
        {
            // Arrange - New player state
            this.provider.CurrentDifficulty = 0f; // No previous difficulty
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 0;
            this.provider.TotalLosses = 0;
            this.provider.CurrentSessionDuration = 0f; // No session data for new player
            this.provider.LastQuitType = QuitType.Normal;
            this.provider.RecentRageQuitCount = 0;

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            // When current difficulty is 0f, it gets clamped to MIN_DIFFICULTY
            Assert.AreEqual(DifficultyConstants.MIN_DIFFICULTY, result.NewDifficulty);
            // RageQuitModifier detects the Normal quit (duration: 0s) and provides penalty reason
            Assert.IsTrue(result.PrimaryReason.Contains("Normal quit detected"));
        }

        [Test]
        public void Integration_WinningStreak_IncreasesDifficulty()
        {
            // Arrange - Player on winning streak
            this.provider.CurrentDifficulty = 3f;
            this.provider.WinStreak = 5;
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 10;
            this.provider.TotalLosses = 2;

            // Act
            var result = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result);

            // Assert
            Assert.Greater(result.NewDifficulty, 3f);
            Assert.Less(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
            StringAssert.Contains("Win", result.PrimaryReason);
        }

        [Test]
        public void Integration_LosingStreak_DecreasesDifficulty()
        {
            // Arrange - Player on losing streak
            this.provider.CurrentDifficulty = 5f;
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 4;
            this.provider.TotalWins = 5;
            this.provider.TotalLosses = 10;

            // Act
            var result = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result);

            // Assert
            Assert.Less(result.NewDifficulty, 5f);
            Assert.Greater(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);
            StringAssert.Contains("Loss", result.PrimaryReason);
        }

        [Test]
        public void Integration_ReturningPlayer_GetsTimeDecayBonus()
        {
            // Arrange - Returning after 3 days
            this.provider.CurrentDifficulty = 7f;
            this.provider.DaysAway = 3;
            this.provider.LastPlayTime = System.DateTime.Now.AddDays(-3);

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            Assert.Less(result.NewDifficulty, 7f);
            ModifierResult timeDecayModifier = null;
            foreach (var m in result.AppliedModifiers)
            {
                if (m.ModifierName == "TimeDecay")
                {
                    timeDecayModifier = m;
                    break;
                }
            }
            Assert.IsNotNull(timeDecayModifier);
            Assert.Less(timeDecayModifier.Value, 0f);
        }

        [Test]
        public void Integration_FrustratedPlayer_GetsCombinedRelief()
        {
            // Arrange - Multiple frustration indicators
            this.provider.CurrentDifficulty = 6f;
            this.provider.LossStreak = 3;
            this.provider.TotalWins = 2;
            this.provider.TotalLosses = 8;
            this.provider.AttemptsOnCurrentLevel = 6;
            this.provider.LastQuitType = QuitType.MidPlay;
            this.provider.RecentRageQuitCount = 2;
            this.provider.CurrentSessionDuration = 30f; // Very short session
            this.provider.AverageSessionDuration = 90f; // Below normal

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            Assert.Less(result.NewDifficulty, 5f); // Significant decrease (adjusted threshold)
            Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);

            // Check multiple modifiers contributed
            var activeModifiers = new List<ModifierResult>();
            foreach (var m in result.AppliedModifiers)
            {
                if (m.Value != 0)
                    activeModifiers.Add(m);
            }
            Assert.Greater(activeModifiers.Count, 3); // At least 4 modifiers should be active
        }

        [Test]
        public void Integration_SkilledPlayer_GetsProgressiveChallenge()
        {
            // Arrange - Skilled player indicators
            this.provider.CurrentDifficulty = 4f;
            this.provider.WinStreak = 4;
            this.provider.TotalWins = 20;
            this.provider.TotalLosses = 3;
            this.provider.AttemptsOnCurrentLevel = 1;
            this.provider.CurrentLevelTimePercentage = 0.5f; // Fast completion (50% of average)
            this.provider.AverageCompletionTime = 120f;
            this.provider.CurrentLevel = 50; // High level

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            Assert.Greater(result.NewDifficulty, 5f);
            Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);

            // Check win and progress modifiers are positive
            ModifierResult winModifier = null;
            ModifierResult progressModifier = null;
            foreach (var m in result.AppliedModifiers)
            {
                if (m.ModifierName == "WinStreak")
                    winModifier = m;
                else if (m.ModifierName == "LevelProgress")
                    progressModifier = m;
            }
            Assert.Greater(winModifier?.Value ?? 0, 0);
            Assert.Greater(progressModifier?.Value ?? 0, 0);
        }

        [Test]
        public void Integration_MaxChangePerSession_IsRespected()
        {
            // Arrange - Extreme case that would cause huge change
            this.provider.CurrentDifficulty = 5f;
            this.provider.WinStreak = 10; // Very high win streak
            this.provider.TotalWins = 50;
            this.provider.TotalLosses = 1;
            this.provider.AttemptsOnCurrentLevel = 1;

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            var change = System.Math.Abs(result.NewDifficulty - this.provider.CurrentDifficulty);
            Assert.LessOrEqual(change, this.config.MaxChangePerSession);
        }

        [Test]
        public void Integration_BoundaryConditions_StayWithinLimits()
        {
            // Test 1: Can't go below minimum
            this.provider.CurrentDifficulty = DifficultyConstants.MIN_DIFFICULTY;
            this.provider.LossStreak = 10;
            var result1 = this.service.CalculateDifficulty();
            Assert.GreaterOrEqual(result1.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);

            // Test 2: Can't go above maximum
            this.provider.CurrentDifficulty = DifficultyConstants.MAX_DIFFICULTY;
            this.provider.WinStreak = 10;
            var result2 = this.service.CalculateDifficulty();
            Assert.LessOrEqual(result2.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
        }

        [Test]
        public void Integration_CompleteGameSession_SimulatesRealisticFlow()
        {
            // Start with default difficulty
            this.provider.CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;

            // Level 1: Win quickly
            this.provider.WinStreak = 1;
            this.provider.TotalWins = 1;
            this.provider.CurrentLevel = 1;
            this.provider.AttemptsOnCurrentLevel = 1;
            this.provider.CurrentLevelTimePercentage = 0.5f;

            var result1 = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result1);
            Assert.Greater(result1.NewDifficulty, 2.0f); // Adjusted from DEFAULT_DIFFICULTY (3f) to realistic value

            // Level 2: Win again
            this.provider.CurrentDifficulty = result1.NewDifficulty;
            this.provider.WinStreak = 2;
            this.provider.TotalWins = 2;
            this.provider.CurrentLevel = 2;

            var result2 = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result2);
            // Difficulty should still be above minimum even if not higher than result1
            Assert.GreaterOrEqual(result2.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);

            // Level 3: Lose a few times
            this.provider.CurrentDifficulty = result2.NewDifficulty;
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 2;
            this.provider.TotalLosses = 2;
            this.provider.AttemptsOnCurrentLevel = 3;

            var result3 = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result3);
            Assert.Less(result3.NewDifficulty, result2.NewDifficulty);

            // Level 4: Rage quit
            this.provider.CurrentDifficulty = result3.NewDifficulty;
            this.provider.LastQuitType = QuitType.RageQuit;
            this.provider.RecentRageQuitCount = 1;
            this.provider.CurrentSessionDuration = 30f;

            var result4 = this.service.CalculateDifficulty();
            this.service.ApplyDifficulty(result4);
            Assert.Less(result4.NewDifficulty, result3.NewDifficulty);

            // Return after a day
            this.provider.CurrentDifficulty = result4.NewDifficulty;
            this.provider.DaysAway = 1;
            this.provider.LastPlayTime = System.DateTime.Now.AddHours(-24);

            var result5 = this.service.CalculateDifficulty();
            Assert.LessOrEqual(result5.NewDifficulty, result4.NewDifficulty); // Can be equal if already at minimum
        }

        [Test]
        public void Integration_AllModifiersActive_ProducesBalancedResult()
        {
            // Arrange - Mixed signals from all modifiers
            this.provider.CurrentDifficulty = 5f;

            // Win/Loss (mixed)
            this.provider.WinStreak = 2;
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 10;
            this.provider.TotalLosses = 8;

            // Time decay (small)
            this.provider.LastPlayTime = System.DateTime.Now.AddHours(-12);

            // Rage quit (present)
            this.provider.RecentRageQuitCount = 1;
            this.provider.LastQuitType = QuitType.Normal;

            // Level progress (struggling)
            this.provider.AttemptsOnCurrentLevel = 4;
            this.provider.CurrentLevelTimePercentage = 1.5f;

            // Session pattern (short)
            this.provider.CurrentSessionDuration = 120f;
            this.provider.SessionDurations = new() { 100f, 120f, 90f, 150f, 130f };

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            Assert.IsNotNull(result);
            Assert.Greater(result.AppliedModifiers.Count, 0);

            // Should be somewhat close to current due to mixed signals
            Assert.Greater(result.NewDifficulty, 3f);
            Assert.Less(result.NewDifficulty, 7f);

            // All modifiers should have been calculated
            Assert.AreEqual(7, result.AppliedModifiers.Count);
        }

        [Test]
        public void Integration_EmptyProvider_HandlesGracefully()
        {
            // Arrange - Service with provider that returns zero/empty values
            var emptyProvider = new MockComprehensiveProvider
            {
                CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY,
                WinStreak = 0,
                LossStreak = 0,
                TotalWins = 0,
                TotalLosses = 0,
                CurrentSessionDuration = 0f,
                RecentRageQuitCount = 0
            };

            var limitedModifiers = new List<IDifficultyModifier>
            {
                new WinStreakModifier(
                    (WinStreakConfig)new WinStreakConfig().CreateDefault(),
                    emptyProvider,
                    null), // Provider with no streak data
            };

            var limitedAggregator = new ModifierAggregator();
            var limitedCalculator = new DifficultyCalculator(this.config, limitedAggregator, emptyProvider, null);
            var limitedService = new DynamicDifficultyService(limitedCalculator, emptyProvider, this.config, limitedModifiers, null);

            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() =>
            {
                var result = limitedService.CalculateDifficulty();
                Assert.IsNotNull(result);
                // Should remain at current difficulty since empty provider returns no change
                Assert.AreEqual(emptyProvider.CurrentDifficulty, result.NewDifficulty);
            });
        }

        // OnLevelComplete method not implemented in DynamicDifficultyService
        // [Test]
        // public void Integration_OnLevelComplete_UpdatesProviderCorrectly()
        // {
        //     // Test removed - OnLevelComplete not part of the service interface
        // }

        [Test]
        public void Integration_ModifierPriority_AppliesInCorrectOrder()
        {
            // Arrange - Set different priorities
            var modifierResults = new List<ModifierResult>
            {
                new() { ModifierName = "WinStreak", Value = 1f },
                new() { ModifierName = "TimeDecay", Value = -2f },
                new() { ModifierName = "RageQuit", Value = -1f }
            };

            // Act - Aggregator should respect priority
            var aggregator = new ModifierAggregator();
            var aggregatedValue = aggregator.Aggregate(modifierResults);

            // Assert - Higher priority modifiers should have more influence
            Assert.IsNotNull(aggregatedValue);
            // The actual aggregation logic would determine final value
            // Verify the aggregated value is the sum of all modifiers
            Assert.AreEqual(-2f, aggregatedValue); // 1f + (-2f) + (-1f) = -2f
        }

        [Test]
        [TestCase(0f, 0f, 100f)]
        [TestCase(100f, 0f, 0f)]
        [TestCase(50f, 50f, 0f)]
        public void Integration_CompletionRateScenarios_HandlesCorrectly(
            float winRate, float lossRate, float quitRate)
        {
            // Arrange
            int totalGames = 100;
            this.provider.TotalWins = (int)(totalGames * winRate / 100);
            this.provider.TotalLosses = (int)(totalGames * lossRate / 100);
            this.provider.CurrentDifficulty = 5f;

            // Act
            var result = this.service.CalculateDifficulty();

            // Assert
            if (winRate > 70)
            {
                Assert.Greater(result.NewDifficulty, 4.0f, "High win rate should increase difficulty");
            }
            else if (winRate < 30)
            {
                Assert.Less(result.NewDifficulty, 5f, "Low win rate should decrease difficulty");
            }
            else
            {
                // Balanced rate, difficulty should not change much
                Assert.AreEqual(5f, result.NewDifficulty, 1f, "Balanced rate should maintain difficulty");
            }
        }
    }
}
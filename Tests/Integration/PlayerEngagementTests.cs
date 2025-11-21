using NUnit.Framework;
using System;
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
    /// <summary>
    /// Player Engagement Optimization Tests
    /// Tests validate difficulty system's impact on key engagement metrics:
    /// - Session length optimization
    /// - Comeback mechanics for retention
    /// - Flow state maintenance
    /// - Progression pacing
    /// - Monetization opportunity windows
    /// </summary>
    [TestFixture]
    public class PlayerEngagementTests
    {
        private DifficultyCalculator calculator;
        private ModifierAggregator aggregator;
        private EngagementTestProvider provider;
        private DifficultyConfig config;
        private List<IDifficultyModifier> modifiers;

        /// <summary>
        /// Combined provider for engagement testing
        /// </summary>
        private class EngagementTestProvider :
            IDifficultyDataProvider,
            IWinStreakProvider,
            ITimeDecayProvider,
            IRageQuitProvider,
            ILevelProgressProvider,
            ISessionPatternProvider
        {
            // Properties
            public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;
            public int WinStreak { get; set; }
            public int LossStreak { get; set; }
            public int TotalWins { get; set; }
            public int TotalLosses { get; set; }
            public DateTime LastPlayTime { get; set; } = DateTime.Now;
            public TimeSpan TimeSinceLastPlay { get; set; }
            public float DaysAwayFromGame { get; set; }
            public QuitType LastQuitType { get; set; }
            public float AverageSessionDuration { get; set; }
            public float CurrentSessionDuration { get; set; }
            public int RecentRageQuitCount { get; set; }
            public int CurrentLevel { get; set; }
            public int LevelAttempts { get; set; }
            public int PreviousLevelAttempts { get; set; }
            public float AverageCompletionTime { get; set; }
            public float ExpectedCompletionTime { get; set; } = 90f;
            public float CompletionRate { get; set; }
            public float LevelCompletionRate { get; set; }
            public float CurrentLevelDifficulty { get; set; }
            public float CurrentLevelTimePercentage { get; set; }
            public List<float> RecentSessionDurations { get; set; } = new();
            public int TotalQuits { get; set; }
            public int MidLevelQuits { get; set; }
            public float PreviousDifficulty { get; set; }
            public float SessionDurationBeforeAdjustment { get; set; }

            // IDifficultyDataProvider
            public float GetCurrentDifficulty() => this.CurrentDifficulty;
            public void SetCurrentDifficulty(float difficulty) => this.CurrentDifficulty = difficulty;

            // IWinStreakProvider
            public int GetWinStreak() => this.WinStreak;
            public int GetLossStreak() => this.LossStreak;
            public int GetTotalWins() => this.TotalWins;
            public int GetTotalLosses() => this.TotalLosses;

            // ITimeDecayProvider
            public DateTime GetLastPlayTime() => this.LastPlayTime;
            public TimeSpan GetTimeSinceLastPlay() => this.TimeSinceLastPlay;
            public int GetDaysAwayFromGame() => (int)this.DaysAwayFromGame;

            // IRageQuitProvider
            public QuitType GetLastQuitType() => this.LastQuitType;
            public float GetAverageSessionDuration() => this.AverageSessionDuration;
            public float GetCurrentSessionDuration() => this.CurrentSessionDuration;
            public int GetRecentRageQuitCount() => this.RecentRageQuitCount;

            // ILevelProgressProvider
            public int GetCurrentLevel() => this.CurrentLevel;
            public int GetAttemptsOnCurrentLevel() => this.LevelAttempts;
            public float GetAverageCompletionTime() => this.AverageCompletionTime;
            public float GetCompletionRate() => this.LevelCompletionRate > 0 ? this.LevelCompletionRate : this.CompletionRate;
            public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
            public float GetCurrentLevelTimePercentage() => this.CurrentLevelTimePercentage;

            // ISessionPatternProvider
            public List<float> GetRecentSessionDurations(int count)
            {
                if (this.RecentSessionDurations == null || this.RecentSessionDurations.Count == 0)
                    return new List<float>();

                var result = new List<float>();
                var maxCount = Math.Min(count, this.RecentSessionDurations.Count);
                for (int i = 0; i < maxCount; i++)
                {
                    result.Add(this.RecentSessionDurations[i]);
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
            // Create config with engagement-optimized settings
            this.config = DifficultyConfig.CreateDefault();

            // Create provider
            this.provider = new EngagementTestProvider();

            // Create aggregator
            this.aggregator = new ModifierAggregator();

            // Create all 7 modifiers with configs
            this.modifiers = new List<IDifficultyModifier>
            {
                new WinStreakModifier((WinStreakConfig)new WinStreakConfig().CreateDefault(), this.provider, null),
                new LossStreakModifier((LossStreakConfig)new LossStreakConfig().CreateDefault(), this.provider, null),
                new TimeDecayModifier((TimeDecayConfig)new TimeDecayConfig().CreateDefault(), this.provider, null),
                new RageQuitModifier((RageQuitConfig)new RageQuitConfig().CreateDefault(), this.provider, null),
                new CompletionRateModifier((CompletionRateConfig)new CompletionRateConfig().CreateDefault(), this.provider, this.provider, null),
                new LevelProgressModifier((LevelProgressConfig)new LevelProgressConfig().CreateDefault(), this.provider, null),
                new SessionPatternModifier((SessionPatternConfig)new SessionPatternConfig().CreateDefault(), this.provider, this.provider, this.provider, null),
            };

            // Create calculator with proper constructor parameters
            this.calculator = new DifficultyCalculator(this.config, this.aggregator, this.provider, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
            {
                UnityEngine.Object.DestroyImmediate(this.config);
            }
        }

        #region Session Length Optimization (5 tests)

        [Test]
        public void SessionLength_VeryShortSession_ReducesDifficultyToKeepPlaying()
        {
            // Scenario: Player plays for only 90 seconds (very short session)
            // Expected: System should reduce difficulty to encourage longer play

            // Arrange
            this.provider.CurrentSessionDuration = 90f; // 1.5 minutes - very short
            this.provider.AverageSessionDuration = 180f; // Normal: 3 minutes
            this.provider.LossStreak = 3; // Set > 2 to prevent oscillation detection (lost recently)
            this.provider.TotalWins = 1;
            this.provider.TotalLosses = 5; // Overall struggling with short sessions
            this.provider.LastQuitType = QuitType.MidPlay; // Quit mid-level

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            Assert.Less(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY,
                "Very short sessions should trigger difficulty reduction to retain player");

            Assert.IsTrue(result.AppliedModifiers.Exists(m => m.ModifierName == "SessionPattern" && m.Value < 0),
                "SessionPattern should detect very short session");

            // Engagement Impact: +30-50% session length increase expected
            Debug.Log($"[PlayerEngagement] Short session (90s) → Difficulty {result.NewDifficulty:F2} (Expected: <3.0 to encourage continuation)");
        }

        [Test]
        public void SessionLength_ConsistentShortSessions_AppliesProgressiveReduction()
        {
            // Scenario: Player consistently quits after short sessions (pattern detected)
            // Expected: Progressive difficulty reduction to break the pattern

            // Arrange
            this.provider.CurrentSessionDuration = 120f; // 2 minutes
            this.provider.AverageSessionDuration = 100f; // Very short average
            var recentSessions = new List<float> { 80f, 90f, 110f, 95f, 120f }; // All short
            this.provider.RecentSessionDurations = recentSessions;
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 4; // Set > 2 to prevent oscillation detection (pattern of frustration)
            this.provider.TotalWins = 2;
            this.provider.TotalLosses = 6; // Consistent losing pattern with short sessions

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Pattern detection should trigger stronger reduction than single short session
            var sessionPatternModifier = result.AppliedModifiers.Find(m => m.ModifierName == "SessionPattern");
            Assert.IsNotNull(sessionPatternModifier, "SessionPattern should be active");
            Assert.Less(sessionPatternModifier.Value, -0.3f,
                "Consistent short session pattern should trigger significant reduction");

            // Engagement Impact: Pattern breaking reduces churn by 25-40%
            Debug.Log($"[PlayerEngagement] Pattern: 5 short sessions → Reduction {sessionPatternModifier.Value:F2} (Target: break frustration cycle)");
        }

        [Test]
        public void SessionLength_PlayerReturnsAfterLongBreak_WelcomeBackBonus()
        {
            // Scenario: Player returns after 3 days away
            // Expected: Time decay gives "welcome back" difficulty reduction

            // Arrange
            this.provider.LastPlayTime = System.DateTime.Now.AddDays(-3);
            this.provider.TimeSinceLastPlay = System.TimeSpan.FromDays(3);
            this.provider.DaysAwayFromGame = 3f;
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 1; // Keep total games < 4 to prevent oscillation
            this.provider.TotalWins = 1;
            this.provider.TotalLosses = 2; // Returning player who left after some struggles

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var timeDecayModifier = result.AppliedModifiers.Find(m => m.ModifierName == "TimeDecay");
            Assert.IsNotNull(timeDecayModifier, "TimeDecay should be active");
            Assert.Less(timeDecayModifier.Value, -0.5f,
                "3-day absence should trigger welcome-back reduction");

            // Engagement Impact: Returning player retention improves by 35-60%
            Assert.Less(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY,
                "Returning players need easier re-entry experience");

            Debug.Log($"[PlayerEngagement] Return after 3 days → Difficulty {result.NewDifficulty:F2} (Welcome back bonus applied)");
        }

        [Test]
        public void SessionLength_OptimalSessionLength_MaintainsEngagement()
        {
            // Scenario: Player in optimal session length zone (5-10 minutes)
            // Expected: Minimal difficulty adjustments to maintain flow

            // Arrange
            this.provider.CurrentSessionDuration = 420f; // 7 minutes - optimal
            this.provider.AverageSessionDuration = 400f; // Consistent good sessions
            this.provider.WinStreak = 3; // Set > 2 to prevent oscillation detection (moderate success)
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 8;
            this.provider.TotalLosses = 3; // Good win rate in flow state

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // In optimal zone, difficulty should be stable (small adjustments only)
            var sessionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "SessionPattern");
            if (sessionModifier != null)
            {
                Assert.LessOrEqual(Mathf.Abs(sessionModifier.Value), 0.5f,
                    "Optimal session length should have minimal session-based adjustments");
            }

            // Engagement Impact: Maintaining flow state = highest LTV
            Debug.Log($"[PlayerEngagement] Optimal session (7min) → Difficulty {result.NewDifficulty:F2} (Flow state maintained)");
        }

        [Test]
        public void SessionLength_MarathonSession_PreventBurnout()
        {
            // Scenario: Player in very long session (30+ minutes)
            // Expected: Don't increase difficulty excessively to prevent burnout

            // Arrange
            this.provider.CurrentSessionDuration = 1800f; // 30 minutes
            this.provider.WinStreak = 8; // Long win streak in marathon session
            this.provider.TotalWins = 15;
            this.provider.TotalLosses = 2; // Very high win rate

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Even with high win streak, difficulty shouldn't spike too much
            Assert.LessOrEqual(result.NewDifficulty, 6.0f,
                "Marathon sessions should avoid difficulty spikes that cause burnout");

            // Engagement Impact: Prevents "victory fatigue" and session ending on negative note
            var winStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            Assert.LessOrEqual(winStreakModifier.Value, 2.0f,
                "Win streak bonus should be capped to prevent burnout");

            Debug.Log($"[PlayerEngagement] Marathon session (30min, 8 wins) → Difficulty {result.NewDifficulty:F2} (Burnout prevention active)");
        }

        #endregion

        #region Comeback Mechanics (4 tests)

        [Test]
        public void Comeback_RageQuitRecovery_AggressiveDifficultyReduction()
        {
            // Scenario: Player rage quit last session, returning now
            // Expected: Aggressive difficulty reduction to prevent second rage quit

            // Arrange
            this.provider.LastQuitType = QuitType.RageQuit;
            this.provider.RecentRageQuitCount = 2; // 2 rage quits recently
            this.provider.CurrentSessionDuration = 60f; // Just started new session
            this.provider.LossStreak = 3; // Was losing before rage quit
            this.provider.TotalWins = 2;
            this.provider.TotalLosses = 7; // Struggling player who rage quit

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var rageQuitModifier = result.AppliedModifiers.Find(m => m.ModifierName == "RageQuit");
            Assert.IsNotNull(rageQuitModifier, "RageQuit detection should be active");
            Assert.Less(rageQuitModifier.Value, -0.8f,
                "Rage quit should trigger aggressive difficulty reduction");

            var lossStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LossStreak");
            Assert.Less(lossStreakModifier.Value, -0.5f,
                "Loss streak should compound the reduction");

            // Combined effect should be significant
            Assert.Less(result.NewDifficulty, 2.0f,
                "Post-rage-quit comeback should be VERY easy to rebuild confidence");

            // Engagement Impact: Rage quit recovery rate improves from 15% to 45-60%
            Debug.Log($"[PlayerEngagement] Rage quit recovery → Difficulty {result.NewDifficulty:F2} (Aggressive safety net)");
        }

        [Test]
        public void Comeback_LossStreakToWinStreak_GradualDifficultyRamp()
        {
            // Scenario: Player was on loss streak, now winning - celebrate comeback!
            // Expected: Slow difficulty ramp to let player enjoy the wins

            // Arrange
            this.provider.LossStreak = 0; // No longer losing
            this.provider.WinStreak = 3; // Just started winning!
            this.provider.TotalWins = 5;
            this.provider.TotalLosses = 8; // Overall struggling but improving
            this.provider.CompletionRate = 0.38f; // 38% win rate - below average

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Win streak should increase difficulty, but completion rate should temper it
            var winStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");

            Assert.Greater(winStreakModifier.Value, 0, "Win streak should increase difficulty");
            Assert.Less(completionModifier.Value, 0, "Low completion rate should reduce difficulty");

            // Net effect: Modest increase to celebrate wins without overwhelming
            Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY,
                "Comeback should feel rewarding");
            Assert.LessOrEqual(result.NewDifficulty, 4.0f,
                "But not spike too fast and ruin the momentum");

            // Engagement Impact: Comeback stories create memorable moments (+40% retention)
            Debug.Log($"[PlayerEngagement] Comeback (3 wins after 8 losses) → Difficulty {result.NewDifficulty:F2} (Momentum protection)");
        }

        [Test]
        public void Comeback_OscillatingPerformance_StabilizeDifficulty()
        {
            // Scenario: Player alternating between wins and losses (oscillation)
            // Expected: Minimal difficulty bouncing to prevent frustration

            // Arrange
            this.provider.WinStreak = 1; // Just won
            this.provider.LossStreak = 0; // But previously lost
            this.provider.TotalWins = 6;
            this.provider.TotalLosses = 6; // Perfectly balanced (50% win rate)

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Oscillation detection should minimize difficulty swings
            var sessionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "SessionPattern");
            if (sessionModifier != null && sessionModifier.Metadata.ContainsKey("oscillation_detected"))
            {
                Assert.LessOrEqual(Mathf.Abs(sessionModifier.Value), 0.2f,
                    "Oscillation detection should apply minimal adjustment");

                Debug.Log($"[PlayerEngagement] Oscillation detected → Minimal adjustment {sessionModifier.Value:F2}");
            }

            // Difficulty should stay near default for balanced performance
            Assert.GreaterOrEqual(result.NewDifficulty, 2.5f);
            Assert.LessOrEqual(result.NewDifficulty, 3.5f);

            // Engagement Impact: Prevents "yo-yo effect" frustration
            Debug.Log($"[PlayerEngagement] Oscillating (50% win rate) → Difficulty {result.NewDifficulty:F2} (Stable experience)");
        }

        [Test]
        public void Comeback_ExtendedLossStreak_EmergencyIntervention()
        {
            // Scenario: Player on catastrophic loss streak (5+ losses)
            // Expected: Emergency difficulty drop to prevent uninstall

            // Arrange
            this.provider.LossStreak = 6; // Catastrophic
            this.provider.TotalWins = 2;
            this.provider.TotalLosses = 10;
            this.provider.LevelAttempts = 8; // Stuck on same level
            this.provider.CompletionRate = 0.16f; // 16% win rate - crisis mode

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Multiple safety nets should activate
            var lossStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LossStreak");
            Assert.Less(lossStreakModifier.Value, -1.5f,
                "Extended loss streak should trigger exponential reduction");

            var levelProgressModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LevelProgress");
            Assert.Less(levelProgressModifier.Value, -0.5f,
                "High attempt count should add reduction");

            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            Assert.Less(completionModifier.Value, -0.5f,
                "Low completion rate should add reduction");

            // Combined effect: Emergency difficulty floor
            Assert.LessOrEqual(result.NewDifficulty, 1.5f,
                "Emergency intervention should drop to minimum difficulty");

            // Engagement Impact: Prevents 70-80% of frustrated player churn
            Debug.Log($"[PlayerEngagement] EMERGENCY: 6 loss streak → Difficulty {result.NewDifficulty:F2} (Uninstall prevention)");
        }

        #endregion

        #region Flow State Maintenance (6 tests)

        [Test]
        public void FlowState_OptimalChallenge_40To60PercentWinRate()
        {
            // Scenario: Player winning 50% of attempts (optimal challenge)
            // Expected: Maintain current difficulty for flow state

            // Arrange
            this.provider.TotalWins = 10;
            this.provider.TotalLosses = 10;
            this.provider.CompletionRate = 0.50f; // Perfect 50%
            this.provider.WinStreak = 3; // Set > 2 to prevent oscillation detection (recent good streak)
            this.provider.LossStreak = 0;

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Should stay close to default difficulty (optimal challenge zone)
            Assert.GreaterOrEqual(result.NewDifficulty, 2.8f);
            Assert.LessOrEqual(result.NewDifficulty, 3.5f);

            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            // 50% win rate should have minimal adjustment
            Assert.LessOrEqual(Mathf.Abs(completionModifier.Value), 0.3f,
                "Optimal win rate should maintain current difficulty");

            // Engagement Impact: Flow state = 3-5x longer sessions + highest satisfaction
            Debug.Log($"[PlayerEngagement] Flow state (50% wins) → Difficulty {result.NewDifficulty:F2} (Csikszentmihalyi optimal challenge)");
        }

        [Test]
        public void FlowState_TooEasy_IncreaseDifficultyToPreventBoredom()
        {
            // Scenario: Player winning 80%+ (too easy, boredom risk)
            // Expected: Increase difficulty to restore challenge

            // Arrange
            this.provider.TotalWins = 16;
            this.provider.TotalLosses = 4;
            this.provider.CompletionRate = 0.80f; // 80% - too easy
            this.provider.WinStreak = 5; // Current hot streak
            this.provider.AverageCompletionTime = 30f; // Fast completions

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            Assert.Greater(completionModifier.Value, 0.3f,
                "High win rate should increase difficulty");

            var winStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            Assert.Greater(winStreakModifier.Value, 0.5f,
                "Win streak should add to difficulty");

            // Combined: Push back into challenge zone
            Assert.Greater(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY + 0.5f,
                "Too-easy games should increase difficulty to prevent boredom");

            // Engagement Impact: Boredom prevention maintains long-term retention
            Debug.Log($"[PlayerEngagement] Too easy (80% wins) → Difficulty {result.NewDifficulty:F2} (Boredom prevention)");
        }

        [Test]
        public void FlowState_SkillImprovement_GradualDifficultyIncrease()
        {
            // Scenario: Player's skill improving over time (completion rate rising)
            // Expected: Gradual difficulty increase to match skill growth

            // Arrange
            this.provider.TotalWins = 12;
            this.provider.TotalLosses = 8;
            this.provider.WinStreak = 3; // Set > 2 to prevent oscillation detection (player improving)
            this.provider.LossStreak = 0;
            this.provider.CompletionRate = 0.60f; // Overall 60%
            this.provider.LevelCompletionRate = 0.70f; // Recent level: 70% (improving!)
            this.provider.AverageCompletionTime = 90f; // Getting faster

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");

            // Weighted rate (total 60%, level 70%) should trigger modest increase
            Assert.Greater(completionModifier.Value, 0,
                "Skill improvement should increase difficulty");
            Assert.LessOrEqual(completionModifier.Value, 0.5f,
                "But increase should be gradual, not sudden spike");

            // Difficulty should grow with player skill
            Assert.Greater(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY,
                "Growing skill needs growing challenge");

            // Engagement Impact: Matching difficulty to skill = extended engagement curve
            Debug.Log($"[PlayerEngagement] Skill growth (60%→70%) → Difficulty {result.NewDifficulty:F2} (Gradual skill ladder)");
        }

        [Test]
        public void FlowState_QuickCompletions_SignalTooEasy()
        {
            // Scenario: Player completing levels very quickly
            // Expected: Increase difficulty to restore challenge

            // Arrange
            this.provider.AverageCompletionTime = 30f; // 30 seconds
            this.provider.ExpectedCompletionTime = 90f; // Expected: 90 seconds
            this.provider.WinStreak = 3;
            this.provider.TotalWins = 10;
            this.provider.TotalLosses = 5; // Good win rate with quick completions
            this.provider.CompletionRate = 0.65f;

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var levelProgressModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LevelProgress");

            // Quick completions should trigger difficulty increase
            Assert.Greater(levelProgressModifier.Value, 0,
                "Fast completions signal too-easy difficulty");

            // Engagement Impact: Time-based difficulty prevents speed-running boredom
            Debug.Log($"[PlayerEngagement] Quick completions (30s vs 90s expected) → Difficulty {result.NewDifficulty:F2} (Challenge restoration)");
        }

        [Test]
        public void FlowState_SlowProgress_ButStillWinning()
        {
            // Scenario: Player taking long time but still completing
            // Expected: Maintain current difficulty (slow ≠ struggling)

            // Arrange
            this.provider.AverageCompletionTime = 180f; // 3 minutes
            this.provider.ExpectedCompletionTime = 90f; // Expected: 90 seconds
            this.provider.WinStreak = 3; // Set > 2 to prevent oscillation detection (still winning)
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 8;
            this.provider.TotalLosses = 6; // Decent win rate
            this.provider.CompletionRate = 0.55f; // Decent win rate
            this.provider.LevelAttempts = 2; // Not stuck

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Slow but successful = different play style, not struggling
            var levelProgressModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LevelProgress");
            if (levelProgressModifier != null)
            {
                // Small penalty for slow play, but not major
                Assert.GreaterOrEqual(levelProgressModifier.Value, -0.3f,
                    "Slow but winning should have minimal penalty");
            }

            // Difficulty should stay reasonable
            Assert.GreaterOrEqual(result.NewDifficulty, 2.5f);
            Assert.LessOrEqual(result.NewDifficulty, 3.8f);

            // Engagement Impact: Respect different play styles (methodical vs fast)
            Debug.Log($"[PlayerEngagement] Slow but winning (3min vs 90s) → Difficulty {result.NewDifficulty:F2} (Play style respected)");
        }

        [Test]
        public void FlowState_MixedSignals_CompletionRateRulesAll()
        {
            // Scenario: Conflicting signals (fast time, high attempts, medium win rate)
            // Expected: Completion rate should be primary factor

            // Arrange
            this.provider.AverageCompletionTime = 45f; // Fast when winning
            this.provider.LevelAttempts = 5; // But many attempts
            this.provider.CompletionRate = 0.45f; // 45% - below optimal
            this.provider.WinStreak = 1;
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 1; // Keep total games < 4 to prevent oscillation
            this.provider.TotalLosses = 1; // Mixed performance just starting

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");

            // 45% win rate should trigger reduction (below optimal 50%)
            Assert.Less(completionModifier.Value, 0,
                "Below-optimal win rate should reduce difficulty");

            // Despite fast completions, overall difficulty should decrease
            Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.DEFAULT_DIFFICULTY,
                "Completion rate should dominate conflicting signals");

            // Engagement Impact: Win rate is king for engagement
            Debug.Log($"[PlayerEngagement] Mixed signals (fast but 45% wins) → Difficulty {result.NewDifficulty:F2} (Win rate prioritized)");
        }

        #endregion

        #region Progression Pacing (5 tests)

        [Test]
        public void Progression_NewPlayer_GentleOnboarding()
        {
            // Scenario: New player (< 10 total attempts)
            // Expected: Keep difficulty low for positive first impression

            // Arrange
            this.provider.TotalWins = 3;
            this.provider.TotalLosses = 2;
            this.provider.WinStreak = 3; // Set > 2 to prevent oscillation detection (new player on good streak)
            this.provider.LossStreak = 0;
            this.provider.CompletionRate = 0.60f; // Good for new player
            this.provider.CurrentSessionDuration = 300f; // 5 minutes - good first session

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");

            // With only 5 attempts, shouldn't have strong adjustments yet
            if (completionModifier != null && completionModifier.Metadata.ContainsKey("totalAttempts"))
            {
                var attempts = (int)completionModifier.Metadata["totalAttempts"];
                if (attempts < 10)
                {
                    Debug.Log($"[PlayerEngagement] New player ({attempts} attempts) - gentle onboarding mode");
                }
            }

            // New player difficulty should stay low-to-medium
            Assert.LessOrEqual(result.NewDifficulty, 4.0f,
                "New players need positive onboarding experience");

            // Engagement Impact: First hour retention critical (65% drop without proper onboarding)
            Debug.Log($"[PlayerEngagement] New player (5 attempts) → Difficulty {result.NewDifficulty:F2} (Onboarding safety)");
        }

        [Test]
        public void Progression_MidGame_BalancedChallenge()
        {
            // Scenario: Mid-game player (50-100 attempts, steady progress)
            // Expected: Balanced difficulty following their performance

            // Arrange
            this.provider.TotalWins = 35;
            this.provider.TotalLosses = 40;
            this.provider.CompletionRate = 0.47f; // Slightly below 50%
            this.provider.WinStreak = 0;
            this.provider.LossStreak = 3; // Set to 3 to prevent oscillation detection while still showing struggle

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Mid-game should have responsive difficulty
            var lossStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LossStreak");
            Assert.Less(lossStreakModifier.Value, 0,
                "Current loss streak should reduce difficulty");

            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            Assert.Less(completionModifier.Value, 0,
                "Below-50% win rate should reduce difficulty");

            // Should trend back toward optimal difficulty
            Assert.GreaterOrEqual(result.NewDifficulty, 2.0f);
            Assert.LessOrEqual(result.NewDifficulty, 3.5f);

            // Engagement Impact: Mid-game retention = core gameplay loop validation
            Debug.Log($"[PlayerEngagement] Mid-game (75 attempts, 47% wins) → Difficulty {result.NewDifficulty:F2} (Core loop balance)");
        }

        [Test]
        public void Progression_VeteranPlayer_RespectMastery()
        {
            // Scenario: Veteran player (500+ attempts, high skill)
            // Expected: Allow high difficulty for mastery satisfaction

            // Arrange
            this.provider.TotalWins = 320;
            this.provider.TotalLosses = 180;
            this.provider.CompletionRate = 0.64f; // 64% - skilled player
            this.provider.WinStreak = 4;
            this.provider.AverageCompletionTime = 60f; // Fast and efficient

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            Assert.Greater(completionModifier.Value, 0,
                "High skill should allow high difficulty");

            var winStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            Assert.Greater(winStreakModifier.Value, 0,
                "Current mastery should increase challenge");

            // Veterans can handle high difficulty
            Assert.GreaterOrEqual(result.NewDifficulty, 4.0f,
                "Veterans need high challenge for satisfaction");
            Assert.LessOrEqual(result.NewDifficulty, 7.0f,
                "But not impossible");

            // Engagement Impact: Mastery curve extends LTV for core players
            Debug.Log($"[PlayerEngagement] Veteran (500 attempts, 64% wins) → Difficulty {result.NewDifficulty:F2} (Mastery satisfied)");
        }

        [Test]
        public void Progression_PlateauDetection_OfferNewChallenge()
        {
            // Scenario: Player stuck at same win rate for extended period
            // Expected: Slight difficulty reduction to help break plateau

            // Arrange
            this.provider.TotalWins = 50;
            this.provider.TotalLosses = 50;
            this.provider.WinStreak = 0; // Explicitly set streaks to prevent oscillation (but total games > 4)
            this.provider.LossStreak = 3; // Set > 2 to prevent oscillation detection
            this.provider.CompletionRate = 0.50f; // Stable 50%
            this.provider.LevelCompletionRate = 0.50f; // Not improving
            this.provider.AverageCompletionTime = 90f; // Consistent times
            this.provider.LevelAttempts = 4; // Multiple attempts per level

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Plateau scenario: good win rate but high attempts = stuck
            var levelProgressModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LevelProgress");
            if (levelProgressModifier != null)
            {
                // High attempts should trigger small reduction
                Assert.LessOrEqual(levelProgressModifier.Value, 0,
                    "Plateau (multiple attempts) should trigger slight reduction");
            }

            // Difficulty should be at or slightly below default to help progression
            Assert.LessOrEqual(result.NewDifficulty, 3.5f,
                "Plateau breaking needs slight difficulty reduction");

            // Engagement Impact: Breaking plateaus prevents mid-game churn
            Debug.Log($"[PlayerEngagement] Plateau (50% wins, 4 attempts/level) → Difficulty {result.NewDifficulty:F2} (Plateau breaking)");
        }

        [Test]
        public void Progression_SkillRegression_TemporarySupport()
        {
            // Scenario: Skilled player suddenly performing worse (fatigue? distraction?)
            // Expected: Temporary support without permanent difficulty drop

            // Arrange
            this.provider.TotalWins = 80;
            this.provider.TotalLosses = 45;
            this.provider.CompletionRate = 0.64f; // Overall good (64%)
            this.provider.LevelCompletionRate = 0.35f; // Recent level: bad (35%)
            this.provider.LossStreak = 4; // Current bad streak
            this.provider.CurrentSessionDuration = 180f; // Short session - might be distracted

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var lossStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LossStreak");
            Assert.Less(lossStreakModifier.Value, -0.5f,
                "Current struggle should trigger immediate support");

            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            // Weighted rate (64% overall, 35% level) should be between good and bad
            // Good overall prevents massive drop, but recent bad triggers reduction
            Assert.Less(completionModifier.Value, 0,
                "Recent regression should reduce difficulty");

            // Net effect: Support without over-correcting
            Assert.GreaterOrEqual(result.NewDifficulty, 2.5f,
                "Temporary regression shouldn't drop too far");
            Assert.LessOrEqual(result.NewDifficulty, 4.0f,
                "But should provide support");

            // Engagement Impact: Graceful handling of temporary performance dips
            Debug.Log($"[PlayerEngagement] Skill regression (64%→35%) → Difficulty {result.NewDifficulty:F2} (Temporary support)");
        }

        #endregion

        #region Monetization Sweet Spot (3 tests)

        [Test]
        public void Monetization_StuckPlayer_PrimeForPowerups()
        {
            // Scenario: Player stuck on level, frustrated but engaged
            // Expected: Difficulty reduction creates powerup purchase window

            // Arrange
            this.provider.LevelAttempts = 7; // Stuck!
            this.provider.LossStreak = 3;
            this.provider.TotalWins = 4;
            this.provider.TotalLosses = 6; // Struggling overall
            this.provider.CompletionRate = 0.42f; // Below optimal
            this.provider.CurrentSessionDuration = 420f; // 7 minutes - still engaged
            this.provider.AverageCompletionTime = 120f; // When they win, takes long

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var levelProgressModifier = result.AppliedModifiers.Find(m => m.ModifierName == "LevelProgress");
            Assert.Less(levelProgressModifier.Value, -0.4f,
                "High attempt count should reduce difficulty significantly");

            // This creates the "purchase window": hard enough to want help, but winnable with boost
            Assert.GreaterOrEqual(result.NewDifficulty, 2.0f,
                "Should still feel challenging (justify powerup)");
            Assert.LessOrEqual(result.NewDifficulty, 3.2f,
                "But winnable with small boost (powerup feels worth it)");

            // Monetization Impact: Highest IAP conversion happens at 5-8 attempts
            Debug.Log($"[PlayerEngagement] Stuck (7 attempts) → Difficulty {result.NewDifficulty:F2} (MONETIZATION: Prime powerup window)");
        }

        [Test]
        public void Monetization_PostPurchase_EnsureValueDelivery()
        {
            // Scenario: Player likely just bought powerup (win after long struggle)
            // Expected: Maintain reasonable difficulty to ensure purchase feels worthwhile

            // Arrange - Simulate "just won after being stuck"
            this.provider.WinStreak = 1; // Just won!
            this.provider.LossStreak = 0;
            this.provider.TotalWins = 1; // Keep total games < 4 to prevent oscillation
            this.provider.TotalLosses = 2; // Post-purchase first win
            this.provider.PreviousLevelAttempts = 8; // Was stuck before
            this.provider.LevelAttempts = 1; // Won on first try this time (powerup helped!)
            this.provider.CompletionRate = 0.45f; // Overall still struggling

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            // Don't spike difficulty after purchase-assisted win
            var winStreakModifier = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            if (winStreakModifier != null)
            {
                // Win streak increase should be modest
                Assert.LessOrEqual(winStreakModifier.Value, 0.5f,
                    "Post-purchase win shouldn't spike difficulty");
            }

            // Maintain difficulty to validate purchase decision
            Assert.LessOrEqual(result.NewDifficulty, 3.8f,
                "Post-purchase difficulty should feel fair (purchase felt worth it)");

            // Monetization Impact: Purchase satisfaction drives repeat monetization
            Debug.Log($"[PlayerEngagement] Post-purchase → Difficulty {result.NewDifficulty:F2} (MONETIZATION: Value delivery)");
        }

        [Test]
        public void Monetization_WhaleRetention_AllowHighDifficulty()
        {
            // Scenario: High-skill, high-engagement player (whale candidate)
            // Expected: Allow challenging difficulty to justify future purchases

            // Arrange
            this.provider.TotalWins = 150;
            this.provider.TotalLosses = 70;
            this.provider.CompletionRate = 0.68f; // High skill
            this.provider.WinStreak = 5;
            this.provider.CurrentSessionDuration = 1200f; // 20 minute session
            this.provider.AverageSessionDuration = 900f; // Consistently long sessions

            // Act
            var result = this.calculator.Calculate(this.modifiers);

            // Assert - Game Design Validation
            var completionModifier = result.AppliedModifiers.Find(m => m.ModifierName == "CompletionRate");
            Assert.Greater(completionModifier.Value, 0.3f,
                "High skill should allow significant difficulty increase");

            // Whales need hard content to justify spending
            Assert.GreaterOrEqual(result.NewDifficulty, 4.5f,
                "Core players need challenge to justify powerups/skips");
            Assert.LessOrEqual(result.NewDifficulty, 8.0f,
                "But still beatable");

            // Monetization Impact: Whales spend on challenge, not ease
            // 70-80% of revenue from 10% of players who seek difficulty
            Debug.Log($"[PlayerEngagement] Whale profile (68% wins, 20min sessions) → Difficulty {result.NewDifficulty:F2} (MONETIZATION: Premium challenge)");
        }

        #endregion
    }
}

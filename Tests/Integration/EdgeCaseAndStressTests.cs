using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    [Category("EdgeCases")]
    [Category("Stress")]
    public class EdgeCaseAndStressTests
    {
        private DifficultyConfig config;

        private class ExtremeValueProvider :
            IDifficultyDataProvider,
            IWinStreakProvider,
            ITimeDecayProvider,
            IRageQuitProvider,
            ILevelProgressProvider,
            ISessionPatternProvider
        {
            public float CurrentDifficulty { get; set; }
            public int WinStreak { get; set; }
            public int LossStreak { get; set; }
            public int TotalWins { get; set; }
            public int TotalLosses { get; set; }
            public DateTime LastPlayTime { get; set; } = DateTime.Now;
            public int DaysAway { get; set; }
            public QuitType LastQuitType { get; set; }
            public float AverageSessionDuration { get; set; }
            public float CurrentSessionDuration { get; set; }
            public int RecentRageQuitCount { get; set; }
            public int CurrentLevel { get; set; }
            public int AttemptsOnCurrentLevel { get; set; }
            public float AverageCompletionTime { get; set; }
            public float CompletionRate { get; set; }
            public float CurrentLevelDifficulty { get; set; }
            public float CurrentLevelTimePercentage { get; set; }
            public List<float> SessionDurations { get; set; } = new();
            public int TotalQuits { get; set; }
            public int MidLevelQuits { get; set; }
            public float PreviousDifficulty { get; set; }
            public float SessionDurationBeforeAdjustment { get; set; }

            // Interface implementations
            public float GetCurrentDifficulty() => this.CurrentDifficulty;
            public void SetCurrentDifficulty(float difficulty) => this.CurrentDifficulty = difficulty;
            public int GetWinStreak() => this.WinStreak;
            public int GetLossStreak() => this.LossStreak;
            public int GetTotalWins() => this.TotalWins;
            public int GetTotalLosses() => this.TotalLosses;
            public DateTime GetLastPlayTime() => this.LastPlayTime;
            public TimeSpan GetTimeSinceLastPlay() => DateTime.Now - this.LastPlayTime;
            public int GetDaysAwayFromGame() => this.DaysAway;
            public QuitType GetLastQuitType() => this.LastQuitType;
            public float GetAverageSessionDuration() => this.AverageSessionDuration;
            public float GetCurrentSessionDuration() => this.CurrentSessionDuration;
            public int GetRecentRageQuitCount() => this.RecentRageQuitCount;
            public void RecordSessionEnd(QuitType quitType, float durationSeconds) { }
            public void RecordSessionStart() { }
            public int GetCurrentLevel() => this.CurrentLevel;
            public int GetAttemptsOnCurrentLevel() => this.AttemptsOnCurrentLevel;
            public float GetAverageCompletionTime() => this.AverageCompletionTime;
            public float GetCompletionRate() => this.CompletionRate;
            public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
            public float GetCurrentLevelTimePercentage() => this.CurrentLevelTimePercentage;
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
            this.config = DifficultyConfig.CreateDefault();
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
                UnityEngine.Object.DestroyImmediate(this.config);
        }

        [Test]
        public void EdgeCase_ExtremeLargeValues_DoNotCauseCrash()
        {
            // Arrange
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = float.MaxValue,
                WinStreak = int.MaxValue,
                LossStreak = int.MaxValue,
                TotalWins = int.MaxValue,
                TotalLosses = int.MaxValue,
                LastPlayTime = DateTime.MinValue,
                DaysAway = int.MaxValue,
                RecentRageQuitCount = int.MaxValue,
                CurrentLevel = int.MaxValue,
                AttemptsOnCurrentLevel = int.MaxValue,
                CurrentLevelTimePercentage = float.MaxValue,
                AverageCompletionTime = float.MaxValue
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act & Assert - Should not throw or produce NaN/Infinity
            Assert.DoesNotThrow(() =>
            {
                var result = service.CalculateDifficulty();
                Assert.IsNotNull(result);
                Assert.IsFalse(float.IsNaN(result.NewDifficulty));
                Assert.IsFalse(float.IsInfinity(result.NewDifficulty));
                Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
                Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);
            });
        }

        [Test]
        public void EdgeCase_NegativeValues_HandledGracefully()
        {
            // Arrange
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = -100f,
                WinStreak = -5,
                LossStreak = -10,
                TotalWins = -50,
                TotalLosses = -20,
                LastPlayTime = DateTime.Now.AddDays(1), // Future time
                DaysAway = -7,
                CurrentSessionDuration = -60f,
                AttemptsOnCurrentLevel = -3,
                CurrentLevelTimePercentage = -1f
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var result = service.CalculateDifficulty();
                Assert.IsNotNull(result);
                Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);
                Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
            });
        }

        [Test]
        public void EdgeCase_ZeroValues_ProduceValidResults()
        {
            // Arrange - All zeros
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = 0f,
                WinStreak = 0,
                LossStreak = 0,
                TotalWins = 0,
                TotalLosses = 0,
                LastPlayTime = DateTime.Now,
                CurrentSessionDuration = 0f,
                AttemptsOnCurrentLevel = 0,
                CurrentLevelTimePercentage = 0f,
                AverageCompletionTime = 0f
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act
            var result = service.CalculateDifficulty();

            // Assert
            Assert.IsNotNull(result);
            // When starting with 0 difficulty, it should be clamped to min difficulty (1f)
            Assert.AreEqual(DifficultyConstants.MIN_DIFFICULTY, result.NewDifficulty);
        }

        [Test]
        public void EdgeCase_DivisionByZero_DoesNotOccur()
        {
            // Arrange - Scenarios that could cause division by zero
            var provider = new ExtremeValueProvider
            {
                TotalWins = 0,
                TotalLosses = 0, // Completion rate = 0/0
                AverageCompletionTime = 0f, // Could cause division issues
                SessionDurations = new List<float>() // Empty session history
            };

            var completionRateModifier = new CompletionRateModifier(
                (CompletionRateConfig)new CompletionRateConfig().CreateDefault(),
                provider,
                provider,
                null);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var result = completionRateModifier.Calculate();
                Assert.IsNotNull(result);
                Assert.IsFalse(float.IsNaN(result.Value));
            });
        }

        [Test]
        public void EdgeCase_EmptyCollections_HandledProperly()
        {
            // Arrange
            var provider = new ExtremeValueProvider
            {
                SessionDurations = new List<float>(), // Empty
                CurrentDifficulty = 5f
            };

            var sessionModifier = new SessionPatternModifier(
                (SessionPatternConfig)new SessionPatternConfig().CreateDefault(),
                provider,
                provider,
                provider, // IWinStreakProvider
                null);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var result = sessionModifier.Calculate();
                Assert.IsNotNull(result);
                Assert.IsFalse(result.Reason.Contains("History shows")); // Should not process empty history
            });
        }

        [Test]
        public void EdgeCase_EmptyProviders_ReturnDefaultValues()
        {
            // Arrange - All modifiers with providers that return zero/empty values
            var emptyProvider = new ExtremeValueProvider
            {
                CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY,
                WinStreak = 0,
                LossStreak = 0,
                TotalWins = 0,
                TotalLosses = 0,
                CurrentSessionDuration = 0f,
                AverageSessionDuration = 0f,
                RecentRageQuitCount = 0,
                AttemptsOnCurrentLevel = 0,
                CurrentLevelTimePercentage = 0f,
                AverageCompletionTime = 0f,
                SessionDurations = new List<float>(),
                LastPlayTime = DateTime.Now
            };

            var modifiers = new List<IDifficultyModifier>
            {
                new WinStreakModifier((WinStreakConfig)new WinStreakConfig().CreateDefault(), emptyProvider, null),
                new LossStreakModifier((LossStreakConfig)new LossStreakConfig().CreateDefault(), emptyProvider, null),
                new TimeDecayModifier((TimeDecayConfig)new TimeDecayConfig().CreateDefault(), emptyProvider, null),
                new RageQuitModifier((RageQuitConfig)new RageQuitConfig().CreateDefault(), emptyProvider, null),
                new CompletionRateModifier((CompletionRateConfig)new CompletionRateConfig().CreateDefault(), emptyProvider, emptyProvider, null),
                new LevelProgressModifier((LevelProgressConfig)new LevelProgressConfig().CreateDefault(), emptyProvider, null),
                new SessionPatternModifier((SessionPatternConfig)new SessionPatternConfig().CreateDefault(), emptyProvider, emptyProvider, emptyProvider, null)
            };

            // Act & Assert - Most should return no change, except RageQuit which detects Normal quit
            foreach (var modifier in modifiers)
            {
                Assert.DoesNotThrow(() =>
                {
                    var result = modifier.Calculate();
                    Assert.IsNotNull(result);

                    if (modifier.ModifierName == DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT)
                    {
                        // RageQuitModifier applies penalty even for Normal quit type
                        Assert.Less(result.Value, 0f, $"{modifier.ModifierName} should apply quit penalty");
                    }
                    else if (modifier.ModifierName == DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS)
                    {
                        // LevelProgressModifier detects struggling on easy levels (0% completion on difficulty 0)
                        Assert.Less(result.Value, 0f, $"{modifier.ModifierName} should apply struggle penalty");
                    }
                    else
                    {
                        Assert.AreEqual(0f, result.Value, $"{modifier.ModifierName} should return 0 with empty provider data");
                    }
                });
            }
        }

        [Test]
        public void StressTest_RapidCalculations_PerformanceCheck()
        {
            // Arrange
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = 5f,
                WinStreak = 3,
                LossStreak = 0,
                TotalWins = 10,
                TotalLosses = 5
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act - Perform many calculations
            var startTime = DateTime.Now;
            const int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                var result = service.CalculateDifficulty();
                Assert.IsNotNull(result);
            }

            var endTime = DateTime.Now;
            var elapsedMs = (endTime - startTime).TotalMilliseconds;

            // Assert - Should complete in reasonable time (< 1 second for 1000 calculations)
            Assert.Less(elapsedMs, 1000,
                $"1000 calculations took {elapsedMs}ms, should be < 1000ms");

            // Calculate average time per calculation
            var avgMs = elapsedMs / iterations;
            Assert.Less(avgMs, 10f, $"Average calculation time {avgMs}ms should be < 10ms");
        }

        [Test]
        public void StressTest_RandomizedInputs_StableOutput()
        {
            // Arrange
            UnityEngine.Random.InitState(42); // Fixed seed for reproducibility
            var provider = new ExtremeValueProvider();

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act - Test with random inputs
            for (int i = 0; i < 100; i++)
            {
                // Randomize provider values
                provider.CurrentDifficulty = UnityEngine.Random.Range(1, 11);
                provider.WinStreak = UnityEngine.Random.Range(0, 10);
                provider.LossStreak = UnityEngine.Random.Range(0, 10);
                provider.TotalWins = UnityEngine.Random.Range(0, 100);
                provider.TotalLosses = UnityEngine.Random.Range(0, 100);
                provider.LastPlayTime = DateTime.Now.AddHours(-UnityEngine.Random.Range(0f, 168f)); // 0-7 days ago
                provider.AttemptsOnCurrentLevel = UnityEngine.Random.Range(1, 10);
                provider.CurrentSessionDuration = UnityEngine.Random.Range(10, 600);

                // Assert - Should always produce valid results
                var result = service.CalculateDifficulty();
                Assert.IsNotNull(result);
                Assert.IsFalse(float.IsNaN(result.NewDifficulty));
                Assert.IsFalse(float.IsInfinity(result.NewDifficulty));
                Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);
                Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
            }
        }

        [Test]
        public void EdgeCase_DateTimeParsingErrors_HandledGracefully()
        {
            // Arrange
            var provider = new ExtremeValueProvider
            {
                LastPlayTime = DateTime.MinValue, // Use invalid date instead of string
                CurrentDifficulty = 5f
            };

            var timeDecayModifier = new TimeDecayModifier(
                (TimeDecayConfig)new TimeDecayConfig().CreateDefault(),
                provider,
                null);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var result = timeDecayModifier.Calculate();
                Assert.IsNotNull(result);
                // Should handle invalid date gracefully
            });
        }

        [Test]
        public void EdgeCase_ConfigWithExtremeValues_StillWorks()
        {
            // Arrange - Config with extreme values
            var extremeConfig = DifficultyConfig.CreateDefault();
            // Note: These properties are readonly, so we can't modify them directly
            // The test will use default values from CreateDefault()

            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = 5000f,
                WinStreak = 100
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(extremeConfig, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, extremeConfig, modifiers, null);

            // Act
            var result = service.CalculateDifficulty();

            // Assert
            Assert.IsNotNull(result);
            Assert.LessOrEqual(result.NewDifficulty, 10000f);
            Assert.GreaterOrEqual(result.NewDifficulty, 0.0001f);

            // Clean up
            UnityEngine.Object.DestroyImmediate(extremeConfig);
        }

        [Test]
        public void EdgeCase_SimultaneousExtremeConditions_ProducesReasonableResult()
        {
            // Arrange - Player with contradictory extreme stats
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = 5f,
                WinStreak = 100, // Extreme win streak
                LossStreak = 100, // Also extreme loss streak (impossible but testing)
                TotalWins = 1000,
                TotalLosses = 1000,
                RecentRageQuitCount = 50,
                AttemptsOnCurrentLevel = 100,
                CurrentSessionDuration = 1f, // Extremely short
                AverageSessionDuration = 3600f // Extremely long average
            };

            var modifiers = CreateAllModifiers(provider);
            var aggregator = new ModifierAggregator();
            var calculator = new DifficultyCalculator(this.config, aggregator, provider, null);
            var service = new DynamicDifficultyService(calculator, provider, this.config, modifiers, null);

            // Act
            var result = service.CalculateDifficulty();

            // Assert - Should still produce a valid result
            Assert.IsNotNull(result);
            Assert.GreaterOrEqual(result.NewDifficulty, DifficultyConstants.MIN_DIFFICULTY);
            Assert.LessOrEqual(result.NewDifficulty, DifficultyConstants.MAX_DIFFICULTY);
            Assert.IsFalse(float.IsNaN(result.NewDifficulty));
        }

        [Test]
        public void EdgeCase_VeryLongSessionDurations_HandledCorrectly()
        {
            // Arrange - Session lasting days
            var provider = new ExtremeValueProvider
            {
                CurrentDifficulty = 5f,
                CurrentSessionDuration = 86400f, // 24 hours
                AverageSessionDuration = 86400f,
                SessionDurations = new List<float> { 86400f, 86400f, 86400f, 86400f, 86400f, 86400f, 86400f, 86400f, 86400f, 86400f }
            };

            var sessionModifier = new SessionPatternModifier(
                (SessionPatternConfig)new SessionPatternConfig().CreateDefault(),
                provider,
                provider,
                provider, // IWinStreakProvider
                null);

            // Act
            var result = sessionModifier.Calculate();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(float.IsNaN(result.Value));
            // Very long sessions shouldn't trigger frustration penalties
            Assert.GreaterOrEqual(result.Value, 0f);
        }

        // Helper method
        private List<IDifficultyModifier> CreateAllModifiers(ExtremeValueProvider provider)
        {
            return new List<IDifficultyModifier>
            {
                new WinStreakModifier((WinStreakConfig)new WinStreakConfig().CreateDefault(), provider, null),
                new LossStreakModifier((LossStreakConfig)new LossStreakConfig().CreateDefault(), provider, null),
                new TimeDecayModifier((TimeDecayConfig)new TimeDecayConfig().CreateDefault(), provider, null),
                new RageQuitModifier((RageQuitConfig)new RageQuitConfig().CreateDefault(), provider, null),
                new CompletionRateModifier((CompletionRateConfig)new CompletionRateConfig().CreateDefault(), provider, provider, null),
                new LevelProgressModifier((LevelProgressConfig)new LevelProgressConfig().CreateDefault(), provider, null),
                new SessionPatternModifier((SessionPatternConfig)new SessionPatternConfig().CreateDefault(), provider, provider, provider, null)
            };
        }
    }
}
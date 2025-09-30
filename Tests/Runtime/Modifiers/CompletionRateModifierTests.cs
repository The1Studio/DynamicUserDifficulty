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


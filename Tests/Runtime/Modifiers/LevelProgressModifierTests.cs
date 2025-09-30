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
            public float GetCurrentLevelTimePercentage() => 1.0f;
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


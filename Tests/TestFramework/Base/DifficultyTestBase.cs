using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base
{
    /// <summary>
    /// Base class for all Dynamic Difficulty tests
    /// </summary>
    public abstract class DifficultyTestBase
    {
        protected DifficultyConfig defaultConfig;
        protected PlayerSessionData testSessionData;
        protected DateTime testDateTime;

        [SetUp]
        public virtual void Setup()
        {
            // Reset test time
            testDateTime = new DateTime(2025, 1, 15, 12, 0, 0);

            // Create default config
            defaultConfig = CreateDefaultConfig();

            // Create default session data
            testSessionData = CreateDefaultSessionData();
        }

        [TearDown]
        public virtual void TearDown()
        {
            // Cleanup if needed
            defaultConfig = null;
            testSessionData = null;
        }

        /// <summary>
        /// Creates a default difficulty configuration for testing
        /// </summary>
        protected virtual DifficultyConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<DifficultyConfig>();
            config.minDifficulty = 1f;
            config.maxDifficulty = 10f;
            config.defaultDifficulty = 3f;
            config.maxChangePerSession = 2f;
            config.debugMode = true;
            config.enableAnalytics = false;
            config.cacheTimeMinutes = 5;

            config.modifierConfigs = new List<ModifierConfig>
            {
                CreateModifierConfig("WinStreak", 1, new Dictionary<string, float>
                {
                    { "WinThreshold", 3 },
                    { "StepSize", 0.5f },
                    { "MaxBonus", 2f },
                    { "ResponseCurve", 1f }
                }),
                CreateModifierConfig("LossStreak", 2, new Dictionary<string, float>
                {
                    { "LossThreshold", 2 },
                    { "StepSize", 0.3f },
                    { "MaxReduction", 1.5f },
                    { "ResponseCurve", 1.2f }
                }),
                CreateModifierConfig("TimeDecay", 3, new Dictionary<string, float>
                {
                    { "DecayPerDay", 0.5f },
                    { "MaxDecay", 2f },
                    { "GraceHours", 6 }
                }),
                CreateModifierConfig("RageQuit", 4, new Dictionary<string, float>
                {
                    { "DetectionTimeSeconds", 30 },
                    { "DifficultyReduction", 1f },
                    { "CooldownHours", 4 }
                })
            };

            return config;
        }

        /// <summary>
        /// Creates a default player session data for testing
        /// </summary>
        protected virtual PlayerSessionData CreateDefaultSessionData()
        {
            return new PlayerSessionData
            {
                CurrentDifficulty = 3f,
                WinStreak = 0,
                LossStreak = 0,
                LastPlayTime = testDateTime.AddDays(-1),
                LastSession = new SessionInfo
                {
                    SessionStart = testDateTime.AddDays(-1),
                    SessionEnd = testDateTime.AddDays(-1).AddMinutes(30),
                    LevelsPlayed = 5,
                    LevelsWon = 3,
                    SessionDuration = 1800f // 30 minutes
                },
                SessionHistory = new List<SessionInfo>()
            };
        }

        /// <summary>
        /// Helper method to create modifier configuration
        /// </summary>
        protected ModifierConfig CreateModifierConfig(string name, int priority, Dictionary<string, float> parameters)
        {
            var config = new ModifierConfig
            {
                modifierName = name,
                enabled = true,
                priority = priority,
                parameters = new List<ModifierParameter>()
            };

            foreach (var kvp in parameters)
            {
                config.parameters.Add(new ModifierParameter { key = kvp.Key, value = kvp.Value });
            }

            return config;
        }

        /// <summary>
        /// Assert that a value is within a range
        /// </summary>
        protected void AssertInRange(float value, float min, float max, string message = "")
        {
            Assert.GreaterOrEqual(value, min, $"{message} - Value {value} should be >= {min}");
            Assert.LessOrEqual(value, max, $"{message} - Value {value} should be <= {max}");
        }

        /// <summary>
        /// Assert that two floats are approximately equal
        /// </summary>
        protected void AssertApproximatelyEqual(float expected, float actual, float tolerance = 0.001f, string message = "")
        {
            Assert.AreEqual(expected, actual, tolerance, message);
        }
    }
}
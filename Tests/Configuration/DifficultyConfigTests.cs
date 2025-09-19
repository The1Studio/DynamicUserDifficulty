using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;
using System.Linq;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Configuration
{
    [TestFixture]
    public class DifficultyConfigTests
    {
        private DifficultyConfig config;

        [SetUp]
        public void Setup()
        {
            this.config = DifficultyConfig.CreateDefault();
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
                Object.DestroyImmediate(this.config);
        }

        [Test]
        public void CreateDefault_ReturnsValidConfig()
        {
            // Assert
            Assert.IsNotNull(this.config);
            Assert.AreEqual(DifficultyConstants.MIN_DIFFICULTY, this.config.MinDifficulty);
            Assert.AreEqual(DifficultyConstants.MAX_DIFFICULTY, this.config.MaxDifficulty);
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.config.DefaultDifficulty);
            Assert.AreEqual(DifficultyConstants.DEFAULT_MAX_CHANGE_PER_SESSION, this.config.MaxChangePerSession);
        }

        [Test]
        public void CreateDefault_HasAllModifiers()
        {
            // Assert
            Assert.IsNotNull(this.config.ModifierConfigs);
            Assert.AreEqual(4, this.config.ModifierConfigs.Count);

            // Check each modifier type exists
            var modifierTypes = this.config.ModifierConfigs.Select(m => m.ModifierType).ToList();
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, modifierTypes);
        }

        [Test]
        public void GetModifierConfig_ReturnsCorrectConfig()
        {
            // Act
            var winStreakConfig  = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            var lossStreakConfig = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);

            // Assert
            Assert.IsNotNull(winStreakConfig);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, winStreakConfig.ModifierType);

            Assert.IsNotNull(lossStreakConfig);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, lossStreakConfig.ModifierType);
        }

        [Test]
        public void GetModifierConfig_NonExistent_ReturnsNull()
        {
            // Act
            var nonExistentConfig = this.config.GetModifierConfig("NonExistentModifier");

            // Assert
            Assert.IsNull(nonExistentConfig);
        }

        [Test]
        public void WinStreakConfig_HasCorrectParameters()
        {
            // Arrange
            var winConfig = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);

            // Assert
            Assert.AreEqual(
                DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD,
                winConfig.GetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD)
            );
            Assert.AreEqual(
                DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE,
                winConfig.GetParameter(DifficultyConstants.PARAM_STEP_SIZE)
            );
            Assert.AreEqual(
                DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS,
                winConfig.GetParameter(DifficultyConstants.PARAM_MAX_BONUS)
            );
        }

        [Test]
        public void LossStreakConfig_HasCorrectParameters()
        {
            // Arrange
            var lossConfig = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);

            // Assert
            Assert.AreEqual(
                DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD,
                lossConfig.GetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD)
            );
            Assert.AreEqual(
                DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE,
                lossConfig.GetParameter(DifficultyConstants.PARAM_STEP_SIZE)
            );
            Assert.AreEqual(
                DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION,
                lossConfig.GetParameter(DifficultyConstants.PARAM_MAX_REDUCTION)
            );
        }

        [Test]
        public void TimeDecayConfig_HasCorrectParameters()
        {
            // Arrange
            var timeConfig = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY);

            // Assert
            Assert.AreEqual(
                DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY,
                timeConfig.GetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY)
            );
            Assert.AreEqual(
                DifficultyConstants.TIME_DECAY_DEFAULT_MAX,
                timeConfig.GetParameter(DifficultyConstants.PARAM_MAX_DECAY)
            );
            Assert.AreEqual(
                DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS,
                timeConfig.GetParameter(DifficultyConstants.PARAM_GRACE_HOURS)
            );
        }

        [Test]
        public void RageQuitConfig_HasCorrectParameters()
        {
            // Arrange
            var rageConfig = this.config.GetModifierConfig(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT);

            // Assert
            Assert.AreEqual(
                DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD,
                rageConfig.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD)
            );
            Assert.AreEqual(
                DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION,
                rageConfig.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION)
            );
            Assert.AreEqual(
                DifficultyConstants.QUIT_DEFAULT_REDUCTION,
                rageConfig.GetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION)
            );
            Assert.AreEqual(
                DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION,
                rageConfig.GetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION)
            );
        }

        [Test]
        public void ModifierConfigs_AreEnabled()
        {
            // Assert - All modifiers should be enabled by default
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                Assert.IsTrue(modifierConfig.Enabled,
                    $"Modifier {modifierConfig.ModifierType} should be enabled by default");
            }
        }

        [Test]
        public void ModifierConfigs_HaveValidResponseCurves()
        {
            // Assert - All modifiers should have valid response curves
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                Assert.IsNotNull(modifierConfig.ResponseCurve,
                    $"Modifier {modifierConfig.ModifierType} should have a response curve");

                // Test curve evaluation
                float value = modifierConfig.ResponseCurve.Evaluate(0.5f);
                Assert.GreaterOrEqual(value, 0f);
                Assert.LessOrEqual(value, 1f);
            }
        }

        [Test]
        public void ModifierConfigs_HaveUniqueTypes()
        {
            // Get all modifier types
            var types = this.config.ModifierConfigs.Select(m => m.ModifierType).ToList();

            // Assert - No duplicate types
            Assert.AreEqual(types.Count, types.Distinct().Count(),
                "All modifier types should be unique");
        }
    }
}
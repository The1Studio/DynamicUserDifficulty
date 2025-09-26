using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;
using System.Linq;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Configuration
{
    [TestFixture]
    [Category("Unit")]
    [Category("Configuration")]
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
            Assert.AreEqual(2f, this.config.MaxChangePerSession); // Default max change per session
        }

        [Test]
        public void CreateDefault_HasAllModifiers()
        {
            // Assert
            Assert.IsNotNull(this.config.ModifierConfigs);
            Assert.AreEqual(7, this.config.ModifierConfigs.Count);

            // Check each modifier type exists
            var modifierTypes = this.config.ModifierConfigs.Select(m => m.ModifierType).ToList();
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_COMPLETION_RATE, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS, modifierTypes);
            Assert.Contains(DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN, modifierTypes);
        }

        [Test]
        public void GetModifierConfig_ReturnsCorrectConfig()
        {
            // Act
            var winStreakConfig  = this.config.GetModifierConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            var lossStreakConfig = this.config.GetModifierConfig<LossStreakConfig>(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);

            // Assert
            Assert.IsNotNull(winStreakConfig);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, winStreakConfig.ModifierType);

            Assert.IsNotNull(lossStreakConfig);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, lossStreakConfig.ModifierType);
        }

        [Test]
        public void GetModifierConfig_NonExistent_ReturnsNull()
        {
            // Act - Test with non-existent modifier type
            var nonExistentConfig = this.config.GetModifierConfig<IModifierConfig>("NonExistentModifier");

            // Assert
            Assert.IsNull(nonExistentConfig);
        }

        [Test]
        public void WinStreakConfig_HasCorrectParameters()
        {
            // Arrange
            var winConfig = this.config.GetModifierConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);

            // Assert
            Assert.IsNotNull(winConfig);
            Assert.AreEqual(
                3f, // Default win threshold
                winConfig.WinThreshold
            );
            Assert.AreEqual(
                0.5f, // Default step size
                winConfig.StepSize
            );
            Assert.AreEqual(
                2f, // Default max bonus
                winConfig.MaxBonus
            );
        }

        [Test]
        public void LossStreakConfig_HasCorrectParameters()
        {
            // Arrange
            var lossConfig = this.config.GetModifierConfig<LossStreakConfig>(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);

            // Assert
            Assert.IsNotNull(lossConfig);
            Assert.AreEqual(
                2f, // Default loss threshold
                lossConfig.LossThreshold
            );
            Assert.AreEqual(
                0.3f, // Default step size
                lossConfig.StepSize
            );
            Assert.AreEqual(
                1.5f, // Default max reduction
                lossConfig.MaxReduction
            );
        }

        [Test]
        public void TimeDecayConfig_HasCorrectParameters()
        {
            // Arrange
            var timeConfig = this.config.GetModifierConfig<TimeDecayConfig>(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY);

            // Assert
            Assert.IsNotNull(timeConfig);
            Assert.AreEqual(
                0.5f, // Default decay per day
                timeConfig.DecayPerDay
            );
            Assert.AreEqual(
                2f, // Default max decay
                timeConfig.MaxDecay
            );
            Assert.AreEqual(
                6f, // Default grace hours
                timeConfig.GraceHours
            );
        }

        [Test]
        public void RageQuitConfig_HasCorrectParameters()
        {
            // Arrange
            var rageConfig = this.config.GetModifierConfig<RageQuitConfig>(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT);

            // Assert
            Assert.IsNotNull(rageConfig);
            Assert.AreEqual(
                DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD,
                rageConfig.RageQuitThreshold
            );
            Assert.AreEqual(
                1f, // Default rage quit reduction
                rageConfig.RageQuitReduction
            );
            Assert.AreEqual(
                0.5f, // Default quit reduction
                rageConfig.QuitReduction
            );
            Assert.AreEqual(
                0.3f, // Default mid play reduction
                rageConfig.MidPlayReduction
            );
        }

        [Test]
        public void ModifierConfigs_AreEnabled()
        {
            // Assert - All modifiers should be enabled by default
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                Assert.IsTrue(modifierConfig.IsEnabled,
                    $"Modifier {modifierConfig.ModifierType} should be enabled by default");
            }
        }

        [Test]
        public void ModifierConfigs_HaveValidPriorities()
        {
            // Assert - All modifiers should have valid priorities
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                Assert.GreaterOrEqual(modifierConfig.Priority, 0,
                    $"Modifier {modifierConfig.ModifierType} should have a valid priority >= 0");
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
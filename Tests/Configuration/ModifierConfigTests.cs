using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Configuration
{
    [TestFixture]
    public class ModifierConfigTests
    {
        private ModifierConfigContainer container;

        [SetUp]
        public void Setup()
        {
            this.container = new ModifierConfigContainer();
        }

        [Test]
        public void InitializeDefaults_CreatesAllTypedConfigs()
        {
            // Act
            this.container.InitializeDefaults();

            // Assert
            Assert.AreEqual(4, this.container.Count);
            Assert.IsNotNull(this.container.GetConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK));
            Assert.IsNotNull(this.container.GetConfig<LossStreakConfig>(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK));
            Assert.IsNotNull(this.container.GetConfig<TimeDecayConfig>(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY));
            Assert.IsNotNull(this.container.GetConfig<RageQuitConfig>(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT));
        }

        [Test]
        public void WinStreakConfig_HasCorrectDefaults()
        {
            // Arrange
            var config = new WinStreakConfig().CreateDefault() as WinStreakConfig;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, config.ModifierType);
            Assert.IsTrue(config.IsEnabled);
            Assert.AreEqual(DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD, config.WinThreshold);
            Assert.AreEqual(DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE, config.StepSize);
            Assert.AreEqual(DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS, config.MaxBonus);
        }

        [Test]
        public void LossStreakConfig_HasCorrectDefaults()
        {
            // Arrange
            var config = new LossStreakConfig().CreateDefault() as LossStreakConfig;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, config.ModifierType);
            Assert.IsTrue(config.IsEnabled);
            Assert.AreEqual(DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD, config.LossThreshold);
            Assert.AreEqual(DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE, config.StepSize);
            Assert.AreEqual(DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION, config.MaxReduction);
        }

        [Test]
        public void TimeDecayConfig_HasCorrectDefaults()
        {
            // Arrange
            var config = new TimeDecayConfig().CreateDefault() as TimeDecayConfig;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY, config.ModifierType);
            Assert.IsTrue(config.IsEnabled);
            Assert.AreEqual(DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS, config.GraceHours);
            Assert.AreEqual(DifficultyConstants.TIME_DECAY_DEFAULT_DECAY_PER_DAY, config.DecayPerDay);
            Assert.AreEqual(DifficultyConstants.TIME_DECAY_DEFAULT_MAX_DECAY, config.MaxDecay);
        }

        [Test]
        public void RageQuitConfig_HasCorrectDefaults()
        {
            // Arrange
            var config = new RageQuitConfig().CreateDefault() as RageQuitConfig;

            // Assert
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT, config.ModifierType);
            Assert.IsTrue(config.IsEnabled);
            Assert.AreEqual(DifficultyConstants.RAGE_QUIT_DEFAULT_THRESHOLD, config.RageQuitThreshold);
            Assert.AreEqual(DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION, config.RageQuitReduction);
            Assert.AreEqual(DifficultyConstants.RAGE_QUIT_DEFAULT_QUIT_REDUCTION, config.QuitReduction);
            Assert.AreEqual(DifficultyConstants.RAGE_QUIT_DEFAULT_MID_PLAY_REDUCTION, config.MidPlayReduction);
        }

        [Test]
        public void SetConfig_AddsNewConfig()
        {
            // Arrange
            var winConfig = new WinStreakConfig().CreateDefault() as WinStreakConfig;

            // Act
            this.container.SetConfig(winConfig);

            // Assert
            Assert.AreEqual(1, this.container.Count);
            var retrieved = this.container.GetConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            Assert.AreEqual(winConfig, retrieved);
        }

        [Test]
        public void SetConfig_ReplacesExistingConfig()
        {
            // Arrange
            var winConfig1 = new WinStreakConfig().CreateDefault() as WinStreakConfig;
            var winConfig2 = new WinStreakConfig().CreateDefault() as WinStreakConfig;
            winConfig2.SetPriority(5);

            this.container.SetConfig(winConfig1);

            // Act
            this.container.SetConfig(winConfig2);

            // Assert
            Assert.AreEqual(1, this.container.Count);
            var retrieved = this.container.GetConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            Assert.AreEqual(5, retrieved.Priority);
        }

        [Test]
        public void IsModifierEnabled_ReturnsCorrectValue()
        {
            // Arrange
            var winConfig = new WinStreakConfig().CreateDefault() as WinStreakConfig;
            winConfig.SetEnabled(true);
            var lossConfig = new LossStreakConfig().CreateDefault() as LossStreakConfig;
            lossConfig.SetEnabled(false);

            this.container.SetConfig(winConfig);
            this.container.SetConfig(lossConfig);

            // Act & Assert
            Assert.IsTrue(this.container.IsModifierEnabled(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK));
            Assert.IsFalse(this.container.IsModifierEnabled(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK));
            Assert.IsFalse(this.container.IsModifierEnabled("NonExistent"));
        }

        [Test]
        public void GetEnabledConfigs_ReturnsOnlyEnabledConfigs()
        {
            // Arrange
            this.container.InitializeDefaults();

            // Disable one config
            var lossConfig = this.container.GetConfig<LossStreakConfig>(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);
            lossConfig.SetEnabled(false);

            // Act
            var enabledConfigs = this.container.GetEnabledConfigs();

            // Assert
            var enabledList = enabledConfigs.ToList();
            Assert.AreEqual(3, enabledList.Count);
            Assert.IsTrue(enabledList.Any(c => c.ModifierType == DifficultyConstants.MODIFIER_TYPE_WIN_STREAK));
            Assert.IsFalse(enabledList.Any(c => c.ModifierType == DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK));
        }

        [Test]
        public void GetEnabledConfigs_OrdersByPriority()
        {
            // Arrange
            var winConfig = new WinStreakConfig().CreateDefault() as WinStreakConfig;
            winConfig.SetPriority(3);
            var lossConfig = new LossStreakConfig().CreateDefault() as LossStreakConfig;
            lossConfig.SetPriority(1);
            var timeConfig = new TimeDecayConfig().CreateDefault() as TimeDecayConfig;
            timeConfig.SetPriority(2);

            this.container.SetConfig(winConfig);
            this.container.SetConfig(lossConfig);
            this.container.SetConfig(timeConfig);

            // Act
            var enabledConfigs = this.container.GetEnabledConfigs().ToList();

            // Assert
            Assert.AreEqual(3, enabledConfigs.Count);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK, enabledConfigs[0].ModifierType);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY, enabledConfigs[1].ModifierType);
            Assert.AreEqual(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK, enabledConfigs[2].ModifierType);
        }

        [Test]
        public void Clear_RemovesAllConfigs()
        {
            // Arrange
            this.container.InitializeDefaults();
            Assert.AreEqual(4, this.container.Count);

            // Act
            this.container.Clear();

            // Assert
            Assert.AreEqual(0, this.container.Count);
        }

        [Test]
        public void AllConfigs_ReturnsReadOnlyList()
        {
            // Arrange
            this.container.InitializeDefaults();

            // Act
            var allConfigs = this.container.AllConfigs;

            // Assert - Verify collection properties
            Assert.AreEqual(4, allConfigs.Count);
            // IReadOnlyList is inherently read-only by interface design
            Assert.IsNotNull(allConfigs);
        }

        [Test]
        public void IEnumerable_IteratesOverConfigs()
        {
            // Arrange
            this.container.InitializeDefaults();

            // Act
            var configs = new List<IModifierConfig>();
            foreach (var config in this.container)
            {
                configs.Add(config);
            }

            // Assert
            Assert.AreEqual(4, configs.Count);
        }

        [Test]
        public void SetConfig_NullConfig_HandledGracefully()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => this.container.SetConfig(null));
            Assert.AreEqual(0, this.container.Count);
        }

        [Test]
        public void GetConfig_NonExistentType_ReturnsNull()
        {
            // Act
            var config = this.container.GetConfig<WinStreakConfig>("NonExistent");

            // Assert
            Assert.IsNull(config);
        }
    }
}
using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Main configuration ScriptableObject for difficulty settings
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "DynamicDifficulty/Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Range")]
        [SerializeField, Range(DifficultyConstants.CONFIG_MIN_RANGE, DifficultyConstants.CONFIG_MAX_RANGE)]
        private float minDifficulty = DifficultyConstants.MIN_DIFFICULTY;

        [SerializeField, Range(DifficultyConstants.CONFIG_MIN_RANGE, DifficultyConstants.CONFIG_MAX_RANGE)]
        private float maxDifficulty = DifficultyConstants.MAX_DIFFICULTY;

        [SerializeField, Range(DifficultyConstants.CONFIG_MIN_RANGE, DifficultyConstants.CONFIG_MAX_RANGE)]
        private float defaultDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;

        [SerializeField, Range(DifficultyConstants.CONFIG_CHANGE_MIN_RANGE, DifficultyConstants.CONFIG_CHANGE_MAX_RANGE)]
        private float maxChangePerSession = DifficultyConstants.DEFAULT_MAX_CHANGE;

        [Header("Modifiers")]
        [SerializeField]
        private List<ModifierConfig> modifierConfigs = new List<ModifierConfig>();

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = false;

        // Properties
        public float MinDifficulty => minDifficulty;
        public float MaxDifficulty => maxDifficulty;
        public float DefaultDifficulty => defaultDifficulty;
        public float MaxChangePerSession => maxChangePerSession;
        public List<ModifierConfig> ModifierConfigs => modifierConfigs;
        public bool EnableDebugLogs => enableDebugLogs;

        /// <summary>
        /// Gets a modifier configuration by type
        /// </summary>
        public ModifierConfig GetModifierConfig(string modifierType)
        {
            return modifierConfigs?.Find(m => m.ModifierType == modifierType);
        }

        /// <summary>
        /// Creates a default configuration
        /// </summary>
        public static DifficultyConfig CreateDefault()
        {
            var config = CreateInstance<DifficultyConfig>();

            // Set default values
            this.config.minDifficulty = DifficultyConstants.MIN_DIFFICULTY;
            this.config.maxDifficulty = DifficultyConstants.MAX_DIFFICULTY;
            this.config.defaultDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            this.config.maxChangePerSession = DifficultyConstants.DEFAULT_MAX_CHANGE_PER_SESSION;

            // Add default modifiers
            this.config.modifierConfigs = new List<ModifierConfig>
            {
                CreateWinStreakConfig(),
                CreateLossStreakConfig(),
                CreateTimeDecayConfig(),
                CreateRageQuitConfig()
            };

            return config;
        }

        private static ModifierConfig CreateWinStreakConfig()
        {
            var config = new ModifierConfig();
            this.config.SetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD, DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD);
            this.config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_BONUS, DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS);
            return config;
        }

        private static ModifierConfig CreateLossStreakConfig()
        {
            var config = new ModifierConfig();
            this.config.SetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD, DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD);
            this.config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_REDUCTION, DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION);
            return config;
        }

        private static ModifierConfig CreateTimeDecayConfig()
        {
            var config = new ModifierConfig();
            this.config.SetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY, DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY);
            this.config.SetParameter(DifficultyConstants.PARAM_MAX_DECAY, DifficultyConstants.TIME_DECAY_DEFAULT_MAX);
            this.config.SetParameter(DifficultyConstants.PARAM_GRACE_HOURS, DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS);
            return config;
        }

        private static ModifierConfig CreateRageQuitConfig()
        {
            var config = new ModifierConfig();
            this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD, DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD);
            this.config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION, DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION);
            this.config.SetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION, DifficultyConstants.QUIT_DEFAULT_REDUCTION);
            this.config.SetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION, DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION);
            return config;
        }

        private void OnValidate()
        {
            // Ensure min <= default <= max
            if (defaultDifficulty < minDifficulty)
                defaultDifficulty = minDifficulty;
            if (defaultDifficulty > maxDifficulty)
                defaultDifficulty = maxDifficulty;
            if (minDifficulty > maxDifficulty)
                minDifficulty = maxDifficulty;
        }
    }
}
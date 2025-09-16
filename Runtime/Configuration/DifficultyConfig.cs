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
        [SerializeField, Range(1f, 10f)]
        private float minDifficulty = DifficultyConstants.MIN_DIFFICULTY;

        [SerializeField, Range(1f, 10f)]
        private float maxDifficulty = DifficultyConstants.MAX_DIFFICULTY;

        [SerializeField, Range(1f, 10f)]
        private float defaultDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;

        [SerializeField, Range(0.5f, 5f)]
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
            config.minDifficulty = 1f;
            config.maxDifficulty = 10f;
            config.defaultDifficulty = 3f;
            config.maxChangePerSession = 2f;

            // Add default modifiers
            config.modifierConfigs = new List<ModifierConfig>
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
            config.SetParameter("WinThreshold", 3f);
            config.SetParameter("StepSize", 0.5f);
            config.SetParameter("MaxBonus", 2f);
            return config;
        }

        private static ModifierConfig CreateLossStreakConfig()
        {
            var config = new ModifierConfig();
            config.SetParameter("LossThreshold", 2f);
            config.SetParameter("StepSize", 0.3f);
            config.SetParameter("MaxReduction", 1.5f);
            return config;
        }

        private static ModifierConfig CreateTimeDecayConfig()
        {
            var config = new ModifierConfig();
            config.SetParameter("DecayPerDay", 0.5f);
            config.SetParameter("MaxDecay", 2f);
            config.SetParameter("GraceHours", 6f);
            return config;
        }

        private static ModifierConfig CreateRageQuitConfig()
        {
            var config = new ModifierConfig();
            config.SetParameter("RageQuitThreshold", 30f);
            config.SetParameter("RageQuitReduction", 1f);
            config.SetParameter("QuitReduction", 0.5f);
            config.SetParameter("MidPlayReduction", 0.3f);
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
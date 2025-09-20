using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Main configuration ScriptableObject for difficulty settings
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = DifficultyConstants.MENU_CREATE_ASSET)]
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
        private ModifierConfigContainer modifierConfigs = new ModifierConfigContainer();

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = false;

        // Properties
        public float                MinDifficulty       => this.minDifficulty;
        public float                MaxDifficulty       => this.maxDifficulty;
        public float                DefaultDifficulty   => this.defaultDifficulty;
        public float                MaxChangePerSession => this.maxChangePerSession;
        public ModifierConfigContainer ModifierConfigs => this.modifierConfigs;
        public bool                 EnableDebugLogs     => this.enableDebugLogs;

        /// <summary>
        /// Gets a modifier configuration by type
        /// </summary>
        public IModifierConfig GetModifierConfig(string modifierType)
        {
            return this.modifierConfigs?.GetConfig(modifierType);
        }

        /// <summary>
        /// Gets a strongly-typed modifier configuration
        /// </summary>
        public T GetModifierConfig<T>(string modifierType) where T : class, IModifierConfig
        {
            return this.modifierConfigs?.GetConfig<T>(modifierType);
        }

        /// <summary>
        /// Creates a default configuration
        /// </summary>
        public static DifficultyConfig CreateDefault()
        {
            var config = CreateInstance<DifficultyConfig>();

            // Set default values
            config.minDifficulty = DifficultyConstants.MIN_DIFFICULTY;
            config.maxDifficulty = DifficultyConstants.MAX_DIFFICULTY;
            config.defaultDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            config.maxChangePerSession = DifficultyConstants.DEFAULT_MAX_CHANGE_PER_SESSION;

            // Add default modifiers
            config.modifierConfigs = new ModifierConfigContainer();
            config.modifierConfigs.InitializeDefaults();

            return config;
        }

        // Removed static factory methods - now handled by ModifierConfigContainer.InitializeDefaults()

        private void OnValidate()
        {
            // Ensure min <= default <= max
            if (this.defaultDifficulty < this.minDifficulty) this.defaultDifficulty = this.minDifficulty;
            if (this.defaultDifficulty > this.maxDifficulty) this.defaultDifficulty = this.maxDifficulty;
            if (this.minDifficulty > this.maxDifficulty) this.minDifficulty         = this.maxDifficulty;
        }
    }
}
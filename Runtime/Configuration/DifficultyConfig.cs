using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Main configuration ScriptableObject for difficulty settings.
    /// Contains all configurable values for the difficulty system.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = DifficultyConstants.MENU_CREATE_ASSET)]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Range")]
        [SerializeField][Range(1f, 10f)]
        private float minDifficulty = 1f;

        [SerializeField][Range(1f, 10f)]
        private float maxDifficulty = 10f;

        [SerializeField][Range(1f, 10f)]
        private float defaultDifficulty = 5f;

        [SerializeField][Range(0.5f, 5f)]
        private float maxChangePerSession = 2f;

        [Header("Precision & Thresholds")]
        [SerializeField][Tooltip("Minimum change in difficulty to be considered significant")]
        private float difficultyChangeThreshold = 0.01f;

        [Header("Caching")]
        [SerializeField][Tooltip("Cache expiry time in minutes")]
        private int cacheExpiryMinutes = 5;

        [Header("Modifiers")]
        [SerializeField]
        private ModifierConfigContainer modifierConfigs = new();

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = false;

        // Properties
        public float MinDifficulty => this.minDifficulty;
        public float MaxDifficulty => this.maxDifficulty;
        public float DefaultDifficulty => this.defaultDifficulty;
        public float MaxChangePerSession => this.maxChangePerSession;

        // Precision
        public float DifficultyChangeThreshold => this.difficultyChangeThreshold;

        // Caching
        public int CacheExpiryMinutes => this.cacheExpiryMinutes;

        // Other
        public ModifierConfigContainer ModifierConfigs => this.modifierConfigs;
        public bool EnableDebugLogs => this.enableDebugLogs;

        /// <summary>
        /// Gets a strongly-typed modifier configuration
        /// </summary>
        public T GetModifierConfig<T>(string modifierType) where T : class, IModifierConfig
        {
            return this.modifierConfigs?.GetConfig<T>(modifierType);
        }

        /// <summary>
        /// Creates a default configuration with standard values
        /// </summary>
        public static DifficultyConfig CreateDefault()
        {
            var config = CreateInstance<DifficultyConfig>();

            // Set default values
            config.minDifficulty = 1f;
            config.maxDifficulty = 10f;
            config.defaultDifficulty = 5f;
            config.maxChangePerSession = 2f;

            // Other defaults
            config.difficultyChangeThreshold = 0.01f;
            config.cacheExpiryMinutes = 5;

            // Add default modifiers
            config.modifierConfigs = new();
            config.modifierConfigs.InitializeDefaults();

            return config;
        }

        private void OnValidate()
        {
            // Ensure min <= default <= max
            if (this.defaultDifficulty < this.minDifficulty) this.defaultDifficulty = this.minDifficulty;
            if (this.defaultDifficulty > this.maxDifficulty) this.defaultDifficulty = this.maxDifficulty;
            if (this.minDifficulty > this.maxDifficulty) this.minDifficulty = this.maxDifficulty;

            // Ensure positive values
            if (this.cacheExpiryMinutes < 1) this.cacheExpiryMinutes = 1;
        }
    }
}
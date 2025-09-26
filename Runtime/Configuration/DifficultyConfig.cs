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

        [Header("Session Management")]
        [SerializeField][Tooltip("Minimum session duration in seconds to be considered valid")]
        private float minSessionDuration = 120f; // 2 minutes

        [SerializeField][Tooltip("Threshold for very short sessions in seconds")]
        private float shortSessionThreshold = 10f;

        [SerializeField][Tooltip("Threshold for normal sessions in seconds")]
        private float normalSessionThreshold = 1800f; // 30 minutes

        [SerializeField][Tooltip("Default session duration fallback in seconds")]
        private float defaultSessionDuration = 60f;

        [SerializeField][Tooltip("Default level completion time fallback in seconds")]
        private float defaultCompletionTime = 60f;

        [SerializeField][Tooltip("Maximum recent sessions to analyze")]
        private int maxRecentSessions = 10;

        [SerializeField][Tooltip("Minimum sessions needed for trend analysis")]
        private int minSessionsForTrend = 3;

        [Header("Rage Quit Detection")]
        [SerializeField][Tooltip("Time threshold in seconds for detecting rage quit")]
        private float rageQuitTimeThreshold = 30f;

        [SerializeField][Tooltip("Difficulty reduction for rage quit")]
        private float rageQuitReduction = 1f;

        [SerializeField][Tooltip("Difficulty reduction for normal quit")]
        private float normalQuitReduction = 0.5f;

        [SerializeField][Tooltip("Difficulty reduction for mid-play quit")]
        private float midPlayQuitReduction = 0.3f;

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

        // Session Management
        public float MinSessionDuration => this.minSessionDuration;
        public float ShortSessionThreshold => this.shortSessionThreshold;
        public float NormalSessionThreshold => this.normalSessionThreshold;
        public float DefaultSessionDuration => this.defaultSessionDuration;
        public float DefaultCompletionTime => this.defaultCompletionTime;
        public int MaxRecentSessions => this.maxRecentSessions;
        public int MinSessionsForTrend => this.minSessionsForTrend;

        // Rage Quit
        public float RageQuitTimeThreshold => this.rageQuitTimeThreshold;
        public float RageQuitReduction => this.rageQuitReduction;
        public float NormalQuitReduction => this.normalQuitReduction;
        public float MidPlayQuitReduction => this.midPlayQuitReduction;

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

            // Session defaults
            config.minSessionDuration = 120f;
            config.shortSessionThreshold = 10f;
            config.normalSessionThreshold = 1800f;
            config.defaultSessionDuration = 60f;
            config.defaultCompletionTime = 60f;
            config.maxRecentSessions = 10;
            config.minSessionsForTrend = 3;

            // Rage quit defaults
            config.rageQuitTimeThreshold = 30f;
            config.rageQuitReduction = 1f;
            config.normalQuitReduction = 0.5f;
            config.midPlayQuitReduction = 0.3f;

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
            if (this.minSessionDuration < 0) this.minSessionDuration = 0;
            if (this.shortSessionThreshold < 0) this.shortSessionThreshold = 0;
            if (this.normalSessionThreshold < 0) this.normalSessionThreshold = 0;
            if (this.defaultSessionDuration < 0) this.defaultSessionDuration = 0;
            if (this.defaultCompletionTime < 0) this.defaultCompletionTime = 0;
            if (this.maxRecentSessions < 1) this.maxRecentSessions = 1;
            if (this.minSessionsForTrend < 1) this.minSessionsForTrend = 1;
            if (this.cacheExpiryMinutes < 1) this.cacheExpiryMinutes = 1;
        }
    }
}
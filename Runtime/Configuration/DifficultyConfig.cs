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
        private float defaultDifficulty = 3f;

        [SerializeField][Range(0.5f, 5f)]
        private float maxChangePerSession = 2f;

        [Header("Modifiers")]
        [SerializeField]
        private ModifierConfigContainer modifierConfigs = new();

        [Header("Aggregation Settings")]
        [SerializeField][Range(0.3f, 0.8f)]
        [Tooltip("Diminishing returns factor: higher value = more weight to secondary signals (0.6 = 60% weight to each subsequent modifier)")]
        private float diminishingFactor = 0.6f;

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogs = false;

        [Header("Configuration Generation")]
        [SerializeField]
        [Tooltip("Game statistics used to generate optimal modifier configurations. Fill this once based on your player data.")]
        private GameStats gameStats = GameStats.CreateDefault();

        // Properties
        public float MinDifficulty => this.minDifficulty;
        public float MaxDifficulty => this.maxDifficulty;
        public float DefaultDifficulty => this.defaultDifficulty;
        public float MaxChangePerSession => this.maxChangePerSession;
        public float DiminishingFactor => this.diminishingFactor;

        // Other
        public ModifierConfigContainer ModifierConfigs => this.modifierConfigs;
        public bool EnableDebugLogs => this.enableDebugLogs;
        public GameStats GameStats => this.gameStats;

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

            // Set default values using constants
            config.minDifficulty = DifficultyConstants.MIN_DIFFICULTY;
            config.maxDifficulty = DifficultyConstants.MAX_DIFFICULTY;
            config.defaultDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            config.maxChangePerSession = 2f;

            // Add default modifiers
            config.modifierConfigs = new();
            config.modifierConfigs.InitializeDefaults();

            return config;
        }

        /// <summary>
        /// Generates all modifier configurations from the current game stats.
        /// Pure data transformation method with no editor dependencies.
        /// Returns true if generation was successful, false otherwise.
        /// </summary>
        public bool GenerateAllConfigsFromStats()
        {
            // Validate game stats first
            if (!this.gameStats.Validate(out string errorMessage))
            {
                Debug.LogError($"[DifficultyConfig] Invalid game stats: {errorMessage}");
                return false;
            }

            // Check for null configs container (fail fast)
            if (this.modifierConfigs == null)
            {
                Debug.LogError("[DifficultyConfig] ModifierConfigs is null. " +
                              "Create a new DifficultyConfig asset or call InitializeDefaults().");
                return false;
            }

            // Check if configs list is empty
            if (this.modifierConfigs.Count == 0)
            {
                Debug.LogWarning("[DifficultyConfig] No modifier configs found. Initializing defaults...");
                this.modifierConfigs.InitializeDefaults();
            }

            // Update difficulty range from game stats
            this.minDifficulty = this.gameStats.difficultyMin;
            this.maxDifficulty = this.gameStats.difficultyMax;
            this.defaultDifficulty = this.gameStats.difficultyDefault;
            this.maxChangePerSession = this.gameStats.maxDifficultyChangePerSession;

            // Generate all modifier configs with post-generation validation (H2 Fix)
            var successCount = 0;
            var validationErrors = new System.Collections.Generic.List<string>();

            foreach (var config in this.modifierConfigs.AllConfigs)
            {
                if (config != null)
                {
                    config.GenerateFromStats(this.gameStats);

                    // Validate generated config values
                    if (!this.ValidateGeneratedConfig(config, out var validationError))
                    {
                        validationErrors.Add($"{config.ModifierType}: {validationError}");
                        Debug.LogError($"[DifficultyConfig] Generated invalid config for {config.ModifierType}: {validationError}");
                    }
                    else
                    {
                        Debug.Log($"[DifficultyConfig] Generated config for {config.ModifierType}");
                        successCount++;
                    }
                }
            }

            if (validationErrors.Count > 0)
            {
                Debug.LogError($"[DifficultyConfig] Config generation completed with {validationErrors.Count} validation errors:\n" +
                             string.Join("\n", validationErrors));
                return false;
            }

            Debug.Log($"[DifficultyConfig] ✓ Generated {successCount}/{this.modifierConfigs.Count} configs successfully");
            return true;
        }

        /// <summary>
        /// Validates generated config values to ensure they're within acceptable ranges.
        /// </summary>
        private bool ValidateGeneratedConfig(IModifierConfig config, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Use reflection to check for common invalid values
            var properties = config.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(float))
                {
                    var value = (float)prop.GetValue(config);

                    // Check for invalid float values
                    if (float.IsNaN(value))
                    {
                        errorMessage = $"{prop.Name} is NaN";
                        return false;
                    }

                    if (float.IsInfinity(value))
                    {
                        errorMessage = $"{prop.Name} is Infinity";
                        return false;
                    }

                    // Check for negative values where they shouldn't exist (common pattern)
                    if (prop.Name.Contains("Threshold") || prop.Name.Contains("Size") || prop.Name.Contains("Bonus") || prop.Name.Contains("Max"))
                    {
                        if (value < 0)
                        {
                            errorMessage = $"{prop.Name} is negative ({value})";
                            return false;
                        }
                    }
                }
                else if (prop.PropertyType == typeof(int))
                {
                    var value = (int)prop.GetValue(config);

                    // Check for negative values in thresholds/counts
                    if (prop.Name.Contains("Threshold") || prop.Name.Contains("Count") || prop.Name.Contains("Max"))
                    {
                        if (value < 0)
                        {
                            errorMessage = $"{prop.Name} is negative ({value})";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void OnValidate()
        {
            // Ensure min <= default <= max
            if (this.defaultDifficulty < this.minDifficulty) this.defaultDifficulty = this.minDifficulty;
            if (this.defaultDifficulty > this.maxDifficulty) this.defaultDifficulty = this.maxDifficulty;
            if (this.minDifficulty > this.maxDifficulty) this.minDifficulty = this.maxDifficulty;
        }
    }
}
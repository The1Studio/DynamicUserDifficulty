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

            // Generate all modifier configs
            int successCount = 0;
            foreach (var config in this.modifierConfigs.AllConfigs)
            {
                if (config != null)
                {
                    config.GenerateFromStats(this.gameStats);
                    Debug.Log($"[DifficultyConfig] Generated config for {config.ModifierType}");
                    successCount++;
                }
            }

            Debug.Log($"[DifficultyConfig] ✓ Generated {successCount}/{this.modifierConfigs.Count} configs successfully");
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
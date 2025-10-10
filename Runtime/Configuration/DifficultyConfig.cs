using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    using Sirenix.OdinInspector;
    using Sirenix.Serialization;
#if UNITY_EDITOR
    using UnityEditor;
#endif

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
        [InfoBox("Fill the Game Statistics fields below, then click 'Generate All Configs' to automatically calculate optimal modifier configurations.")]
        [SerializeField]
        [Tooltip("Game statistics used to generate optimal modifier configurations. Fill this once based on your player data.")]
        private GameStats gameStats = GameStats.CreateDefault();

        [Button("Preview Generated Values", ButtonSizes.Large)]
        [GUIColor(0.5f, 1f, 1f)]
        private void PreviewGeneratedValues()
        {
#if UNITY_EDITOR
            if (!this.gameStats.Validate(out string errorMessage))
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Invalid Game Stats",
                    $"Cannot preview configs: {errorMessage}\n\nPlease correct the Game Statistics fields and try again.",
                    "OK"
                );
                return;
            }

            var gameStats = this.gameStats;
            string preview = "📊 Preview of Generated Config Values:\n\n";

            preview += "=== Difficulty Range ===\n";
            preview += $"Min Difficulty: {gameStats.difficultyMin:F1}\n";
            preview += $"Max Difficulty: {gameStats.difficultyMax:F1}\n";
            preview += $"Default Difficulty: {gameStats.difficultyDefault:F1}\n";
            preview += $"Max Change/Session: {gameStats.maxDifficultyChangePerSession:F1}\n\n";

            preview += "=== Win Streak Modifier ===\n";
            float winThreshold = Mathf.Max(2f, Mathf.Round(gameStats.avgConsecutiveWins * 0.75f));
            float diffRange = gameStats.difficultyMax - gameStats.difficultyMin;
            float winStepSize = Mathf.Clamp(diffRange / (gameStats.avgConsecutiveWins * 2f), 0.1f, 2f);
            float winMaxBonus = Mathf.Clamp(diffRange * 0.3f, 0.5f, 5f);
            preview += $"Win Threshold: {winThreshold:F1}\n";
            preview += $"Step Size: {winStepSize:F2}\n";
            preview += $"Max Bonus: {winMaxBonus:F2}\n\n";

            preview += "...and 6 more modifiers\n\n";
            preview += "Click 'Generate All Configs from Stats' to apply these values.";

            UnityEditor.EditorUtility.DisplayDialog("Config Preview", preview, "OK");
#endif
        }

        [Button("✨ Generate All Configs from Stats", ButtonSizes.Large)]
        [GUIColor(0.5f, 1f, 0.5f)]
        private void GenerateAllConfigsButton()
        {
#if UNITY_EDITOR
            if (!this.gameStats.Validate(out string errorMessage))
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Invalid Game Stats",
                    $"Cannot generate configs: {errorMessage}\n\nPlease correct the Game Statistics fields and try again.",
                    "OK"
                );
                return;
            }

            bool confirmed = UnityEditor.EditorUtility.DisplayDialog(
                "Generate All Configs",
                "This will overwrite all current modifier configurations with values calculated from your Game Statistics.\n\n" +
                "Are you sure you want to continue?\n\n" +
                "(You can undo this action with Ctrl+Z)",
                "Yes, Generate",
                "Cancel"
            );

            if (!confirmed) return;

            UnityEditor.Undo.RecordObject(this, "Generate Configs from Stats");
            GenerateAllConfigsFromStats();
            UnityEditor.EditorUtility.SetDirty(this);

            UnityEditor.EditorUtility.DisplayDialog(
                "Success",
                "✓ All modifier configurations have been generated successfully from your Game Statistics!\n\n" +
                "Check the Console for details about each generated config.\n\n" +
                "You can now manually fine-tune individual configs if needed.",
                "OK"
            );
#endif
        }

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
        /// This method is called by the custom editor when the "Generate All Configs" button is clicked.
        /// </summary>
        public void GenerateAllConfigsFromStats()
        {
#if UNITY_EDITOR
            // Validate game stats first
            if (!this.gameStats.Validate(out string errorMessage))
            {
                Debug.LogError($"[DifficultyConfig] Invalid game stats: {errorMessage}");
                return;
            }

            // Update difficulty range from game stats
            this.minDifficulty = this.gameStats.difficultyMin;
            this.maxDifficulty = this.gameStats.difficultyMax;
            this.defaultDifficulty = this.gameStats.difficultyDefault;
            this.maxChangePerSession = this.gameStats.maxDifficultyChangePerSession;

            // Generate all modifier configs
            if (this.modifierConfigs != null)
            {
                foreach (var config in this.modifierConfigs.AllConfigs)
                {
                    if (config != null)
                    {
                        config.GenerateFromStats(this.gameStats);
                        Debug.Log($"[DifficultyConfig] Generated config for {config.ModifierType}");
                    }
                }

                Debug.Log("[DifficultyConfig] ✓ All configs generated successfully from game stats");
            }
            else
            {
                Debug.LogError("[DifficultyConfig] ModifierConfigs is null - cannot generate configs");
            }
#else
            Debug.LogWarning("[DifficultyConfig] GenerateAllConfigsFromStats() can only be called in editor");
#endif
        }

        /// <summary>
        /// Updates the game stats (for editor use)
        /// </summary>
        public void UpdateGameStats(GameStats newStats)
        {
#if UNITY_EDITOR
            this.gameStats = newStats;
#endif
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
#nullable enable

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using Sirenix.OdinInspector.Editor;

namespace TheOneStudio.DynamicUserDifficulty.Editor.Configuration
{
    /// <summary>
    /// Custom editor for DifficultyConfig with automatic configuration generation from game stats.
    /// Compatible with Odin Inspector.
    /// </summary>
    [CustomEditor(typeof(DifficultyConfig))]
    public sealed class DifficultyConfigEditor : OdinEditor
    {
        private bool showGameStatsHelp = false;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            // Get the target config
            DifficultyConfig config = (DifficultyConfig)this.target;

            // Draw Odin inspector (this draws all fields including GameStats)
            base.OnInspectorGUI();

            // Add space before custom buttons
            EditorGUILayout.Space(10);

            // Draw help box with instructions
            EditorGUILayout.HelpBox(
                "Fill the Game Statistics fields above based on your player data, then click 'Generate All Configs from Stats' to automatically calculate optimal modifier configurations.",
                MessageType.Info
            );

            // Optional: Show/Hide detailed help
            this.showGameStatsHelp = EditorGUILayout.Foldout(this.showGameStatsHelp, "📖 Game Stats Help");
            if (this.showGameStatsHelp)
            {
                EditorGUILayout.HelpBox(
                    "How to fill Game Statistics:\n\n" +
                    "• Player Behavior: Analyze your game analytics for average streaks and win rates\n" +
                    "• Session & Time: Track average session duration and time between sessions\n" +
                    "• Level Design: Use your game's existing difficulty range settings\n" +
                    "• Progression: Consider your game's total levels and retention goals\n\n" +
                    "The generation formulas will calculate appropriate thresholds, step sizes, and limits based on these values.",
                    MessageType.None
                );
            }

            EditorGUILayout.Space(5);

            // Preview button (optional feature)
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("🔍 Preview Generated Values", GUILayout.Height(35)))
            {
                PreviewGeneratedConfigs(config);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            // Main generation button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("✨ Generate All Configs from Stats", GUILayout.Height(40)))
            {
                GenerateConfigsWithConfirmation(config);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Warning about manual edits
            EditorGUILayout.HelpBox(
                "⚠️ Generated configs can be manually adjusted afterward. Changes will be saved with the ScriptableObject.",
                MessageType.Warning
            );
        }

        private void PreviewGeneratedConfigs(DifficultyConfig config)
        {
            // Validate game stats
            if (!config.GameStats.Validate(out string errorMessage))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Game Stats",
                    $"Cannot preview configs: {errorMessage}\n\nPlease correct the Game Statistics fields and try again.",
                    "OK"
                );
                return;
            }

            // Generate temporary configs to preview values
            var gameStats = config.GameStats;
            var winConfig = new WinStreakConfig();
            winConfig.GenerateFromStats(gameStats);

            var lossConfig = new LossStreakConfig();
            lossConfig.GenerateFromStats(gameStats);

            var timeConfig = new TimeDecayConfig();
            timeConfig.GenerateFromStats(gameStats);

            // Create preview message using actual generated values
            string preview = "📊 Preview of Generated Config Values:\n\n";

            preview += "=== Difficulty Range ===\n";
            preview += $"Min Difficulty: {gameStats.difficultyMin:F1}\n";
            preview += $"Max Difficulty: {gameStats.difficultyMax:F1}\n";
            preview += $"Default Difficulty: {gameStats.difficultyDefault:F1}\n";
            preview += $"Max Change/Session: {gameStats.maxDifficultyChangePerSession:F1}\n\n";

            preview += "=== Win Streak Modifier ===\n";
            preview += $"Win Threshold: {winConfig.WinThreshold:F1}\n";
            preview += $"Step Size: {winConfig.StepSize:F2}\n";
            preview += $"Max Bonus: {winConfig.MaxBonus:F2}\n\n";

            preview += "=== Loss Streak Modifier ===\n";
            preview += $"Loss Threshold: {lossConfig.LossThreshold:F1}\n";
            preview += $"Step Size: {lossConfig.StepSize:F2}\n";
            preview += $"Max Reduction: {lossConfig.MaxReduction:F2}\n\n";

            preview += "=== Time Decay Modifier ===\n";
            preview += $"Decay/Day: {timeConfig.DecayPerDay:F2}\n";
            preview += $"Max Decay: {timeConfig.MaxDecay:F2}\n";
            preview += $"Grace Hours: {timeConfig.GraceHours:F1}\n\n";

            preview += "...and 4 more modifiers\n\n";
            preview += "Click 'Generate All Configs' to apply these values.";

            EditorUtility.DisplayDialog(
                "Config Preview",
                preview,
                "OK"
            );
        }

        private void GenerateConfigsWithConfirmation(DifficultyConfig config)
        {
            // Validate game stats first
            if (!config.GameStats.Validate(out string errorMessage))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Game Stats",
                    $"Cannot generate configs: {errorMessage}\n\nPlease correct the Game Statistics fields and try again.",
                    "OK"
                );
                return;
            }

            // Show confirmation dialog
            bool confirmed = EditorUtility.DisplayDialog(
                "Generate All Configs",
                "This will overwrite all current modifier configurations with values calculated from your Game Statistics.\n\n" +
                "Are you sure you want to continue?\n\n" +
                "(You can undo this action with Ctrl+Z)",
                "Yes, Generate",
                "Cancel"
            );

            if (!confirmed)
            {
                return;
            }

            // Record undo for the entire config
            Undo.RecordObject(config, "Generate Configs from Stats");

            // Generate all configs (pure data transformation in runtime)
            bool success = config.GenerateAllConfigsFromStats();

            // Mark as dirty to ensure Unity saves the changes
            if (success)
            {
                EditorUtility.SetDirty(config);

                // Show success dialog
                EditorUtility.DisplayDialog(
                    "Success",
                    "✓ All modifier configurations have been generated successfully from your Game Statistics!\n\n" +
                    "Check the Console for details about each generated config.\n\n" +
                    "You can now manually fine-tune individual configs if needed.",
                    "OK"
                );

                // Log summary
                Debug.Log("[DifficultyConfigEditor] Config Generation Complete\n" +
                         $"  • Difficulty Range: {config.MinDifficulty:F1} - {config.MaxDifficulty:F1}\n" +
                         $"  • Default Difficulty: {config.DefaultDifficulty:F1}\n" +
                         $"  • Max Change/Session: {config.MaxChangePerSession:F1}\n" +
                         $"  • All 7 modifier configs updated");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Generation Failed",
                    "Failed to generate configurations. Check the Console for error details.",
                    "OK"
                );
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using Sirenix.OdinInspector.Editor;

namespace TheOneStudio.DynamicUserDifficulty.Editor.Configuration
{
    /// <summary>
    /// Custom editor for DifficultyConfig with automatic configuration generation from game stats.
    /// Compatible with Odin Inspector.
    /// </summary>
    public class DifficultyConfigEditor : OdinEditor
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

            // Create preview message
            var gameStats = config.GameStats;
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

            preview += "=== Loss Streak Modifier ===\n";
            float lossThreshold = Mathf.Max(2f, Mathf.Round(gameStats.avgConsecutiveLosses * 0.8f));
            float lossStepSize = Mathf.Clamp(diffRange / (gameStats.avgConsecutiveLosses * 3f), 0.1f, 2f);
            float lossMaxReduction = Mathf.Clamp(diffRange * 0.25f, 0.5f, 5f);
            preview += $"Loss Threshold: {lossThreshold:F1}\n";
            preview += $"Step Size: {lossStepSize:F2}\n";
            preview += $"Max Reduction: {lossMaxReduction:F2}\n\n";

            preview += "=== Time Decay Modifier ===\n";
            float decayPerDay = Mathf.Clamp(gameStats.maxDifficultyChangePerSession / gameStats.targetRetentionDays, 0.1f, 2f);
            float maxDecay = Mathf.Clamp(gameStats.maxDifficultyChangePerSession, 0.5f, 5f);
            float graceHours = Mathf.Clamp(gameStats.avgHoursBetweenSessions, 0f, 48f);
            preview += $"Decay/Day: {decayPerDay:F2}\n";
            preview += $"Max Decay: {maxDecay:F2}\n";
            preview += $"Grace Hours: {graceHours:F1}\n\n";

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

            // Generate all configs
            config.GenerateAllConfigsFromStats();

            // Mark as dirty to ensure Unity saves the changes
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
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("[DifficultyConfigEditor] Config Generation Complete");
            Debug.Log($"  • Difficulty Range: {config.MinDifficulty:F1} - {config.MaxDifficulty:F1}");
            Debug.Log($"  • Default Difficulty: {config.DefaultDifficulty:F1}");
            Debug.Log($"  • Max Change/Session: {config.MaxChangePerSession:F1}");
            Debug.Log($"  • All 7 modifier configs updated");
            Debug.Log("═══════════════════════════════════════");
        }
    }
}
#endif

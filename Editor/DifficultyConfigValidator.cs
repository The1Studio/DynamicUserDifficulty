using System.IO;
using UnityEngine;
using UnityEditor;
using TheOneStudio.DynamicUserDifficulty.Configuration;

namespace TheOneStudio.DynamicUserDifficulty.Editor
{
    using TheOneStudio.DynamicUserDifficulty.Core;

    /// <summary>
    /// Editor validator that checks for DifficultyConfig existence on Unity load
    /// and helps developers create it if missing
    /// </summary>
    [InitializeOnLoad]
    public static class DifficultyConfigValidator
    {
        // Use constants instead of hardcoded paths - Updated to use GameConfigs
        private static readonly string RESOURCES_PATH = DifficultyConstants.ASSET_DIRECTORY_RESOURCES;
        private static readonly string CONFIGS_PATH = DifficultyConstants.ASSET_DIRECTORY_GAMECONFIGS;
        private static readonly string CONFIG_ASSET_PATH = DifficultyConstants.ASSET_PATH_GAMECONFIGS;
        private const string PREF_KEY_SKIP_CHECK = "DynamicDifficulty_SkipConfigCheck";

        static DifficultyConfigValidator()
        {
            // Check if we should skip the check
            if (EditorPrefs.GetBool(PREF_KEY_SKIP_CHECK, false))
                return;

            // Delay the check to ensure Unity is fully loaded
            EditorApplication.delayCall += CheckForDifficultyConfig;
        }

        private static void CheckForDifficultyConfig()
        {
            // Check if the config already exists
            var config = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(CONFIG_ASSET_PATH);
            if (config != null)
                return;

            // Check if any DifficultyConfig exists anywhere in the project
            var guids = AssetDatabase.FindAssets("t:DifficultyConfig");
            if (guids.Length > DifficultyConstants.STREAK_RESET_VALUE)
            {
                Debug.Log($"[DynamicDifficulty] Found existing DifficultyConfig at: {AssetDatabase.GUIDToAssetPath(guids[0])}");
                return;
            }

            // Show popup to create config
            EditorApplication.delayCall += ShowConfigCreationDialog;
        }

        private static void ShowConfigCreationDialog()
        {
            DifficultyConfigCreatorWindow.ShowWindow();
        }

        public static void CreateDifficultyConfig()
        {
            // Ensure directories exist
            if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
            {
                AssetDatabase.CreateFolder(DifficultyConstants.FOLDER_NAME_ASSETS, DifficultyConstants.FOLDER_NAME_RESOURCES);
            }

            if (!AssetDatabase.IsValidFolder(CONFIGS_PATH))
            {
                AssetDatabase.CreateFolder(RESOURCES_PATH, "GameConfigs");
            }

            // Create the config asset
            var config = DifficultyConfig.CreateDefault();

            // Save the asset
            AssetDatabase.CreateAsset(config, CONFIG_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[DynamicDifficulty] Created DifficultyConfig at: {CONFIG_ASSET_PATH}");
        }

        [MenuItem("TheOne/Dynamic Difficulty/Create Config", false, 100)]
        public static void CreateConfigFromMenu()
        {
            var config = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(CONFIG_ASSET_PATH);
            if (config != null)
            {
                var recreate = EditorUtility.DisplayDialog(
                    "Config Already Exists",
                    "A DifficultyConfig already exists. Do you want to select it?",
                    "Select Existing",
                    "Cancel"
                );

                if (recreate)
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                }
                return;
            }

            CreateDifficultyConfig();
        }

        [MenuItem("TheOne/Dynamic Difficulty/Validate Setup", false, 101)]
        public static void ValidateSetup()
        {
            var messages = new System.Text.StringBuilder();
            var hasErrors = false;

            // Check config exists
            var config = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(CONFIG_ASSET_PATH);
            if (config == null)
            {
                messages.AppendLine("❌ DifficultyConfig not found at expected path");
                hasErrors = true;
            }
            else
            {
                messages.AppendLine("✅ DifficultyConfig found");

                // Validate config settings
                if (config.ModifierConfigs == null || config.ModifierConfigs.Count == DifficultyConstants.STREAK_RESET_VALUE)
                {
                    messages.AppendLine("⚠️ No modifiers configured");
                }
                else
                {
                    messages.AppendLine($"✅ {config.ModifierConfigs.Count} modifiers configured");
                }
            }

            // Check if integrated in GameLifetimeScope
            var gameLifetimeScopePath = DifficultyConstants.INTEGRATION_GAMELIFETIMESCOPE_PATH;
            if (File.Exists(gameLifetimeScopePath))
            {
                var content = File.ReadAllText(gameLifetimeScopePath);
                if (!content.Contains("DynamicDifficultyModule"))
                {
                    messages.AppendLine("⚠️ DynamicDifficultyModule not registered in GameLifetimeScope");
                }
                else
                {
                    messages.AppendLine("✅ DynamicDifficultyModule registered");
                }
            }

            // Show results
            EditorUtility.DisplayDialog(
                "Dynamic Difficulty Setup Validation",
                messages.ToString(),
                "OK"
            );
        }

        [MenuItem("TheOne/Dynamic Difficulty/Reset Validation Check", false, 200)]
        public static void ResetValidationCheck()
        {
            EditorPrefs.DeleteKey(PREF_KEY_SKIP_CHECK);
            Debug.Log("[DynamicDifficulty] Validation check reset. Will check on next Unity load.");
        }
    }

    /// <summary>
    /// Editor window for creating DifficultyConfig with user confirmation
    /// </summary>
    public class DifficultyConfigCreatorWindow : EditorWindow
    {
        private static DifficultyConfigCreatorWindow window;
        private bool dontShowAgain = false;

        public static void ShowWindow()
        {
            if (window == null)
            {
                window = GetWindow<DifficultyConfigCreatorWindow>(true, "Dynamic Difficulty Setup", true);
                window.maxSize = new Vector2(500, 250);
                window.minSize = new Vector2(500, 250);
                window.position = new Rect(
                    (Screen.currentResolution.width - 500) / 2,
                    (Screen.currentResolution.height - 250) / 2,
                    500,
                    250
                );
            }

            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            // Title
            var titleStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField("Dynamic User Difficulty Module", titleStyle);

            GUILayout.Space(10);

            // Message
            EditorGUILayout.HelpBox(
                "The Dynamic User Difficulty module requires a configuration asset.\n\n" +
                "This configuration controls difficulty adjustments based on player performance, " +
                "including win/loss streaks, time decay, and rage quit detection.\n\n" +
                "Would you like to create the default configuration now?",
                MessageType.Info
            );

            GUILayout.Space(20);

            // Don't show again checkbox
            this.dontShowAgain = EditorGUILayout.Toggle("Don't show this again", this.dontShowAgain);

            GUILayout.Space(20);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Later", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (this.dontShowAgain)
                {
                    EditorPrefs.SetBool("DynamicDifficulty_SkipConfigCheck", true);
                }
                this.Close();
            }

            GUILayout.Space(10);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Create Config", GUILayout.Width(120), GUILayout.Height(30)))
            {
                DifficultyConfigValidator.CreateDifficultyConfig();

                // Show success message
                EditorUtility.DisplayDialog(
                    "Success",
                    "DifficultyConfig has been created and selected in the Project window.\n\n" +
                    "You can now configure the difficulty settings in the Inspector.",
                    "OK"
                );

                this.Close();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            window = null;
        }
    }
}
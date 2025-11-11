#nullable enable

using UnityEngine;
using UnityEditor;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using System.IO;

namespace TheOneStudio.DynamicUserDifficulty.Editor
{
    using TheOneStudio.DynamicUserDifficulty.Core;

    /// <summary>
    /// Editor tool for creating DifficultyConfig assets
    /// </summary>
    public static class DifficultyConfigCreator
    {
        public static void CreateDefaultConfig()
        {
            // Single path to use - GameConfigs only
            string configPath = DifficultyConstants.CONFIG_ASSET_PATH;

            // Check if config already exists
            var existingConfig = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(configPath);
            if (existingConfig != null)
            {
                Debug.Log($"[DynamicDifficulty] DifficultyConfig already exists at {configPath}");
                Selection.activeObject = existingConfig;
                EditorGUIUtility.PingObject(existingConfig);
                EditorUtility.DisplayDialog("Config Already Exists",
                    $"DifficultyConfig already exists at:\\n{configPath}", "OK");
                return;
            }

            // Use the single GameConfigs path
            var targetPath = configPath;
            var directory = Path.GetDirectoryName(targetPath);

            // Ensure directory exists
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // Create default config
            var config = DifficultyConfig.CreateDefault();

            // Create the asset
            AssetDatabase.CreateAsset(config, targetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select and ping the created asset
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"[DynamicDifficulty] Created DifficultyConfig at {targetPath}");

            // Show success message
            EditorUtility.DisplayDialog(
                "Config Created Successfully",
                $"DifficultyConfig created at:\\n{targetPath}\\n\\nYou can now customize the settings in the Inspector.",
                "OK");
        }

        public static void FindConfig()
        {
            // Try to find existing config using single GameConfigs path
            string configPath = DifficultyConstants.CONFIG_ASSET_PATH;

            var config = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(configPath);
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
                Debug.Log($"[DynamicDifficulty] Found DifficultyConfig at {configPath}");
                return;
            }

            // Not found, offer to create
            Debug.LogWarning("[DynamicDifficulty] DifficultyConfig not found in any standard location");
            if (EditorUtility.DisplayDialog(
                "Config Not Found",
                "DifficultyConfig not found in Resources.\\nWould you like to create a default configuration?",
                "Create Config",
                "Cancel"))
            {
                CreateDefaultConfig();
            }
        }
    }
}
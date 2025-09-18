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
        [MenuItem(DifficultyConstants.MENU_CREATE_CONFIG)]
        public static void CreateDefaultConfig()
        {
            // Possible paths to try using constants
            string[] possiblePaths = {
                DifficultyConstants.ASSET_PATH_GAMECONFIGS,
                DifficultyConstants.ASSET_PATH_CONFIGS,
                DifficultyConstants.ASSET_PATH_ROOT
            };

            // Check if any config already exists
            foreach (var path in possiblePaths)
            {
                var existingConfig = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(path);
                if (existingConfig != null)
                {
                    Debug.Log($"[DynamicDifficulty] DifficultyConfig already exists at {path}");
                    Selection.activeObject = existingConfig;
                    EditorGUIUtility.PingObject(existingConfig);
                    EditorUtility.DisplayDialog("Config Already Exists",
                        $"DifficultyConfig already exists at:\\n{path}", "OK");
                    return;
                }
            }

            // Use the first path as default
            string targetPath = possiblePaths[0];
            string directory = Path.GetDirectoryName(targetPath);

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

        [MenuItem(DifficultyConstants.MENU_FIND_CONFIG)]
        public static void FindConfig()
        {
            // Try to find existing config using constants
            string[] possiblePaths = {
                DifficultyConstants.ASSET_PATH_GAMECONFIGS,
                DifficultyConstants.ASSET_PATH_CONFIGS,
                DifficultyConstants.ASSET_PATH_ROOT
            };

            foreach (var path in possiblePaths)
            {
                var config = AssetDatabase.LoadAssetAtPath<DifficultyConfig>(path);
                if (config != null)
                {
                    Selection.activeObject = config;
                    EditorGUIUtility.PingObject(config);
                    Debug.Log($"[DynamicDifficulty] Found DifficultyConfig at {path}");
                    return;
                }
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
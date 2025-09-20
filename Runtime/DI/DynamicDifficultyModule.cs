using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TheOneStudio.DynamicUserDifficulty.DI
{
    // Fixed: Removed duplicate DynamicDifficultyInitializer class
    /// <summary>
    /// VContainer module for registering Dynamic Difficulty dependencies
    /// </summary>
    public class DynamicDifficultyModule : IInstaller
    {
        private readonly DifficultyConfig config;

        public DynamicDifficultyModule(DifficultyConfig config)
        {
            this.config = config;
        }

        public void Install(IContainerBuilder builder)
        {
            if (this.config == null)
            {
                // Try to load config from Resources
                var loadedConfig = this.LoadConfigFromResources();
                if (loadedConfig != null)
                {
                    Debug.Log("[DynamicDifficultyModule] Found DifficultyConfig in Resources, using it");
                    // Update the config reference
                    var configField = this.GetType().GetField("config",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    configField?.SetValue(this, loadedConfig);
                }
                else
                {
                    Debug.LogError("[DynamicDifficultyModule] DifficultyConfig is null and not found in Resources. " +
                                   "Please create a DifficultyConfig asset at Resources/GameConfigs/DifficultyConfig or " +
                                   "use Tools > Dynamic Difficulty > Create Default Config. " +
                                   "Skipping Dynamic Difficulty registration.");
                    return;
                }
            }

            // Register configuration
            builder.RegisterInstance(this.config);

            // Register core service
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);

            // Register calculators
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(Lifetime.Singleton);
            builder.Register<ModifierAggregator>(Lifetime.Singleton);

            // Register modifiers with provider-based pattern
            this.RegisterModifiersWithProviders(builder);

            Debug.Log("[DynamicDifficultyModule] Provider-based Dynamic Difficulty system registered successfully");
        }

        private void RegisterModifiersWithProviders(IContainerBuilder builder)
        {
            // Get modifier configs from DifficultyConfig
            if (this.config.ModifierConfigs == null || this.config.ModifierConfigs.Count == 0)
            {
                Debug.LogWarning("[DynamicDifficultyModule] No modifier configs found");
                return;
            }

            // Register each configured modifier with provider dependency
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                if (modifierConfig == null)
                    continue;

                this.RegisterModifierByTypeWithProvider(builder, modifierConfig);
            }
        }

        private void RegisterModifierByTypeWithProvider(IContainerBuilder builder, ModifierConfig modifierConfig)
        {
            switch (modifierConfig.ModifierType)
            {
                case DifficultyConstants.MODIFIER_TYPE_WIN_STREAK:
                    // Register WinStreakModifier - will get IWinStreakProvider via DI
                    builder.Register<WinStreakModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK:
                    // Register LossStreakModifier - will get IWinStreakProvider via DI
                    builder.Register<LossStreakModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case DifficultyConstants.MODIFIER_TYPE_TIME_DECAY:
                    // Register TimeDecayModifier - will get ITimeDecayProvider via DI
                    builder.Register<TimeDecayModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT:
                    // Register RageQuitModifier - will get IRageQuitProvider via DI
                    builder.Register<RageQuitModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                default:
                    Debug.LogWarning($"[DynamicDifficultyModule] Unknown modifier type: {modifierConfig.ModifierType}");
                    break;
            }
        }

        private DifficultyConfig LoadConfigFromResources()
        {
            // Try multiple common paths using constants
            string[] possiblePaths = {
                DifficultyConstants.RESOURCES_PATH_GAMECONFIGS,
                DifficultyConstants.RESOURCES_PATH_CONFIGS,
                DifficultyConstants.RESOURCES_PATH_ROOT
            };

            foreach (var path in possiblePaths)
            {
                var config = Resources.Load<DifficultyConfig>(path);
                if (config != null)
                {
                    Debug.Log($"[DynamicDifficultyModule] Loaded DifficultyConfig from Resources/{path}");
                    return config;
                }
            }

            Debug.LogWarning("[DynamicDifficultyModule] DifficultyConfig not found in Resources. Checked paths: " +
                           string.Join(", ", possiblePaths));
            return null;
        }
    }
}
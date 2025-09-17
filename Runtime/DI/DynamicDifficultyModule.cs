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
            if (config == null)
            {
                Debug.LogError("[DynamicDifficultyModule] DifficultyConfig is null, skipping registration");
                return;
            }


            // Register configuration
            builder.RegisterInstance(config);

            // Register core service
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);

            // Register providers
            builder.Register<ISessionDataProvider, SessionDataProvider>(Lifetime.Singleton);

            // Register calculators
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(Lifetime.Singleton);
            builder.Register<ModifierAggregator>(Lifetime.Singleton);

            // Register modifiers
            RegisterModifiers(builder);

            // Register initializer to auto-register modifiers
            builder.RegisterEntryPoint<DynamicDifficultyInitializer>();
        }

        private void RegisterModifiers(IContainerBuilder builder)
        {
            // Get modifier configs from DifficultyConfig
            if (this.config.ModifierConfigs == null || this.config.ModifierConfigs.Count == 0)
            {
                Debug.LogWarning("[DynamicDifficultyModule] No modifier configs found");
                return;
            }

            // Register each configured modifier
            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                if (modifierConfig == null)
                    continue;

                RegisterModifierByType(builder, modifierConfig);
            }
        }

        private void RegisterModifierByType(IContainerBuilder builder, ModifierConfig modifierConfig)
        {
            switch (modifierConfig.ModifierType)
            {
                case "WinStreak":
                    builder.Register<WinStreakModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case "LossStreak":
                    builder.Register<LossStreakModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case "TimeDecay":
                    builder.Register<TimeDecayModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                case "RageQuit":
                    builder.Register<RageQuitModifier>(Lifetime.Singleton)
                        .WithParameter(modifierConfig)
                        .As<IDifficultyModifier>();
                    break;

                default:
                    Debug.LogWarning($"[DynamicDifficultyModule] Unknown modifier type: {modifierConfig.ModifierType}");
                    break;
            }
        }
    }
}
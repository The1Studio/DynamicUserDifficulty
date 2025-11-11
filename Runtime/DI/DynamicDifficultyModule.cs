#nullable enable

using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using UnityEngine;
using VContainer;

namespace TheOneStudio.DynamicUserDifficulty.DI
{
    /// <summary>
    /// VContainer extension methods for registering Dynamic Difficulty dependencies.
    /// Automatically registers all difficulty modifiers - the game determines which ones
    /// are active by implementing the corresponding provider interfaces.
    /// </summary>
    public static class DynamicDifficultyModule
    {
        /// <summary>
        /// Register Dynamic User Difficulty system with all 7 modifiers.
        /// </summary>
        public static void RegisterDynamicDifficulty(this IContainerBuilder builder, DifficultyConfig config = null)
        {
            // Try to load or create default config if not provided
            var actualConfig = config ? config : LoadOrCreateDefaultConfig();

            // Register configuration
            builder.RegisterInstance(actualConfig);

            // Register core services
            builder.Register<DynamicDifficultyService>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<DifficultyCalculator>(Lifetime.Singleton).AsInterfacesAndSelf();
            builder.Register<ModifierAggregator>(Lifetime.Singleton);
            builder.Register<DifficultyManager>(Lifetime.Singleton);

            // Register ALL modifiers by default - no configuration needed!
            // The game determines which modifiers are active by implementing the corresponding provider interfaces
            RegisterAllModifiers(builder, actualConfig);
        }

        /// <summary>
        /// Registers all available difficulty modifiers.
        /// Each modifier will only be active if its corresponding provider interface is implemented.
        /// </summary>
        private static void RegisterAllModifiers(IContainerBuilder builder, DifficultyConfig config)
        {
            // Get configs from the configuration or create defaults
            var configContainer = config?.ModifierConfigs ?? new ModifierConfigContainer();
            if (configContainer.AllConfigs.Count == 0)
            {
                configContainer.InitializeDefaults();
            }

            // Get typed configs for each modifier
            var winStreakConfig = configContainer.GetConfig<WinStreakConfig>(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK)
                ?? new WinStreakConfig().CreateDefault() as WinStreakConfig;
            var lossStreakConfig = configContainer.GetConfig<LossStreakConfig>(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK)
                ?? new LossStreakConfig().CreateDefault() as LossStreakConfig;
            var timeDecayConfig = configContainer.GetConfig<TimeDecayConfig>(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY)
                ?? new TimeDecayConfig().CreateDefault() as TimeDecayConfig;
            var rageQuitConfig = configContainer.GetConfig<RageQuitConfig>(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT)
                ?? new RageQuitConfig().CreateDefault() as RageQuitConfig;
            var completionRateConfig = configContainer.GetConfig<CompletionRateConfig>(DifficultyConstants.MODIFIER_TYPE_COMPLETION_RATE)
                ?? new CompletionRateConfig().CreateDefault() as CompletionRateConfig;
            var levelProgressConfig = configContainer.GetConfig<LevelProgressConfig>(DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS)
                ?? new LevelProgressConfig().CreateDefault() as LevelProgressConfig;
            var sessionPatternConfig = configContainer.GetConfig<SessionPatternConfig>(DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN)
                ?? new SessionPatternConfig().CreateDefault() as SessionPatternConfig;

            // Register all modifiers with typed configs
            builder.Register<WinStreakModifier>(Lifetime.Singleton)
                .WithParameter(winStreakConfig)
                .As<IDifficultyModifier>();

            builder.Register<LossStreakModifier>(Lifetime.Singleton)
                .WithParameter(lossStreakConfig)
                .As<IDifficultyModifier>();

            builder.Register<TimeDecayModifier>(Lifetime.Singleton)
                .WithParameter(timeDecayConfig)
                .As<IDifficultyModifier>();

            builder.Register<RageQuitModifier>(Lifetime.Singleton)
                .WithParameter(rageQuitConfig)
                .As<IDifficultyModifier>();

            builder.Register<CompletionRateModifier>(Lifetime.Singleton)
                .WithParameter(completionRateConfig)
                .As<IDifficultyModifier>();

            builder.Register<LevelProgressModifier>(Lifetime.Singleton)
                .WithParameter(levelProgressConfig)
                .As<IDifficultyModifier>();

            builder.Register<SessionPatternModifier>(Lifetime.Singleton)
                .WithParameter(sessionPatternConfig)
                .As<IDifficultyModifier>();
        }

        private static DifficultyConfig LoadOrCreateDefaultConfig()
        {
            // Load from the single standard location
            var config = Resources.Load<DifficultyConfig>(DifficultyConstants.CONFIG_RESOURCES_PATH);
            if (config != null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[DynamicDifficultyModule] Loaded DifficultyConfig from Resources/{DifficultyConstants.CONFIG_RESOURCES_PATH}");
                #endif
                #endif
                return config;
            }

            // Keep this as Debug.LogWarning since it's an important configuration issue that should always be visible
            Debug.LogWarning($"[DynamicDifficultyModule] DifficultyConfig not found at: {DifficultyConstants.CONFIG_RESOURCES_PATH}. " +
                           "Creating default configuration.");
            return ScriptableObject.CreateInstance<DifficultyConfig>();
        }
    }
}
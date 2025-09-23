using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TheOneStudio.DynamicUserDifficulty.DI
{
    using GameFoundation.DI;

    /// <summary>
    /// VContainer module for registering Dynamic Difficulty dependencies.
    /// Automatically registers all difficulty modifiers - the game determines which ones
    /// are active by implementing the corresponding provider interfaces.
    /// </summary>
    public class DynamicDifficultyModule : IInstaller
    {
        private readonly DifficultyConfig config;
        private readonly ILoggerManager loggerManager;

        public DynamicDifficultyModule(DifficultyConfig config = null, ILoggerManager loggerManager = null)
        {
            this.config = config;
            this.loggerManager = loggerManager;
        }

        public void Install(IContainerBuilder builder)
        {
            // Try to load or create default config if not provided
            var actualConfig = this.config ?? this.LoadOrCreateDefaultConfig();

            // Register configuration
            builder.RegisterInstance(actualConfig);

            // Register logger manager if provided
            if (this.loggerManager != null)
            {
                builder.RegisterInstance(this.loggerManager);
            }

            // Register core services
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(Lifetime.Singleton);
            builder.Register<ModifierAggregator>(Lifetime.Singleton);
            builder.Register<DifficultyManager>(Lifetime.Singleton);

            // Register ALL modifiers by default - no configuration needed!
            // The game determines which modifiers are active by implementing the corresponding provider interfaces
            this.RegisterAllModifiers(builder);

            // Use conditional compilation for debug output in production code
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (this.loggerManager != null)
            {
                var logger = this.loggerManager.GetLogger(this);
                logger.Info("[DynamicDifficultyModule] All difficulty modifiers registered. Active modifiers depend on which provider interfaces are implemented.");
            }
            #endif
        }

        /// <summary>
        /// Registers all available difficulty modifiers.
        /// Each modifier will only be active if its corresponding provider interface is implemented.
        /// </summary>
        private void RegisterAllModifiers(IContainerBuilder builder)
        {
            // Get configs from the configuration or create defaults
            var configContainer = this.config?.ModifierConfigs ?? new ModifierConfigContainer();
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

            // Use conditional compilation for debug output in production code
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (this.loggerManager != null)
            {
                var logger = this.loggerManager.GetLogger(this);
                logger.Info("[DynamicDifficultyModule] Registered 7 difficulty modifiers with typed configs: WinStreak, LossStreak, TimeDecay, RageQuit, CompletionRate, LevelProgress, SessionPattern");
            }
            #endif
        }

        // Removed CreateModifierConfig method - now using typed configs directly

        private DifficultyConfig LoadOrCreateDefaultConfig()
        {
            // Load from the single standard location
            var config = Resources.Load<DifficultyConfig>(DifficultyConstants.CONFIG_RESOURCES_PATH);
            if (config != null)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (this.loggerManager != null)
                {
                    var logger = this.loggerManager.GetLogger(this);
                    logger.Info($"[DynamicDifficultyModule] Loaded DifficultyConfig from Resources/{DifficultyConstants.CONFIG_RESOURCES_PATH}");
                }
                #endif
                return config;
            }

            // Keep this as Debug.LogWarning since it's an important configuration issue that should always be visible
            UnityEngine.Debug.LogWarning($"[DynamicDifficultyModule] DifficultyConfig not found at: {DifficultyConstants.CONFIG_RESOURCES_PATH}. " +
                           "Creating default configuration.");
            return ScriptableObject.CreateInstance<DifficultyConfig>();
        }
    }
}
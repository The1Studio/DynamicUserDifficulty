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
    /// <summary>
    /// VContainer module for registering Dynamic Difficulty dependencies.
    /// Automatically registers all difficulty modifiers - the game determines which ones
    /// are active by implementing the corresponding provider interfaces.
    /// </summary>
    public class DynamicDifficultyModule : IInstaller
    {
        private readonly DifficultyConfig config;

        public DynamicDifficultyModule(DifficultyConfig config = null)
        {
            this.config = config;
        }

        public void Install(IContainerBuilder builder)
        {
            // Try to load or create default config if not provided
            var actualConfig = this.config ?? this.LoadOrCreateDefaultConfig();

            // Register configuration
            builder.RegisterInstance(actualConfig);

            // Register core services
            builder.Register<IDynamicDifficultyService, DynamicDifficultyService>(Lifetime.Singleton);
            builder.Register<IDifficultyCalculator, DifficultyCalculator>(Lifetime.Singleton);
            builder.Register<ModifierAggregator>(Lifetime.Singleton);
            builder.Register<DifficultyManager>(Lifetime.Singleton);

            // Register ALL modifiers by default - no configuration needed!
            // The game determines which modifiers are active by implementing the corresponding provider interfaces
            this.RegisterAllModifiers(builder);

            Debug.Log("[DynamicDifficultyModule] All difficulty modifiers registered. Active modifiers depend on which provider interfaces are implemented.");
        }

        /// <summary>
        /// Registers all available difficulty modifiers.
        /// Each modifier will only be active if its corresponding provider interface is implemented.
        /// </summary>
        private void RegisterAllModifiers(IContainerBuilder builder)
        {
            // Create default configs for each modifier type
            var winStreakConfig  = this.CreateModifierConfig(DifficultyConstants.MODIFIER_TYPE_WIN_STREAK);
            var lossStreakConfig = this.CreateModifierConfig(DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK);
            var timeDecayConfig  = this.CreateModifierConfig(DifficultyConstants.MODIFIER_TYPE_TIME_DECAY);
            var rageQuitConfig   = this.CreateModifierConfig(DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT);

            // Register all modifiers - they will be injected as IEnumerable<IDifficultyModifier>
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


            Debug.Log("[DynamicDifficultyModule] Registered 4 difficulty modifiers: WinStreak, LossStreak, TimeDecay, RageQuit");
        }

        /// <summary>
        /// Creates a default modifier configuration for the specified type
        /// </summary>
        private ModifierConfig CreateModifierConfig(string modifierType)
        {
            var config = new ModifierConfig();
            config.SetModifierType(modifierType);

            // Set default parameters based on modifier type
            switch (modifierType)
            {
                case DifficultyConstants.MODIFIER_TYPE_WIN_STREAK:
                    config.SetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD, DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD);
                    config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE);
                    config.SetParameter(DifficultyConstants.PARAM_MAX_BONUS, DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS);
                    break;

                case DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK:
                    config.SetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD, DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD);
                    config.SetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE);
                    config.SetParameter(DifficultyConstants.PARAM_MAX_REDUCTION, DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION);
                    break;

                case DifficultyConstants.MODIFIER_TYPE_TIME_DECAY:
                    config.SetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY, DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY);
                    config.SetParameter(DifficultyConstants.PARAM_MAX_DECAY, DifficultyConstants.TIME_DECAY_DEFAULT_MAX);
                    config.SetParameter(DifficultyConstants.PARAM_GRACE_HOURS, DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS);
                    break;

                case DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT:
                    config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD, DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD);
                    config.SetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION, DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION);
                    config.SetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION, DifficultyConstants.QUIT_DEFAULT_REDUCTION);
                    config.SetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION, DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION);
                    break;
            }

            return config;
        }

        private DifficultyConfig LoadOrCreateDefaultConfig()
        {
            // Load from the single standard location
            var config = Resources.Load<DifficultyConfig>(DifficultyConstants.CONFIG_RESOURCES_PATH);
            if (config != null)
            {
                Debug.Log($"[DynamicDifficultyModule] Loaded DifficultyConfig from Resources/{DifficultyConstants.CONFIG_RESOURCES_PATH}");
                return config;
            }

            Debug.LogWarning($"[DynamicDifficultyModule] DifficultyConfig not found at: {DifficultyConstants.CONFIG_RESOURCES_PATH}. " +
                           "Creating default configuration.");
            return ScriptableObject.CreateInstance<DifficultyConfig>();
        }
    }
}
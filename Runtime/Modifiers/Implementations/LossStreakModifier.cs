using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Decreases difficulty based on consecutive losses
    /// Requires IWinStreakProvider to be implemented by the game
    /// </summary>
    public class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK;

        // Constructor for typed config
        public LossStreakModifier(LossStreakConfig config, IWinStreakProvider winStreakProvider) : base(config)
        {
            this.winStreakProvider = winStreakProvider ?? throw new System.ArgumentNullException(nameof(winStreakProvider));
        }

        // Backwards compatibility constructor
        public LossStreakModifier(ModifierConfig oldConfig, IWinStreakProvider winStreakProvider)
            : this(ConvertConfig(oldConfig), winStreakProvider)
        {
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            try
            {
                // Return NoChange if session data is null (convention for null handling)
                if (sessionData == null)
                {
                    return ModifierResult.NoChange();
                }

                var lossStreak = this.winStreakProvider.GetLossStreak();

                // Use strongly-typed properties instead of string parameters
                var lossThreshold = this.config.LossThreshold;
                var stepSize = this.config.StepSize;
                var maxReduction = this.config.MaxReduction;

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No loss streak";

                if (lossStreak >= lossThreshold)
                {
                    // Calculate base reduction
                    value = -(lossStreak - lossThreshold + 1) * stepSize;

                    // Apply max limit
                    value = Mathf.Max(value, -maxReduction);

                    // Response curve logic removed from typed config
                    // Can be re-added if needed

                    reason = $"Loss streak: {lossStreak} consecutive losses";

                    this.LogDebug($"Loss streak {lossStreak} -> adjustment {value:F2}");
                }

                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["streak"] = lossStreak,
                        ["threshold"] = lossThreshold,
                        ["applied"] = value < DifficultyConstants.ZERO_VALUE
                    }
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LossStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

        private static LossStreakConfig ConvertConfig(ModifierConfig oldConfig)
        {
            if (oldConfig == null)
            {
                return new LossStreakConfig().CreateDefault() as LossStreakConfig;
            }

            var config = new LossStreakConfig().CreateDefault() as LossStreakConfig;
            // The old config parameters would be converted here if needed
            return config;
        }
    }
}
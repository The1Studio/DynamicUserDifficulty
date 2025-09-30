using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine.Scripting;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Decreases difficulty based on consecutive losses
    /// Requires IWinStreakProvider to be implemented by the game
    /// </summary>
    [Preserve]
    public class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK;

        // Constructor for typed config
        public LossStreakModifier(LossStreakConfig config, IWinStreakProvider winStreakProvider, ILogger logger) : base(config, logger)
        {
            this.winStreakProvider = winStreakProvider ?? throw new System.ArgumentNullException(nameof(winStreakProvider));
        }


        public override ModifierResult Calculate()
        {
            try
            {
                // Get data from provider - stateless approach
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

                return new()
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["streak"] = lossStreak,
                        ["threshold"] = lossThreshold,
                        ["applied"] = value < DifficultyConstants.ZERO_VALUE,
                    },
                };
            }
            catch (System.Exception e)
            {
                this.logger?.Error($"[LossStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
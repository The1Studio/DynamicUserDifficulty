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
                Debug.Log("[LossStreakModifier] --- Calculate START ---");

                // Get data from provider - stateless approach
                var lossStreak = this.winStreakProvider.GetLossStreak();
                Debug.Log($"[LossStreakModifier] Loss streak from provider: {lossStreak}");

                // Use strongly-typed properties instead of string parameters
                var lossThreshold = this.config.LossThreshold;
                var stepSize = this.config.StepSize;
                var maxReduction = this.config.MaxReduction;
                Debug.Log($"[LossStreakModifier] Config - Threshold: {lossThreshold}, StepSize: {stepSize}, MaxReduction: {maxReduction}");

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No loss streak";

                if (lossStreak >= lossThreshold)
                {
                    // Calculate base reduction
                    value = -(lossStreak - lossThreshold + 1) * stepSize;
                    Debug.Log($"[LossStreakModifier] Raw calculation: -({lossStreak} - {lossThreshold} + 1) * {stepSize} = {value:F2}");

                    // Apply max limit
                    var beforeClamp = value;
                    value = Mathf.Max(value, -maxReduction);
                    if (beforeClamp != value)
                    {
                        Debug.Log($"[LossStreakModifier] Clamped to max reduction: {beforeClamp:F2} → {value:F2}");
                    }

                    reason = $"Loss streak: {lossStreak} consecutive losses";

                    this.LogDebug($"Loss streak {lossStreak} -> adjustment {value:F2}");
                }
                else
                {
                    Debug.Log($"[LossStreakModifier] Below threshold ({lossStreak} < {lossThreshold}) - no adjustment");
                }

                Debug.Log($"[LossStreakModifier] --- Calculate END --- Result: {value:+0.##;-0.##} ({reason})");

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
                Debug.LogError($"[LossStreakModifier] ❌ ERROR: {e.Message}");
                this.logger?.Error($"[LossStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
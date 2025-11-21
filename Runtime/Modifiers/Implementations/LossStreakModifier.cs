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

                // Defensive null checks
                if (this.config == null || this.winStreakProvider == null)
                {
                    Debug.LogWarning("[LossStreakModifier] Config or provider is null - returning no change");
                    return ModifierResult.NoChange();
                }

                // Get data from provider - stateless approach
                var lossStreak = this.winStreakProvider.GetLossStreak();
                Debug.Log($"[LossStreakModifier] Loss streak from provider: {lossStreak}");

                // Use strongly-typed properties instead of string parameters
                var lossThreshold = this.config.LossThreshold;
                var stepSize = this.config.StepSize;
                var maxReduction = this.config.MaxReduction;
                var exponentialFactor = this.config.ExponentialFactor;
                Debug.Log($"[LossStreakModifier] Config - Threshold: {lossThreshold}, StepSize: {stepSize}, MaxReduction: {maxReduction}, ExponentialFactor: {exponentialFactor:F2}");

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No loss streak";

                if (lossStreak >= lossThreshold)
                {
                    // Calculate base value (linear component)
                    var streakAboveThreshold = lossStreak - lossThreshold + 1;
                    var baseAdjustment = streakAboveThreshold * stepSize;
                    Debug.Log($"[LossStreakModifier] Linear base: ({lossStreak} - {lossThreshold} + 1) * {stepSize} = {baseAdjustment:F2}");

                    // Apply exponential acceleration
                    var exponent = lossStreak - lossThreshold;
                    var exponentialMultiplier = Mathf.Pow(exponentialFactor, exponent);
                    value = -(baseAdjustment * exponentialMultiplier);
                    Debug.Log($"[LossStreakModifier] Exponential multiplier: {exponentialFactor:F2}^{exponent} = {exponentialMultiplier:F2}");
                    Debug.Log($"[LossStreakModifier] Exponential value: -{baseAdjustment:F2} * {exponentialMultiplier:F2} = {value:F2}");

                    // Apply max limit
                    var beforeClamp = value;
                    value = Mathf.Max(value, -maxReduction);
                    if (beforeClamp != value)
                    {
                        Debug.Log($"[LossStreakModifier] Clamped to max reduction: {beforeClamp:F2} → {value:F2}");
                    }

                    reason = $"Loss streak: {lossStreak} consecutive losses (exponential x{exponentialMultiplier:F2})";

                    this.LogDebug($"Loss streak {lossStreak} -> adjustment {value:F2} (exponential)");
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
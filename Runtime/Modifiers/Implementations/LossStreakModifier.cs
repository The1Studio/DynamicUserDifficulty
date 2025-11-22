#nullable enable

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
    public sealed class LossStreakModifier : BaseDifficultyModifier<LossStreakConfig>
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug("[LossStreakModifier] --- Calculate START ---");
#endif

                // Get data from provider - stateless approach
                var lossStreak = this.winStreakProvider.GetLossStreak();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[LossStreakModifier] Loss streak from provider: {lossStreak}");
#endif

                // Use strongly-typed properties instead of string parameters
                var lossThreshold = this.config.LossThreshold;
                var stepSize = this.config.StepSize;
                var maxReduction = this.config.MaxReduction;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[LossStreakModifier] Config - Threshold: {lossThreshold}, StepSize: {stepSize}, MaxReduction: {maxReduction}");
#endif

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No loss streak";

                if (lossStreak >= lossThreshold)
                {
                    // Calculate base reduction
                    value = -(lossStreak - lossThreshold + 1) * stepSize;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    this.logger?.Debug($"[LossStreakModifier] Raw calculation: -({lossStreak} - {lossThreshold} + 1) * {stepSize} = {value:F2}");
#endif

                    // Apply max limit
                    var beforeClamp = value;
                    value = Mathf.Max(value, -maxReduction);
                    if (beforeClamp != value)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        this.logger?.Debug($"[LossStreakModifier] Clamped to max reduction: {beforeClamp:F2} → {value:F2}");
#endif
                    }

                    reason = $"Loss streak: {lossStreak} consecutive losses";

                    this.LogDebug($"Loss streak {lossStreak} -> adjustment {value:F2}");
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    this.logger?.Debug($"[LossStreakModifier] Below threshold ({lossStreak} < {lossThreshold}) - no adjustment");
#endif
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[LossStreakModifier] --- Calculate END --- Result: {value:+0.##;-0.##} ({reason})");
#endif

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
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"[LossStreakModifier] ❌ ERROR: {e.Message}");
                #endif
                this.logger?.Error($"[LossStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
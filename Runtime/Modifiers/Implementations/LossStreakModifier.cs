using TheOneStudio.DynamicUserDifficulty.Configuration;
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
    public class LossStreakModifier : BaseDifficultyModifier
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => "LossStreak";

        public LossStreakModifier(ModifierConfig config, IWinStreakProvider winStreakProvider) : base(config)
        {
            this.winStreakProvider = winStreakProvider ?? throw new System.ArgumentNullException(nameof(winStreakProvider));
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

                var lossThreshold = this.GetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD, DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD);
                var stepSize      = this.GetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE);
                var maxReduction  = this.GetParameter(DifficultyConstants.PARAM_MAX_REDUCTION, DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION);

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No loss streak";

                if (lossStreak >= lossThreshold)
                {
                    // Calculate base reduction
                    value = -(lossStreak - lossThreshold + 1) * stepSize;

                    // Apply max limit
                    value = Mathf.Max(value, -maxReduction);

                    // Apply response curve if configured
                    if (maxReduction > DifficultyConstants.ZERO_VALUE)
                    {
                        var normalizedValue = Mathf.Abs(value) / maxReduction;
                        value = -this.ApplyCurve(normalizedValue) * maxReduction;
                    }

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
    }
}
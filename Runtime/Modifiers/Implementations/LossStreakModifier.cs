using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Decreases difficulty based on consecutive losses
    /// </summary>
    public class LossStreakModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "LossStreak";

        public LossStreakModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            if (sessionData == null)
                return ModifierResult.NoChange();

            var lossThreshold = GetParameter(DifficultyConstants.PARAM_LOSS_THRESHOLD, DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD);
            var stepSize = GetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE);
            var maxReduction = GetParameter(DifficultyConstants.PARAM_MAX_REDUCTION, DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION);

            float value = DifficultyConstants.ZERO_VALUE;
            string reason = "No loss streak";

            if (sessionData.LossStreak >= lossThreshold)
            {
                // Calculate base reduction
                value = -(sessionData.LossStreak - lossThreshold + 1) * stepSize;

                // Apply max limit
                value = Mathf.Max(value, -maxReduction);

                // Apply response curve if configured
                if (maxReduction > DifficultyConstants.ZERO_VALUE)
                {
                    var normalizedValue = Mathf.Abs(value) / maxReduction;
                    value = -ApplyCurve(normalizedValue) * maxReduction;
                }

                reason = $"Loss streak: {sessionData.LossStreak} consecutive losses";

                LogDebug($"Loss streak {sessionData.LossStreak} -> adjustment {value:F2}");
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata =
                {
                    ["streak"] = sessionData.LossStreak,
                    ["threshold"] = lossThreshold,
                    ["applied"] = value < DifficultyConstants.ZERO_VALUE
                }
            };
        }
    }
}
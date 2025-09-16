using TheOneStudio.DynamicUserDifficulty.Configuration;
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

            var lossThreshold = GetParameter("LossThreshold", 2f);
            var stepSize = GetParameter("StepSize", 0.3f);
            var maxReduction = GetParameter("MaxReduction", 1.5f);

            float value = 0f;
            string reason = "No loss streak";

            if (sessionData.LossStreak >= lossThreshold)
            {
                // Calculate base reduction
                value = -(sessionData.LossStreak - lossThreshold + 1) * stepSize;

                // Apply max limit
                value = Mathf.Max(value, -maxReduction);

                // Apply response curve if configured
                if (maxReduction > 0)
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
                    ["applied"] = value < 0
                }
            };
        }
    }
}
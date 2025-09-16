using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Increases difficulty based on consecutive wins
    /// </summary>
    public class WinStreakModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "WinStreak";

        public WinStreakModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            if (sessionData == null)
                return ModifierResult.NoChange();

            var winThreshold = GetParameter("WinThreshold", 3f);
            var stepSize = GetParameter("StepSize", 0.5f);
            var maxBonus = GetParameter("MaxBonus", 2f);

            float value = 0f;
            string reason = "No win streak";

            if (sessionData.WinStreak >= winThreshold)
            {
                // Calculate base value
                value = (sessionData.WinStreak - winThreshold + 1) * stepSize;

                // Apply max limit
                value = Mathf.Min(value, maxBonus);

                // Apply response curve if configured
                if (maxBonus > 0)
                {
                    value = ApplyCurve(value / maxBonus) * maxBonus;
                }

                reason = $"Win streak: {sessionData.WinStreak} consecutive wins";

                LogDebug($"Win streak {sessionData.WinStreak} -> adjustment {value:F2}");
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata =
                {
                    ["streak"] = sessionData.WinStreak,
                    ["threshold"] = winThreshold,
                    ["applied"] = value > 0
                }
            };
        }
    }
}
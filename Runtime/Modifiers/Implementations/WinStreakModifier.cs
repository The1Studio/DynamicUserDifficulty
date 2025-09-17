using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
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

            var winThreshold = GetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD, DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD);
            var stepSize = GetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE);
            var maxBonus = GetParameter(DifficultyConstants.PARAM_MAX_BONUS, DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS);

            float value = DifficultyConstants.ZERO_VALUE;
            string reason = "No win streak";

            if (sessionData.WinStreak >= winThreshold)
            {
                // Calculate base value
                value = (sessionData.WinStreak - winThreshold + 1) * stepSize;

                // Apply max limit
                value = Mathf.Min(value, maxBonus);

                // Apply response curve if configured
                if (maxBonus > DifficultyConstants.ZERO_VALUE)
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
                    ["applied"] = value > DifficultyConstants.ZERO_VALUE
                }
            };
        }
    }
}
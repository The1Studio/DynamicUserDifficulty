using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Increases difficulty based on consecutive wins
    /// Requires IWinStreakProvider to be implemented by the game
    /// </summary>
    public class WinStreakModifier : BaseDifficultyModifier
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => "WinStreak";

        public WinStreakModifier(ModifierConfig config, IWinStreakProvider winStreakProvider) : base(config)
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

                var winStreak = this.winStreakProvider.GetWinStreak();

                var winThreshold = this.GetParameter(DifficultyConstants.PARAM_WIN_THRESHOLD, DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD);
                var stepSize     = this.GetParameter(DifficultyConstants.PARAM_STEP_SIZE, DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE);
                var maxBonus     = this.GetParameter(DifficultyConstants.PARAM_MAX_BONUS, DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS);

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No win streak";

                if (winStreak >= winThreshold)
                {
                    // Calculate base value
                    value = (winStreak - winThreshold + 1) * stepSize;

                    // Apply max limit
                    value = Mathf.Min(value, maxBonus);

                    // Apply response curve if configured
                    if (maxBonus > DifficultyConstants.ZERO_VALUE)
                    {
                        value = this.ApplyCurve(value / maxBonus) * maxBonus;
                    }

                    reason = $"Win streak: {winStreak} consecutive wins";

                    this.LogDebug($"Win streak {winStreak} -> adjustment {value:F2}");
                }

                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["streak"] = winStreak,
                        ["threshold"] = winThreshold,
                        ["applied"] = value > DifficultyConstants.ZERO_VALUE
                    }
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WinStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }
    }
}
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
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
    public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_WIN_STREAK;

        // Constructor for typed config
        public WinStreakModifier(WinStreakConfig config, IWinStreakProvider winStreakProvider) : base(config)
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

                // Use strongly-typed properties instead of string parameters
                var winThreshold = this.config.WinThreshold;
                var stepSize = this.config.StepSize;
                var maxBonus = this.config.MaxBonus;

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No win streak";

                if (winStreak >= winThreshold)
                {
                    // Calculate base value
                    value = (winStreak - winThreshold + 1) * stepSize;

                    // Apply max limit
                    value = Mathf.Min(value, maxBonus);

                    reason = $"Win streak: {winStreak} consecutive wins";

                    this.LogDebug($"Win streak {winStreak} -> adjustment {value:F2}");
                }

                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value = value,
                    Reason = reason,
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
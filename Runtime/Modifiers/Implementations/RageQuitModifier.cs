using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty when rage quit is detected
    /// Requires IRageQuitProvider to be implemented by the game
    /// </summary>
    public class RageQuitModifier : BaseDifficultyModifier
    {
        private readonly IRageQuitProvider rageQuitProvider;

        public override string ModifierName => "RageQuit";

        public RageQuitModifier(ModifierConfig config, IRageQuitProvider rageQuitProvider) : base(config)
        {
            this.rageQuitProvider = rageQuitProvider ?? throw new System.ArgumentNullException(nameof(rageQuitProvider));
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            try
            {
                var lastQuitType = this.rageQuitProvider.GetLastQuitType();
                var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();
                var sessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();

                var rageQuitThreshold = this.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD, DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD);
                var rageQuitReduction = this.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION, DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION);
                var quitReduction = this.GetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION, DifficultyConstants.QUIT_DEFAULT_REDUCTION);
                var midPlayReduction = this.GetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION, DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION);

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No quit penalty";

                // Apply different penalties based on quit type
                switch (lastQuitType)
                {
                    case QuitType.RageQuit:
                        value = -rageQuitReduction;
                        reason = $"Rage quit detected (duration: {sessionDuration:F0}s, recent quits: {recentRageQuits})";
                        this.LogDebug($"Rage quit detected with {recentRageQuits} recent rage quits");
                        break;

                    case QuitType.Normal:
                        value = -quitReduction;
                        reason = $"Normal quit detected (duration: {sessionDuration:F0}s)";
                        this.LogDebug($"Normal quit detected after {sessionDuration:F0}s");
                        break;

                    case QuitType.MidPlay:
                        value = -midPlayReduction;
                        reason = $"Mid-play quit detected (duration: {sessionDuration:F0}s)";
                        this.LogDebug($"Mid-play quit detected after {sessionDuration:F0}s");
                        break;

                    default:
                        value = DifficultyConstants.ZERO_VALUE;
                        reason = "Unknown quit type";
                        break;
                }

                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["last_quit_type"] = lastQuitType.ToString(),
                        ["session_duration"] = sessionDuration,
                        ["recent_rage_quits"] = recentRageQuits,
                        ["rage_quit_detected"] = lastQuitType == QuitType.RageQuit
                    }
                };
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RageQuitModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }
    }
}
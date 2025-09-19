using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty when rage quit is detected
    /// </summary>
    public class RageQuitModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "RageQuit";

        public RageQuitModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            if (sessionData?.LastSession == null)
            {
                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value        = DifficultyConstants.ZERO_VALUE,
                    Reason       = "No previous session"
                };
            }

            var rageQuitThreshold = this.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_THRESHOLD, DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD);
            var rageQuitReduction = this.GetParameter(DifficultyConstants.PARAM_RAGE_QUIT_REDUCTION, DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION);
            var quitReduction     = this.GetParameter(DifficultyConstants.PARAM_QUIT_REDUCTION, DifficultyConstants.QUIT_DEFAULT_REDUCTION);
            var midPlayReduction  = this.GetParameter(DifficultyConstants.PARAM_MID_PLAY_REDUCTION, DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION);

            var value = DifficultyConstants.ZERO_VALUE;
            var reason = "Normal session end";
            var lastSession = sessionData.LastSession;

            switch (lastSession.EndType)
            {
                case SessionEndType.QuitAfterLoss:
                    // Check if it was a rage quit (quit quickly after losing)
                    if (lastSession.PlayDuration < rageQuitThreshold)
                    {
                        value = -rageQuitReduction;
                        reason = $"Rage quit detected (played only {lastSession.PlayDuration:F0}s)";
                        this.LogDebug($"Rage quit: {lastSession.PlayDuration}s < {rageQuitThreshold}s threshold");
                    }
                    else
                    {
                        value = -quitReduction;
                        reason = "Quit after losing";
                    }
                    break;

                case SessionEndType.QuitDuringPlay:
                    value = -midPlayReduction;
                    reason = "Quit during play";
                    break;

                case SessionEndType.Timeout:
                    value = -midPlayReduction * DifficultyConstants.MID_PLAY_PARTIAL_MULTIPLIER;
                    reason = "Session timeout";
                    break;

                case SessionEndType.CompletedWin:
                case SessionEndType.QuitAfterWin:
                    // No penalty for winning or quitting after win
                    break;
            }

            return new ModifierResult
            {
                ModifierName = this.ModifierName,
                Value        = value,
                Reason       = reason,
                Metadata =
                {
                    ["last_session_type"] = lastSession.EndType.ToString(),
                    ["play_duration"] = lastSession.PlayDuration,
                    ["rage_quit_detected"] = lastSession.EndType == SessionEndType.QuitAfterLoss
                                           && lastSession.PlayDuration < rageQuitThreshold
                }
            };
        }
    }
}
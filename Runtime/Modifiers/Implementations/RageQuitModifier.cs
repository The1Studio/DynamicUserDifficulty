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
                    ModifierName = ModifierName,
                    Value = 0f,
                    Reason = "No previous session"
                };
            }

            var rageQuitThreshold = GetParameter("RageQuitThreshold", DifficultyConstants.RAGE_QUIT_SECONDS);
            var rageQuitReduction = GetParameter("RageQuitReduction", 1f);
            var quitReduction = GetParameter("QuitReduction", 0.5f);
            var midPlayReduction = GetParameter("MidPlayReduction", 0.3f);

            float value = 0f;
            string reason = "Normal session end";
            var lastSession = sessionData.LastSession;

            switch (lastSession.EndType)
            {
                case SessionEndType.QuitAfterLoss:
                    // Check if it was a rage quit (quit quickly after losing)
                    if (lastSession.PlayDuration < rageQuitThreshold)
                    {
                        value = -rageQuitReduction;
                        reason = $"Rage quit detected (played only {lastSession.PlayDuration:F0}s)";
                        LogDebug($"Rage quit: {lastSession.PlayDuration}s < {rageQuitThreshold}s threshold");
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
                    value = -midPlayReduction * 0.5f;
                    reason = "Session timeout";
                    break;

                case SessionEndType.CompletedWin:
                case SessionEndType.QuitAfterWin:
                    // No penalty for winning or quitting after win
                    break;
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
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
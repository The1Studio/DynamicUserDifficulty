using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine.Scripting;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty when rage quit is detected
    /// Requires IRageQuitProvider to be implemented by the game
    /// </summary>
    [Preserve]
    public class RageQuitModifier : BaseDifficultyModifier<RageQuitConfig>
    {
        private readonly IRageQuitProvider rageQuitProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT;

        // Constructor for typed config
        public RageQuitModifier(RageQuitConfig config, IRageQuitProvider rageQuitProvider, ILogger logger) : base(config, logger)
        {
            this.rageQuitProvider = rageQuitProvider ?? throw new System.ArgumentNullException(nameof(rageQuitProvider));
        }


        public override ModifierResult Calculate()
        {
            try
            {
                // Defensive null checks
                if (this.config == null || this.rageQuitProvider == null)
                {
                    return ModifierResult.NoChange();
                }

                // Get data from providers - stateless approach
                var lastQuitType = this.rageQuitProvider.GetLastQuitType();
                var recentRageQuits = this.rageQuitProvider.GetRecentRageQuitCount();
                var sessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();

                // Use strongly-typed properties instead of string parameters
                var rageQuitThreshold = this.config.RageQuitThreshold;
                var rageQuitReduction = this.config.RageQuitReduction;
                var quitReduction = this.config.QuitReduction;
                var midPlayReduction = this.config.MidPlayReduction;

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

                return new()
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["last_quit_type"] = lastQuitType.ToString(),
                        ["session_duration"] = sessionDuration,
                        ["recent_rage_quits"] = recentRageQuits,
                        ["rage_quit_detected"] = lastQuitType == QuitType.RageQuit,
                    },
                };
            }
            catch (System.Exception e)
            {
                this.logger?.Error($"[RageQuitModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
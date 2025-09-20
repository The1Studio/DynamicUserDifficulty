using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
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
    public class RageQuitModifier : BaseDifficultyModifier<RageQuitConfig>
    {
        private readonly IRageQuitProvider rageQuitProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT;

        // Constructor for typed config
        public RageQuitModifier(RageQuitConfig config, IRageQuitProvider rageQuitProvider) : base(config)
        {
            this.rageQuitProvider = rageQuitProvider ?? throw new System.ArgumentNullException(nameof(rageQuitProvider));
        }

        // Backwards compatibility constructor
        public RageQuitModifier(ModifierConfig oldConfig, IRageQuitProvider rageQuitProvider)
            : this(ConvertConfig(oldConfig), rageQuitProvider)
        {
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

        private static RageQuitConfig ConvertConfig(ModifierConfig oldConfig)
        {
            if (oldConfig == null)
            {
                return new RageQuitConfig().CreateDefault() as RageQuitConfig;
            }

            var config = new RageQuitConfig().CreateDefault() as RageQuitConfig;
            // The old config parameters would be converted here if needed
            return config;
        }
    }
}
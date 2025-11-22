#nullable enable

using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine.Scripting;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Increases difficulty based on consecutive wins
    /// Requires IWinStreakProvider to be implemented by the game
    /// </summary>
    [Preserve]
    public sealed class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
    {
        private readonly IWinStreakProvider winStreakProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_WIN_STREAK;

        // Constructor for typed config
        [Preserve]
        public WinStreakModifier(WinStreakConfig config, IWinStreakProvider winStreakProvider, ILogger logger) : base(config, logger)
        {
            this.winStreakProvider = winStreakProvider ?? throw new System.ArgumentNullException(nameof(winStreakProvider));
        }


        public override ModifierResult Calculate()
        {
            try
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug("[WinStreakModifier] --- Calculate START ---");
#endif

                // Get data from provider - stateless approach
                var winStreak = this.winStreakProvider.GetWinStreak();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[WinStreakModifier] Win streak from provider: {winStreak}");
#endif

                // Use strongly-typed properties instead of string parameters
                var winThreshold = this.config.WinThreshold;
                var stepSize = this.config.StepSize;
                var maxBonus = this.config.MaxBonus;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[WinStreakModifier] Config - Threshold: {winThreshold}, StepSize: {stepSize}, MaxBonus: {maxBonus}");
#endif

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No win streak";

                if (winStreak >= winThreshold)
                {
                    // Calculate base value
                    value = (winStreak - winThreshold + 1) * stepSize;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    this.logger?.Debug($"[WinStreakModifier] Raw calculation: ({winStreak} - {winThreshold} + 1) * {stepSize} = {value:F2}");
#endif

                    // Apply max limit
                    var beforeClamp = value;
                    value = Mathf.Min(value, maxBonus);
                    if (beforeClamp != value)
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        this.logger?.Debug($"[WinStreakModifier] Clamped to max bonus: {beforeClamp:F2} → {value:F2}");
#endif
                    }

                    reason = $"Win streak: {winStreak} consecutive wins";

                    this.LogDebug($"Win streak {winStreak} -> adjustment {value:F2}");
                }
                else
                {
                    #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"[WinStreakModifier] Below threshold ({winStreak} < {winThreshold}) - no adjustment");
                    #endif
                }

                Debug.Log($"[WinStreakModifier] --- Calculate END --- Result: {value:+0.##;-0.##} ({reason})");

                return new()
                {
                    ModifierName = this.ModifierName,
                    Value = value,
                    Reason = reason,
                    Metadata =
                    {
                        ["streak"] = winStreak,
                        ["threshold"] = winThreshold,
                        ["applied"] = value > DifficultyConstants.ZERO_VALUE,
                    },
                };
            }
            catch (System.Exception e)
            {
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError($"[WinStreakModifier] ❌ ERROR: {e.Message}");
                #endif
                this.logger?.Error($"[WinStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
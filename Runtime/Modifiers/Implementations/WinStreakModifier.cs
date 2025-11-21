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
    public class WinStreakModifier : BaseDifficultyModifier<WinStreakConfig>
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
                Debug.Log("[WinStreakModifier] --- Calculate START ---");

                // Defensive null checks
                if (this.config == null || this.winStreakProvider == null)
                {
                    Debug.LogWarning("[WinStreakModifier] Config or provider is null - returning no change");
                    return ModifierResult.NoChange();
                }

                // Get data from provider - stateless approach
                var winStreak = this.winStreakProvider.GetWinStreak();
                Debug.Log($"[WinStreakModifier] Win streak from provider: {winStreak}");

                // Use strongly-typed properties instead of string parameters
                var winThreshold = this.config.WinThreshold;
                var stepSize = this.config.StepSize;
                var maxBonus = this.config.MaxBonus;
                var exponentialFactor = this.config.ExponentialFactor;
                Debug.Log($"[WinStreakModifier] Config - Threshold: {winThreshold}, StepSize: {stepSize}, MaxBonus: {maxBonus}, ExponentialFactor: {exponentialFactor:F2}");

                var value = DifficultyConstants.ZERO_VALUE;
                var reason = "No win streak";

                if (winStreak >= winThreshold)
                {
                    // Calculate base value (linear component)
                    var streakAboveThreshold = winStreak - winThreshold + 1;
                    var baseAdjustment = streakAboveThreshold * stepSize;
                    Debug.Log($"[WinStreakModifier] Linear base: ({winStreak} - {winThreshold} + 1) * {stepSize} = {baseAdjustment:F2}");

                    // Apply exponential acceleration
                    var exponent = winStreak - winThreshold;
                    var exponentialMultiplier = Mathf.Pow(exponentialFactor, exponent);
                    value = baseAdjustment * exponentialMultiplier;
                    Debug.Log($"[WinStreakModifier] Exponential multiplier: {exponentialFactor:F2}^{exponent} = {exponentialMultiplier:F2}");
                    Debug.Log($"[WinStreakModifier] Exponential value: {baseAdjustment:F2} * {exponentialMultiplier:F2} = {value:F2}");

                    // Apply max limit
                    var beforeClamp = value;
                    value = Mathf.Min(value, maxBonus);
                    if (beforeClamp != value)
                    {
                        Debug.Log($"[WinStreakModifier] Clamped to max bonus: {beforeClamp:F2} → {value:F2}");
                    }

                    reason = $"Win streak: {winStreak} consecutive wins (exponential x{exponentialMultiplier:F2})";

                    this.LogDebug($"Win streak {winStreak} -> adjustment {value:F2} (exponential)");
                }
                else
                {
                    Debug.Log($"[WinStreakModifier] Below threshold ({winStreak} < {winThreshold}) - no adjustment");
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
                Debug.LogError($"[WinStreakModifier] ❌ ERROR: {e.Message}");
                this.logger?.Error($"[WinStreakModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
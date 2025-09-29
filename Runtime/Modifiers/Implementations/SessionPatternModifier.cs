using System;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine.Scripting;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations
{
    /// <summary>
    /// Adjusts difficulty based on play session patterns including duration,
    /// frequency of rage quits, and session end reasons.
    /// </summary>
    [Preserve]
    public class SessionPatternModifier : BaseDifficultyModifier<SessionPatternConfig>
    {
        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN;

        private readonly IRageQuitProvider rageQuitProvider;

        public SessionPatternModifier(
            SessionPatternConfig config,
            IRageQuitProvider rageQuitProvider,
            ILoggerManager loggerManager = null)
            : base(config, loggerManager)
        {
            this.rageQuitProvider = rageQuitProvider;
        }

        public override ModifierResult Calculate()
{
    try
    {
        // Get data from providers - stateless approach
        if (this.rageQuitProvider == null)
        {
            return ModifierResult.NoChange();
        }

        var value = 0f;
        var reasons = new System.Collections.Generic.List<string>();

        // 1. Check current session duration
        var currentSessionDuration = this.rageQuitProvider.GetCurrentSessionDuration();
        if (currentSessionDuration < this.config.VeryShortSessionThreshold && currentSessionDuration > 0)
        {
            value -= this.config.VeryShortSessionDecrease;
            reasons.Add($"Very short session ({currentSessionDuration:F0}s)");
            this.LogDebug($"Very short session {currentSessionDuration:F0}s -> decrease {this.config.VeryShortSessionDecrease:F2}");
        }

        // 2. Check average session duration pattern
        var avgSessionDuration = this.rageQuitProvider.GetAverageSessionDuration();
        if (avgSessionDuration > 0 && avgSessionDuration < this.config.MinNormalSessionDuration)
        {
            var durationRatio = avgSessionDuration / this.config.MinNormalSessionDuration;
            var durationAdjustment = -(1f - durationRatio) * this.config.ConsistentShortSessionsDecrease;
            value += durationAdjustment;
            reasons.Add($"Short avg sessions ({avgSessionDuration:F0}s)");
            this.LogDebug($"Avg session {avgSessionDuration:F0}s < {this.config.MinNormalSessionDuration:F0}s -> decrease {durationAdjustment:F2}");
        }

        // 3. Use rage quit data from provider
        var lastQuitType = this.rageQuitProvider.GetLastQuitType();
        var recentRageQuitCount = this.rageQuitProvider.GetRecentRageQuitCount();

        // Check for rage quit patterns
        if (recentRageQuitCount >= this.config.RageQuitCountThreshold)
        {
            var rageQuitPenalty = this.config.RageQuitPatternDecrease * this.config.RageQuitPenaltyMultiplier;
            value -= rageQuitPenalty;
            reasons.Add($"Recent rage quits ({recentRageQuitCount})");
            this.LogDebug($"Rage quit pattern detected: {recentRageQuitCount} rage quits -> decrease {rageQuitPenalty:F2}");
        }

        // 4. Check last quit type for mid-level quit pattern
        if (lastQuitType == QuitType.MidPlay)
        {
            value -= this.config.MidLevelQuitDecrease;
            reasons.Add("Mid-level quit detected");
            this.LogDebug($"Mid-level quit detected -> decrease {this.config.MidLevelQuitDecrease:F2}");
        }

        // 5. Analyze session patterns based on average duration
        // If average session is consistently short, it indicates frustration
        if (avgSessionDuration > 0)
        {
            var sessionRatio = avgSessionDuration / this.config.MinNormalSessionDuration;

            // Check if sessions are consistently too short
            if (sessionRatio < this.config.ShortSessionRatio)
            {
                var shortSessionPenalty = this.config.ConsistentShortSessionsDecrease * (1f - sessionRatio);
                value -= shortSessionPenalty;
                reasons.Add($"Pattern of short sessions ({sessionRatio:P0})");
                this.LogDebug($"Short session pattern {sessionRatio:P0} < {this.config.ShortSessionRatio:P0} -> decrease {shortSessionPenalty:F2}");
            }
        }

        var finalReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Normal session patterns";

        return new()
        {
            ModifierName = this.ModifierName,
            Value = value,
            Reason = finalReason,
            Metadata =
            {
                ["currentSessionDuration"] = currentSessionDuration,
                ["avgSessionDuration"] = avgSessionDuration,
                ["rageQuitCount"] = recentRageQuitCount,
                ["lastQuitType"] = lastQuitType.ToString(),
                ["applied"] = Math.Abs(value) > DifficultyConstants.ZERO_VALUE,
            },
        };
    }
    catch (Exception e)
    {
        this.logger?.Error($"[SessionPatternModifier] Error calculating: {e.Message}");
        return ModifierResult.NoChange();
    }
}
    }
}
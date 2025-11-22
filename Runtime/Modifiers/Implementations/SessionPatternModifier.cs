#nullable enable

using System;
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
    public sealed class SessionPatternModifier : BaseDifficultyModifier<SessionPatternConfig>
    {
        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_SESSION_PATTERN;

        private readonly IRageQuitProvider rageQuitProvider;
        private readonly ISessionPatternProvider sessionPatternProvider;

        public SessionPatternModifier(
            SessionPatternConfig config,
            IRageQuitProvider rageQuitProvider,
            ISessionPatternProvider sessionPatternProvider,
            ILogger logger)
            : base(config, logger)
        {
            this.rageQuitProvider = rageQuitProvider;
            this.sessionPatternProvider = sessionPatternProvider;
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

        // 6. Advanced session history analysis (if provider available)
        if (this.sessionPatternProvider != null)
        {
            // 6a. Analyze recent session history for patterns
            var recentSessions = this.sessionPatternProvider.GetRecentSessionDurations(this.config.SessionHistorySize);
            if (recentSessions != null && recentSessions.Count > 0)
            {
                // Count how many recent sessions were very short
                var shortSessionCount = 0;
                foreach (var duration in recentSessions)
                {
                    if (duration > 0 && duration < this.config.VeryShortSessionThreshold)
                        shortSessionCount++;
                }

                // Check if too many recent sessions were short
                if (recentSessions.Count >= this.config.SessionHistorySize)
                {
                    var shortRatio = (float)shortSessionCount / recentSessions.Count;
                    if (shortRatio > this.config.ShortSessionRatio)
                    {
                        var historyPenalty = this.config.ConsistentShortSessionsDecrease * shortRatio;
                        value -= historyPenalty;
                        reasons.Add($"History shows {shortSessionCount}/{recentSessions.Count} short sessions");
                        this.LogDebug($"Session history analysis: {shortSessionCount}/{recentSessions.Count} were short -> decrease {historyPenalty:F2}");
                    }
                }
            }

            // 6b. Check mid-level quit ratio
            var totalQuits = this.sessionPatternProvider.GetTotalRecentQuits();
            var midLevelQuits = this.sessionPatternProvider.GetRecentMidLevelQuits();
            if (totalQuits > 0)
            {
                var midQuitRatio = (float)midLevelQuits / totalQuits;
                if (midQuitRatio > this.config.MidLevelQuitRatio)
                {
                    var midQuitPenalty = this.config.MidLevelQuitDecrease * (midQuitRatio / this.config.MidLevelQuitRatio);
                    value -= midQuitPenalty;
                    reasons.Add($"High mid-level quit ratio ({midQuitRatio:P0})");
                    this.LogDebug($"Mid-level quit ratio {midQuitRatio:P0} > {this.config.MidLevelQuitRatio:P0} -> decrease {midQuitPenalty:F2}");
                }
            }

            // 6c. Check if difficulty adjustments are improving experience
            var previousDifficulty = this.sessionPatternProvider.GetPreviousDifficulty();
            var previousSessionDuration = this.sessionPatternProvider.GetSessionDurationBeforeLastAdjustment();
            if (previousDifficulty > 0 && previousSessionDuration > 0 && currentSessionDuration > 0)
            {
                // Check if session duration improved after difficulty adjustment
                var improvementRatio = currentSessionDuration / previousSessionDuration;

                // If we decreased difficulty but sessions are still short, decrease more
                if (previousDifficulty > 0 && improvementRatio < this.config.DifficultyImprovementThreshold)
                {
                    var additionalAdjustment = (this.config.DifficultyImprovementThreshold - improvementRatio) * 0.5f;
                    value -= additionalAdjustment;
                    reasons.Add($"Difficulty adjustment not effective (improvement: {improvementRatio:F2}x)");
                    this.LogDebug($"Previous adjustment not effective enough ({improvementRatio:F2}x < {this.config.DifficultyImprovementThreshold:F2}x) -> additional decrease {additionalAdjustment:F2}");
                }
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
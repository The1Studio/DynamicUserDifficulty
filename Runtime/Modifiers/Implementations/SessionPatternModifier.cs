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

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            try
            {
                if (sessionData == null || this.rageQuitProvider == null)
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

                // 3. Check recent session patterns
                if (sessionData.RecentSessions is { Count: >= 3 })
                {
                    var recentSessions = sessionData.RecentSessions.Take(this.config.SessionHistorySize).ToList();

                    // Count short sessions
                    var shortSessions = recentSessions.Count(s => s.PlayDuration < this.config.MinNormalSessionDuration);
                    var shortRatio = (float)shortSessions / recentSessions.Count;

                    if (shortRatio > this.config.ShortSessionRatio)
                    {
                        value -= this.config.ConsistentShortSessionsDecrease;
                        reasons.Add($"Pattern of short sessions ({shortRatio:P0})");
                        this.LogDebug($"Short session pattern {shortRatio:P0} > {this.config.ShortSessionRatio:P0} -> decrease {this.config.ConsistentShortSessionsDecrease:F2}");
                    }

                    // Check for rage quit patterns based on session end type
                    // SessionInfo uses SessionEndType which maps to our rage quit detection
                    var rageQuits = recentSessions.Count(s => s.EndType == SessionEndType.QuitDuringPlay && s.PlayDuration < 30f);
                    if (rageQuits >= 2)
                    {
                        value -= this.config.RageQuitPatternDecrease;
                        reasons.Add($"Multiple rage quits ({rageQuits})");
                        this.LogDebug($"Rage quit pattern detected: {rageQuits} rage quits -> decrease {this.config.RageQuitPatternDecrease:F2}");
                    }
                }

                // 4. Check detailed session info if available
                float midLevelRatio = 0f;
                if (sessionData.DetailedSessions is { Count: >= 3 })
                {
                    var recentDetailed = sessionData.DetailedSessions.Take(this.config.SessionHistorySize).ToList();

                    // Check for consistent short sessions in DetailedSessions
                    var shortDetailedSessions = recentDetailed.Count(s => s.Duration < this.config.MinNormalSessionDuration);
                    var shortDetailedRatio = (float)shortDetailedSessions / recentDetailed.Count;

                    if (shortDetailedRatio > this.config.ShortSessionRatio)
                    {
                        value -= this.config.ConsistentShortSessionsDecrease;
                        reasons.Add($"Consistent short sessions ({shortDetailedRatio:P0})");
                        this.LogDebug($"Detailed short session pattern {shortDetailedRatio:P0} > {this.config.ShortSessionRatio:P0} -> decrease {this.config.ConsistentShortSessionsDecrease:F2}");
                    }

                    // Count mid-level quits
                    var midLevelQuits = recentDetailed.Count(s => s.EndReason == SessionEndReason.QuitMidLevel);
                    midLevelRatio = (float)midLevelQuits / recentDetailed.Count;

                    if (midLevelRatio > this.config.MidLevelQuitRatio)
                    {
                        value -= this.config.MidLevelQuitDecrease;
                        reasons.Add($"Frequent mid-level quits ({midLevelRatio:P0})");
                        this.LogDebug($"Mid-level quit pattern {midLevelRatio:P0} > {this.config.MidLevelQuitRatio:P0} -> decrease {this.config.MidLevelQuitDecrease:F2}");
                    }

                    // Check if difficulty changes are having positive effect
                    var difficultyTrend = AnalyzeDifficultyTrend(recentDetailed);
                    if (difficultyTrend != null)
                    {
                        reasons.Add(difficultyTrend);
                    }
                }

                // 5. Use rage quit count from provider
                var recentRageQuitCount = this.rageQuitProvider.GetRecentRageQuitCount();
                if (recentRageQuitCount >= 2)
                {
                    value -= this.config.RageQuitPatternDecrease * 0.5f;
                    reasons.Add($"Recent rage quits ({recentRageQuitCount})");
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
                        ["midLevelQuitRatio"] = midLevelRatio,
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

        private string AnalyzeDifficultyTrend(System.Collections.Generic.List<DetailedSessionInfo> sessions)
        {
            if (sessions.Count < 2) return null;

            // Check if difficulty decreases are helping (sessions getting longer)
            var difficultyDecreased = sessions.Any(s => s.EndDifficulty < s.StartDifficulty);
            if (difficultyDecreased)
            {
                var beforeDecrease = sessions.Where(s => s.EndDifficulty >= s.StartDifficulty).Average(s => s.Duration);
                var afterDecrease = sessions.Where(s => s.EndDifficulty < s.StartDifficulty).Average(s => s.Duration);

                if (afterDecrease > beforeDecrease * 1.2f)
                {
                    return "Difficulty adjustments helping";
                }
            }

            return null;
        }
    }
}
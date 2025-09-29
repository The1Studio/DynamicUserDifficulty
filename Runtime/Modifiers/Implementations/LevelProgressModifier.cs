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
    /// Adjusts difficulty based on level progression metrics including attempts,
    /// completion time, and progression speed.
    /// </summary>
    [Preserve]
    public class LevelProgressModifier : BaseDifficultyModifier<LevelProgressConfig>
    {
        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS;

        private readonly ILevelProgressProvider levelProgressProvider;

        public LevelProgressModifier(
            LevelProgressConfig config,
            ILevelProgressProvider levelProgressProvider,
            ILoggerManager loggerManager = null)
            : base(config, loggerManager)
        {
            this.levelProgressProvider = levelProgressProvider;
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
{
    try
    {
        if (sessionData == null || this.levelProgressProvider == null)
        {
            return ModifierResult.NoChange();
        }

        var value = 0f;
        var reasons = new System.Collections.Generic.List<string>();

        // 1. Check attempts on current level
        var attempts = this.levelProgressProvider.GetAttemptsOnCurrentLevel();
        if (attempts > this.config.HighAttemptsThreshold)
        {
            var attemptsAdjustment = -(attempts - this.config.HighAttemptsThreshold) * this.config.DifficultyDecreasePerAttempt;
            value += attemptsAdjustment;
            reasons.Add($"High attempts ({attempts})");
            this.LogDebug($"Attempts {attempts} > {this.config.HighAttemptsThreshold} -> decrease {attemptsAdjustment:F2}");
        }

        // 2. Check completion time performance using PercentUsingTimeToComplete
        var timePercentage = this.levelProgressProvider.GetCurrentLevelTimePercentage();
        if (timePercentage > 0)
        {
            if (timePercentage < this.config.FastCompletionRatio)
            {
                // Completing levels faster than expected (< 100% of time limit/average)
                var bonus = this.config.FastCompletionBonus * (1.0f - timePercentage) * this.config.FastCompletionMultiplier;
                value += bonus;
                reasons.Add($"Fast completion ({timePercentage:P0} of expected time)");
                this.LogDebug($"Fast completion {timePercentage:P0} < {this.config.FastCompletionRatio:P0} -> increase {bonus:F2}");
            }
            else if (timePercentage > this.config.SlowCompletionRatio)
            {
                // Taking longer than expected (> 100% of time limit/average)
                var penalty = this.config.SlowCompletionPenalty * Math.Min(timePercentage - 1.0f, this.config.MaxPenaltyMultiplier); // Cap penalty using config
                value -= penalty;
                reasons.Add($"Slow completion ({timePercentage:P0} of expected time)");
                this.LogDebug($"Slow completion {timePercentage:P0} > {this.config.SlowCompletionRatio:P0} -> decrease {penalty:F2}");
            }
        }

        // 3. Fallback to session-based completion time if PercentUsingTimeToComplete is not available
        else
        {
            var avgCompletionTime = this.levelProgressProvider.GetAverageCompletionTime();
            if (avgCompletionTime > 0 && sessionData.RecentSessions.Count > 0)
            {
                // Get the most recent level completion time
                var lastSession = sessionData.RecentSessions.Peek();
                if (lastSession is { PlayDuration: > 0 })
                {
                    var timeRatio = lastSession.PlayDuration / avgCompletionTime;

                    if (timeRatio < this.config.FastCompletionRatio)
                    {
                        // Completing levels faster than average
                        value += this.config.FastCompletionBonus;
                        reasons.Add($"Fast completion ({timeRatio:P0} of avg)");
                        this.LogDebug($"Fast completion {timeRatio:P0} < {this.config.FastCompletionRatio:P0} -> increase {this.config.FastCompletionBonus:F2}");
                    }
                    else if (timeRatio > this.config.SlowCompletionRatio)
                    {
                        // Taking longer than average
                        value -= this.config.SlowCompletionPenalty;
                        reasons.Add($"Slow completion ({timeRatio:P0} of avg)");
                        this.LogDebug($"Slow completion {timeRatio:P0} > {this.config.SlowCompletionRatio:P0} -> decrease {this.config.SlowCompletionPenalty:F2}");
                    }
                }
            }
        }

        // 4. Check overall level progression speed
        var currentLevel = this.levelProgressProvider.GetCurrentLevel();
        if (sessionData.SessionCount > 0)
        {
            // Estimate hours played using configurable value
            var estimatedHoursPlayed = sessionData.SessionCount * this.config.EstimatedHoursPerSession;
            var expectedLevel = (int)(estimatedHoursPlayed * this.config.ExpectedLevelsPerHour);

            if (expectedLevel > 0)
            {
                var levelDifference = currentLevel - expectedLevel;
                var progressionAdjustment = levelDifference * this.config.LevelProgressionFactor;

                if (Math.Abs(progressionAdjustment) > 0.01f)
                {
                    value += progressionAdjustment;
                    if (levelDifference > 0)
                    {
                        reasons.Add($"Fast progression (L{currentLevel} vs expected L{expectedLevel})");
                    }
                    else
                    {
                        reasons.Add($"Slow progression (L{currentLevel} vs expected L{expectedLevel})");
                    }
                    this.LogDebug($"Level progression: current {currentLevel} vs expected {expectedLevel} -> adjustment {progressionAdjustment:F2}");
                }
            }
        }

        // 5. Check level difficulty vs player performance using configurable thresholds
        var levelDifficulty = this.levelProgressProvider.GetCurrentLevelDifficulty();
        var completionRate = this.levelProgressProvider.GetCompletionRate();

        // If player is succeeding on hard levels, increase difficulty further
        if (levelDifficulty >= this.config.HardLevelThreshold && completionRate > this.config.MasteryCompletionRate)
        {
            value += this.config.MasteryBonus;
            reasons.Add("Mastering hard levels");
        }
        // If player is struggling on easy levels, decrease difficulty more
        else if (levelDifficulty <= this.config.EasyLevelThreshold && completionRate < this.config.StruggleCompletionRate)
        {
            value -= this.config.StrugglePenalty;
            reasons.Add("Struggling on easy levels");
        }

        var finalReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Normal level progression";

        return new ModifierResult
        {
            ModifierName = this.ModifierName,
            Value = value,
            Reason = finalReason,
            Metadata =
            {
                ["attempts"] = attempts,
                ["currentLevel"] = currentLevel,
                ["levelDifficulty"] = levelDifficulty,
                ["completionRate"] = completionRate,
                ["avgCompletionTime"] = this.levelProgressProvider.GetAverageCompletionTime(),
                ["applied"] = Math.Abs(value) > DifficultyConstants.ZERO_VALUE,
            },
        };
    }
    catch (Exception e)
    {
        this.logger?.Error($"[LevelProgressModifier] Error calculating: {e.Message}");
        return ModifierResult.NoChange();
    }
}
    }
}
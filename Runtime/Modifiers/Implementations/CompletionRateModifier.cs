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
    /// Adjusts difficulty based on player's overall completion rate.
    /// Uses total wins/losses to calculate success rate and adjusts accordingly.
    /// </summary>
    [Preserve]
    public class CompletionRateModifier : BaseDifficultyModifier<CompletionRateConfig>
    {
        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_COMPLETION_RATE;

        private readonly IWinStreakProvider winStreakProvider;
        private readonly ILevelProgressProvider levelProgressProvider;

        public CompletionRateModifier(
            CompletionRateConfig config,
            IWinStreakProvider winStreakProvider,
            ILevelProgressProvider levelProgressProvider,
            ILoggerManager loggerManager = null)
            : base(config, loggerManager)
        {
            this.winStreakProvider = winStreakProvider;
            this.levelProgressProvider = levelProgressProvider;
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            try
            {
                if (sessionData == null || this.winStreakProvider == null || this.levelProgressProvider == null)
                {
                    return ModifierResult.NoChange();
                }

                // Get total wins and losses
                var totalWins = this.winStreakProvider.GetTotalWins();
                var totalLosses = this.winStreakProvider.GetTotalLosses();
                var totalAttempts = totalWins + totalLosses;

                // Check minimum attempts requirement
                if (totalAttempts < this.config.MinAttemptsRequired)
                {
                    return new()
                    {
                        ModifierName = this.ModifierName,
                        Value = 0f,
                        Reason = $"Not enough attempts ({totalAttempts}/{this.config.MinAttemptsRequired})",
                        Metadata =
                        {
                            ["totalAttempts"] = totalAttempts,
                            ["required"] = this.config.MinAttemptsRequired,
                        },
                    };
                }

                // Calculate completion rate
                var completionRate = totalAttempts > 0 ? (float)totalWins / totalAttempts : 0.5f;

                // Also get level-specific completion rate for more accurate assessment
                var levelCompletionRate = this.levelProgressProvider.GetCompletionRate();

                // Weighted average of overall and level-specific rates
                var weightedRate = completionRate * (1f - this.config.TotalStatsWeight) +
                                  levelCompletionRate * this.config.TotalStatsWeight;

                var value = 0f;
                var reason = "Completion rate normal";

                if (weightedRate < this.config.LowCompletionThreshold)
                {
                    // Player struggling - decrease difficulty
                    value = -this.config.LowCompletionDecrease;
                    reason = $"Low completion rate ({weightedRate:P0}) - decreasing difficulty";
                }
                else if (weightedRate > this.config.HighCompletionThreshold)
                {
                    // Player doing well - increase difficulty
                    value = this.config.HighCompletionIncrease;
                    reason = $"High completion rate ({weightedRate:P0}) - increasing difficulty";
                }

                this.LogDebug($"Completion rate {weightedRate:P0} (W:{totalWins}/L:{totalLosses}) -> adjustment {value:F2}");

                return new()
                {
                    ModifierName = this.ModifierName,
                    Value = value,
                    Reason = reason,
                    Metadata =
                    {
                        ["completionRate"] = completionRate,
                        ["levelCompletionRate"] = levelCompletionRate,
                        ["weightedRate"] = weightedRate,
                        ["totalWins"] = totalWins,
                        ["totalLosses"] = totalLosses,
                        ["applied"] = Math.Abs(value) > DifficultyConstants.ZERO_VALUE,
                    },
                };
            }
            catch (Exception e)
            {
                this.logger?.Error($"[CompletionRateModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    /// <summary>
    /// Default implementation of difficulty calculator
    /// </summary>
    public class DifficultyCalculator : IDifficultyCalculator
    {
        private readonly DifficultyConfig config;
        private readonly ModifierAggregator aggregator;
        private readonly ILogger logger;

        public DifficultyCalculator(DifficultyConfig config, ModifierAggregator aggregator, ILoggerManager loggerManager)
        {
            this.config = config;
            this.aggregator = aggregator;
            this.logger = loggerManager.GetLogger(this);
        }

        public DifficultyResult Calculate(PlayerSessionData sessionData, IEnumerable<IDifficultyModifier> modifiers)
        {
            if (sessionData == null)
            {
                this.logger.Warning("SessionData is null, returning default difficulty");
                return new DifficultyResult
                {
                    PreviousDifficulty = this.config.DefaultDifficulty,
                    NewDifficulty = this.config.DefaultDifficulty,
                    PrimaryReason = "No session data"
                };
            }

            var currentDifficulty = sessionData.CurrentDifficulty;

            // Calculate all modifier results
            var modifierResults = new List<ModifierResult>();
            foreach (var modifier in modifiers)
            {
                if (modifier == null || !modifier.IsEnabled)
                    continue;

                try
                {
                    var modifierResult = modifier.Calculate(sessionData);
                    if (modifierResult != null)
                    {
                        modifierResults.Add(modifierResult);

                        if (this.config.EnableDebugLogs)
                        {
                            this.logger.Info($"[DifficultyCalculator] {modifierResult.ModifierName}: {modifierResult.Value:F2} ({modifierResult.Reason})");
                        }
                    }
                }
                catch (Exception e)
                {
                    this.logger.Error($"Error calculating modifier {modifier.ModifierName}: {e.Message}");
                }
            }

            // Aggregate all modifier values
            var totalAdjustment = this.aggregator.Aggregate(modifierResults);

            // Apply max change per session limit
            totalAdjustment = Mathf.Clamp(
                totalAdjustment,
                -this.config.MaxChangePerSession,
                this.config.MaxChangePerSession
            );

            // Calculate new difficulty
            var newDifficulty = currentDifficulty + totalAdjustment;

            // Clamp to valid range
            newDifficulty = ClampDifficulty(newDifficulty);

            // Create result
            var result = new DifficultyResult
            {
                PreviousDifficulty = currentDifficulty,
                NewDifficulty = newDifficulty,
                AppliedModifiers = modifierResults,
                CalculatedAt = DateTime.Now,
                PrimaryReason = GetPrimaryReason(modifierResults)
            };

            if (this.config.EnableDebugLogs)
            {
                this.logger.Info($"[DifficultyCalculator] Final: {currentDifficulty:F2} -> {newDifficulty:F2} " +
                         $"(Change: {totalAdjustment:F2}, Reason: {result.PrimaryReason})");
            }

            return result;
        }

        private float ClampDifficulty(float value)
        {
            return Mathf.Clamp(value, this.config.MinDifficulty, this.config.MaxDifficulty);
        }

        private string GetPrimaryReason(List<ModifierResult> results)
        {
            if (results == null || results.Count == 0)
                return "No change";

            // Find the modifier with the largest absolute value
            var primaryModifier = results
                .OrderByDescending(r => Math.Abs(r.Value))
                .FirstOrDefault();

            return primaryModifier?.Reason ?? "No change";
        }
    }
}
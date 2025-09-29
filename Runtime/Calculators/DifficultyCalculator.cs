using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine.Scripting;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    using UnityEngine;

    /// <summary>
    /// Default implementation of difficulty calculator.
    /// Gets all data from provider interfaces - truly stateless.
    /// </summary>
    [Preserve]
    public class DifficultyCalculator : IDifficultyCalculator
    {
        private readonly DifficultyConfig config;
        private readonly ModifierAggregator aggregator;
        private readonly IDifficultyDataProvider dataProvider;
        private readonly TheOne.Logging.ILogger logger;

        [Preserve]
        public DifficultyCalculator(
            DifficultyConfig config,
            ModifierAggregator aggregator,
            IDifficultyDataProvider dataProvider,
            ILoggerManager loggerManager)
        {
            this.config = config;
            this.aggregator = aggregator;
            this.dataProvider = dataProvider;
            this.logger = loggerManager.GetLogger(this);
        }

        public DifficultyResult Calculate(IEnumerable<IDifficultyModifier> modifiers)
        {
            // Get current difficulty from provider - single source of truth
            var currentDifficulty = this.dataProvider?.GetCurrentDifficulty() ?? this.config.DefaultDifficulty;

            // Calculate all modifier results - each modifier uses its own providers
            var modifierResults = new List<ModifierResult>();
            foreach (var modifier in modifiers)
            {
                if (modifier == null || !modifier.IsEnabled)
                    continue;

                try
                {
                    // Modifiers now get all data from their injected providers
                    var modifierResult = modifier.Calculate();
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
            newDifficulty = this.ClampDifficulty(newDifficulty);

            // Create result
            var result = new DifficultyResult
            {
                PreviousDifficulty = currentDifficulty,
                NewDifficulty = newDifficulty,
                AppliedModifiers = modifierResults,
                CalculatedAt = DateTime.Now,
                PrimaryReason = this.GetPrimaryReason(modifierResults),
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
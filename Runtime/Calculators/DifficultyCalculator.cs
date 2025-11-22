#nullable enable

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
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Default implementation of difficulty calculator.
    /// Gets all data from provider interfaces - truly stateless.
    /// </summary>
    [Preserve]
    public sealed class DifficultyCalculator : IDifficultyCalculator
    {
        private readonly DifficultyConfig config;
        private readonly ModifierAggregator aggregator;
        private readonly IDifficultyDataProvider dataProvider;
        private readonly ILogger logger;

        [Preserve]
        public DifficultyCalculator(
            DifficultyConfig config,
            ModifierAggregator aggregator,
            IDifficultyDataProvider dataProvider,
            ILogger logger)
        {
            this.config = config;
            this.aggregator = aggregator;
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        public DifficultyResult Calculate(IEnumerable<IDifficultyModifier> modifiers)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug("[DifficultyCalculator] ===== CALCULATOR START =====");
#endif

            // Get current difficulty from provider - single source of truth
            var currentDifficulty = this.dataProvider?.GetCurrentDifficulty() ?? this.config.DefaultDifficulty;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug($"[DifficultyCalculator] Current difficulty from provider: {currentDifficulty:F2}");
            this.logger?.Debug($"[DifficultyCalculator] Max change per session: ±{this.config.MaxChangePerSession:F2}");
            this.logger?.Debug($"[DifficultyCalculator] Difficulty range: {this.config.MinDifficulty:F2} - {this.config.MaxDifficulty:F2}");
#endif

            // Calculate all modifier results - each modifier uses its own providers
            var modifierResults = new List<ModifierResult>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug("[DifficultyCalculator] Processing modifiers...");
#endif
            foreach (var modifier in modifiers)
            {
                if (modifier == null || !modifier.IsEnabled)
                    continue;

                try
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    this.logger?.Debug($"[DifficultyCalculator] → Calling {modifier.ModifierName}.Calculate()...");
#endif
                    // Modifiers now get all data from their injected providers
                    var modifierResult = modifier.Calculate();
                    if (modifierResult != null)
                    {
                        modifierResults.Add(modifierResult);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        this.logger?.Debug($"[DifficultyCalculator]   ✓ {modifierResult.ModifierName}: {modifierResult.Value:+0.##;-0.##} ({modifierResult.Reason})");
#endif
                        if (this.config.EnableDebugLogs)
                        {
                            this.logger.Info($"[DifficultyCalculator] {modifierResult.ModifierName}: {modifierResult.Value:F2} ({modifierResult.Reason})");
                        }
                    }
                }
                catch (Exception e)
                {
                    this.logger?.Error($"Error calculating modifier {modifier.ModifierName}: {e.Message}");
                    this.logger?.Exception(e);
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug($"[DifficultyCalculator] Processed {modifierResults.Count} modifiers");
#endif

            // Aggregate all modifier values
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug("[DifficultyCalculator] Aggregating modifier values...");
#endif
            var totalAdjustment = this.aggregator.Aggregate(modifierResults);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug($"[DifficultyCalculator] Total raw adjustment: {totalAdjustment:+0.##;-0.##}");
#endif

            // Apply max change per session limit
            var clampedAdjustment = Mathf.Clamp(
                totalAdjustment,
                -this.config.MaxChangePerSession,
                this.config.MaxChangePerSession
            );
            if (Math.Abs(clampedAdjustment - totalAdjustment) > 0.01f)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[DifficultyCalculator] ⚠️  Adjustment clamped: {totalAdjustment:+0.##;-0.##} → {clampedAdjustment:+0.##;-0.##}");
#endif
            }
            totalAdjustment = clampedAdjustment;

            // Calculate new difficulty
            var newDifficulty = currentDifficulty + totalAdjustment;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug($"[DifficultyCalculator] New difficulty before clamp: {newDifficulty:F2}");
#endif

            // Clamp to valid range
            var clampedDifficulty = this.ClampDifficulty(newDifficulty);
            if (Math.Abs(clampedDifficulty - newDifficulty) > 0.01f)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[DifficultyCalculator] ⚠️  Difficulty clamped: {newDifficulty:F2} → {clampedDifficulty:F2}");
#endif
            }
            newDifficulty = clampedDifficulty;

            // Create result
            var result = new DifficultyResult
            {
                PreviousDifficulty = currentDifficulty,
                NewDifficulty = newDifficulty,
                AppliedModifiers = modifierResults,
                CalculatedAt = DateTime.Now,
                PrimaryReason = this.GetPrimaryReason(modifierResults),
            };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.logger?.Debug("[DifficultyCalculator] ===== CALCULATOR COMPLETE =====");
            this.logger?.Debug($"[DifficultyCalculator] Final: {currentDifficulty:F2} → {newDifficulty:F2} (Change: {totalAdjustment:+0.##;-0.##})");
            this.logger?.Debug($"[DifficultyCalculator] Primary reason: {result.PrimaryReason}");
#endif

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

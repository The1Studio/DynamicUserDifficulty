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
    public class DifficultyCalculator : IDifficultyCalculator
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
            Debug.Log("[DifficultyCalculator] ===== CALCULATOR START =====");

            // H3 Fix: Add timeout protection
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int MAX_TOTAL_TIME_MS = 150; // Increased from 100ms to handle stress tests
            const int MAX_MODIFIER_TIME_MS = 20;

            // Get current difficulty from provider - single source of truth
            var currentDifficulty = this.dataProvider?.GetCurrentDifficulty() ?? this.config.DefaultDifficulty;
            Debug.Log($"[DifficultyCalculator] Current difficulty from provider: {currentDifficulty:F2}");
            Debug.Log($"[DifficultyCalculator] Max change per session: ±{this.config.MaxChangePerSession:F2}");
            Debug.Log($"[DifficultyCalculator] Difficulty range: {this.config.MinDifficulty:F2} - {this.config.MaxDifficulty:F2}");

            // Calculate all modifier results with timeout protection
            var modifierResults = new List<ModifierResult>();
            Debug.Log("[DifficultyCalculator] Processing modifiers...");

            foreach (var modifier in modifiers)
            {
                if (modifier == null || !modifier.IsEnabled)
                    continue;

                // Check total time budget
                if (stopwatch.ElapsedMilliseconds > MAX_TOTAL_TIME_MS)
                {
                    this.logger?.Error($"Calculation timeout after {stopwatch.ElapsedMilliseconds}ms (max: {MAX_TOTAL_TIME_MS}ms). Processed {modifierResults.Count} modifiers.");
                    Debug.LogError($"[DifficultyCalculator] ⏱️ Timeout after {stopwatch.ElapsedMilliseconds}ms (max: {MAX_TOTAL_TIME_MS}ms)");
                    break;
                }

                var modifierStartTime = stopwatch.ElapsedMilliseconds;

                try
                {
                    Debug.Log($"[DifficultyCalculator] → Calling {modifier.ModifierName}.Calculate()...");

                    var modifierResult = modifier.Calculate();

                    var modifierTime = stopwatch.ElapsedMilliseconds - modifierStartTime;
                    if (modifierTime > MAX_MODIFIER_TIME_MS)
                    {
                        this.logger?.Warning($"{modifier.ModifierName} took {modifierTime}ms (threshold: {MAX_MODIFIER_TIME_MS}ms)");
                        Debug.LogWarning($"[DifficultyCalculator] ⚠️ {modifier.ModifierName} slow: {modifierTime}ms");
                    }

                    if (modifierResult != null)
                    {
                        modifierResults.Add(modifierResult);

                        Debug.Log($"[DifficultyCalculator]   ✓ {modifierResult.ModifierName}: {modifierResult.Value:+0.##;-0.##} ({modifierResult.Reason})");
                        if (this.config.EnableDebugLogs)
                        {
                            this.logger?.Info($"[DifficultyCalculator] {modifierResult.ModifierName}: {modifierResult.Value:F2} ({modifierResult.Reason})");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DifficultyCalculator] ❌ Error in {modifier.ModifierName}: {e.Message}");
                    this.logger?.Error($"Error calculating modifier {modifier.ModifierName}: {e.Message}");
                }
            }

            Debug.Log($"[DifficultyCalculator] Processed {modifierResults.Count} modifiers in {stopwatch.ElapsedMilliseconds}ms");

            // Aggregate all modifier values using diminishing returns strategy
            Debug.Log("[DifficultyCalculator] Aggregating modifier values with diminishing returns...");
            Debug.Log($"[DifficultyCalculator] Diminishing factor: {this.config.DiminishingFactor:F2}");
            var totalAdjustment = this.aggregator.AggregateDiminishing(modifierResults, this.config.DiminishingFactor);
            Debug.Log($"[DifficultyCalculator] Total raw adjustment: {totalAdjustment:+0.##;-0.##}");

            // Apply max change per session limit
            var clampedAdjustment = Mathf.Clamp(
                totalAdjustment,
                -this.config.MaxChangePerSession,
                this.config.MaxChangePerSession
            );
            if (Math.Abs(clampedAdjustment - totalAdjustment) > 0.01f)
            {
                Debug.Log($"[DifficultyCalculator] ⚠️  Adjustment clamped: {totalAdjustment:+0.##;-0.##} → {clampedAdjustment:+0.##;-0.##}");
            }
            totalAdjustment = clampedAdjustment;

            // Calculate new difficulty
            var newDifficulty = currentDifficulty + totalAdjustment;
            Debug.Log($"[DifficultyCalculator] New difficulty before clamp: {newDifficulty:F2}");

            // Clamp to valid range
            var clampedDifficulty = this.ClampDifficulty(newDifficulty);
            if (Math.Abs(clampedDifficulty - newDifficulty) > 0.01f)
            {
                Debug.Log($"[DifficultyCalculator] ⚠️  Difficulty clamped: {newDifficulty:F2} → {clampedDifficulty:F2}");
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

            Debug.Log("[DifficultyCalculator] ===== CALCULATOR COMPLETE =====");
            Debug.Log($"[DifficultyCalculator] Final: {currentDifficulty:F2} → {newDifficulty:F2} (Change: {totalAdjustment:+0.##;-0.##})");
            Debug.Log($"[DifficultyCalculator] Primary reason: {result.PrimaryReason}");

            if (this.config.EnableDebugLogs)
            {
                this.logger?.Info($"[DifficultyCalculator] Final: {currentDifficulty:F2} -> {newDifficulty:F2} " +
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
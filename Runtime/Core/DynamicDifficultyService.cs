#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;
using UnityEngine.Scripting;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Stateless calculation service for dynamic difficulty.
    /// This service does NOT store any data - it only calculates and returns a difficulty float value.
    /// All data should be retrieved from external services via providers.
    /// </summary>
    [Preserve]
    public sealed class DynamicDifficultyService : IDynamicDifficultyService
    {
        private readonly IDifficultyCalculator calculator;
        private readonly IDifficultyDataProvider dataProvider;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ILogger logger;

        [Preserve]
        public DynamicDifficultyService(
            IDifficultyCalculator calculator,
            IDifficultyDataProvider dataProvider,
            DifficultyConfig config,
            IEnumerable<IDifficultyModifier> modifiers,
            ILogger logger)
        {
            this.calculator   = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.config       = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers    = modifiers?.ToList() ?? new List<IDifficultyModifier>();
            this.logger       = logger;

            this.logger?.Info($"[DynamicDifficultyService] Initialized stateless service with {this.modifiers.Count} modifiers");
        }

        public float CurrentDifficulty => this.dataProvider?.GetCurrentDifficulty() ?? DifficultyConstants.DEFAULT_DIFFICULTY;

        public DifficultyResult CalculateDifficulty()
        {
            try
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug("[DynamicDifficultyService] ===== START CALCULATION =====");
                this.logger?.Debug($"[DynamicDifficultyService] Current difficulty: {this.CurrentDifficulty:F2}");
#endif

                // Get enabled modifiers sorted by priority
                var enabledModifiers = this.modifiers
                    .Where(m => m is { IsEnabled: true })
                    .OrderBy(m => m.Priority)
                    .ToList();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug($"[DynamicDifficultyService] Found {enabledModifiers.Count} enabled modifiers:");
                foreach (var modifier in enabledModifiers)
                {
                    this.logger?.Debug($"  - {modifier.ModifierName} (Priority: {modifier.Priority}, Enabled: {modifier.IsEnabled})");
                }

                // Calculate new difficulty - all data comes from providers
                this.logger?.Debug("[DynamicDifficultyService] Calling calculator.Calculate()...");
#endif
                var result = this.calculator.Calculate(enabledModifiers);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.logger?.Debug("[DynamicDifficultyService] ===== CALCULATION COMPLETE =====");
                this.logger?.Debug($"[DynamicDifficultyService] Result:");
                this.logger?.Debug($"  - Previous: {result.PreviousDifficulty:F2}");
                this.logger?.Debug($"  - New: {result.NewDifficulty:F2}");
                this.logger?.Debug($"  - Change: {result.NewDifficulty - result.PreviousDifficulty:+0.##;-0.##}");
                this.logger?.Debug($"  - Reason: {result.PrimaryReason}");
#endif

                this.logger?.Info($"[DynamicDifficultyService] Calculated difficulty: " +
                         $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");

                // NOTE: Result is returned for inspection, but not automatically applied
                // Use ApplyDifficulty() to persist the change
                return result;
            }
            catch (Exception e)
            {
                this.logger?.Error($"[DynamicDifficultyService] Calculation failed: {e.Message}");
                this.logger?.Exception(e);

                var currentDiff = this.dataProvider?.GetCurrentDifficulty() ?? DifficultyConstants.DEFAULT_DIFFICULTY;

                // Return unchanged difficulty on error
                return new()
                {
                    NewDifficulty      = currentDiff,
                    PreviousDifficulty = currentDiff,
                    AppliedModifiers   = new(),
                    CalculatedAt       = DateTime.Now,
                    PrimaryReason      = "Calculation error",
                };
            }
        }

        public void ApplyDifficulty(DifficultyResult result)
        {
            if (result == null)
            {
                this.logger?.Warning("[DynamicDifficultyService] Cannot apply null difficulty result");
                return;
            }

            // Use provider to persist the difficulty with null safety
            this.dataProvider?.SetCurrentDifficulty(result.NewDifficulty);

            this.logger?.Info($"[DynamicDifficultyService] Applied difficulty: {result.NewDifficulty:F2}");
        }

        public float GetDefaultDifficulty()
        {
            return this.config?.DefaultDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        public float ClampDifficulty(float difficulty)
        {
            return Math.Max(this.config.MinDifficulty, Math.Min(this.config.MaxDifficulty, difficulty));
        }
    }
}
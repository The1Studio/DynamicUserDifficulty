using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
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
    public class DynamicDifficultyService : IDynamicDifficultyService
    {
        private readonly IDifficultyCalculator calculator;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ILogger logger;

        [Preserve]
        public DynamicDifficultyService(
            IDifficultyCalculator calculator,
            DifficultyConfig config,
            IEnumerable<IDifficultyModifier> modifiers,
            ILoggerManager loggerManager)
        {
            this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers = modifiers?.ToList() ?? new List<IDifficultyModifier>();
            this.logger = loggerManager?.GetLogger(this);

            this.logger?.Info($"[DynamicDifficultyService] Initialized stateless service with {this.modifiers.Count} modifiers");
        }

        public DifficultyResult CalculateDifficulty(float currentDifficulty, PlayerSessionData sessionData)
        {
            try
            {
                // Get enabled modifiers sorted by priority
                var enabledModifiers = this.modifiers
                    .Where(m => m != null && m.IsEnabled)
                    .OrderBy(m => m.Priority);

                // Calculate new difficulty using pure function
                // If sessionData is null, the calculator will use providers to get data
                var result = this.calculator.Calculate(sessionData, enabledModifiers);

                this.logger?.Info($"[DynamicDifficultyService] Calculated difficulty: " +
                         $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");

                // NOTE: This is a pure calculation service - it does NOT store state
                // The caller is responsible for persisting the new difficulty if needed
                return result;
            }
            catch (Exception e)
            {
                this.logger?.Error($"[DynamicDifficultyService] Calculation failed: {e.Message}");

                // Return unchanged difficulty on error
                return new()
                {
                    NewDifficulty      = currentDifficulty,
                    PreviousDifficulty = currentDifficulty,
                    AppliedModifiers   = new(),
                    CalculatedAt       = DateTime.Now,
                    PrimaryReason      = "Calculation error",
                };
            }
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
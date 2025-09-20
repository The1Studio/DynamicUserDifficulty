using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Stateless calculation service for dynamic difficulty.
    /// This service does NOT store any data - it only calculates difficulty based on input data.
    /// All data should be retrieved from external services.
    /// </summary>
    public class DynamicDifficultyService : IDynamicDifficultyService
    {
        private readonly IDifficultyDataProvider dataProvider;
        private readonly IDifficultyCalculator calculator;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ILogger logger;

        public DynamicDifficultyService(
            IDifficultyDataProvider dataProvider,
            IDifficultyCalculator calculator,
            DifficultyConfig config,
            IEnumerable<IDifficultyModifier> modifiers,
            ILoggerManager loggerManager)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers = modifiers?.ToList() ?? new List<IDifficultyModifier>();
            this.logger = loggerManager?.GetLogger(this);

            this.logger?.Info($"[DynamicDifficultyService] Initialized with {this.modifiers.Count} modifiers");
        }

        public DifficultyResult CalculateDifficulty(float currentDifficulty, PlayerSessionData sessionData)
        {
            if (sessionData == null)
            {
                throw new ArgumentNullException(nameof(sessionData));
            }

            try
            {
                // Get enabled modifiers sorted by priority
                var enabledModifiers = this.modifiers
                    .Where(m => m != null && m.IsEnabled)
                    .OrderBy(m => m.Priority);

                // Calculate new difficulty using pure function
                var result = this.calculator.Calculate(sessionData, enabledModifiers);

                this.logger?.Info($"[DynamicDifficultyService] Calculated difficulty: " +
                         $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");

                // Update stored difficulty value
                if (Math.Abs(result.NewDifficulty - currentDifficulty) > 0.01f)
                {
                    this.dataProvider.SetCurrentDifficulty(result.NewDifficulty);
                }

                return result;
            }
            catch (Exception e)
            {
                this.logger?.Error($"[DynamicDifficultyService] Calculation failed: {e.Message}");

                // Return unchanged difficulty on error
                return new DifficultyResult
                {
                    NewDifficulty = currentDifficulty,
                    PreviousDifficulty = currentDifficulty,
                    TotalChange = 0f,
                    ModifierResults = new List<ModifierResult>(),
                    Timestamp = DateTime.Now
                };
            }
        }

        public float CalculateAdjustment(
            float currentDifficulty,
            int winStreak = 0,
            int lossStreak = 0,
            float hoursSinceLastPlay = 0,
            QuitType? lastQuitType = null)
        {
            // Create synthetic session data for what-if analysis
            var sessionData = new PlayerSessionData
            {
                CurrentDifficulty = currentDifficulty,
                WinStreak = winStreak,
                LossStreak = lossStreak,
                LastPlayTime = DateTime.Now.AddHours(-hoursSinceLastPlay),
                LastQuitType = lastQuitType ?? QuitType.Normal,
                TotalSessionsPlayed = 1,
                Sessions = new List<SessionInfo>()
            };

            var result = this.CalculateDifficulty(currentDifficulty, sessionData);
            return result.TotalChange;
        }

        public float GetDefaultDifficulty()
        {
            return this.config?.DefaultDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        public bool IsValidDifficulty(float difficulty)
        {
            return difficulty >= this.config.MinDifficulty && difficulty <= this.config.MaxDifficulty;
        }

        public float ClampDifficulty(float difficulty)
        {
            return Math.Max(this.config.MinDifficulty, Math.Min(this.config.MaxDifficulty, difficulty));
        }
    }
}
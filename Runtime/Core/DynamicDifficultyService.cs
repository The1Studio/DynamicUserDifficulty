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
    /// This service does NOT store any data - it only calculates difficulty based on input data.
    /// All data should be retrieved from external services.
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
                QuitType = lastQuitType,
                SessionCount = 1,
            };

            var result = this.CalculateDifficulty(currentDifficulty, sessionData);
            return result.TotalAdjustment;
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

        /// <summary>
        /// Determines quit type based on session data.
        /// This method analyzes session behavior to classify how/why the player ended their session.
        /// </summary>
        /// <param name="sessionDuration">Duration of the session in seconds</param>
        /// <param name="wasLastLevelWon">Whether the last level played was completed successfully</param>
        /// <param name="lastLevelEndTime">When the last level ended</param>
        /// <returns>Classified quit type</returns>
        public QuitType DetermineQuitType(float sessionDuration, bool wasLastLevelWon, DateTime lastLevelEndTime)
        {
            var timeSinceLevelEnd = (DateTime.Now - lastLevelEndTime).TotalSeconds;

            // Check if player quit soon after losing a level
            if (!wasLastLevelWon)
            {
                // Immediate quit after loss - definite rage quit
                if (timeSinceLevelEnd < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD)
                {
                    return QuitType.RageQuit;
                }
                // Quick quit after loss with short session - likely rage quit
                if (timeSinceLevelEnd < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD * 2 &&
                    sessionDuration < DifficultyConstants.MIN_SESSION_DURATION)
                {
                    return QuitType.RageQuit;
                }
                // Stuck on level for a while then quit - mid-play quit
                if (sessionDuration > DifficultyConstants.MIN_SESSION_DURATION &&
                    sessionDuration < DifficultyConstants.MIN_SESSION_DURATION * 3)
                {
                    return QuitType.MidPlay;
                }
            }

            // Very short sessions are rage quits regardless of outcome
            if (sessionDuration < DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD)
            {
                return QuitType.RageQuit;
            }

            // Medium duration sessions without completion might be mid-play
            if (sessionDuration >= DifficultyConstants.MIN_SESSION_DURATION &&
                sessionDuration < DifficultyConstants.MIN_SESSION_DURATION * 5 &&
                timeSinceLevelEnd > DifficultyConstants.MIN_SESSION_DURATION)
            {
                return QuitType.MidPlay;
            }

            // Normal quit for longer sessions or after winning
            return QuitType.Normal;
        }
    }
}
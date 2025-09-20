using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Stateless utility class for difficulty calculations and adjustments.
    /// This class contains pure functions for difficulty management without storing any state.
    /// </summary>
    public class DifficultyManager
    {
        private readonly DifficultyConfig config;

        /// <summary>
        /// Default constructor using default configuration
        /// </summary>
        public DifficultyManager()
        {
            this.config = null;
        }

        /// <summary>
        /// Constructor with configuration
        /// </summary>
        /// <param name="config">Difficulty configuration</param>
        public DifficultyManager(DifficultyConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// Adjusts difficulty based on a modifier value (pure function)
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty value</param>
        /// <param name="adjustment">Adjustment to apply</param>
        /// <param name="minDifficulty">Minimum allowed difficulty</param>
        /// <param name="maxDifficulty">Maximum allowed difficulty</param>
        /// <returns>New difficulty value</returns>
        public float AdjustDifficulty(float currentDifficulty, float adjustment, float minDifficulty, float maxDifficulty)
        {
            var newDifficulty = currentDifficulty + adjustment;
            return Mathf.Clamp(newDifficulty, minDifficulty, maxDifficulty);
        }

        /// <summary>
        /// Gets the difficulty level from a numeric difficulty value (pure function)
        /// </summary>
        /// <param name="difficulty">Numeric difficulty value</param>
        /// <returns>Difficulty level category</returns>
        public DifficultyLevel GetDifficultyLevel(float difficulty)
        {
            if (difficulty <= 3f)
                return DifficultyLevel.Easy;
            else if (difficulty <= 7f)
                return DifficultyLevel.Medium;
            else
                return DifficultyLevel.Hard;
        }

        /// <summary>
        /// Calculates new difficulty based on current difficulty and modifiers (pure function)
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty value</param>
        /// <param name="modifierResults">Modifier calculation results</param>
        /// <returns>Calculated difficulty value</returns>
        public float CalculateDifficulty(float currentDifficulty, List<ModifierResult> modifierResults)
        {
            var totalAdjustment = 0f;

            if (modifierResults != null)
            {
                totalAdjustment = modifierResults.Sum(result => result.Value);
            }

            var newDifficulty = currentDifficulty + totalAdjustment;

            // Apply max change per session if config is available
            if (this.config != null)
            {
                var maxChange = this.config.MaxChangePerSession;
                var actualChange = newDifficulty - currentDifficulty;

                if (Mathf.Abs(actualChange) > maxChange)
                {
                    newDifficulty = currentDifficulty + Mathf.Sign(actualChange) * maxChange;
                }
            }

            return this.ClampDifficulty(newDifficulty);
        }

        /// <summary>
        /// Clamps a difficulty value to valid range (pure function)
        /// </summary>
        /// <param name="difficulty">Difficulty value to clamp</param>
        /// <returns>Clamped difficulty value</returns>
        public float ClampDifficulty(float difficulty)
        {
            var min = this.config?.MinDifficulty ?? DifficultyConstants.MIN_DIFFICULTY;
            var max = this.config?.MaxDifficulty ?? DifficultyConstants.MAX_DIFFICULTY;
            return Mathf.Clamp(difficulty, min, max);
        }

        /// <summary>
        /// Gets the default difficulty value (pure function)
        /// </summary>
        /// <returns>Default difficulty value</returns>
        public float GetDefaultDifficulty()
        {
            return this.config?.DefaultDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        /// <summary>
        /// Checks if a difficulty value is within valid range (pure function)
        /// </summary>
        /// <param name="difficulty">Difficulty value to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidDifficulty(float difficulty)
        {
            var min = this.config?.MinDifficulty ?? DifficultyConstants.MIN_DIFFICULTY;
            var max = this.config?.MaxDifficulty ?? DifficultyConstants.MAX_DIFFICULTY;
            return difficulty >= min && difficulty <= max;
        }

        /// <summary>
        /// Calculates the percentage of difficulty within the valid range (pure function)
        /// </summary>
        /// <param name="difficulty">Difficulty value</param>
        /// <returns>Percentage from 0 to 1</returns>
        public float GetDifficultyPercentage(float difficulty)
        {
            var min = this.config?.MinDifficulty ?? DifficultyConstants.MIN_DIFFICULTY;
            var max = this.config?.MaxDifficulty ?? DifficultyConstants.MAX_DIFFICULTY;

            if (max <= min) return 0f;

            var clamped = this.ClampDifficulty(difficulty);
            return (clamped - min) / (max - min);
        }
    }
}
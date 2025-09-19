using System;
using System.Collections.Generic;
using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Manages difficulty calculations and adjustments
    /// </summary>
    public class DifficultyManager
    {
        private readonly DifficultyConfig config;
        private float currentDifficulty;

        public float CurrentDifficulty => currentDifficulty;

        /// <summary>
        /// Default constructor using default difficulty
        /// </summary>
        public DifficultyManager()
        {
            this.config = null;
            this.currentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
        }

        /// <summary>
        /// Constructor with configuration
        /// </summary>
        /// <param name="config">Difficulty configuration</param>
        public DifficultyManager(DifficultyConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.currentDifficulty = config.DefaultDifficulty;
        }

        /// <summary>
        /// Adjusts difficulty based on a modifier value
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty value</param>
        /// <param name="adjustment">Adjustment to apply</param>
        /// <param name="minDifficulty">Minimum allowed difficulty</param>
        /// <param name="maxDifficulty">Maximum allowed difficulty</param>
        /// <returns>New difficulty value</returns>
        public float AdjustDifficulty(float currentDifficulty, float adjustment, float minDifficulty, float maxDifficulty)
        {
            float newDifficulty = currentDifficulty + adjustment;
            return Mathf.Clamp(newDifficulty, minDifficulty, maxDifficulty);
        }

        /// <summary>
        /// Gets the difficulty level from a numeric difficulty value
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
        /// Gets the current difficulty level
        /// </summary>
        /// <returns>Current difficulty level category</returns>
        public DifficultyLevel GetDifficultyLevel()
        {
            return GetDifficultyLevel(currentDifficulty);
        }

        /// <summary>
        /// Sets the current difficulty
        /// </summary>
        /// <param name="difficulty">New difficulty value</param>
        public void SetDifficulty(float difficulty)
        {
            currentDifficulty = Mathf.Clamp(difficulty, DifficultyConstants.MIN_DIFFICULTY, DifficultyConstants.MAX_DIFFICULTY);
        }

        /// <summary>
        /// Calculates new difficulty based on session data and modifiers
        /// </summary>
        /// <param name="sessionData">Current session data</param>
        /// <param name="modifierResults">Modifier calculation results</param>
        /// <returns>Calculated difficulty value</returns>
        public float CalculateDifficulty(PlayerSessionData sessionData, List<ModifierResult> modifierResults)
        {
            float totalAdjustment = 0f;

            if (modifierResults != null)
            {
                foreach (var result in modifierResults)
                {
                    totalAdjustment += result.Value;
                }
            }

            float newDifficulty = currentDifficulty + totalAdjustment;

            // Apply max change per session if config is available
            if (config != null)
            {
                float maxChange = config.MaxChangePerSession;
                float actualChange = newDifficulty - currentDifficulty;

                if (Mathf.Abs(actualChange) > maxChange)
                {
                    newDifficulty = currentDifficulty + Mathf.Sign(actualChange) * maxChange;
                }
            }

            return newDifficulty;
        }

        /// <summary>
        /// Applies a calculated difficulty value
        /// </summary>
        /// <param name="difficulty">Difficulty value to apply</param>
        public void ApplyDifficulty(float difficulty)
        {
            float min = config?.MinDifficulty ?? DifficultyConstants.MIN_DIFFICULTY;
            float max = config?.MaxDifficulty ?? DifficultyConstants.MAX_DIFFICULTY;

            currentDifficulty = Mathf.Clamp(difficulty, min, max);
        }

        /// <summary>
        /// Resets difficulty to default
        /// </summary>
        public void ResetDifficulty()
        {
            currentDifficulty = config?.DefaultDifficulty ?? DifficultyConstants.DEFAULT_DIFFICULTY;
        }
    }
}
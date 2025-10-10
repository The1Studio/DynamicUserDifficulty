using System;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Game statistics used to generate optimal modifier configurations.
    /// Fill these values based on your game's player data and design parameters.
    /// </summary>
    [Serializable]
    public struct GameStats
    {
        #region Player Behavior Stats

        [Header("Player Behavior")]
        [Tooltip("Average number of consecutive wins before a loss (e.g., 3.5)")]
        [SerializeField] public float avgConsecutiveWins;

        [Tooltip("Average number of consecutive losses before a win (e.g., 2.0)")]
        [SerializeField] public float avgConsecutiveLosses;

        [Tooltip("Overall win rate as a percentage (0-100, e.g., 65.0 for 65%)")]
        [SerializeField][Range(0f, 100f)] public float winRatePercentage;

        [Tooltip("Average number of attempts per level before completion (e.g., 2.5)")]
        [SerializeField] public float avgAttemptsPerLevel;

        #endregion

        #region Session & Time Stats

        [Header("Session & Time")]
        [Tooltip("Average hours between play sessions (e.g., 24.0 for daily players)")]
        [SerializeField] public float avgHoursBetweenSessions;

        [Tooltip("Average session duration in minutes (e.g., 15.0)")]
        [SerializeField] public float avgSessionDurationMinutes;

        [Tooltip("Average number of levels completed per session (e.g., 5.0)")]
        [SerializeField] public float avgLevelsPerSession;

        [Tooltip("Percentage of sessions that end in rage quits (0-100, e.g., 10.0 for 10%)")]
        [SerializeField][Range(0f, 100f)] public float rageQuitPercentage;

        #endregion

        #region Level Design Stats

        [Header("Level Design")]
        [Tooltip("Minimum difficulty value for your game (e.g., 1.0)")]
        [SerializeField] public float difficultyMin;

        [Tooltip("Maximum difficulty value for your game (e.g., 10.0)")]
        [SerializeField] public float difficultyMax;

        [Tooltip("Starting difficulty for new players (e.g., 3.0)")]
        [SerializeField] public float difficultyDefault;

        [Tooltip("Average time to complete a level in seconds (e.g., 60.0)")]
        [SerializeField] public float avgLevelCompletionTimeSeconds;

        #endregion

        #region Progression Stats

        [Header("Progression")]
        [Tooltip("Total number of levels in your game (e.g., 100)")]
        [SerializeField] public int totalLevels;

        [Tooltip("Average level number where players start struggling (e.g., 20)")]
        [SerializeField] public int difficultyIncreaseStartLevel;

        [Tooltip("How many days you want to retain returning players (e.g., 7)")]
        [SerializeField] public int targetRetentionDays;

        [Tooltip("Maximum difficulty change allowed per session (e.g., 2.0)")]
        [SerializeField] public float maxDifficultyChangePerSession;

        [Tooltip("Percentage of players who complete the entire game (0-100, e.g., 5.0 for 5%)")]
        [SerializeField][Range(0f, 100f)] public float gameCompletionRate;

        #endregion

        /// <summary>
        /// Validates the game stats to ensure they are within reasonable ranges.
        /// Comprehensive validation for all 17 fields to prevent division by zero and invalid configs.
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            // Player Behavior validations
            if (this.avgConsecutiveWins <= 0)
            {
                errorMessage = "Average consecutive wins must be greater than 0";
                return false;
            }

            if (this.avgConsecutiveLosses <= 0)
            {
                errorMessage = "Average consecutive losses must be greater than 0";
                return false;
            }

            if (this.winRatePercentage < 0 || this.winRatePercentage > 100)
            {
                errorMessage = "Win rate percentage must be between 0 and 100";
                return false;
            }

            if (this.avgAttemptsPerLevel <= 0)
            {
                errorMessage = "Average attempts per level must be greater than 0";
                return false;
            }

            // Session & Time validations
            if (this.avgHoursBetweenSessions <= 0)
            {
                errorMessage = "Average hours between sessions must be greater than 0";
                return false;
            }

            if (this.avgSessionDurationMinutes <= 0)
            {
                errorMessage = "Average session duration must be greater than 0 minutes";
                return false;
            }

            if (this.avgLevelsPerSession <= 0)
            {
                errorMessage = "Average levels per session must be greater than 0";
                return false;
            }

            if (this.rageQuitPercentage < 0 || this.rageQuitPercentage > 100)
            {
                errorMessage = "Rage quit percentage must be between 0 and 100";
                return false;
            }

            // Level Design validations
            if (this.difficultyMin >= this.difficultyMax)
            {
                errorMessage = "Difficulty min must be less than difficulty max";
                return false;
            }

            if (this.difficultyDefault < this.difficultyMin || this.difficultyDefault > this.difficultyMax)
            {
                errorMessage = "Difficulty default must be between min and max";
                return false;
            }

            if (this.avgLevelCompletionTimeSeconds <= 0)
            {
                errorMessage = "Average level completion time must be greater than 0 seconds";
                return false;
            }

            // Progression validations
            if (this.totalLevels <= 0)
            {
                errorMessage = "Total levels must be greater than 0";
                return false;
            }

            if (this.difficultyIncreaseStartLevel < 0)
            {
                errorMessage = "Difficulty increase start level cannot be negative";
                return false;
            }

            if (this.difficultyIncreaseStartLevel > this.totalLevels)
            {
                errorMessage = $"Difficulty increase start level ({this.difficultyIncreaseStartLevel}) cannot exceed total levels ({this.totalLevels})";
                return false;
            }

            if (this.targetRetentionDays <= 0)
            {
                errorMessage = "Target retention days must be greater than 0";
                return false;
            }

            if (this.maxDifficultyChangePerSession <= 0)
            {
                errorMessage = "Max difficulty change per session must be greater than 0";
                return false;
            }

            if (this.gameCompletionRate < 0 || this.gameCompletionRate > 100)
            {
                errorMessage = "Game completion rate must be between 0 and 100";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Creates a default GameStats instance with typical mobile game values.
        /// </summary>
        public static GameStats CreateDefault()
        {
            return new GameStats
            {
                // Player Behavior
                avgConsecutiveWins = 3.5f,
                avgConsecutiveLosses = 2.0f,
                winRatePercentage = 65f,
                avgAttemptsPerLevel = 2.5f,

                // Session & Time
                avgHoursBetweenSessions = 24f,
                avgSessionDurationMinutes = 15f,
                avgLevelsPerSession = 5f,
                rageQuitPercentage = 10f,

                // Level Design
                difficultyMin = 1f,
                difficultyMax = 10f,
                difficultyDefault = 3f,
                avgLevelCompletionTimeSeconds = 60f,

                // Progression
                totalLevels = 100,
                difficultyIncreaseStartLevel = 20,
                targetRetentionDays = 7,
                maxDifficultyChangePerSession = 2f,
                gameCompletionRate = 5f
            };
        }
    }
}

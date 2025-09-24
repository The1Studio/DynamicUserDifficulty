using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    using System;

    /// <summary>
    /// Stateless calculation service for dynamic difficulty.
    /// This service does NOT store any data - it only calculates difficulty based on input data.
    /// All data should be retrieved from external services (player profile, analytics, etc.)
    /// </summary>
    public interface IDynamicDifficultyService
    {
        /// <summary>
        /// Calculates the recommended difficulty based on provided player data.
        /// This is a pure function - same input always produces same output.
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty from external service</param>
        /// <param name="sessionData">Player session data from external service</param>
        /// <returns>Calculated difficulty result with adjustment details</returns>
        DifficultyResult CalculateDifficulty(float currentDifficulty, PlayerSessionData sessionData);

        /// <summary>
        /// Calculates difficulty adjustment for a specific scenario.
        /// Useful for preview or what-if analysis.
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty level</param>
        /// <param name="winStreak">Number of consecutive wins</param>
        /// <param name="lossStreak">Number of consecutive losses</param>
        /// <param name="hoursSinceLastPlay">Hours since last play session</param>
        /// <param name="lastQuitType">How the last session ended</param>
        /// <returns>Calculated difficulty adjustment</returns>
        float CalculateAdjustment(
            float currentDifficulty,
            int winStreak = 0,
            int lossStreak = 0,
            float hoursSinceLastPlay = 0,
            QuitType? lastQuitType = null);

        /// <summary>
        /// Gets the recommended difficulty for a new player.
        /// </summary>
        /// <returns>Default starting difficulty</returns>
        float GetDefaultDifficulty();

        /// <summary>
        /// Validates if a difficulty value is within acceptable range.
        /// </summary>
        /// <param name="difficulty">Difficulty value to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidDifficulty(float difficulty);

        /// <summary>
        /// Clamps a difficulty value to the valid range.
        /// </summary>
        /// <param name="difficulty">Difficulty value to clamp</param>
        /// <returns>Clamped difficulty value</returns>
        float ClampDifficulty(float difficulty);

        /// <summary>
        /// Determines quit type based on session data.
        /// This method analyzes session behavior to classify how/why the player ended their session.
        /// </summary>
        /// <param name="sessionDuration">Duration of the session in seconds</param>
        /// <param name="wasLastLevelWon">Whether the last level played was completed successfully</param>
        /// <param name="lastLevelEndTime">When the last level ended</param>
        /// <returns>Classified quit type</returns>
        QuitType DetermineQuitType(float sessionDuration, bool wasLastLevelWon, DateTime lastLevelEndTime);
    }
}
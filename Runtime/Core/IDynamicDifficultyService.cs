using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Stateless calculation service for dynamic difficulty.
    /// This service does NOT store any data - it only calculates and returns a difficulty float value.
    /// All data should be retrieved from external services via providers.
    /// </summary>
    public interface IDynamicDifficultyService
    {
        /// <summary>
        /// Calculates the recommended difficulty based on provided player data.
        /// This is a pure function - same input always produces same output.
        /// </summary>
        /// <param name="currentDifficulty">Current difficulty from external service</param>
        /// <param name="sessionData">Player session data from external service (can be null to use providers)</param>
        /// <returns>Calculated difficulty result with adjustment details</returns>
        DifficultyResult CalculateDifficulty(float currentDifficulty, PlayerSessionData sessionData);

        /// <summary>
        /// Gets the recommended difficulty for a new player.
        /// </summary>
        /// <returns>Default starting difficulty</returns>
        float GetDefaultDifficulty();

        /// <summary>
        /// Clamps a difficulty value to the valid range.
        /// </summary>
        /// <param name="difficulty">Difficulty value to clamp</param>
        /// <returns>Clamped difficulty value</returns>
        float ClampDifficulty(float difficulty);
    }
}
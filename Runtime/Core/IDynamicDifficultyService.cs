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
        /// Gets the current difficulty value from the provider.
        /// </summary>
        float CurrentDifficulty { get; }

        /// <summary>
        /// Calculates the recommended difficulty based on provider data.
        /// Uses IDifficultyDataProvider internally to get/set difficulty.
        /// </summary>
        /// <param name="sessionData">Player session data from external service (can be null to use providers)</param>
        /// <returns>Calculated difficulty result with adjustment details</returns>
        DifficultyResult CalculateDifficulty(PlayerSessionData sessionData = null);

        /// <summary>
        /// Applies the calculated difficulty result to the provider.
        /// </summary>
        /// <param name="result">The difficulty result to apply</param>
        void ApplyDifficulty(DifficultyResult result);

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
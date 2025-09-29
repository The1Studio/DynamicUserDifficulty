using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Stateless calculation service for dynamic difficulty.
    /// This service does NOT store any data - it only calculates and returns a difficulty float value.
    /// All data should be retrieved from external services via providers.
    /// </summary>
    /// <summary>
    /// Main service interface for dynamic difficulty calculation.
    /// This is a stateless calculation engine that uses provider interfaces for all data.
    /// </summary>
    public interface IDynamicDifficultyService
    {
        /// <summary>
        /// Gets the current difficulty value (1-10 scale)
        /// </summary>
        float CurrentDifficulty { get; }

        /// <summary>
        /// Calculates new difficulty based on data from provider interfaces.
        /// This is a pure function that doesn't modify state.
        /// </summary>
        /// <returns>Result containing new difficulty and calculation details</returns>
        DifficultyResult CalculateDifficulty();

        /// <summary>
        /// Applies a calculated difficulty result, updating the current difficulty value.
        /// This is the only method that modifies state.
        /// </summary>
        /// <param name="result">The calculated result to apply</param>
        void ApplyDifficulty(DifficultyResult result);

        /// <summary>
        /// Gets the default difficulty value from configuration
        /// </summary>
        /// <returns>Default difficulty value</returns>
        float GetDefaultDifficulty();

        /// <summary>
        /// Clamps a difficulty value to the configured min/max range
        /// </summary>
        /// <param name="difficulty">Value to clamp</param>
        /// <returns>Clamped difficulty value</returns>
        float ClampDifficulty(float difficulty);
    }
}
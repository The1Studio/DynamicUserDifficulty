#nullable enable

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provider interface for managing the current difficulty value.
    /// This module ONLY stores the current difficulty - all other data comes from external services.
    /// </summary>
    public interface IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the current difficulty level (typically 1-10 scale)
        /// </summary>
        float GetCurrentDifficulty();

        /// <summary>
        /// Updates the current difficulty level after calculation
        /// </summary>
        /// <param name="newDifficulty">The new calculated difficulty value</param>
        void SetCurrentDifficulty(float newDifficulty);
    }
}
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Base provider interface for common difficulty data operations
    /// All modifier providers extend this interface
    /// </summary>
    public interface IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the current player session data
        /// </summary>
        PlayerSessionData GetSessionData();

        /// <summary>
        /// Saves player session data
        /// </summary>
        void SaveSessionData(PlayerSessionData data);

        /// <summary>
        /// Gets the current difficulty level
        /// </summary>
        float GetCurrentDifficulty();

        /// <summary>
        /// Saves the current difficulty level
        /// </summary>
        void SaveDifficulty(float difficulty);

        /// <summary>
        /// Clears all stored data (for testing/reset purposes)
        /// </summary>
        void ClearData();
    }
}
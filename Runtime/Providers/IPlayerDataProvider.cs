using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Interface for player data persistence including difficulty and session data
    /// </summary>
    public interface IPlayerDataProvider
    {
        /// <summary>
        /// Loads the current player session data
        /// </summary>
        /// <returns>Player session data or null if not found</returns>
        PlayerSessionData LoadSessionData();

        /// <summary>
        /// Saves player session data
        /// </summary>
        /// <param name="data">Session data to save</param>
        void SaveSessionData(PlayerSessionData data);

        /// <summary>
        /// Loads the current difficulty value
        /// </summary>
        /// <returns>Current difficulty value</returns>
        float LoadDifficulty();

        /// <summary>
        /// Saves the difficulty value
        /// </summary>
        /// <param name="difficulty">Difficulty value to save</param>
        void SaveDifficulty(float difficulty);

        /// <summary>
        /// Clears all player data
        /// </summary>
        void ClearData();
    }
}
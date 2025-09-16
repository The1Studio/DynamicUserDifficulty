using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Interface for session data persistence
    /// </summary>
    public interface ISessionDataProvider
    {
        /// <summary>
        /// Gets the current session data
        /// </summary>
        /// <returns>Current player session data</returns>
        PlayerSessionData GetCurrentSession();

        /// <summary>
        /// Saves session data to persistence
        /// </summary>
        /// <param name="data">Session data to save</param>
        void SaveSession(PlayerSessionData data);

        /// <summary>
        /// Updates the win streak count
        /// </summary>
        /// <param name="streak">New win streak value</param>
        void UpdateWinStreak(int streak);

        /// <summary>
        /// Updates the loss streak count
        /// </summary>
        /// <param name="streak">New loss streak value</param>
        void UpdateLossStreak(int streak);

        /// <summary>
        /// Records how a session ended
        /// </summary>
        /// <param name="endType">The type of session end</param>
        void RecordSessionEnd(SessionEndType endType);

        /// <summary>
        /// Clears all session data
        /// </summary>
        void ClearData();
    }
}
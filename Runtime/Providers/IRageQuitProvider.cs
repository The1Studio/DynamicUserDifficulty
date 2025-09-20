using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provider interface for rage quit detection data
    /// Implement this interface to enable rage quit difficulty adjustments
    /// </summary>
    public interface IRageQuitProvider : IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the type of the last quit (normal, rage quit, etc.)
        /// </summary>
        QuitType GetLastQuitType();

        /// <summary>
        /// Gets the average session duration for recent sessions
        /// </summary>
        float GetAverageSessionDuration();

        /// <summary>
        /// Records the end of a session with quit type and duration
        /// </summary>
        void RecordSessionEnd(QuitType quitType, float durationSeconds);

        /// <summary>
        /// Gets the time spent on current level/attempt
        /// </summary>
        float GetCurrentSessionDuration();

        /// <summary>
        /// Gets the number of rage quits in recent history
        /// </summary>
        int GetRecentRageQuitCount();

        /// <summary>
        /// Records the start of a new session
        /// </summary>
        void RecordSessionStart();
    }
}
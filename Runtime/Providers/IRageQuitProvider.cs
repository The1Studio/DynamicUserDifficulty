using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Read-only provider interface for rage quit detection data from external services.
    /// This interface does NOT store data - it only reads from external game services.
    /// Implement this interface to enable rage quit difficulty adjustments.
    /// </summary>
    public interface IRageQuitProvider
    {
        /// <summary>
        /// Gets the type of the last quit (normal, rage quit, etc.) from external service
        /// </summary>
        QuitType GetLastQuitType();

        /// <summary>
        /// Gets the average session duration for recent sessions from external service
        /// </summary>
        float GetAverageSessionDuration();

        /// <summary>
        /// Gets the time spent on current level/attempt from external service
        /// </summary>
        float GetCurrentSessionDuration();

        /// <summary>
        /// Gets the number of rage quits in recent history from external service
        /// </summary>
        int GetRecentRageQuitCount();
    }
}
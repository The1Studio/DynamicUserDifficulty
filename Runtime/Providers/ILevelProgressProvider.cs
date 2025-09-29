namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Read-only provider interface for level progress data from external services.
    /// This interface does NOT store data - it only reads from external game services.
    /// Implement this interface to enable level-based difficulty adjustments.
    /// </summary>
    public interface ILevelProgressProvider
    {
        /// <summary>
        /// Gets the current level number from external service
        /// </summary>
        int GetCurrentLevel();

        /// <summary>
        /// Gets the average completion time for recent levels from external service
        /// </summary>
        float GetAverageCompletionTime();

        /// <summary>
        /// Gets the number of attempts on the current level from external service
        /// </summary>
        int GetAttemptsOnCurrentLevel();

        /// <summary>
        /// Gets the completion rate (wins/total attempts) for recent levels from external service
        /// </summary>
        float GetCompletionRate();

        /// <summary>
        /// Gets the difficulty rating of the current level (if available) from external service
        /// </summary>
        float GetCurrentLevelDifficulty();

        /// <summary>
        /// Gets the performance percentage for current level completion time relative to time limit or average.
        /// Values < 1.0 = faster than expected, values > 1.0 = slower than expected
        /// </summary>
        float GetCurrentLevelTimePercentage();
    }
}
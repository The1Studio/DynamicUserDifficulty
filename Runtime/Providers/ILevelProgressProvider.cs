namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provider interface for level progress related data
    /// Implement this interface to enable level-based difficulty adjustments
    /// </summary>
    public interface ILevelProgressProvider : IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the current level number
        /// </summary>
        int GetCurrentLevel();

        /// <summary>
        /// Gets the average completion time for recent levels
        /// </summary>
        float GetAverageCompletionTime();

        /// <summary>
        /// Gets the number of attempts on the current level
        /// </summary>
        int GetAttemptsOnCurrentLevel();

        /// <summary>
        /// Gets the completion rate (wins/total attempts) for recent levels
        /// </summary>
        float GetCompletionRate();

        /// <summary>
        /// Records completion time for a level
        /// </summary>
        void RecordLevelCompletion(int levelId, float completionTime, bool won);

        /// <summary>
        /// Gets the difficulty rating of the current level (if available)
        /// </summary>
        float GetCurrentLevelDifficulty();
    }
}
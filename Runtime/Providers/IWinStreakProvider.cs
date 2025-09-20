namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Read-only provider interface for win/loss streak data from external services.
    /// This interface does NOT store data - it only reads from external game services.
    /// Implement this interface to enable win/loss streak difficulty modifications.
    /// </summary>
    public interface IWinStreakProvider
    {
        /// <summary>
        /// Gets the current win streak count from external service
        /// </summary>
        int GetWinStreak();

        /// <summary>
        /// Gets the current loss streak count from external service
        /// </summary>
        int GetLossStreak();

        /// <summary>
        /// Gets total number of wins in current session from external service
        /// </summary>
        int GetTotalWins();

        /// <summary>
        /// Gets total number of losses in current session from external service
        /// </summary>
        int GetTotalLosses();
    }
}
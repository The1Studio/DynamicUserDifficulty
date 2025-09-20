namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provider interface for win streak related data
    /// Implement this interface to enable win streak difficulty modifications
    /// </summary>
    public interface IWinStreakProvider : IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the current win streak count
        /// </summary>
        int GetWinStreak();

        /// <summary>
        /// Gets the current loss streak count
        /// </summary>
        int GetLossStreak();

        /// <summary>
        /// Records a win and updates streaks
        /// </summary>
        void RecordWin();

        /// <summary>
        /// Records a loss and updates streaks
        /// </summary>
        void RecordLoss();

        /// <summary>
        /// Gets total number of wins in current session
        /// </summary>
        int GetTotalWins();

        /// <summary>
        /// Gets total number of losses in current session
        /// </summary>
        int GetTotalLosses();
    }
}
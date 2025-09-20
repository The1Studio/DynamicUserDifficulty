using System;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Provider interface for time decay related data
    /// Implement this interface to enable time-based difficulty decay
    /// </summary>
    public interface ITimeDecayProvider : IDifficultyDataProvider
    {
        /// <summary>
        /// Gets the last time the player played
        /// </summary>
        DateTime GetLastPlayTime();

        /// <summary>
        /// Gets the time elapsed since last play session
        /// </summary>
        TimeSpan GetTimeSinceLastPlay();

        /// <summary>
        /// Records that a new play session has started
        /// </summary>
        void RecordPlaySession();

        /// <summary>
        /// Gets the number of consecutive days without playing
        /// </summary>
        int GetDaysAwayFromGame();
    }
}
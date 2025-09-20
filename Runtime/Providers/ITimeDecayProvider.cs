using System;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Read-only provider interface for time-based data from external services.
    /// This interface does NOT store data - it only reads from external game services.
    /// Implement this interface to enable time-based difficulty decay.
    /// </summary>
    public interface ITimeDecayProvider
    {
        /// <summary>
        /// Gets the last time the player played from external service
        /// </summary>
        DateTime GetLastPlayTime();

        /// <summary>
        /// Gets the time elapsed since last play session from external service
        /// </summary>
        TimeSpan GetTimeSinceLastPlay();

        /// <summary>
        /// Gets the number of consecutive days without playing from external service
        /// </summary>
        int GetDaysAwayFromGame();
    }
}
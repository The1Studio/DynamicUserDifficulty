#nullable enable

using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Read-only provider interface for detailed session pattern analysis.
    /// This interface does NOT store data - it only reads from external game services.
    /// Implement this interface to enable advanced session pattern difficulty adjustments.
    /// </summary>
    public interface ISessionPatternProvider
    {
        /// <summary>
        /// Gets the recent session durations from external service
        /// Used for analyzing patterns across multiple sessions
        /// </summary>
        /// <param name="count">Number of recent sessions to retrieve</param>
        /// <returns>List of session durations in seconds, most recent first</returns>
        List<float> GetRecentSessionDurations(int count);

        /// <summary>
        /// Gets the total number of quits in recent history
        /// </summary>
        int GetTotalRecentQuits();

        /// <summary>
        /// Gets the number of mid-level quits in recent history
        /// </summary>
        int GetRecentMidLevelQuits();

        /// <summary>
        /// Gets the difficulty value from before the last adjustment
        /// Used to track if difficulty changes are improving player experience
        /// </summary>
        float GetPreviousDifficulty();

        /// <summary>
        /// Gets the session duration from before the last difficulty adjustment
        /// Used to track if difficulty changes are improving session length
        /// </summary>
        float GetSessionDurationBeforeLastAdjustment();
    }
}
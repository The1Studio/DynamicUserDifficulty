using System;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Detailed session information for pattern analysis and difficulty adjustments
    /// </summary>
    [Serializable]
    public class DetailedSessionInfo
    {
        /// <summary>
        /// Session start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Session end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Session duration in seconds
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Reason for session end
        /// </summary>
        public SessionEndReason EndReason { get; set; }

        /// <summary>
        /// Number of levels completed in this session
        /// </summary>
        public int LevelsCompleted { get; set; }

        /// <summary>
        /// Number of levels failed in this session
        /// </summary>
        public int LevelsFailed { get; set; }

        /// <summary>
        /// Difficulty level at session start
        /// </summary>
        public float StartDifficulty { get; set; }

        /// <summary>
        /// Difficulty level at session end
        /// </summary>
        public float EndDifficulty { get; set; }

        /// <summary>
        /// Last level played in this session
        /// </summary>
        public int LastLevelPlayed { get; set; }

        /// <summary>
        /// Was the last level in this session completed
        /// </summary>
        public bool LastLevelWon { get; set; }

        /// <summary>
        /// Average completion percentage for levels in this session
        /// </summary>
        public float AverageCompletionPercentage { get; set; }

        /// <summary>
        /// Number of rage quits detected in this session
        /// </summary>
        public int RageQuitCount { get; set; }
    }

    /// <summary>
    /// Reasons for session ending
    /// </summary>
    public enum SessionEndReason
    {
        /// <summary>
        /// Normal app close or pause
        /// </summary>
        Normal,

        /// <summary>
        /// Completed a level and quit
        /// </summary>
        CompletedLevel,

        /// <summary>
        /// Failed a level and quit
        /// </summary>
        FailedLevel,

        /// <summary>
        /// Quit during gameplay (potential frustration)
        /// </summary>
        QuitMidLevel,

        /// <summary>
        /// Rage quit detected (very short session after failure)
        /// </summary>
        RageQuit,

        /// <summary>
        /// Session timeout or inactivity
        /// </summary>
        Timeout,

        /// <summary>
        /// App crashed or force closed
        /// </summary>
        Crash,

        /// <summary>
        /// Unknown reason
        /// </summary>
        Unknown
    }
}
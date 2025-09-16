namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Constants used throughout the difficulty system
    /// </summary>
    public static class DifficultyConstants
    {
        // Difficulty range
        public const float MIN_DIFFICULTY = 1f;
        public const float MAX_DIFFICULTY = 10f;
        public const float DEFAULT_DIFFICULTY = 3f;

        // Modifier defaults
        public const float DEFAULT_WIN_THRESHOLD = 3f;
        public const float DEFAULT_LOSS_THRESHOLD = 2f;
        public const float DEFAULT_STEP_SIZE = 0.5f;
        public const float DEFAULT_MAX_CHANGE = 2f;

        // Time thresholds
        public const float HOURS_IN_DAY = 24f;
        public const float DEFAULT_GRACE_HOURS = 6f;
        public const float RAGE_QUIT_SECONDS = 30f;

        // Session limits
        public const int MAX_RECENT_SESSIONS = 10;
        public const int MIN_SESSIONS_FOR_TREND = 3;

        // PlayerPrefs keys
        public const string PREFS_CURRENT_DIFFICULTY = "DUD_CurrentDifficulty";
        public const string PREFS_WIN_STREAK = "DUD_WinStreak";
        public const string PREFS_LOSS_STREAK = "DUD_LossStreak";
        public const string PREFS_LAST_PLAY_TIME = "DUD_LastPlayTime";
        public const string PREFS_SESSION_DATA = "DUD_SessionData";
    }
}
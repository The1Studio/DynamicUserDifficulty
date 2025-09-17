namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Constants used throughout the difficulty system
    /// </summary>
    public static class DifficultyConstants
    {
        // ========== DIFFICULTY RANGE ==========
        public const float MIN_DIFFICULTY = 1f;
        public const float MAX_DIFFICULTY = 10f;
        public const float DEFAULT_DIFFICULTY = 3f;
        public const float DEFAULT_MAX_CHANGE_PER_SESSION = 2f;

        // Config UI ranges for Inspector
        public const float CONFIG_MIN_RANGE = 1f;
        public const float CONFIG_MAX_RANGE = 10f;
        public const float CONFIG_CHANGE_MIN_RANGE = 0.5f;
        public const float CONFIG_CHANGE_MAX_RANGE = 5f;

        // ========== WIN STREAK MODIFIER ==========
        public const float WIN_STREAK_DEFAULT_THRESHOLD = 3f;
        public const float WIN_STREAK_DEFAULT_STEP_SIZE = 0.5f;
        public const float WIN_STREAK_DEFAULT_MAX_BONUS = 2f;

        // ========== LOSS STREAK MODIFIER ==========
        public const float LOSS_STREAK_DEFAULT_THRESHOLD = 2f;
        public const float LOSS_STREAK_DEFAULT_STEP_SIZE = 0.3f;
        public const float LOSS_STREAK_DEFAULT_MAX_REDUCTION = 1.5f;

        // ========== TIME DECAY MODIFIER ==========
        public const float TIME_DECAY_DEFAULT_PER_DAY = 0.5f;
        public const float TIME_DECAY_DEFAULT_MAX = 2f;
        public const float TIME_DECAY_DEFAULT_GRACE_HOURS = 6f;
        public const float TIME_DECAY_WEEK_THRESHOLD = 7f;
        public const float TIME_DECAY_HALF_EFFECT_MULTIPLIER = 0.5f;

        // ========== RAGE QUIT MODIFIER ==========
        public const float RAGE_QUIT_TIME_THRESHOLD = 30f; // seconds
        public const float RAGE_QUIT_DEFAULT_REDUCTION = 1f;
        public const float QUIT_DEFAULT_REDUCTION = 0.5f;
        public const float MID_PLAY_DEFAULT_REDUCTION = 0.3f;
        public const float MID_PLAY_PARTIAL_MULTIPLIER = 0.5f;

        // ========== TIME CONSTANTS ==========
        public const float HOURS_IN_DAY = 24f;
        public const int DAYS_IN_WEEK = 7;

        // ========== SESSION MANAGEMENT ==========
        public const int MAX_RECENT_SESSIONS = 10;
        public const int MIN_SESSIONS_FOR_TREND = 3;
        public const int CACHE_EXPIRY_MINUTES = 5;

        // ========== AGGREGATION ==========
        public const float DEFAULT_AGGREGATION_WEIGHT = 1f;
        public const float DEFAULT_DIMINISHING_FACTOR = 0.5f;

        // ========== PRECISION ==========
        public const float EPSILON = 0.01f; // For float comparisons
        public const float ZERO_VALUE = 0f;

        // ========== MODIFIER PRIORITIES ==========
        public const int DEFAULT_MODIFIER_PRIORITY = 0;

        // ========== ANIMATION CURVES ==========
        public const float CURVE_START_TIME = 0f;
        public const float CURVE_START_VALUE = 0f;
        public const float CURVE_END_TIME = 1f;
        public const float CURVE_END_VALUE = 1f;

        // ========== PLAYERPREFS KEYS ==========
        public const string PREFS_CURRENT_DIFFICULTY = "DUD_CurrentDifficulty";
        public const string PREFS_WIN_STREAK = "DUD_WinStreak";
        public const string PREFS_LOSS_STREAK = "DUD_LossStreak";
        public const string PREFS_LAST_PLAY_TIME = "DUD_LastPlayTime";
        public const string PREFS_SESSION_DATA = "DUD_SessionData";

        // ========== PARAMETER KEYS ==========
        public const string PARAM_WIN_THRESHOLD = "WinThreshold";
        public const string PARAM_LOSS_THRESHOLD = "LossThreshold";
        public const string PARAM_STEP_SIZE = "StepSize";
        public const string PARAM_MAX_BONUS = "MaxBonus";
        public const string PARAM_MAX_REDUCTION = "MaxReduction";
        public const string PARAM_DECAY_PER_DAY = "DecayPerDay";
        public const string PARAM_MAX_DECAY = "MaxDecay";
        public const string PARAM_GRACE_HOURS = "GraceHours";
        public const string PARAM_RAGE_QUIT_THRESHOLD = "RageQuitThreshold";
        public const string PARAM_RAGE_QUIT_REDUCTION = "RageQuitReduction";
        public const string PARAM_QUIT_REDUCTION = "QuitReduction";
        public const string PARAM_MID_PLAY_REDUCTION = "MidPlayReduction";
    }
}
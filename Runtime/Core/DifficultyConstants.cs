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
        public const float DEFAULT_MAX_CHANGE = 2f; // Alias for backwards compatibility

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
        public const float TIME_DECAY_DEFAULT_DECAY_PER_DAY = 0.5f;
        public const float TIME_DECAY_DEFAULT_PER_DAY = 0.5f; // Alias for backwards compatibility
        public const float TIME_DECAY_DEFAULT_MAX_DECAY = 2f;
        public const float TIME_DECAY_DEFAULT_MAX = 2f; // Alias for backwards compatibility
        public const float TIME_DECAY_DEFAULT_GRACE_HOURS = 6f;
        public const float TIME_DECAY_WEEK_THRESHOLD = 7f;
        public const float TIME_DECAY_HALF_EFFECT_MULTIPLIER = 0.5f;

        // ========== RAGE QUIT MODIFIER ==========
        public const float RAGE_QUIT_DEFAULT_THRESHOLD = 30f; // seconds
        public const float RAGE_QUIT_TIME_THRESHOLD = 30f; // Alias for backwards compatibility
        public const float RAGE_QUIT_DEFAULT_REDUCTION = 1f;
        public const float RAGE_QUIT_DEFAULT_QUIT_REDUCTION = 0.5f;
        public const float QUIT_DEFAULT_REDUCTION = 0.5f; // Alias for backwards compatibility
        public const float RAGE_QUIT_DEFAULT_MID_PLAY_REDUCTION = 0.3f;
        public const float MID_PLAY_DEFAULT_REDUCTION = 0.3f; // Alias for backwards compatibility
        public const float MID_PLAY_PARTIAL_MULTIPLIER = 0.5f;

        // ========== TIME CONSTANTS ==========
        public const float HOURS_IN_DAY = 24f;
        public const int DAYS_IN_WEEK = 7;

        // ========== SESSION MANAGEMENT ==========
        public const int MAX_RECENT_SESSIONS = 10;
        public const int MIN_SESSIONS_FOR_TREND = 3;
        public const int CACHE_EXPIRY_MINUTES = 5;
        public const float MIN_SESSION_DURATION = 120f; // Minimum session duration in seconds (2 minutes)

        // ========== AGGREGATION ==========
        public const float DEFAULT_AGGREGATION_WEIGHT = 1f;
        public const float DEFAULT_DIMINISHING_FACTOR = 0.5f;

        // ========== PRECISION ==========
        public const float EPSILON = 0.01f; // For float comparisons
        public const float ZERO_VALUE = 0f; // Still used by aggregator and modifiers

        // ========== MODIFIER PRIORITIES ==========
        public const int DEFAULT_MODIFIER_PRIORITY = 0;

        // ========== ANIMATION CURVES ==========
        public const float CURVE_START_TIME = 0f;  // Still used by ModifierConfig
        public const float CURVE_START_VALUE = 0f; // Still used by ModifierConfig
        public const float CURVE_END_TIME = 1f;    // Still used by ModifierConfig
        public const float CURVE_END_VALUE = 1f;   // Still used by ModifierConfig

        // ========== MODIFIER TYPE NAMES ==========
        /// <summary>Identifier for Win Streak modifier in configurations</summary>
        public const string MODIFIER_TYPE_WIN_STREAK = "WinStreak";

        /// <summary>Identifier for Loss Streak modifier in configurations</summary>
        public const string MODIFIER_TYPE_LOSS_STREAK = "LossStreak";

        /// <summary>Identifier for Time Decay modifier in configurations</summary>
        public const string MODIFIER_TYPE_TIME_DECAY = "TimeDecay";

        /// <summary>Identifier for Rage Quit modifier in configurations</summary>
        public const string MODIFIER_TYPE_RAGE_QUIT = "RageQuit";

        /// <summary>Identifier for Completion Rate modifier in configurations</summary>
        public const string MODIFIER_TYPE_COMPLETION_RATE = "CompletionRate";

        /// <summary>Identifier for Level Progress modifier in configurations</summary>
        public const string MODIFIER_TYPE_LEVEL_PROGRESS = "LevelProgress";

        /// <summary>Identifier for Session Pattern modifier in configurations</summary>
        public const string MODIFIER_TYPE_SESSION_PATTERN = "SessionPattern";

        // ========== CONFIG PATH ==========
        /// <summary>Resources path for loading DifficultyConfig (only location)</summary>
        public const string CONFIG_RESOURCES_PATH = "GameConfigs/DifficultyConfig";

        /// <summary>Asset path for creating/finding DifficultyConfig</summary>
        public const string CONFIG_ASSET_PATH = "Assets/Resources/GameConfigs/DifficultyConfig.asset";

        /// <summary>Directory path for config assets</summary>
        public const string CONFIG_DIRECTORY = "Assets/Resources/GameConfigs";

        // ========== UNITY MENU PATHS ==========
        /// <summary>Unity Create Asset menu path for DifficultyConfig</summary>
        public const string MENU_CREATE_ASSET = "DynamicDifficulty/Config";

        // ========== RESET VALUES ==========
        /// <summary>Value used to reset win/loss streaks to zero</summary>
        public const int STREAK_RESET_VALUE = 0; // Still used by PlayerSessionData

        // ========== FOLDER NAMES ==========
        /// <summary>Base Assets folder name for Unity folder operations</summary>
        public const string FOLDER_NAME_ASSETS = "Assets";

        /// <summary>Resources folder name for Unity folder operations</summary>
        public const string FOLDER_NAME_RESOURCES = "Resources";

        // ========== INTEGRATION PATHS ==========
        /// <summary>Path to GameLifetimeScope for integration validation</summary>
        public const string INTEGRATION_GAMELIFETIMESCOPE_PATH = "Assets/Scripts/GameLifetimeScope.cs"; // Still used by validator
    }
}
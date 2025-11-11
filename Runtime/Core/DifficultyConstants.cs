#nullable enable

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Constants used throughout the difficulty system
    /// </summary>
    public static class DifficultyConstants
{
    // ========== DIFFICULTY RANGE ==========
    // These are default values - actual values should come from DifficultyConfig ScriptableObject
    public const float MIN_DIFFICULTY = 1f;
    public const float MAX_DIFFICULTY = 10f;
    public const float DEFAULT_DIFFICULTY = 3f;  // Lower starting point for new players (was 5f)

    // ========== TIME CONSTANTS ==========
    public const float HOURS_IN_DAY = 24f;
    public const int DAYS_IN_WEEK = 7;
    public const int MINUTES_IN_HOUR = 60;
    public const int SECONDS_IN_MINUTE = 60;
    public const float SECONDS_IN_HOUR = 3600f;
    public const float SECONDS_IN_DAY = 86400f;

    // ========== LEVEL AND PROGRESS DEFAULTS ==========
    /// <summary>Default starting level when no level data is available</summary>
    public const int DEFAULT_STARTING_LEVEL = 1;

    /// <summary>Default time percentage when completion data is not available (100% of expected time)</summary>
    public const float DEFAULT_TIME_PERCENTAGE = 1.0f;

    // ========== SESSION MANAGEMENT ==========
    /// <summary>Maximum number of recent sessions to keep in memory</summary>
    public const int MAX_RECENT_SESSIONS = 10;

    /// <summary>Short session threshold in seconds (10 seconds)</summary>
    public const float SHORT_SESSION_THRESHOLD_SECONDS = 10f;

    /// <summary>Normal session threshold in seconds (30 minutes)</summary>
    public const float NORMAL_SESSION_THRESHOLD_SECONDS = 1800f;

    /// <summary>Default session duration in seconds when no data available</summary>
    public const float DEFAULT_SESSION_DURATION_SECONDS = 60f;

    /// <summary>Default level completion time in seconds when no data available</summary>
    public const float DEFAULT_COMPLETION_TIME_SECONDS = 60f;

    // ========== PRECISION ==========
    public const float EPSILON = 0.01f; // For float comparisons and difficulty change threshold
    public const float ZERO_VALUE = 0f; // Used by aggregator and modifiers

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
    public const int STREAK_RESET_VALUE = 0; // Used by PlayerSessionData

    // ========== FOLDER NAMES ==========
    /// <summary>Base Assets folder name for Unity folder operations</summary>
    public const string FOLDER_NAME_ASSETS = "Assets";

    /// <summary>Resources folder name for Unity folder operations</summary>
    public const string FOLDER_NAME_RESOURCES = "Resources";

    // ========== INTEGRATION PATHS ==========
    /// <summary>Path to GameLifetimeScope for integration validation</summary>
    public const string INTEGRATION_GAMELIFETIMESCOPE_PATH = "Assets/Scripts/GameLifetimeScope.cs"; // Used by validator

    // ========== RAGE QUIT TIME THRESHOLD ==========
    /// <summary>Time threshold in seconds for detecting rage quit behavior</summary>
    public const float RAGE_QUIT_TIME_THRESHOLD = 30f; // Used in tests and providers
}
}
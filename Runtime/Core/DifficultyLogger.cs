using TheOne.Logging;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Logger wrapper for the Dynamic User Difficulty module
    /// </summary>
    public static class DifficultyLogger
    {
        private static ILogger logger;

        /// <summary>
        /// Gets the logger instance for this module
        /// </summary>
        public static ILogger Logger
        {
            get
            {
                if (logger == null)
                {
                    // This will be injected by DI, but provide a fallback
                    logger = new DefaultLogger();
                }
                return logger;
            }
        }

        /// <summary>
        /// Initialize the logger with a specific instance
        /// </summary>
        /// <param name="loggerInstance">The logger instance to use</param>
        public static void Initialize(ILogger loggerInstance)
        {
            logger = loggerInstance;
        }

        // Convenience methods that match Debug.Log patterns
        public static void Log(string message) => Logger.Info(message);
        public static void LogWarning(string message) => Logger.Warning(message);
        public static void LogError(string message) => Logger.Error(message);

        // Fallback logger implementation when DI is not available
        private class DefaultLogger : ILogger
        {
            public void Info(string message) => UnityEngine.Debug.Log($"[DifficultyModule] {message}");
            public void Warning(string message) => UnityEngine.Debug.LogWarning($"[DifficultyModule] {message}");
            public void Error(string message) => UnityEngine.Debug.LogError($"[DifficultyModule] {message}");
            public void Log(string message) => Info(message);
        }
    }
}
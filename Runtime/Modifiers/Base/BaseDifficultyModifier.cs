using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Generic base class for difficulty modifiers with strongly-typed configuration
    /// </summary>
    public abstract class BaseDifficultyModifier<TConfig> : IDifficultyModifier<TConfig>
        where TConfig : class, IModifierConfig
    {
        protected TConfig config;
        protected readonly ILogger logger;

        public abstract string ModifierName { get; }
        public virtual int Priority => this.config?.Priority ?? 0;
        public bool IsEnabled { get; set; }

        public TConfig Config => this.config;

        protected BaseDifficultyModifier(TConfig config, ILoggerManager loggerManager = null)
        {
            this.config = config;
            this.IsEnabled = config?.IsEnabled ?? true;
            this.logger = loggerManager?.GetLogger(this);
        }

        public abstract ModifierResult Calculate();

        public virtual void OnApplied(DifficultyResult result)
        {
            // Optional hook for post-application logic
            // Can be overridden by specific modifiers
        }

        public virtual void UpdateConfig(TConfig newConfig)
        {
            this.config = newConfig;
            this.IsEnabled = newConfig?.IsEnabled ?? true;
        }

        /// <summary>
        /// Logs debug information
        /// </summary>
        protected void LogDebug(string message)
        {
            this.logger?.Info($"[{this.ModifierName}] {message}");
        }
    }
}
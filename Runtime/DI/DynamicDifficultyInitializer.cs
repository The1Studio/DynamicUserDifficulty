using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Core;
using VContainer.Unity;

namespace TheOneStudio.DynamicUserDifficulty.DI
{
    /// <summary>
    /// Initializes the Dynamic Difficulty system on startup
    /// </summary>
    public class DynamicDifficultyInitializer : IStartable
    {
        private readonly ILogger logger;
        private readonly IDynamicDifficultyService difficultyService;

        public DynamicDifficultyInitializer(ILoggerManager loggerManager, IDynamicDifficultyService difficultyService)
        {
            this.logger = loggerManager?.GetLogger(this) ?? throw new System.ArgumentNullException(nameof(loggerManager));
            this.difficultyService = difficultyService;
        }

        public void Start()
        {
            logger.Info("[DynamicDifficulty] Module initialized successfully");

            // Auto-register modifiers
            difficultyService.RegisterAllModifiers();
        }
    }
}
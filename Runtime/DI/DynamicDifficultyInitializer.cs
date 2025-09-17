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

        public DynamicDifficultyInitializer(ILogger logger, IDynamicDifficultyService difficultyService)
        {
            this.logger = logger;
            this.difficultyService = difficultyService;
        }

        public void Start()
        {
            // Initialize the static logger
            DifficultyLogger.Initialize(logger);

            logger.Info("[DynamicDifficulty] Module initialized successfully");

            // Auto-register modifiers
            difficultyService.RegisterAllModifiers();
        }
    }
}
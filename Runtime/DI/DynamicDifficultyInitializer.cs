using System.Collections.Generic;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
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
        private readonly IEnumerable<IDifficultyModifier> modifiers;

        public DynamicDifficultyInitializer(
            ILoggerManager loggerManager,
            IDynamicDifficultyService difficultyService,
            IEnumerable<IDifficultyModifier> modifiers)
        {
            this.logger = loggerManager?.GetLogger(this) ?? throw new System.ArgumentNullException(nameof(loggerManager));
            this.difficultyService = difficultyService;
            this.modifiers = modifiers;
        }

        public void Start()
        {
            // Initialize the service
            this.difficultyService.Initialize();

            // Register all modifiers that were registered in DI
            if (this.modifiers != null)
            {
                var registeredCount = 0;
                foreach (var modifier in this.modifiers)
                {
                    if (modifier != null)
                    {
                        this.difficultyService.RegisterModifier(modifier);
                        registeredCount++;
                        this.logger.Info($"[DynamicDifficulty] Registered modifier: {modifier.ModifierName}");
                    }
                }

                this.logger.Info($"[DynamicDifficulty] Module initialized with {registeredCount} modifiers");
            }
            else
            {
                this.logger.Info("[DynamicDifficulty] Module initialized with no modifiers");
            }
        }
    }
}
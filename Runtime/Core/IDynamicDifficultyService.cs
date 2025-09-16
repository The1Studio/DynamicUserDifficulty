using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    /// <summary>
    /// Main service interface for managing dynamic difficulty
    /// </summary>
    public interface IDynamicDifficultyService
    {
        /// <summary>
        /// Gets the current difficulty level (1-10 scale)
        /// </summary>
        float CurrentDifficulty { get; }

        /// <summary>
        /// Initializes the service with saved data
        /// </summary>
        void Initialize();

        /// <summary>
        /// Calculates new difficulty based on current session data
        /// </summary>
        /// <returns>Result containing new difficulty and applied modifiers</returns>
        DifficultyResult CalculateDifficulty();

        /// <summary>
        /// Applies the calculated difficulty and saves to persistence
        /// </summary>
        /// <param name="result">The calculation result to apply</param>
        void ApplyDifficulty(DifficultyResult result);

        /// <summary>
        /// Registers a new difficulty modifier
        /// </summary>
        /// <param name="modifier">The modifier to register</param>
        void RegisterModifier(IDifficultyModifier modifier);

        /// <summary>
        /// Unregisters a difficulty modifier
        /// </summary>
        /// <param name="modifier">The modifier to unregister</param>
        void UnregisterModifier(IDifficultyModifier modifier);

        /// <summary>
        /// Called when a game session starts
        /// </summary>
        void OnSessionStart();

        /// <summary>
        /// Called when a level starts
        /// </summary>
        /// <param name="levelId">The ID of the level</param>
        void OnLevelStart(int levelId);

        /// <summary>
        /// Called when a level is completed
        /// </summary>
        /// <param name="won">Whether the level was won</param>
        /// <param name="completionTime">Time taken to complete in seconds</param>
        void OnLevelComplete(bool won, float completionTime);

        /// <summary>
        /// Called when a game session ends
        /// </summary>
        /// <param name="endType">How the session ended</param>
        void OnSessionEnd(SessionEndType endType);
    }
}
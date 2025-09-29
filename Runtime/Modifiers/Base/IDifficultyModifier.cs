using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Interface for all difficulty modifiers
    /// </summary>
    public interface IDifficultyModifier
    {
        /// <summary>
        /// Unique name of the modifier
        /// </summary>
        string ModifierName { get; }

        /// <summary>
        /// Execution order priority (lower = earlier)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Whether this modifier is currently active
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Calculates the difficulty adjustment based on data from providers.
        /// This is a pure function that gets all data from injected providers.
        /// </summary>
        /// <returns>The modifier result with adjustment value</returns>
        ModifierResult Calculate();

        /// <summary>
        /// Called after difficulty has been applied
        /// </summary>
        /// <param name="result">The applied difficulty result</param>
        void OnApplied(DifficultyResult result);
    }

    /// <summary>
    /// Generic interface for modifiers with strongly-typed configuration
    /// </summary>
    public interface IDifficultyModifier<TConfig> : IDifficultyModifier
        where TConfig : class, IModifierConfig
    {
        /// <summary>
        /// The typed configuration for this modifier
        /// </summary>
        TConfig Config { get; }

        /// <summary>
        /// Updates the modifier's configuration
        /// </summary>
        void UpdateConfig(TConfig config);
    }
}
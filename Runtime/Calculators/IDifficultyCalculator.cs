#nullable enable

using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    /// <summary>
    /// Interface for difficulty calculation logic
    /// </summary>
    /// <summary>
    /// Interface for difficulty calculation logic.
    /// Uses provider interfaces to get all required data.
    /// </summary>
    public interface IDifficultyCalculator
    {
        /// <summary>
        /// Calculates new difficulty based on data from provider interfaces.
        /// This is a pure function that retrieves all data from providers.
        /// </summary>
        /// <param name="modifiers">Collection of modifiers to apply</param>
        /// <returns>Result containing new difficulty and calculation details</returns>
        DifficultyResult Calculate(IEnumerable<IDifficultyModifier> modifiers);
    }
}
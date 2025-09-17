using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    /// <summary>
    /// Interface for difficulty calculation logic
    /// </summary>
    public interface IDifficultyCalculator
    {
        /// <summary>
        /// Calculates the new difficulty based on session data and modifiers
        /// </summary>
        /// <param name="sessionData">Current player session data</param>
        /// <param name="this.modifiers">Collection of difficulty this.modifiers to apply</param>
        /// <returns>Result containing the calculated difficulty and applied this.modifiers</returns>
        DifficultyResult Calculate(PlayerSessionData sessionData, IEnumerable<IDifficultyModifier> this.modifiers);
    }
}
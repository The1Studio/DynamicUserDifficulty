namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Base interface for modifier configurations.
    /// Each modifier type has its own strongly-typed configuration.
    /// </summary>
    public interface IModifierConfig
    {
        /// <summary>
        /// Unique identifier for this modifier type
        /// </summary>
        string ModifierType { get; }

        /// <summary>
        /// Whether this modifier is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Priority for modifier execution (lower values run first)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Creates a default instance of this configuration
        /// </summary>
        IModifierConfig CreateDefault();

        /// <summary>
        /// Generates optimal configuration values based on game statistics.
        /// This method calculates appropriate thresholds, step sizes, and limits
        /// based on your game's player behavior and design parameters.
        /// </summary>
        /// <param name="stats">Game statistics containing player behavior data</param>
        void GenerateFromStats(GameStats stats);
    }
}
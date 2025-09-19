namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Types of quit behavior that affect difficulty
    /// </summary>
    public enum QuitType
    {
        /// <summary>
        /// Player quit very quickly, likely frustrated
        /// </summary>
        RageQuit,

        /// <summary>
        /// Normal quit after playing for a while
        /// </summary>
        Quit,

        /// <summary>
        /// Quit during mid-play, might be stuck
        /// </summary>
        MidPlay
    }
}
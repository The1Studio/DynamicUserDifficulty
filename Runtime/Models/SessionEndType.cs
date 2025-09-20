namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Describes how a game session ended
    /// </summary>
    public enum SessionEndType
    {
        /// <summary>
        /// Player completed the level successfully
        /// </summary>
        CompletedWin,

        /// <summary>
        /// Player failed to complete the level
        /// </summary>
        CompletedLoss,

        /// <summary>
        /// Player quit while playing
        /// </summary>
        QuitDuringPlay,

        /// <summary>
        /// Player quit after winning
        /// </summary>
        QuitAfterWin,

        /// <summary>
        /// Player quit after losing (potential rage quit)
        /// </summary>
        QuitAfterLoss,

        /// <summary>
        /// Session timed out due to inactivity
        /// </summary>
        Timeout,

        /// <summary>
        /// Normal session end
        /// </summary>
        Normal,

        /// <summary>
        /// Session paused
        /// </summary>
        Paused,

        /// <summary>
        /// Rage quit (quick loss)
        /// </summary>
        RageQuit
    }
}
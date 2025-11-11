#nullable enable

using System;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Information about a single game session
    /// </summary>
    [Serializable]
    public sealed class SessionInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public SessionEndType EndType { get; set; }
        public int LevelId { get; set; }
        public float PlayDuration { get; set; }
        public bool Won { get; set; }

        public SessionInfo()
        {
            this.StartTime = DateTime.Now;
            this.EndTime   = DateTime.Now;
            this.EndType      = SessionEndType.QuitDuringPlay;
        }

        public SessionInfo(int levelId, bool won, float duration, SessionEndType endType)
        {
            this.StartTime    = DateTime.Now.AddSeconds(-duration);
            this.EndTime      = DateTime.Now;
            this.EndType      = endType;
            this.LevelId      = levelId;
            this.PlayDuration = duration;
            this.Won             = won;
        }
    }
}
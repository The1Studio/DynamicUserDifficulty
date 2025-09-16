using System;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Information about a single game session
    /// </summary>
    [Serializable]
    public class SessionInfo
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public SessionEndType EndType { get; set; }
        public int LevelId { get; set; }
        public float PlayDuration { get; set; }
        public bool Won { get; set; }

        public SessionInfo()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            EndType = SessionEndType.QuitDuringPlay;
        }

        public SessionInfo(int levelId, bool won, float duration, SessionEndType endType)
        {
            StartTime = DateTime.Now.AddSeconds(-duration);
            EndTime = DateTime.Now;
            EndType = endType;
            LevelId = levelId;
            PlayDuration = duration;
            Won = won;
        }
    }
}
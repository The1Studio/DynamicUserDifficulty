using System;
using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Stores player session information for difficulty calculation
    /// </summary>
    [Serializable]
    public class PlayerSessionData
    {
        public float CurrentDifficulty { get; set; } = 3f;
        public int WinStreak { get; set; }
        public int LossStreak { get; set; }
        public DateTime LastPlayTime { get; set; }
        public SessionInfo LastSession { get; set; }
        public Queue<SessionInfo> RecentSessions { get; set; }

        public PlayerSessionData()
        {
            CurrentDifficulty = 3f;
            WinStreak = 0;
            LossStreak = 0;
            LastPlayTime = DateTime.Now;
            RecentSessions = new Queue<SessionInfo>(10);
        }

        public void RecordWin(int levelId, float duration)
        {
            WinStreak++;
            LossStreak = 0;
            LastPlayTime = DateTime.Now;

            var session = new SessionInfo(levelId, true, duration, SessionEndType.CompletedWin);
            LastSession = session;
            AddRecentSession(session);
        }

        public void RecordLoss(int levelId, float duration)
        {
            LossStreak++;
            WinStreak = 0;
            LastPlayTime = DateTime.Now;

            var session = new SessionInfo(levelId, false, duration, SessionEndType.CompletedLoss);
            LastSession = session;
            AddRecentSession(session);
        }

        private void AddRecentSession(SessionInfo session)
        {
            if (RecentSessions.Count >= 10)
            {
                RecentSessions.Dequeue();
            }
            RecentSessions.Enqueue(session);
        }

        public void ResetStreaks()
        {
            WinStreak = 0;
            LossStreak = 0;
        }
    }
}
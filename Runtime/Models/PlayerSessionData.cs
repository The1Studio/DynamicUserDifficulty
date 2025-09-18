using System;
using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Models
{
    /// <summary>
    /// Stores player session information for difficulty calculation
    /// </summary>
    [Serializable]
    public class PlayerSessionData
    {
        public float CurrentDifficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;
        public int WinStreak { get; set; }
        public int LossStreak { get; set; }
        public DateTime LastPlayTime { get; set; }
        public SessionInfo LastSession { get; set; }
        public Queue<SessionInfo> RecentSessions { get; set; }

        public PlayerSessionData()
        {
            CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            WinStreak = DifficultyConstants.STREAK_RESET_VALUE;
            LossStreak = DifficultyConstants.STREAK_RESET_VALUE;
            LastPlayTime = DateTime.Now;
            RecentSessions = new Queue<SessionInfo>(DifficultyConstants.MAX_RECENT_SESSIONS);
        }

        public void RecordWin(int levelId, float duration)
        {
            WinStreak++;
            LossStreak = DifficultyConstants.STREAK_RESET_VALUE;
            LastPlayTime = DateTime.Now;

            var session = new SessionInfo(levelId, true, duration, SessionEndType.CompletedWin);
            LastSession = session;
            AddRecentSession(session);
        }

        public void RecordLoss(int levelId, float duration)
        {
            LossStreak++;
            WinStreak = DifficultyConstants.STREAK_RESET_VALUE;
            LastPlayTime = DateTime.Now;

            var session = new SessionInfo(levelId, false, duration, SessionEndType.CompletedLoss);
            LastSession = session;
            AddRecentSession(session);
        }

        private void AddRecentSession(SessionInfo session)
        {
            if (RecentSessions.Count >= DifficultyConstants.MAX_RECENT_SESSIONS)
            {
                RecentSessions.Dequeue();
            }
            RecentSessions.Enqueue(session);
        }

        public void ResetStreaks()
        {
            WinStreak = DifficultyConstants.STREAK_RESET_VALUE;
            LossStreak = DifficultyConstants.STREAK_RESET_VALUE;
        }
    }
}
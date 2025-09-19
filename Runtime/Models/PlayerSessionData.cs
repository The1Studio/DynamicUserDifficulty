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

        // Additional properties for service compatibility
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int SessionCount { get; set; }
        public DateTime LastSessionTime { get; set; }
        public float SessionLength { get; set; }
        public QuitType? QuitType { get; set; }
        public float CurrentProgress { get; set; }

        public PlayerSessionData()
        {
            this.CurrentDifficulty = DifficultyConstants.DEFAULT_DIFFICULTY;
            this.WinStreak         = DifficultyConstants.STREAK_RESET_VALUE;
            this.LossStreak        = DifficultyConstants.STREAK_RESET_VALUE;
            this.LastPlayTime      = DateTime.Now;
            this.RecentSessions       = new Queue<SessionInfo>(DifficultyConstants.MAX_RECENT_SESSIONS);
        }

        public void RecordWin(int levelId, float duration)
        {
            this.WinStreak++;
            this.LossStreak = DifficultyConstants.STREAK_RESET_VALUE;
            this.LastPlayTime  = DateTime.Now;

            var session = new SessionInfo(levelId, true, duration, SessionEndType.CompletedWin);
            this.LastSession = session;
            this.AddRecentSession(session);
        }

        public void RecordLoss(int levelId, float duration)
        {
            this.LossStreak++;
            this.WinStreak = DifficultyConstants.STREAK_RESET_VALUE;
            this.LastPlayTime = DateTime.Now;

            var session = new SessionInfo(levelId, false, duration, SessionEndType.CompletedLoss);
            this.LastSession = session;
            this.AddRecentSession(session);
        }

        private void AddRecentSession(SessionInfo session)
        {
            if (this.RecentSessions.Count >= DifficultyConstants.MAX_RECENT_SESSIONS)
            {
                this.RecentSessions.Dequeue();
            }
            this.RecentSessions.Enqueue(session);
        }

        public void ResetStreaks()
        {
            this.WinStreak = DifficultyConstants.STREAK_RESET_VALUE;
            this.LossStreak   = DifficultyConstants.STREAK_RESET_VALUE;
        }
    }
}
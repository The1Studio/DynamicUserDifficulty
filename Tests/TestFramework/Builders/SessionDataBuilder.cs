using System;
using System.Collections.Generic;
using TheOneStudio.DynamicUserDifficulty.Models;

namespace TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders
{
    /// <summary>
    /// Builder pattern for creating PlayerSessionData in tests
    /// </summary>
    public class SessionDataBuilder
    {
        private PlayerSessionData data;

        private SessionDataBuilder()
        {
            data = new PlayerSessionData
            {
                CurrentDifficulty = 3f,
                WinStreak = 0,
                LossStreak = 0,
                LastPlayTime = DateTime.Now,
                LastSession = null,
                SessionHistory = new List<SessionInfo>()
            };
        }

        public static SessionDataBuilder Create()
        {
            return new SessionDataBuilder();
        }

        public SessionDataBuilder WithDifficulty(float difficulty)
        {
            data.CurrentDifficulty = difficulty;
            return this;
        }

        public SessionDataBuilder WithWinStreak(int streak)
        {
            data.WinStreak = streak;
            data.LossStreak = 0; // Reset loss streak
            return this;
        }

        public SessionDataBuilder WithLossStreak(int streak)
        {
            data.LossStreak = streak;
            data.WinStreak = 0; // Reset win streak
            return this;
        }

        public SessionDataBuilder WithStreaks(int winStreak, int lossStreak)
        {
            data.WinStreak = winStreak;
            data.LossStreak = lossStreak;
            return this;
        }

        public SessionDataBuilder WithLastPlayTime(DateTime time)
        {
            data.LastPlayTime = time;
            return this;
        }

        public SessionDataBuilder WithDaysAgo(int days)
        {
            data.LastPlayTime = DateTime.Now.AddDays(-days);
            return this;
        }

        public SessionDataBuilder WithHoursAgo(int hours)
        {
            data.LastPlayTime = DateTime.Now.AddHours(-hours);
            return this;
        }

        public SessionDataBuilder WithLastSession(SessionInfo session)
        {
            data.LastSession = session;
            return this;
        }

        public SessionDataBuilder WithLastSession(Action<SessionInfoBuilder> configure)
        {
            var sessionBuilder = SessionInfoBuilder.Create();
            configure(sessionBuilder);
            data.LastSession = sessionBuilder.Build();
            return this;
        }

        public SessionDataBuilder WithQuickLoss(float duration = 25f)
        {
            data.LastSession = SessionInfoBuilder.Create()
                .WithDuration(duration)
                .WithLevelsPlayed(1)
                .WithLevelsWon(0)
                .Build();
            return this;
        }

        public SessionDataBuilder WithNormalSession(int played = 5, int won = 3)
        {
            data.LastSession = SessionInfoBuilder.Create()
                .WithLevelsPlayed(played)
                .WithLevelsWon(won)
                .WithDuration(played * 120f) // 2 minutes per level average
                .Build();
            return this;
        }

        public SessionDataBuilder AddToHistory(SessionInfo session)
        {
            if (data.SessionHistory == null)
                data.SessionHistory = new List<SessionInfo>();
            data.SessionHistory.Add(session);
            return this;
        }

        public SessionDataBuilder WithEmptyHistory()
        {
            data.SessionHistory = new List<SessionInfo>();
            return this;
        }

        public PlayerSessionData Build()
        {
            return data;
        }
    }

    /// <summary>
    /// Builder for SessionInfo
    /// </summary>
    public class SessionInfoBuilder
    {
        private SessionInfo info;

        private SessionInfoBuilder()
        {
            var now = DateTime.Now;
            info = new SessionInfo
            {
                SessionStart = now.AddMinutes(-30),
                SessionEnd = now,
                LevelsPlayed = 5,
                LevelsWon = 3,
                SessionDuration = 1800f // 30 minutes
            };
        }

        public static SessionInfoBuilder Create()
        {
            return new SessionInfoBuilder();
        }

        public SessionInfoBuilder WithStartTime(DateTime start)
        {
            info.SessionStart = start;
            return this;
        }

        public SessionInfoBuilder WithEndTime(DateTime end)
        {
            info.SessionEnd = end;
            info.SessionDuration = (float)(end - info.SessionStart).TotalSeconds;
            return this;
        }

        public SessionInfoBuilder WithDuration(float seconds)
        {
            info.SessionDuration = seconds;
            info.SessionEnd = info.SessionStart.AddSeconds(seconds);
            return this;
        }

        public SessionInfoBuilder WithLevelsPlayed(int count)
        {
            info.LevelsPlayed = count;
            return this;
        }

        public SessionInfoBuilder WithLevelsWon(int count)
        {
            info.LevelsWon = count;
            return this;
        }

        public SessionInfoBuilder AsWinSession(int levelsPlayed = 5, int levelsWon = 5)
        {
            info.LevelsPlayed = levelsPlayed;
            info.LevelsWon = levelsWon;
            return this;
        }

        public SessionInfoBuilder AsLossSession(int levelsPlayed = 5, int levelsWon = 0)
        {
            info.LevelsPlayed = levelsPlayed;
            info.LevelsWon = levelsWon;
            return this;
        }

        public SessionInfoBuilder AsRageQuit(float duration = 15f)
        {
            info.SessionDuration = duration;
            info.LevelsPlayed = 1;
            info.LevelsWon = 0;
            info.SessionEnd = info.SessionStart.AddSeconds(duration);
            return this;
        }

        public SessionInfo Build()
        {
            return info;
        }
    }
}
using System;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Mocks
{
    /// <summary>
    /// Mock implementation of ISessionDataProvider for testing
    /// </summary>
    public class MockSessionDataProvider : ISessionDataProvider
    {
        private PlayerSessionData mockData;
        private DateTime? lastRetrievedTime;
        private DateTime? lastSavedTime;

        public int SaveCallCount { get; private set; }
        public int RetrieveCallCount { get; private set; }
        public bool ThrowOnSave { get; set; }
        public bool ThrowOnRetrieve { get; set; }
        public PlayerSessionData LastSavedData { get; private set; }

        public MockSessionDataProvider(PlayerSessionData initialData = null)
        {
            mockData = initialData ?? CreateDefaultData();
            SaveCallCount = 0;
            RetrieveCallCount = 0;
        }

        public PlayerSessionData RetrieveSessionData()
        {
            RetrieveCallCount++;
            lastRetrievedTime = DateTime.Now;

            if (ThrowOnRetrieve)
            {
                throw new Exception("Mock retrieve exception");
            }

            return mockData != null ? CloneData(mockData) : null;
        }

        public void SaveSessionData(PlayerSessionData data)
        {
            SaveCallCount++;
            lastSavedTime = DateTime.Now;
            LastSavedData = CloneData(data);

            if (ThrowOnSave)
            {
                throw new Exception("Mock save exception");
            }

            mockData = CloneData(data);
        }

        public void ClearSessionData()
        {
            mockData = CreateDefaultData();
            SaveCallCount = 0;
            RetrieveCallCount = 0;
            lastRetrievedTime = null;
            lastSavedTime = null;
            LastSavedData = null;
        }

        public bool HasData()
        {
            return mockData != null;
        }

        /// <summary>
        /// Set the mock data to return
        /// </summary>
        public void SetMockData(PlayerSessionData data)
        {
            mockData = CloneData(data);
        }

        /// <summary>
        /// Get statistics about provider usage
        /// </summary>
        public (DateTime? lastRetrieve, DateTime? lastSave) GetAccessTimes()
        {
            return (lastRetrievedTime, lastSavedTime);
        }

        /// <summary>
        /// Verify save was called with expected data
        /// </summary>
        public bool VerifySaveCalledWith(Func<PlayerSessionData, bool> predicate)
        {
            return LastSavedData != null && predicate(LastSavedData);
        }

        private PlayerSessionData CreateDefaultData()
        {
            return new PlayerSessionData
            {
                CurrentDifficulty = 3f,
                WinStreak = 0,
                LossStreak = 0,
                LastPlayTime = DateTime.Now,
                LastSession = null,
                SessionHistory = new System.Collections.Generic.List<SessionInfo>()
            };
        }

        private PlayerSessionData CloneData(PlayerSessionData data)
        {
            if (data == null) return null;

            return new PlayerSessionData
            {
                CurrentDifficulty = data.CurrentDifficulty,
                WinStreak = data.WinStreak,
                LossStreak = data.LossStreak,
                LastPlayTime = data.LastPlayTime,
                LastSession = data.LastSession != null ? CloneSessionInfo(data.LastSession) : null,
                SessionHistory = data.SessionHistory != null
                    ? new System.Collections.Generic.List<SessionInfo>(data.SessionHistory)
                    : new System.Collections.Generic.List<SessionInfo>()
            };
        }

        private SessionInfo CloneSessionInfo(SessionInfo info)
        {
            return new SessionInfo
            {
                SessionStart = info.SessionStart,
                SessionEnd = info.SessionEnd,
                LevelsPlayed = info.LevelsPlayed,
                LevelsWon = info.LevelsWon,
                SessionDuration = info.SessionDuration
            };
        }
    }
}
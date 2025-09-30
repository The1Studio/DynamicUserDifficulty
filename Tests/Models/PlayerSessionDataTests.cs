using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Core;
using System;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Models
{
    [TestFixture]
    public class PlayerSessionDataTests
    {
        private PlayerSessionData sessionData;

        [SetUp]
        public void Setup()
        {
            this.sessionData = new();
        }

        [Test]
        public void Constructor_InitializesDefaultValues()
        {
            // Assert
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.WinStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.LossStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.SessionCount);
            Assert.AreEqual(0, this.sessionData.TotalWins);
            Assert.AreEqual(0, this.sessionData.TotalLosses);
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, this.sessionData.SessionLength);
            Assert.IsNull(this.sessionData.QuitType);
            Assert.AreNotEqual(default(DateTime), this.sessionData.LastPlayTime);
        }

        [Test]
        public void RecordWin_IncrementsWinStreak()
        {
            // Act
            this.sessionData.RecordWin(1, 60f);

            // Assert
            Assert.AreEqual(1, this.sessionData.WinStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.LossStreak);
        }

        [Test]
        public void RecordWin_ResetsLossStreak()
        {
            // Arrange
            this.sessionData.LossStreak = 5;

            // Act
            this.sessionData.RecordWin(1, 60f);

            // Assert
            Assert.AreEqual(1, this.sessionData.WinStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.LossStreak);
        }

        [Test]
        public void RecordWin_MultipleTimes_IncrementsCorrectly()
        {
            // Act
            this.sessionData.RecordWin(1, 60f);
            this.sessionData.RecordWin(1, 60f);
            this.sessionData.RecordWin(1, 60f);

            // Assert
            Assert.AreEqual(3, this.sessionData.WinStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.LossStreak);
        }

        [Test]
        public void RecordLoss_IncrementsLossStreak()
        {
            // Act
            this.sessionData.RecordLoss(1, 30f);

            // Assert
            Assert.AreEqual(1, this.sessionData.LossStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.WinStreak);
        }

        [Test]
        public void RecordLoss_ResetsWinStreak()
        {
            // Arrange
            this.sessionData.WinStreak = 7;

            // Act
            this.sessionData.RecordLoss(1, 30f);

            // Assert
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.WinStreak);
            Assert.AreEqual(1, this.sessionData.LossStreak);
        }

        [Test]
        public void RecordLoss_MultipleTimes_IncrementsCorrectly()
        {
            // Act
            this.sessionData.RecordLoss(1, 30f);
            this.sessionData.RecordLoss(1, 30f);
            this.sessionData.RecordLoss(1, 30f);

            // Assert
            Assert.AreEqual(3, this.sessionData.LossStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.WinStreak);
        }

        [Test]
        public void ResetStreaks_ResetsAllStreaks()
        {
            // Arrange
            this.sessionData.WinStreak = 5;
            this.sessionData.LossStreak   = 3;

            // Act
            this.sessionData.ResetStreaks();

            // Assert
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.WinStreak);
            Assert.AreEqual(DifficultyConstants.STREAK_RESET_VALUE, this.sessionData.LossStreak);
        }

        [Test]
        public void SessionData_CanUpdateSessionCount()
        {
            // Arrange
            var startTime = DateTime.Now;

            // Act - Simulate what the service does
            this.sessionData.SessionCount++;
            this.sessionData.LastSessionTime = startTime;

            // Assert
            Assert.AreEqual(1, this.sessionData.SessionCount);
            Assert.GreaterOrEqual(this.sessionData.LastSessionTime, startTime);
            Assert.LessOrEqual(this.sessionData.LastSessionTime, DateTime.Now);
        }

        [Test]
        public void SessionData_CanUpdateSessionLength()
        {
            // Arrange - Use deterministic time values instead of Thread.Sleep
            var startTime = new DateTime(2024, 1, 1, 12, 0, 0);
            var endTime = new DateTime(2024, 1, 1, 12, 1, 40); // 100 seconds later

            this.sessionData.SessionCount++;
            this.sessionData.LastSessionTime = startTime;

            // Act - Simulate what the service does
            this.sessionData.SessionLength = 100f;
            this.sessionData.LastPlayTime = endTime;

            // Assert
            Assert.Greater(this.sessionData.SessionLength, 0);
            Assert.GreaterOrEqual(this.sessionData.LastPlayTime, this.sessionData.LastSessionTime);
        }

        [Test]
        public void QuitType_CanBeSet()
        {
            // Act
            this.sessionData.QuitType = QuitType.RageQuit;

            // Assert
            Assert.AreEqual(QuitType.RageQuit, this.sessionData.QuitType);
        }

        [Test]
        public void RecordQuitType_AllTypes()
        {
            // Test all quit types
            var quitTypes = Enum.GetValues(typeof(QuitType));

            foreach (QuitType quitType in quitTypes)
            {
                // Arrange
                var newSession = new PlayerSessionData();

                // Act
                newSession.QuitType = quitType;

                // Assert
                Assert.AreEqual(quitType, newSession.QuitType);
            }
        }

        [Test]
        public void CurrentProgress_CanBeSet()
        {
            // Act
            this.sessionData.CurrentProgress = 0.75f;

            // Assert
            Assert.AreEqual(0.75f, this.sessionData.CurrentProgress);
        }

        [Test]
        public void CurrentProgress_AcceptsVariousValues()
        {
            // Test lower bound (no auto-clamping in property)
            this.sessionData.CurrentProgress = -0.5f;
            Assert.AreEqual(-0.5f, this.sessionData.CurrentProgress);

            // Test upper bound (no auto-clamping in property)
            this.sessionData.CurrentProgress = 1.5f;
            Assert.AreEqual(1.5f, this.sessionData.CurrentProgress);

            // Test valid range
            this.sessionData.CurrentProgress = 0.5f;
            Assert.AreEqual(0.5f, this.sessionData.CurrentProgress);
        }

        [Test]
        public void RecentSessions_InitializedProperly()
        {
            // Assert
            Assert.IsNotNull(this.sessionData.RecentSessions);
            Assert.AreEqual(0, this.sessionData.RecentSessions.Count);
        }

        [Test]
        public void LastSession_CanBeSet()
        {
            // Arrange
            var session = new SessionInfo(1, true, 100f, SessionEndType.CompletedWin);

            // Act
            this.sessionData.LastSession = session;

            // Assert
            Assert.AreEqual(session, this.sessionData.LastSession);
            Assert.IsTrue(this.sessionData.LastSession.Won);
        }

        [Test]
        public void RecordWin_UpdatesLastSession()
        {
            // Act
            this.sessionData.RecordWin(2, 120f);

            // Assert
            Assert.IsNotNull(this.sessionData.LastSession);
            Assert.IsTrue(this.sessionData.LastSession.Won);
            Assert.AreEqual(2, this.sessionData.LastSession.LevelId);
            Assert.AreEqual(120f, this.sessionData.LastSession.PlayDuration);
        }

        [Test]
        public void RecordLoss_UpdatesLastSession()
        {
            // Act
            this.sessionData.RecordLoss(3, 45f);

            // Assert
            Assert.IsNotNull(this.sessionData.LastSession);
            Assert.IsFalse(this.sessionData.LastSession.Won);
            Assert.AreEqual(3, this.sessionData.LastSession.LevelId);
            Assert.AreEqual(45f, this.sessionData.LastSession.PlayDuration);
        }

        [Test]
        public void RecordWin_AddsToRecentSessions()
        {
            // Act
            this.sessionData.RecordWin(4, 200f);

            // Assert
            Assert.AreEqual(1, this.sessionData.RecentSessions.Count);
            var recent = this.sessionData.RecentSessions.Peek();
            Assert.IsTrue(recent.Won);
            Assert.AreEqual(4, recent.LevelId);
        }

        [Test]
        public void RecentSessions_LimitedToMaxSize()
        {
            // Arrange & Act
            for (var i = 0; i < DifficultyConstants.MAX_RECENT_SESSIONS + 5; i++)
            {
                this.sessionData.RecordWin(i, 60f + i);
            }

            // Assert
            Assert.AreEqual(DifficultyConstants.MAX_RECENT_SESSIONS, this.sessionData.RecentSessions.Count);
        }
    }
}
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Services;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;
using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Services
{
    [TestFixture]
    public class DynamicUserDifficultyServiceTests
    {
        private DynamicUserDifficultyService service;
        private DifficultyConfig config;
        private TestDataProvider dataProvider;

        [SetUp]
        public void Setup()
        {
            // Create test config
            this.config = ScriptableObject.CreateInstance<DifficultyConfig>();

            // Create test data provider
            this.dataProvider = new TestDataProvider();

            // Create service
            this.service = new DynamicUserDifficultyService(this.config, this.dataProvider);
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
                Object.DestroyImmediate(this.config);
        }

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Assert
            Assert.IsNotNull(this.service);
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.service.CurrentDifficulty);
        }

        [Test]
        public void RecordWin_UpdatesSessionData()
        {
            // Arrange
            var initialWinStreak = this.dataProvider.SessionData.WinStreak;

            // Act
            this.service.RecordWin();

            // Assert
            Assert.AreEqual(initialWinStreak + 1, this.dataProvider.SessionData.WinStreak);
            Assert.AreEqual(0, this.dataProvider.SessionData.LossStreak);
        }

        [Test]
        public void RecordLoss_UpdatesSessionData()
        {
            // Arrange
            var initialLossStreak = this.dataProvider.SessionData.LossStreak;

            // Act
            this.service.RecordLoss();

            // Assert
            Assert.AreEqual(initialLossStreak + 1, this.dataProvider.SessionData.LossStreak);
            Assert.AreEqual(0, this.dataProvider.SessionData.WinStreak);
        }

        [Test]
        public void UpdateDifficulty_CallsAllModifiers()
        {
            // Arrange
            this.service.RecordWin();
            this.service.RecordWin();
            this.service.RecordWin(); // Build win streak

            // Act
            float newDifficulty = this.service.UpdateDifficulty();

            // Assert
            Assert.GreaterOrEqual(newDifficulty, DifficultyConstants.MIN_DIFFICULTY);
            Assert.LessOrEqual(newDifficulty, DifficultyConstants.MAX_DIFFICULTY);
        }

        [Test]
        public void ResetDifficulty_SetsToDefault()
        {
            // Arrange
            this.service.UpdateDifficulty(); // Change difficulty
            this.dataProvider.Difficulty = 8f;

            // Act
            this.service.ResetDifficulty();

            // Assert
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.service.CurrentDifficulty);
        }

        [Test]
        public void GetDifficultyLevel_ReturnsCorrectLevel()
        {
            // Test Easy
            this.dataProvider.Difficulty = 2f;
            Assert.AreEqual(DifficultyLevel.Easy, this.service.GetDifficultyLevel());

            // Test Medium
            this.dataProvider.Difficulty = 5f;
            Assert.AreEqual(DifficultyLevel.Medium, this.service.GetDifficultyLevel());

            // Test Hard
            this.dataProvider.Difficulty = 8f;
            Assert.AreEqual(DifficultyLevel.Hard, this.service.GetDifficultyLevel());
        }

        [Test]
        public void RecordSessionStart_UpdatesSessionData()
        {
            // Arrange
            var initialSessionCount = this.dataProvider.SessionData.SessionCount;

            // Act
            this.service.RecordSessionStart();

            // Assert
            Assert.AreEqual(initialSessionCount + 1, this.dataProvider.SessionData.SessionCount);
        }

        [Test]
        public void RecordSessionEnd_UpdatesSessionData()
        {
            // Arrange
            this.service.RecordSessionStart();

            // Act
            this.service.RecordSessionEnd();

            // Assert
            Assert.Greater(this.dataProvider.SessionData.SessionLength, 0);
        }

        [Test]
        public void RecordQuit_WithDifferentTypes()
        {
            // Test RageQuit
            this.service.RecordQuit(QuitType.RageQuit);
            Assert.AreEqual(QuitType.RageQuit, this.dataProvider.SessionData.QuitType);

            // Test normal Quit
            this.service.RecordQuit(QuitType.Quit);
            Assert.AreEqual(QuitType.Quit, this.dataProvider.SessionData.QuitType);

            // Test MidPlay
            this.service.RecordQuit(QuitType.MidPlay);
            Assert.AreEqual(QuitType.MidPlay, this.dataProvider.SessionData.QuitType);
        }

        [Test]
        public void GetSessionData_ReturnsCurrentData()
        {
            // Arrange
            this.service.RecordWin();
            this.service.RecordSessionStart();

            // Act
            var sessionData = this.service.GetSessionData();

            // Assert
            Assert.IsNotNull(sessionData);
            Assert.AreEqual(1, sessionData.WinStreak);
            Assert.AreEqual(1, sessionData.SessionCount);
        }

        [Test]
        public void SaveData_CallsProvider()
        {
            // Arrange
            this.dataProvider.SaveCalled = false;

            // Act
            this.service.SaveData();

            // Assert
            Assert.IsTrue(this.dataProvider.SaveCalled);
        }

        [Test]
        public void LoadData_CallsProvider()
        {
            // Arrange
            this.dataProvider.LoadCalled = false;

            // Act
            this.service.LoadData();

            // Assert
            Assert.IsTrue(this.dataProvider.LoadCalled);
        }

        [Test]
        public void GetDifficultyStats_ReturnsValidStats()
        {
            // Arrange
            this.service.RecordWin();
            this.service.RecordWin();
            this.service.RecordLoss();

            // Act
            var stats = this.service.GetDifficultyStats();

            // Assert
            Assert.IsNotNull(stats);
            Assert.AreEqual(2, stats["WinStreak"]);
            Assert.AreEqual(0, stats["LossStreak"]);
            Assert.AreEqual(this.service.CurrentDifficulty, stats["CurrentDifficulty"]);
        }

        [Test]
        public void UpdateDifficulty_ClampsToValidRange()
        {
            // Test min clamping
            this.dataProvider.Difficulty = DifficultyConstants.MIN_DIFFICULTY;
            this.service.RecordLoss();
            this.service.RecordLoss();
            this.service.RecordLoss();
            var minResult = this.service.UpdateDifficulty();
            Assert.GreaterOrEqual(minResult, DifficultyConstants.MIN_DIFFICULTY);

            // Test max clamping
            this.dataProvider.Difficulty = DifficultyConstants.MAX_DIFFICULTY;
            this.service.RecordWin();
            this.service.RecordWin();
            this.service.RecordWin();
            var maxResult = this.service.UpdateDifficulty();
            Assert.LessOrEqual(maxResult, DifficultyConstants.MAX_DIFFICULTY);
        }

        // Test helper class
        private class TestDataProvider : IPlayerDataProvider
        {
            public PlayerSessionData SessionData { get; private set; } = new PlayerSessionData();
            public float Difficulty { get; set; } = DifficultyConstants.DEFAULT_DIFFICULTY;
            public bool SaveCalled { get; set; }
            public bool LoadCalled { get; set; }

            public PlayerSessionData LoadSessionData()
            {
                this.LoadCalled = true;
                return this.SessionData;
            }

            public void SaveSessionData(PlayerSessionData data)
            {
                this.SaveCalled = true;
                this.SessionData   = data;
            }

            public float LoadDifficulty()
            {
                return this.Difficulty;
            }

            public void SaveDifficulty(float difficulty)
            {
                this.Difficulty = difficulty;
            }

            public void ClearData()
            {
                this.SessionData = new PlayerSessionData();
                this.Difficulty     = DifficultyConstants.DEFAULT_DIFFICULTY;
            }
        }
    }
}
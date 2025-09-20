using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Core
{
    [TestFixture]
    public class DifficultyManagerTests
    {
        private DifficultyManager difficultyManager;
        private DifficultyConfig config;
        private PlayerSessionData sessionData;

        [SetUp]
        public void Setup()
        {
            // Create test config with default values
            this.config = DifficultyConfig.CreateDefault();

            // Initialize session data
            this.sessionData = new PlayerSessionData();

            // Create difficulty manager
            this.difficultyManager = new DifficultyManager(this.config);
        }

        [TearDown]
        public void TearDown()
        {
            if (this.config != null)
                Object.DestroyImmediate(this.config);
        }

        [Test]
        public void Constructor_WithValidConfig_InitializesCorrectly()
        {
            // Assert
            Assert.IsNotNull(this.difficultyManager);
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.difficultyManager.CurrentDifficulty);
        }

        [Test]
        public void Constructor_WithNullConfig_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() => new DifficultyManager(null));
        }

        [Test]
        public void CalculateDifficulty_WithNoModifiers_ReturnsCurrentDifficulty()
        {
            // Arrange
            float currentDifficulty = 5.0f;
            this.difficultyManager.SetDifficulty(currentDifficulty);

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, new List<ModifierResult>());

            // Assert
            Assert.AreEqual(currentDifficulty, result);
        }

        [Test]
        public void CalculateDifficulty_WithPositiveModifier_IncreasesDifficulty()
        {
            // Arrange
            float currentDifficulty = 5.0f;
            this.difficultyManager.SetDifficulty(currentDifficulty);

            var modifierResults = new List<ModifierResult>
            {
                new ModifierResult { ModifierName = "TestModifier", Value = 1.5f }
            };

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, modifierResults);

            // Assert
            Assert.Greater(result, currentDifficulty);
        }

        [Test]
        public void CalculateDifficulty_WithNegativeModifier_DecreasesDifficulty()
        {
            // Arrange
            float currentDifficulty = 5.0f;
            this.difficultyManager.SetDifficulty(currentDifficulty);

            var modifierResults = new List<ModifierResult>
            {
                new ModifierResult { ModifierName = "TestModifier", Value = -1.5f }
            };

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, modifierResults);

            // Assert
            Assert.Less(result, currentDifficulty);
        }

        [Test]
        public void ApplyDifficulty_ClampsToMinimum()
        {
            // Arrange
            this.difficultyManager.SetDifficulty(DifficultyConstants.MIN_DIFFICULTY);

            var modifierResults = new List<ModifierResult>
            {
                new ModifierResult { ModifierName = "TestModifier", Value = -10f }
            };

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, modifierResults);
            this.difficultyManager.ApplyDifficulty(result);

            // Assert
            Assert.AreEqual(DifficultyConstants.MIN_DIFFICULTY, this.difficultyManager.CurrentDifficulty);
        }

        [Test]
        public void ApplyDifficulty_ClampsToMaximum()
        {
            // Arrange
            this.difficultyManager.SetDifficulty(DifficultyConstants.MAX_DIFFICULTY - 1);

            var modifierResults = new List<ModifierResult>
            {
                new ModifierResult { ModifierName = "TestModifier", Value = 10f }
            };

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, modifierResults);
            this.difficultyManager.ApplyDifficulty(result);

            // Assert
            Assert.AreEqual(DifficultyConstants.MAX_DIFFICULTY, this.difficultyManager.CurrentDifficulty);
        }

        [Test]
        public void ApplyDifficulty_RespectsMaxChangePerSession()
        {
            // Arrange
            float startDifficulty = 5.0f;
            float maxChange = 2.0f;
            this.difficultyManager.SetDifficulty(startDifficulty);

            var modifierResults = new List<ModifierResult>
            {
                new ModifierResult { ModifierName = "TestModifier", Value = 5f } // Try to add 5
            };

            // Act
            float result = this.difficultyManager.CalculateDifficulty(this.sessionData, modifierResults);
            this.difficultyManager.ApplyDifficulty(result);

            // Assert
            Assert.LessOrEqual(this.difficultyManager.CurrentDifficulty - startDifficulty, maxChange);
        }

        [Test]
        public void ResetDifficulty_SetsToDefault()
        {
            // Arrange
            this.difficultyManager.SetDifficulty(8.0f);

            // Act
            this.difficultyManager.ResetDifficulty();

            // Assert
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.difficultyManager.CurrentDifficulty);
        }

        [Test]
        public void GetDifficultyLevel_ReturnCorrectLevel()
        {
            // Test Easy
            this.difficultyManager.SetDifficulty(2.0f);
            Assert.AreEqual(DifficultyLevel.Easy, this.difficultyManager.GetDifficultyLevel());

            // Test Medium
            this.difficultyManager.SetDifficulty(5.0f);
            Assert.AreEqual(DifficultyLevel.Medium, this.difficultyManager.GetDifficultyLevel());

            // Test Hard
            this.difficultyManager.SetDifficulty(8.0f);
            Assert.AreEqual(DifficultyLevel.Hard, this.difficultyManager.GetDifficultyLevel());
        }
    }
}
#nullable enable

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
            this.sessionData = new();

            // Create difficulty manager
            this.difficultyManager = new(this.config);
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
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, this.difficultyManager.GetDefaultDifficulty());
        }

        [Test]
        public void Constructor_WithNullConfig_WorksWithDefaults()
        {
            // Act
            var manager = new DifficultyManager();

            // Assert
            Assert.IsNotNull(manager);
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, manager.GetDefaultDifficulty());
        }

        [Test]
        public void CalculateDifficulty_WithNoModifiers_ReturnsCurrentDifficulty()
        {
            // Arrange
            var currentDifficulty = 5.0f;

            // Act
            var result = this.difficultyManager.CalculateDifficulty(currentDifficulty, new());

            // Assert
            Assert.AreEqual(currentDifficulty, result);
        }

        [Test]
        public void CalculateDifficulty_WithPositiveModifier_IncreasesDifficulty()
        {
            // Arrange
            var currentDifficulty = 5.0f;

            var modifierResults = new List<ModifierResult>
            {
                new() { ModifierName = "TestModifier", Value = 1.5f },
            };

            // Act
            var result = this.difficultyManager.CalculateDifficulty(currentDifficulty, modifierResults);

            // Assert
            Assert.Greater(result, currentDifficulty);
        }

        [Test]
        public void CalculateDifficulty_WithNegativeModifier_DecreasesDifficulty()
        {
            // Arrange
            var currentDifficulty = 5.0f;

            var modifierResults = new List<ModifierResult>
            {
                new() { ModifierName = "TestModifier", Value = -1.5f },
            };

            // Act
            var result = this.difficultyManager.CalculateDifficulty(currentDifficulty, modifierResults);

            // Assert
            Assert.Less(result, currentDifficulty);
        }

        [Test]
        public void ClampDifficulty_ClampsToMinimum()
        {
            // Arrange
            var belowMin = DifficultyConstants.MIN_DIFFICULTY - 5f;

            // Act
            var result = this.difficultyManager.ClampDifficulty(belowMin);

            // Assert
            Assert.AreEqual(DifficultyConstants.MIN_DIFFICULTY, result);
        }

        [Test]
        public void ClampDifficulty_ClampsToMaximum()
        {
            // Arrange
            var aboveMax = DifficultyConstants.MAX_DIFFICULTY + 5f;

            // Act
            var result = this.difficultyManager.ClampDifficulty(aboveMax);

            // Assert
            Assert.AreEqual(DifficultyConstants.MAX_DIFFICULTY, result);
        }

        [Test]
        public void CalculateDifficulty_RespectsMaxChangePerSession()
        {
            // Arrange
            var startDifficulty = 5.0f;
            var maxChange = this.config.MaxChangePerSession;

            var modifierResults = new List<ModifierResult>
            {
                new() { ModifierName = "TestModifier", Value = 5f }, // Try to add 5
            };

            // Act
            var result = this.difficultyManager.CalculateDifficulty(startDifficulty, modifierResults);

            // Assert
            Assert.LessOrEqual(result - startDifficulty, maxChange);
        }

        [Test]
        public void GetDefaultDifficulty_ReturnsCorrectValue()
        {
            // Act
            var defaultDiff = this.difficultyManager.GetDefaultDifficulty();

            // Assert
            Assert.AreEqual(DifficultyConstants.DEFAULT_DIFFICULTY, defaultDiff);
        }

        [Test]
        public void GetDifficultyLevel_ReturnsCorrectLevel()
        {
            // Test Easy
            Assert.AreEqual(DifficultyLevel.Easy, this.difficultyManager.GetDifficultyLevel(2.0f));

            // Test Medium
            Assert.AreEqual(DifficultyLevel.Medium, this.difficultyManager.GetDifficultyLevel(5.0f));

            // Test Hard
            Assert.AreEqual(DifficultyLevel.Hard, this.difficultyManager.GetDifficultyLevel(8.0f));
        }
    }
}
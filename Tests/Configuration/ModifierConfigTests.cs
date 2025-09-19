using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Configuration
{
    [TestFixture]
    public class ModifierConfigTests
    {
        private ModifierConfig config;

        [SetUp]
        public void Setup()
        {
            this.config = new ModifierConfig();
        }

        [Test]
        public void Constructor_InitializesDefaults()
        {
            // Assert
            Assert.IsTrue(this.config.Enabled);
            Assert.AreEqual(DifficultyConstants.DEFAULT_MODIFIER_PRIORITY, this.config.Priority);
            Assert.IsNotNull(this.config.Parameters);
            Assert.IsNotNull(this.config.ResponseCurve);
        }

        [Test]
        public void SetModifierType_SetsCorrectly()
        {
            // Act
            this.config.SetModifierType("TestModifier");

            // Assert
            Assert.AreEqual("TestModifier", this.config.ModifierType);
        }

        [Test]
        public void SetParameter_AddsNewParameter()
        {
            // Act
            this.config.SetParameter("TestParam", 5.5f);

            // Assert
            Assert.AreEqual(5.5f, this.config.GetParameter("TestParam"));
        }

        [Test]
        public void SetParameter_UpdatesExistingParameter()
        {
            // Arrange
            this.config.SetParameter("TestParam", 3.0f);

            // Act
            this.config.SetParameter("TestParam", 7.0f);

            // Assert
            Assert.AreEqual(7.0f, this.config.GetParameter("TestParam"));
            Assert.AreEqual(1, this.config.Parameters.Count);
        }

        [Test]
        public void GetParameter_NonExistent_ReturnsDefault()
        {
            // Act
            float value = this.config.GetParameter("NonExistent", 10f);

            // Assert
            Assert.AreEqual(10f, value);
        }

        [Test]
        public void GetParameter_Existent_ReturnsValue()
        {
            // Arrange
            this.config.SetParameter("ExistingParam", 15f);

            // Act
            float value = this.config.GetParameter("ExistingParam", 10f);

            // Assert
            Assert.AreEqual(15f, value);
        }

        [Test]
        public void GetParameter_DefaultZero()
        {
            // Act
            float value = this.config.GetParameter("NonExistent");

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, value);
        }

        [Test]
        public void SetParameter_MultipleParameters()
        {
            // Act
            this.config.SetParameter("Param1", 1f);
            this.config.SetParameter("Param2", 2f);
            this.config.SetParameter("Param3", 3f);

            // Assert
            Assert.AreEqual(1f, this.config.GetParameter("Param1"));
            Assert.AreEqual(2f, this.config.GetParameter("Param2"));
            Assert.AreEqual(3f, this.config.GetParameter("Param3"));
            Assert.AreEqual(3, this.config.Parameters.Count);
        }

        [Test]
        public void Parameters_NullSafe()
        {
            // Arrange - Force null parameters
            var newConfig = new ModifierConfig();
            var field = typeof(ModifierConfig).GetField("parameters",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(newConfig, null);

            // Act & Assert - Should handle null gracefully
            Assert.DoesNotThrow(() => newConfig.SetParameter("Test", 1f));
            Assert.AreEqual(1f, newConfig.GetParameter("Test"));
        }

        [Test]
        public void ResponseCurve_DefaultIsLinear()
        {
            // Assert
            Assert.AreEqual(0f, this.config.ResponseCurve.Evaluate(0f));
            Assert.AreEqual(1f, this.config.ResponseCurve.Evaluate(1f));

            // Check linearity at midpoint
            float midValue = this.config.ResponseCurve.Evaluate(0.5f);
            Assert.AreEqual(0.5f, midValue, 0.1f); // Allow some tolerance for curve approximation
        }

        [Test]
        public void Enabled_DefaultIsTrue()
        {
            // Assert
            Assert.IsTrue(this.config.Enabled);
        }

        [Test]
        public void Priority_DefaultIsZero()
        {
            // Assert
            Assert.AreEqual(DifficultyConstants.DEFAULT_MODIFIER_PRIORITY, this.config.Priority);
        }

        [Test]
        public void ModifierParameter_StoresCorrectly()
        {
            // Arrange
            var param = new ModifierParameter
            {
                Key = "TestKey",
                Value = 42f
            };

            // Assert
            Assert.AreEqual("TestKey", param.Key);
            Assert.AreEqual(42f, param.Value);
        }

        [Test]
        public void AllProperties_AreAccessible()
        {
            // Arrange
            this.config.SetModifierType("TestType");
            this.config.SetParameter("TestParam", 5f);

            // Act & Assert - Test all public properties
            Assert.AreEqual("TestType", this.config.ModifierType);
            Assert.IsTrue(this.config.Enabled);
            Assert.AreEqual(DifficultyConstants.DEFAULT_MODIFIER_PRIORITY, this.config.Priority);
            Assert.IsNotNull(this.config.ResponseCurve);
            Assert.IsNotNull(this.config.Parameters);
        }
    }
}
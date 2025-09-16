using System.Collections.Generic;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Unit.Calculators
{
    [TestFixture]
    public class DifficultyCalculatorTests : DifficultyTestBase
    {
        private DifficultyCalculator calculator;
        private List<TestModifier> testModifiers;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            testModifiers = new List<TestModifier>();
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());
        }

        [Test]
        public void Calculate_NoModifiers_ReturnsCurrentDifficulty()
        {
            // Arrange
            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(5f, result.PreviousDifficulty);
            Assert.AreEqual(5f, result.NewDifficulty);
            Assert.IsEmpty(result.AppliedModifiers);
            Assert.AreEqual("No modifiers applied", result.PrimaryReason);
        }

        [Test]
        public void Calculate_SingleModifier_AppliesModification()
        {
            // Arrange
            var modifier = new TestModifier("Test", 1, 2.0f);
            testModifiers.Add(modifier);
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(5f, result.PreviousDifficulty);
            Assert.AreEqual(7f, result.NewDifficulty); // 5 + 2
            Assert.AreEqual(1, result.AppliedModifiers.Count);
            Assert.AreEqual("Test", result.AppliedModifiers[0].ModifierName);
            Assert.AreEqual(2.0f, result.AppliedModifiers[0].Value);
        }

        [Test]
        public void Calculate_MultipleModifiers_AggregatesValues()
        {
            // Arrange
            testModifiers.Add(new TestModifier("Mod1", 1, 1.5f));
            testModifiers.Add(new TestModifier("Mod2", 2, -0.5f));
            testModifiers.Add(new TestModifier("Mod3", 3, 0.5f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(5f, result.PreviousDifficulty);
            Assert.AreEqual(6.5f, result.NewDifficulty); // 5 + 1.5 - 0.5 + 0.5
            Assert.AreEqual(3, result.AppliedModifiers.Count);
        }

        [Test]
        public void Calculate_RespectsMinimumDifficulty()
        {
            // Arrange
            testModifiers.Add(new TestModifier("BigReduction", 1, -10f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(3f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(3f, result.PreviousDifficulty);
            Assert.AreEqual(1f, result.NewDifficulty); // Clamped to min (1)
            Assert.That(result.PrimaryReason, Does.Contain("clamped to minimum"));
        }

        [Test]
        public void Calculate_RespectsMaximumDifficulty()
        {
            // Arrange
            testModifiers.Add(new TestModifier("BigIncrease", 1, 20f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(8f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(8f, result.PreviousDifficulty);
            Assert.AreEqual(10f, result.NewDifficulty); // Clamped to max (10)
            Assert.That(result.PrimaryReason, Does.Contain("clamped to maximum"));
        }

        [Test]
        public void Calculate_RespectsMaxChangePerSession()
        {
            // Arrange - maxChangePerSession = 2
            testModifiers.Add(new TestModifier("BigChange", 1, 5f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(5f, result.PreviousDifficulty);
            Assert.AreEqual(7f, result.NewDifficulty); // 5 + 2 (max change)
            Assert.That(result.PrimaryReason, Does.Contain("limited to max change"));
        }

        [Test]
        public void Calculate_DisabledModifier_IsSkipped()
        {
            // Arrange
            var enabledModifier = new TestModifier("Enabled", 1, 1f, enabled: true);
            var disabledModifier = new TestModifier("Disabled", 2, 2f, enabled: false);
            testModifiers.Add(enabledModifier);
            testModifiers.Add(disabledModifier);
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(6f, result.NewDifficulty); // Only enabled modifier applied
            Assert.AreEqual(1, result.AppliedModifiers.Count);
            Assert.AreEqual("Enabled", result.AppliedModifiers[0].ModifierName);
        }

        [Test]
        public void Calculate_ModifiersAppliedInPriorityOrder()
        {
            // Arrange
            testModifiers.Add(new TestModifier("Low", 10, 1f));
            testModifiers.Add(new TestModifier("High", 1, 2f));
            testModifiers.Add(new TestModifier("Medium", 5, 1.5f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual(3, result.AppliedModifiers.Count);
            Assert.AreEqual("High", result.AppliedModifiers[0].ModifierName);
            Assert.AreEqual("Medium", result.AppliedModifiers[1].ModifierName);
            Assert.AreEqual("Low", result.AppliedModifiers[2].ModifierName);
        }

        [Test]
        public void Calculate_PrimaryReasonFromLargestModifier()
        {
            // Arrange
            testModifiers.Add(new TestModifier("Small", 1, 0.5f, "Small change"));
            testModifiers.Add(new TestModifier("Large", 2, -2f, "Large reduction"));
            testModifiers.Add(new TestModifier("Medium", 3, 1f, "Medium increase"));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert
            Assert.AreEqual("Large reduction", result.PrimaryReason);
        }

        [Test]
        public void Calculate_NullSessionData_UsesDefault()
        {
            // Act
            var result = calculator.Calculate(null);

            // Assert
            Assert.AreEqual(3f, result.PreviousDifficulty); // Default difficulty
            Assert.AreEqual(3f, result.NewDifficulty);
        }

        [Test]
        public void Calculate_NegativeMaxChange_HandlesGracefully()
        {
            // Arrange - Test with negative max change
            testModifiers.Add(new TestModifier("Reduction", 1, -3f));
            calculator = new DifficultyCalculator(defaultConfig, testModifiers.As<List<IDifficultyModifier>>());

            var sessionData = SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build();

            // Act
            var result = calculator.Calculate(sessionData);

            // Assert - Should still respect max change of 2
            Assert.AreEqual(3f, result.NewDifficulty); // 5 - 2 (max change)
        }

        // Test helper class
        private class TestModifier : IDifficultyModifier
        {
            public string ModifierName { get; }
            public int Priority { get; }
            private float value;
            private string reason;
            private bool enabled;

            public TestModifier(string name, int priority, float value, string reason = null, bool enabled = true)
            {
                ModifierName = name;
                Priority = priority;
                this.value = value;
                this.reason = reason ?? $"{name} applied";
                this.enabled = enabled;
            }

            public ModifierResult Calculate(PlayerSessionData sessionData)
            {
                return new ModifierResult
                {
                    ModifierName = ModifierName,
                    Value = value,
                    Reason = reason
                };
            }

            public bool IsEnabled() => enabled;
            public void OnApplied(ModifierResult result) { }
        }
    }

    // Extension helper for test
    internal static class ListExtensions
    {
        public static List<T> As<T>(this object list) => list as List<T>;
    }
}
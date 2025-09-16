using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Unit.Modifiers
{
    [TestFixture]
    public class WinStreakModifierTests : DifficultyTestBase
    {
        private WinStreakModifier modifier;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var config = defaultConfig.modifierConfigs.Find(m => m.modifierName == "WinStreak");
            modifier = new WinStreakModifier(config);
        }

        [Test]
        public void Calculate_NoWinStreak_ReturnsZero()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithWinStreak(0)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No win streak", result.Reason);
        }

        [Test]
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithWinStreak(2) // Below threshold of 3
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Win streak below threshold", result.Reason);
        }

        [Test]
        public void Calculate_AtThreshold_ReturnsStepSize()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithWinStreak(3) // At threshold
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0.5f, result.Value); // StepSize = 0.5
            Assert.That(result.Reason, Does.Contain("3 wins in a row"));
        }

        [Test]
        public void Calculate_AboveThreshold_ScalesLinearly()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithWinStreak(5) // 2 above threshold
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // Base (0.5) + (2 * 0.5) = 1.5
            Assert.AreEqual(1.5f, result.Value);
            Assert.That(result.Reason, Does.Contain("5 wins in a row"));
        }

        [Test]
        public void Calculate_RespectsMaxBonus()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithWinStreak(10) // Very high streak
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(2f, result.Value); // MaxBonus = 2
            Assert.That(result.Reason, Does.Contain("10 wins in a row"));
            Assert.That(result.Reason, Does.Contain("capped"));
        }

        [Test]
        public void Calculate_WithResponseCurve_AppliesCurve()
        {
            // ResponseCurve = 1.0 means linear (no change)
            // Test with different streak values
            var testCases = new[]
            {
                (streak: 3, expected: 0.5f),
                (streak: 4, expected: 1.0f),
                (streak: 5, expected: 1.5f),
                (streak: 6, expected: 2.0f) // Hits max
            };

            foreach (var testCase in testCases)
            {
                var data = SessionDataBuilder.Create()
                    .WithWinStreak(testCase.streak)
                    .Build();

                var result = modifier.Calculate(data);

                AssertApproximatelyEqual(testCase.expected, result.Value,
                    message: $"Win streak {testCase.streak}");
            }
        }

        [Test]
        public void Calculate_NegativeWinStreak_ReturnsZero()
        {
            // Edge case: negative win streak (shouldn't happen but handle gracefully)
            var data = SessionDataBuilder.Create()
                .WithWinStreak(-5)
                .Build();

            var result = modifier.Calculate(data);

            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void ModifierName_ReturnsCorrectName()
        {
            Assert.AreEqual("WinStreak", modifier.ModifierName);
        }

        [Test]
        public void Priority_ReturnsCorrectPriority()
        {
            Assert.AreEqual(1, modifier.Priority);
        }

        [Test]
        public void IsEnabled_ReturnsTrue()
        {
            Assert.IsTrue(modifier.IsEnabled());
        }

        [TestCase(3, 0.5f, "At threshold")]
        [TestCase(4, 1.0f, "One above threshold")]
        [TestCase(5, 1.5f, "Two above threshold")]
        [TestCase(7, 2.0f, "Capped at max")]
        [TestCase(100, 2.0f, "Very high streak still capped")]
        public void Calculate_VariousStreaks_ReturnsExpectedValue(int streak, float expected, string description)
        {
            var data = SessionDataBuilder.Create()
                .WithWinStreak(streak)
                .Build();

            var result = modifier.Calculate(data);

            AssertApproximatelyEqual(expected, result.Value,
                message: $"{description} (streak: {streak})");
        }
    }
}
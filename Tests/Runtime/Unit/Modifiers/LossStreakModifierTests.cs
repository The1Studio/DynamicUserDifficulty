using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Unit.Modifiers
{
    [TestFixture]
    public class LossStreakModifierTests : DifficultyTestBase
    {
        private LossStreakModifier modifier;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var config = defaultConfig.modifierConfigs.Find(m => m.modifierName == "LossStreak");
            modifier = new LossStreakModifier(config);
        }

        [Test]
        public void Calculate_NoLossStreak_ReturnsZero()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLossStreak(0)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No loss streak", result.Reason);
        }

        [Test]
        public void Calculate_BelowThreshold_ReturnsZero()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLossStreak(1) // Below threshold of 2
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Loss streak below threshold", result.Reason);
        }

        [Test]
        public void Calculate_AtThreshold_ReturnsNegativeStepSize()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLossStreak(2) // At threshold
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-0.3f, result.Value); // StepSize = 0.3, negative for reduction
            Assert.That(result.Reason, Does.Contain("2 losses in a row"));
        }

        [Test]
        public void Calculate_AboveThreshold_ScalesWithResponseCurve()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLossStreak(4) // 2 above threshold
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // With ResponseCurve = 1.2:
            // scaledStreak = 2^1.2 ≈ 2.297
            // value = -0.3 * (1 + 2.297) ≈ -0.989
            AssertApproximatelyEqual(-0.989f, result.Value, 0.01f);
            Assert.That(result.Reason, Does.Contain("4 losses in a row"));
        }

        [Test]
        public void Calculate_RespectsMaxReduction()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLossStreak(10) // Very high loss streak
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-1.5f, result.Value); // MaxReduction = 1.5
            Assert.That(result.Reason, Does.Contain("10 losses in a row"));
            Assert.That(result.Reason, Does.Contain("capped"));
        }

        [Test]
        public void Calculate_AlwaysReturnsNegativeValue()
        {
            // Test various loss streaks to ensure they always reduce difficulty
            var testCases = new[] { 2, 3, 4, 5, 10, 20 };

            foreach (var streak in testCases)
            {
                var data = SessionDataBuilder.Create()
                    .WithLossStreak(streak)
                    .Build();

                var result = modifier.Calculate(data);

                Assert.Less(result.Value, 0f,
                    $"Loss streak {streak} should return negative value");
                Assert.GreaterOrEqual(result.Value, -1.5f,
                    $"Loss streak {streak} should not exceed max reduction");
            }
        }

        [Test]
        public void Calculate_ResponseCurveAmplification()
        {
            // ResponseCurve = 1.2 should make higher streaks more impactful
            var testCases = new[]
            {
                (streak: 2, minExpected: -0.3f, maxExpected: -0.3f), // At threshold
                (streak: 3, minExpected: -0.6f, maxExpected: -0.65f),  // Some amplification
                (streak: 4, minExpected: -0.95f, maxExpected: -1.0f),  // More amplification
                (streak: 5, minExpected: -1.3f, maxExpected: -1.4f),   // Strong amplification
                (streak: 6, minExpected: -1.5f, maxExpected: -1.5f)    // Capped at max
            };

            foreach (var testCase in testCases)
            {
                var data = SessionDataBuilder.Create()
                    .WithLossStreak(testCase.streak)
                    .Build();

                var result = modifier.Calculate(data);

                AssertInRange(result.Value, testCase.minExpected, testCase.maxExpected,
                    $"Loss streak {testCase.streak}");
            }
        }

        [Test]
        public void ModifierName_ReturnsCorrectName()
        {
            Assert.AreEqual("LossStreak", modifier.ModifierName);
        }

        [Test]
        public void Priority_ReturnsCorrectPriority()
        {
            Assert.AreEqual(2, modifier.Priority);
        }

        [Test]
        public void Calculate_NegativeLossStreak_ReturnsZero()
        {
            // Edge case: negative loss streak
            var data = SessionDataBuilder.Create()
                .WithLossStreak(-5)
                .Build();

            var result = modifier.Calculate(data);

            Assert.AreEqual(0f, result.Value);
        }

        [TestCase(0, 0f, "No losses")]
        [TestCase(1, 0f, "Below threshold")]
        [TestCase(2, -0.3f, "At threshold")]
        public void Calculate_EdgeCases_ReturnsExpectedValue(int streak, float expected, string description)
        {
            var data = SessionDataBuilder.Create()
                .WithLossStreak(streak)
                .Build();

            var result = modifier.Calculate(data);

            AssertApproximatelyEqual(expected, result.Value, 0.01f,
                message: $"{description} (streak: {streak})");
        }

        [Test]
        public void Calculate_ConsecutiveLosses_IncreasesAssistance()
        {
            // Simulate increasing loss streak
            float previousValue = 0f;

            for (int streak = 2; streak <= 5; streak++)
            {
                var data = SessionDataBuilder.Create()
                    .WithLossStreak(streak)
                    .Build();

                var result = modifier.Calculate(data);

                Assert.Less(result.Value, previousValue,
                    $"Loss streak {streak} should provide more assistance than {streak - 1}");

                previousValue = result.Value;
            }
        }
    }
}
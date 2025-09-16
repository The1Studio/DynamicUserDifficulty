using System;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Unit.Modifiers
{
    [TestFixture]
    public class TimeDecayModifierTests : DifficultyTestBase
    {
        private TimeDecayModifier modifier;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var config = defaultConfig.modifierConfigs.Find(m => m.modifierName == "TimeDecay");
            modifier = new TimeDecayModifier(config);
        }

        [Test]
        public void Calculate_PlayedRecently_NoDecay()
        {
            // Arrange - Played 2 hours ago (within grace period of 6 hours)
            var data = SessionDataBuilder.Create()
                .WithHoursAgo(2)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Played recently, no decay needed", result.Reason);
        }

        [Test]
        public void Calculate_JustOutsideGracePeriod_NoDecay()
        {
            // Arrange - Played exactly 6 hours ago (at grace period boundary)
            var data = SessionDataBuilder.Create()
                .WithHoursAgo(6)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Played recently, no decay needed", result.Reason);
        }

        [Test]
        public void Calculate_OneDayAway_AppliesDecay()
        {
            // Arrange - Played 1 day ago
            var data = SessionDataBuilder.Create()
                .WithDaysAgo(1)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // 1 day * 0.5 decay per day = -0.5
            Assert.AreEqual(-0.5f, result.Value);
            Assert.That(result.Reason, Does.Contain("1 days away"));
        }

        [Test]
        public void Calculate_MultipleDaysAway_ScalesLinearly()
        {
            // Arrange - Played 3 days ago
            var data = SessionDataBuilder.Create()
                .WithDaysAgo(3)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // 3 days * 0.5 decay per day = -1.5
            Assert.AreEqual(-1.5f, result.Value);
            Assert.That(result.Reason, Does.Contain("3 days away"));
        }

        [Test]
        public void Calculate_RespectsMaxDecay()
        {
            // Arrange - Played 10 days ago
            var data = SessionDataBuilder.Create()
                .WithDaysAgo(10)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // Should be capped at MaxDecay = -2.0
            Assert.AreEqual(-2.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("10 days away"));
            Assert.That(result.Reason, Does.Contain("capped"));
        }

        [Test]
        public void Calculate_PartialDays_CalculatesCorrectly()
        {
            // Arrange - Played 36 hours ago (1.5 days)
            var data = SessionDataBuilder.Create()
                .WithHoursAgo(36)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            // 1.5 days * 0.5 decay per day = -0.75
            AssertApproximatelyEqual(-0.75f, result.Value, 0.01f);
        }

        [Test]
        public void Calculate_FutureDate_NoDecay()
        {
            // Edge case: LastPlayTime is in the future
            var data = SessionDataBuilder.Create()
                .WithLastPlayTime(DateTime.Now.AddDays(1))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void Calculate_ExactlyAtGracePeriodBoundary()
        {
            // Test the exact boundary condition
            var testCases = new[]
            {
                (hours: 5.99, expectedDecay: 0f, description: "Just inside grace period"),
                (hours: 6.0, expectedDecay: 0f, description: "Exactly at grace period"),
                (hours: 6.01, expectedDecay: 0f, description: "Just outside grace period, but less than a day"),
                (hours: 24.0, expectedDecay: -0.5f, description: "Exactly one day"),
                (hours: 30.0, expectedDecay: -0.625f, description: "1.25 days")
            };

            foreach (var testCase in testCases)
            {
                var data = SessionDataBuilder.Create()
                    .WithLastPlayTime(DateTime.Now.AddHours(-testCase.hours))
                    .Build();

                var result = modifier.Calculate(data);

                AssertApproximatelyEqual(testCase.expectedDecay, result.Value, 0.01f,
                    message: testCase.description);
            }
        }

        [Test]
        public void ModifierName_ReturnsCorrectName()
        {
            Assert.AreEqual("TimeDecay", modifier.ModifierName);
        }

        [Test]
        public void Priority_ReturnsCorrectPriority()
        {
            Assert.AreEqual(3, modifier.Priority);
        }

        [TestCase(0, 0f, "Just played")]
        [TestCase(3, 0f, "Within grace period")]
        [TestCase(6, 0f, "At grace period")]
        [TestCase(24, -0.5f, "One day")]
        [TestCase(48, -1.0f, "Two days")]
        [TestCase(72, -1.5f, "Three days")]
        [TestCase(96, -2.0f, "Four days")]
        [TestCase(240, -2.0f, "Ten days - capped")]
        public void Calculate_VariousTimePeriods_ReturnsExpectedDecay(
            int hoursAgo, float expectedDecay, string description)
        {
            var data = SessionDataBuilder.Create()
                .WithHoursAgo(hoursAgo)
                .Build();

            var result = modifier.Calculate(data);

            AssertApproximatelyEqual(expectedDecay, result.Value, 0.01f,
                message: $"{description} ({hoursAgo} hours ago)");
        }

        [Test]
        public void Calculate_ReturningPlayer_EncouragesWithLowerDifficulty()
        {
            // Test that returning players get progressive difficulty reduction
            var testDays = new[] { 1, 2, 3, 4, 5 };
            float previousDecay = 0f;

            foreach (var days in testDays)
            {
                var data = SessionDataBuilder.Create()
                    .WithDaysAgo(days)
                    .Build();

                var result = modifier.Calculate(data);

                Assert.Less(result.Value, previousDecay,
                    $"Day {days} should have more decay than day {days - 1}");

                previousDecay = result.Value;
            }
        }
    }
}
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Unit.Modifiers
{
    [TestFixture]
    public class RageQuitModifierTests : DifficultyTestBase
    {
        private RageQuitModifier modifier;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            var config = defaultConfig.modifierConfigs.Find(m => m.modifierName == "RageQuit");
            modifier = new RageQuitModifier(config);
        }

        [Test]
        public void Calculate_NoLastSession_ReturnsZero()
        {
            // Arrange
            var data = SessionDataBuilder.Create()
                .WithLastSession(null)
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("No previous session", result.Reason);
        }

        [Test]
        public void Calculate_WonLastSession_ReturnsZero()
        {
            // Arrange - Won the last session
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(5)
                    .WithLevelsWon(3) // Won more than half
                    .WithDuration(300f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Last session was not a loss", result.Reason);
        }

        [Test]
        public void Calculate_QuickLossUnderThreshold_AppliesReduction()
        {
            // Arrange - Lost quickly (25 seconds, under 30 second threshold)
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .AsRageQuit(25f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-1.0f, result.Value); // DifficultyReduction = 1.0
            Assert.That(result.Reason, Does.Contain("Rage quit detected"));
            Assert.That(result.Reason, Does.Contain("25.0s"));
        }

        [Test]
        public void Calculate_LossOverThreshold_NoReduction()
        {
            // Arrange - Lost but played for longer (60 seconds)
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(1)
                    .WithLevelsWon(0)
                    .WithDuration(60f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Loss but not rage quit (60.0s)", result.Reason);
        }

        [Test]
        public void Calculate_ExactlyAtThreshold_NoReduction()
        {
            // Arrange - Exactly at 30 second threshold
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(1)
                    .WithLevelsWon(0)
                    .WithDuration(30f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
            Assert.AreEqual("Loss but not rage quit (30.0s)", result.Reason);
        }

        [Test]
        public void Calculate_JustUnderThreshold_AppliesReduction()
        {
            // Arrange - Just under 30 second threshold
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(1)
                    .WithLevelsWon(0)
                    .WithDuration(29.9f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-1.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("Rage quit detected"));
        }

        [Test]
        public void Calculate_MultipleQuickLosses_DetectsRageQuit()
        {
            // Arrange - Played 2 levels quickly and lost both
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(2)
                    .WithLevelsWon(0)
                    .WithDuration(25f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-1.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("Rage quit detected"));
        }

        [Test]
        public void Calculate_ZeroDuration_AppliesReduction()
        {
            // Edge case: Zero duration session (instant quit)
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(1)
                    .WithLevelsWon(0)
                    .WithDuration(0f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(-1.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("Rage quit detected"));
        }

        [Test]
        public void Calculate_NegativeDuration_ReturnsZero()
        {
            // Edge case: Negative duration (shouldn't happen)
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(1)
                    .WithLevelsWon(0)
                    .WithDuration(-10f))
                .Build();

            // Act
            var result = modifier.Calculate(data);

            // Assert
            Assert.AreEqual(0f, result.Value);
        }

        [Test]
        public void ModifierName_ReturnsCorrectName()
        {
            Assert.AreEqual("RageQuit", modifier.ModifierName);
        }

        [Test]
        public void Priority_ReturnsCorrectPriority()
        {
            Assert.AreEqual(4, modifier.Priority);
        }

        [TestCase(0, 1, 10f, -1.0f, "Very quick loss")]
        [TestCase(0, 1, 20f, -1.0f, "Quick loss")]
        [TestCase(0, 1, 29f, -1.0f, "Just under threshold")]
        [TestCase(0, 1, 30f, 0f, "At threshold")]
        [TestCase(0, 1, 31f, 0f, "Just over threshold")]
        [TestCase(0, 1, 120f, 0f, "Normal loss")]
        [TestCase(1, 1, 25f, 0f, "Won but quit quick")]
        [TestCase(3, 5, 25f, 0f, "Won majority but quit")]
        public void Calculate_VariousScenarios_ReturnsExpectedValue(
            int levelsWon, int levelsPlayed, float duration,
            float expectedValue, string description)
        {
            var data = SessionDataBuilder.Create()
                .WithLastSession(session => session
                    .WithLevelsPlayed(levelsPlayed)
                    .WithLevelsWon(levelsWon)
                    .WithDuration(duration))
                .Build();

            var result = modifier.Calculate(data);

            AssertApproximatelyEqual(expectedValue, result.Value, 0.01f,
                message: description);
        }

        [Test]
        public void Calculate_WithCooldownPeriod_ConsidersFreshness()
        {
            // Test that rage quit detection might consider how recent it was
            // (though current implementation doesn't use CooldownHours)
            var data = SessionDataBuilder.Create()
                .WithHoursAgo(2) // Recent play
                .WithQuickLoss(20f)
                .Build();

            var result = modifier.Calculate(data);

            Assert.AreEqual(-1.0f, result.Value);
            Assert.That(result.Reason, Does.Contain("Rage quit detected"));
        }

        [Test]
        public void OnApplied_Called_HandlesGracefully()
        {
            // Test that OnApplied can be called without errors
            var result = new ModifierResult
            {
                ModifierName = "RageQuit",
                Value = -1.0f,
                Reason = "Rage quit detected"
            };

            Assert.DoesNotThrow(() => modifier.OnApplied(result));
        }
    }
}
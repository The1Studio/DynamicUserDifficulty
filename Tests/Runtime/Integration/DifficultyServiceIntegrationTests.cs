using System;
using System.Collections.Generic;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Modifiers.Implementations;
using TheOneStudio.DynamicUserDifficulty.Providers;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Base;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Builders;
using TheOneStudio.DynamicUserDifficulty.Tests.TestFramework.Mocks;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Runtime.Integration
{
    [TestFixture]
    public class DifficultyServiceIntegrationTests : DifficultyTestBase
    {
        private DynamicDifficultyService service;
        private MockSessionDataProvider mockProvider;
        private List<IDifficultyModifier> modifiers;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // Create mock provider
            mockProvider = new MockSessionDataProvider();

            // Create real modifiers with test config
            modifiers = new List<IDifficultyModifier>
            {
                new WinStreakModifier(defaultConfig.modifierConfigs[0]),
                new LossStreakModifier(defaultConfig.modifierConfigs[1]),
                new TimeDecayModifier(defaultConfig.modifierConfigs[2]),
                new RageQuitModifier(defaultConfig.modifierConfigs[3])
            };

            // Create calculator
            var calculator = new DifficultyCalculator(defaultConfig, modifiers);

            // Create service
            service = new DynamicDifficultyService(calculator, mockProvider, defaultConfig);
            service.Initialize();
        }

        [Test]
        public void FullGameplay_WinStreak_IncreasesDifficulty()
        {
            // Simulate winning streak
            float initialDifficulty = service.CurrentDifficulty;

            // Win 3 games in a row
            for (int i = 0; i < 3; i++)
            {
                service.OnLevelComplete(won: true, completionTime: 120f);
            }

            // Calculate new difficulty
            var result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);

            // Assert difficulty increased
            Assert.Greater(result.NewDifficulty, initialDifficulty);
            Assert.That(result.PrimaryReason, Does.Contain("wins in a row"));
        }

        [Test]
        public void FullGameplay_LossStreak_DecreasesDifficulty()
        {
            // Set initial difficulty
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .Build());

            service.Initialize();

            // Lose 3 games in a row
            for (int i = 0; i < 3; i++)
            {
                service.OnLevelComplete(won: false, completionTime: 180f);
            }

            // Calculate new difficulty
            var result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);

            // Assert difficulty decreased
            Assert.Less(result.NewDifficulty, 5f);
            Assert.That(result.PrimaryReason, Does.Contain("losses in a row"));
        }

        [Test]
        public void FullGameplay_RageQuit_AppliesAssistance()
        {
            // Set initial difficulty
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(7f)
                .Build());

            service.Initialize();

            // Simulate rage quit
            service.OnLevelComplete(won: false, completionTime: 15f);
            service.OnSessionEnd(SessionEndType.QuitAfterLoss);

            // Start new session
            service.Initialize();
            var result = service.CalculateDifficulty();

            // Assert rage quit modifier applied
            var rageQuitModifier = result.AppliedModifiers.Find(m => m.ModifierName == "RageQuit");
            Assert.IsNotNull(rageQuitModifier);
            Assert.Less(rageQuitModifier.Value, 0);
        }

        [Test]
        public void FullGameplay_ReturningPlayer_GetsTimeDecay()
        {
            // Set last play time to 3 days ago
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(8f)
                .WithDaysAgo(3)
                .Build());

            service.Initialize();

            // Calculate difficulty for returning player
            var result = service.CalculateDifficulty();

            // Assert time decay applied
            var timeDecayModifier = result.AppliedModifiers.Find(m => m.ModifierName == "TimeDecay");
            Assert.IsNotNull(timeDecayModifier);
            Assert.Less(timeDecayModifier.Value, 0);
            Assert.Less(result.NewDifficulty, 8f);
        }

        [Test]
        public void FullGameplay_MultipleModifiers_CombineCorrectly()
        {
            // Set up scenario with multiple modifiers
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(5f)
                .WithWinStreak(4)      // Win streak bonus
                .WithDaysAgo(2)        // Time decay reduction
                .Build());

            service.Initialize();

            // Calculate difficulty
            var result = service.CalculateDifficulty();

            // Assert multiple modifiers applied
            Assert.GreaterOrEqual(result.AppliedModifiers.Count, 2);

            // Find specific modifiers
            var winStreakMod = result.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            var timeDecayMod = result.AppliedModifiers.Find(m => m.ModifierName == "TimeDecay");

            Assert.IsNotNull(winStreakMod);
            Assert.IsNotNull(timeDecayMod);
            Assert.Greater(winStreakMod.Value, 0); // Increases difficulty
            Assert.Less(timeDecayMod.Value, 0);    // Decreases difficulty
        }

        [Test]
        public void SessionTracking_RecordsWinsAndLosses()
        {
            // Play a mixed session
            service.OnLevelComplete(won: true, completionTime: 100f);
            service.OnLevelComplete(won: false, completionTime: 150f);
            service.OnLevelComplete(won: true, completionTime: 120f);
            service.OnSessionEnd(SessionEndType.Normal);

            // Check saved data
            var savedData = mockProvider.LastSavedData;
            Assert.IsNotNull(savedData);
            Assert.IsNotNull(savedData.LastSession);
            Assert.AreEqual(3, savedData.LastSession.LevelsPlayed);
            Assert.AreEqual(2, savedData.LastSession.LevelsWon);
        }

        [Test]
        public void Persistence_SavesAndLoadsData()
        {
            // Set initial state
            service.OnLevelComplete(won: true, completionTime: 100f);
            service.OnLevelComplete(won: true, completionTime: 120f);
            service.OnLevelComplete(won: true, completionTime: 110f);

            var firstResult = service.CalculateDifficulty();
            service.ApplyDifficulty(firstResult);

            // Simulate app restart with new service instance
            var newService = new DynamicDifficultyService(
                new DifficultyCalculator(defaultConfig, modifiers),
                mockProvider,
                defaultConfig);

            newService.Initialize();

            // Should load previous data
            Assert.AreEqual(firstResult.NewDifficulty, newService.CurrentDifficulty);

            // Continue playing
            newService.OnLevelComplete(won: true, completionTime: 100f);
            var secondResult = newService.CalculateDifficulty();

            // Win streak should continue
            var winStreakMod = secondResult.AppliedModifiers.Find(m => m.ModifierName == "WinStreak");
            Assert.IsNotNull(winStreakMod);
            Assert.Greater(winStreakMod.Value, 0);
        }

        [Test]
        public void DifficultyBounds_RespectsLimits()
        {
            // Test maximum difficulty
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(9f)
                .WithWinStreak(10) // Very high streak
                .Build());

            service.Initialize();
            var result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);

            Assert.LessOrEqual(result.NewDifficulty, 10f);

            // Test minimum difficulty
            mockProvider.SetMockData(SessionDataBuilder.Create()
                .WithDifficulty(2f)
                .WithLossStreak(10) // Very high loss streak
                .Build());

            service.Initialize();
            result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);

            Assert.GreaterOrEqual(result.NewDifficulty, 1f);
        }

        [Test]
        public void SessionEnd_DifferentTypes_HandledCorrectly()
        {
            // Test different session end types
            var endTypes = new[]
            {
                SessionEndType.Normal,
                SessionEndType.QuitAfterWin,
                SessionEndType.QuitAfterLoss,
                SessionEndType.Timeout
            };

            foreach (var endType in endTypes)
            {
                service.OnLevelComplete(won: endType == SessionEndType.QuitAfterWin,
                                       completionTime: 30f);
                service.OnSessionEnd(endType);

                var savedData = mockProvider.LastSavedData;
                Assert.IsNotNull(savedData);
                Assert.IsNotNull(savedData.LastSession);
            }
        }

        [Test]
        public void CompletePlayerJourney_NewToExperienced()
        {
            // Start as new player
            Assert.AreEqual(3f, service.CurrentDifficulty); // Default difficulty

            // Early losses - difficulty should decrease
            service.OnLevelComplete(won: false, completionTime: 200f);
            service.OnLevelComplete(won: false, completionTime: 180f);

            var result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);
            float strugglingDifficulty = result.NewDifficulty;
            Assert.Less(strugglingDifficulty, 3f);

            // Start winning - difficulty should increase
            for (int i = 0; i < 5; i++)
            {
                service.OnLevelComplete(won: true, completionTime: 120f);
            }

            result = service.CalculateDifficulty();
            service.ApplyDifficulty(result);
            float improvingDifficulty = result.NewDifficulty;
            Assert.Greater(improvingDifficulty, strugglingDifficulty);

            // Mixed performance - difficulty stabilizes
            service.OnLevelComplete(won: true, completionTime: 100f);
            service.OnLevelComplete(won: false, completionTime: 150f);
            service.OnLevelComplete(won: true, completionTime: 110f);

            result = service.CalculateDifficulty();
            float stabilizedDifficulty = result.NewDifficulty;

            // Should be somewhere in the middle
            AssertInRange(stabilizedDifficulty, 1f, 10f);
        }

        [Test]
        public void ErrorHandling_InvalidData_HandlesGracefully()
        {
            // Test with null provider data
            mockProvider.SetMockData(null);
            Assert.DoesNotThrow(() => service.Initialize());

            // Test with throwing provider
            mockProvider.ThrowOnRetrieve = true;
            var newService = new DynamicDifficultyService(
                new DifficultyCalculator(defaultConfig, modifiers),
                mockProvider,
                defaultConfig);

            Assert.DoesNotThrow(() => newService.Initialize());
            Assert.AreEqual(defaultConfig.defaultDifficulty, newService.CurrentDifficulty);
        }
    }
}
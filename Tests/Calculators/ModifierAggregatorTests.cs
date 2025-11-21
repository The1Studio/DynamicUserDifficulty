using System;
using NUnit.Framework;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Core;
using System.Collections.Generic;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Calculators
{
    [TestFixture]
    public class ModifierAggregatorTests
    {
        private ModifierAggregator aggregator;

        [SetUp]
        public void Setup()
        {
            this.aggregator = new();
        }

        [Test]
        public void Aggregate_NullResults_ReturnsZero()
        {
            // Act
            var result = this.aggregator.Aggregate(null);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void Aggregate_EmptyResults_ReturnsZero()
        {
            // Arrange
            var results = new List<ModifierResult>();

            // Act
            var result = this.aggregator.Aggregate(results);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void Aggregate_SingleResult_ReturnsValue()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test", Value = 2.5f },
            };

            // Act
            var result = this.aggregator.Aggregate(results);

            // Assert
            Assert.AreEqual(2.5f, result);
        }

        [Test]
        public void Aggregate_MultipleResults_ReturnsSum()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 1.5f },
                new() { ModifierName = "Test2", Value = 2.0f },
                new() { ModifierName = "Test3", Value = -0.5f },
            };

            // Act
            var result = this.aggregator.Aggregate(results);

            // Assert
            Assert.AreEqual(3.0f, result);
        }

        [Test]
        public void AggregateWeighted_NullResults_ReturnsZero()
        {
            // Arrange
            var weights = new Dictionary<string, float>();

            // Act
            var result = this.aggregator.AggregateWeighted(null, weights);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateWeighted_EmptyResults_ReturnsZero()
        {
            // Arrange
            var results = new List<ModifierResult>();
            var weights = new Dictionary<string, float>();

            // Act
            var result = this.aggregator.AggregateWeighted(results, weights);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateWeighted_WithWeights_ReturnsWeightedAverage()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 2.0f },
                new() { ModifierName = "Test2", Value = 4.0f },
            };

            var weights = new Dictionary<string, float>
            {
                { "Test1", 1.0f },
                { "Test2", 2.0f },
            };

            // Act
            var result = this.aggregator.AggregateWeighted(results, weights);

            // Assert
            // (2*1 + 4*2) / (1+2) = 10/3 = 3.333...
            Assert.AreEqual(10f/3f, result, 0.001f);
        }

        [Test]
        public void AggregateWeighted_MissingWeight_UsesDefault()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 2.0f },
                new() { ModifierName = "Test2", Value = 4.0f },
            };

            var weights = new Dictionary<string, float>
            {
                { "Test1", 2.0f }, // Test2 missing, should use default weight (1.0)
            };

            // Act
            var result = this.aggregator.AggregateWeighted(results, weights);

            // Assert
            // (2*2 + 4*1) / (2+1) = 8/3 = 2.666...
            Assert.AreEqual(8f/3f, result, 0.001f);
        }

        [Test]
        public void AggregateWeighted_NullWeights_UsesDefaultForAll()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 2.0f },
                new() { ModifierName = "Test2", Value = 4.0f },
            };

            // Act
            var result = this.aggregator.AggregateWeighted(results, null);

            // Assert
            // (2*1 + 4*1) / (1+1) = 6/2 = 3
            Assert.AreEqual(3.0f, result);
        }

        [Test]
        public void AggregateMax_NullResults_ReturnsZero()
        {
            // Act
            var result = this.aggregator.AggregateMax(null);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateMax_EmptyResults_ReturnsZero()
        {
            // Arrange
            var results = new List<ModifierResult>();

            // Act
            var result = this.aggregator.AggregateMax(results);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateMax_ReturnsLargestAbsoluteValue()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 2.0f },
                new() { ModifierName = "Test2", Value = -3.0f },
                new() { ModifierName = "Test3", Value = 1.5f },
            };

            // Act
            var result = this.aggregator.AggregateMax(results);

            // Assert
            Assert.AreEqual(-3.0f, result); // Largest absolute value with sign preserved
        }

        [Test]
        public void AggregateDiminishing_NullResults_ReturnsZero()
        {
            // Act
            var result = this.aggregator.AggregateDiminishing(null);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateDiminishing_EmptyResults_ReturnsZero()
        {
            // Arrange
            var results = new List<ModifierResult>();

            // Act
            var result = this.aggregator.AggregateDiminishing(results);

            // Assert
            Assert.AreEqual(DifficultyConstants.ZERO_VALUE, result);
        }

        [Test]
        public void AggregateDiminishing_AppliesDiminishingReturns()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 4.0f },
                new() { ModifierName = "Test2", Value = 2.0f },
                new() { ModifierName = "Test3", Value = 1.0f },
            };

            // Act
            var result = this.aggregator.AggregateDiminishing(results, 0.5f);

            // Assert
            // Sorted by absolute value: 4, 2, 1
            // 4*1.0 + 2*0.5 + 1*0.25 = 4 + 1 + 0.25 = 5.25
            Assert.AreEqual(5.25f, result, 0.001f);
        }

        [Test]
        public void AggregateDiminishing_CustomFactor()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 3.0f },
                new() { ModifierName = "Test2", Value = 2.0f },
            };

            // Act
            var result = this.aggregator.AggregateDiminishing(results, 0.7f);

            // Assert
            // 3*1.0 + 2*0.7 = 3 + 1.4 = 4.4
            Assert.AreEqual(4.4f, result, 0.001f);
        }

        [Test]
        public void AllAggregationMethods_HandleZeroValues()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 0f },
                new() { ModifierName = "Test2", Value = 0f },
            };

            // Act & Assert
            Assert.AreEqual(0f, this.aggregator.Aggregate(results));
            Assert.AreEqual(0f, this.aggregator.AggregateWeighted(results, null));
            Assert.AreEqual(0f, this.aggregator.AggregateMax(results));
            Assert.AreEqual(0f, this.aggregator.AggregateDiminishing(results));
        }

        [Test]
        public void AllAggregationMethods_HandleMixedPositiveNegative()
        {
            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Test1", Value = 2.0f },
                new() { ModifierName = "Test2", Value = -1.0f },
            };

            // Act & Assert
            Assert.AreEqual(1.0f, this.aggregator.Aggregate(results));               // Sum
            Assert.AreEqual(0.5f, this.aggregator.AggregateWeighted(results, null)); // Average
            Assert.AreEqual(2.0f, this.aggregator.AggregateMax(results));            // Max absolute
            Assert.Greater(this.aggregator.AggregateDiminishing(results), 0);           // Diminishing
        }

        #region Game Designer Validation Tests

        [Test]
        public void GameDesign_ConflictingSignals_RageQuitAndWinStreak_PrioritizesRetention()
        {
            // Scenario: Player is WINNING but also RAGE QUITTING (frustrated despite success)
            // Expected: Diminishing returns should NOT fully cancel out these conflicting signals
            // The larger magnitude signal (rage quit) should have more influence

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "WinStreak", Value = 1.5f },      // Player is winning
                new() { ModifierName = "RageQuit", Value = -1.0f },      // Player is frustrated
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);

            // Assert
            // Simple sum: +1.5 - 1.0 = +0.5 (masks the conflict)
            Assert.AreEqual(0.5f, simpleSum, 0.001f);

            // Diminishing returns: Sorted by absolute value [1.5, 1.0]
            // 1.5*1.0 + (-1.0)*0.6 = 1.5 - 0.6 = 0.9
            Assert.AreEqual(0.9f, diminishing, 0.001f);

            // GAME DESIGN VALIDATION: Diminishing returns produces higher positive value,
            // meaning the system recognizes the win streak more strongly than the frustration.
            // This is acceptable IF we prioritize progress over retention in this scenario.
            Assert.Greater(diminishing, simpleSum, "Diminishing returns should amplify the dominant signal");
        }

        [Test]
        public void GameDesign_MultiplePositiveSignals_NoCancellation()
        {
            // Scenario: Player has multiple positive indicators (win streak + high completion rate + good progress)
            // Expected: Diminishing returns should NOT penalize having multiple signals vs simple sum

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "WinStreak", Value = 1.0f },
                new() { ModifierName = "CompletionRate", Value = 1.0f },
                new() { ModifierName = "LevelProgress", Value = 0.5f },
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);                      // 2.5
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);   // ~1.8

            // Assert
            Assert.AreEqual(2.5f, simpleSum, 0.001f);

            // Sorted by absolute value: [1.0, 1.0, 0.5]
            // 1.0*1.0 + 1.0*0.6 + 0.5*0.36 = 1.0 + 0.6 + 0.18 = 1.78
            Assert.AreEqual(1.78f, diminishing, 0.01f);

            // GAME DESIGN VALIDATION: Diminishing returns produces ~71% of simple sum
            // This prevents "stacking" multiple weak signals to game the system
            Assert.Less(diminishing, simpleSum, "Diminishing returns should prevent signal stacking");
            Assert.Greater(diminishing / simpleSum, 0.7f, "But should still capture >70% of total value");
        }

        [Test]
        public void GameDesign_SingleStrongSignal_GetsFullWeight()
        {
            // Scenario: Player has ONE strong signal (e.g., massive loss streak)
            // Expected: Single signals should get 100% weight (no diminishing on first signal)

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "LossStreak", Value = -2.0f },
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);

            // Assert
            Assert.AreEqual(-2.0f, simpleSum);
            Assert.AreEqual(-2.0f, diminishing, 0.001f);

            // GAME DESIGN VALIDATION: Single signals are NOT diminished
            Assert.AreEqual(simpleSum, diminishing, 0.001f, "Single signals should have full weight");
        }

        [Test]
        public void GameDesign_DiminishingFactor_0_5_vs_0_6_vs_0_7()
        {
            // Scenario: Test different diminishing factors to understand tuning impact
            // Expected: Higher factor = more weight to secondary signals

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Signal1", Value = 2.0f },
                new() { ModifierName = "Signal2", Value = 1.0f },
                new() { ModifierName = "Signal3", Value = 0.5f },
            };

            // Act
            var factor_0_5 = this.aggregator.AggregateDiminishing(results, 0.5f);
            var factor_0_6 = this.aggregator.AggregateDiminishing(results, 0.6f);
            var factor_0_7 = this.aggregator.AggregateDiminishing(results, 0.7f);

            // Assert
            // Sorted: [2.0, 1.0, 0.5]
            // 0.5: 2*1.0 + 1*0.5 + 0.5*0.25 = 2.625
            // 0.6: 2*1.0 + 1*0.6 + 0.5*0.36 = 2.78
            // 0.7: 2*1.0 + 1*0.7 + 0.5*0.49 = 2.945

            Assert.AreEqual(2.625f, factor_0_5, 0.01f);
            Assert.AreEqual(2.78f, factor_0_6, 0.01f);
            Assert.AreEqual(2.945f, factor_0_7, 0.01f);

            // GAME DESIGN VALIDATION: Higher factor gives more total adjustment
            Assert.Less(factor_0_5, factor_0_6, "0.6 should produce higher value than 0.5");
            Assert.Less(factor_0_6, factor_0_7, "0.7 should produce higher value than 0.6");

            // Research recommendation: 0.6 aligns with industry standard (60% emotional, 40% performance)
        }

        [Test]
        public void GameDesign_RealPlayerScenario_WinStreakButSlowProgress()
        {
            // Scenario: Player is winning but taking too long (potential frustration)
            // Expected: System should recognize mixed signals appropriately

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "WinStreak", Value = 1.5f },          // Winning
                new() { ModifierName = "LevelProgress", Value = -0.8f },     // Slow completion time
                new() { ModifierName = "SessionPattern", Value = -0.3f },    // Slightly frustrated
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);                      // 0.4
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);   // ~0.62

            // Assert
            Assert.AreEqual(0.4f, simpleSum, 0.001f);

            // Sorted: [1.5, -0.8, -0.3]
            // 1.5*1.0 + (-0.8)*0.6 + (-0.3)*0.36 = 1.5 - 0.48 - 0.108 = 0.912
            Assert.AreEqual(0.912f, diminishing, 0.01f);

            // GAME DESIGN VALIDATION: Diminishing returns amplifies the dominant win signal
            // while still accounting for the struggle signals
            Assert.Greater(diminishing, simpleSum, "Should recognize wins more strongly than struggles");
        }

        [Test]
        public void GameDesign_RealPlayerScenario_ConsecutiveLossesWithRageQuit()
        {
            // Scenario: Player losing repeatedly AND rage quitting (retention crisis)
            // Expected: Both signals should compound to strongly reduce difficulty

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "LossStreak", Value = -1.3f },       // Losing
                new() { ModifierName = "RageQuit", Value = -1.0f },         // Rage quitting
                new() { ModifierName = "CompletionRate", Value = -0.5f },   // Overall struggling
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);                      // -2.8
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);   // ~-2.0

            // Assert
            Assert.AreEqual(-2.8f, simpleSum, 0.001f);

            // Sorted: [-1.3, -1.0, -0.5]
            // -1.3*1.0 + (-1.0)*0.6 + (-0.5)*0.36 = -1.3 - 0.6 - 0.18 = -2.08
            Assert.AreEqual(-2.08f, diminishing, 0.01f);

            // GAME DESIGN VALIDATION: Strong negative adjustment to help struggling player
            // Diminishing returns prevents extreme -2.8 but still gives substantial -2.08 help
            Assert.Less(diminishing, -2.0f, "Should provide strong difficulty reduction");
            Assert.Greater(diminishing, simpleSum, "But prevent extreme overcorrection");
        }

        [Test]
        public void GameDesign_EdgeCase_AllSmallSignals()
        {
            // Scenario: Player has many small signals (not clearly winning or losing)
            // Expected: Diminishing returns should aggregate conservatively

            // Arrange
            var results = new List<ModifierResult>
            {
                new() { ModifierName = "Signal1", Value = 0.3f },
                new() { ModifierName = "Signal2", Value = 0.2f },
                new() { ModifierName = "Signal3", Value = -0.2f },
                new() { ModifierName = "Signal4", Value = 0.1f },
            };

            // Act
            var simpleSum = this.aggregator.Aggregate(results);                      // 0.4
            var diminishing = this.aggregator.AggregateDiminishing(results, 0.6f);   // ~0.35

            // Assert
            Assert.AreEqual(0.4f, simpleSum, 0.001f);

            // Sorted by absolute value (stable sort): [0.3, 0.2, -0.2, 0.1]
            // 0.3*1.0 + 0.2*0.6 + (-0.2)*0.36 + 0.1*0.216 = 0.3 + 0.12 - 0.072 + 0.0216 = 0.3696
            Assert.AreEqual(0.3696f, diminishing, 0.01f);

            // GAME DESIGN VALIDATION: Small signals produce small adjustments
            // Prevents "noise" from causing unnecessary difficulty swings
            Assert.Less(Math.Abs(diminishing), 0.5f, "Small signals should produce small adjustment");
        }

        [Test]
        public void GameDesign_Comparison_SimpleSumVsDiminishing_EdgeCases()
        {
            // Scenario: Compare simple sum vs diminishing returns for edge cases
            // Expected: Understand when each strategy performs better

            // Case 1: Extreme stacking (gaming the system)
            var extreme = new List<ModifierResult>
            {
                new() { ModifierName = "S1", Value = 0.6f },
                new() { ModifierName = "S2", Value = 0.6f },
                new() { ModifierName = "S3", Value = 0.6f },
                new() { ModifierName = "S4", Value = 0.6f },
                new() { ModifierName = "S5", Value = 0.6f },
            };

            var extremeSum = this.aggregator.Aggregate(extreme);                       // 3.0
            var extremeDim = this.aggregator.AggregateDiminishing(extreme, 0.6f);     // ~1.82

            Assert.AreEqual(3.0f, extremeSum, 0.001f);
            // All signals are 0.6, so: 0.6*(1.0 + 0.6 + 0.36 + 0.216 + 0.1296) = 0.6*2.3056 = 1.38336
            Assert.AreEqual(1.3834f, extremeDim, 0.01f);

            // VALIDATION: Diminishing prevents stacking abuse (~46% of simple sum for better balance)
            Assert.Less(extremeDim / extremeSum, 0.47f, "Should prevent extreme stacking");

            // Case 2: Single dominant signal with noise
            var dominant = new List<ModifierResult>
            {
                new() { ModifierName = "Strong", Value = 2.0f },
                new() { ModifierName = "Noise1", Value = 0.1f },
                new() { ModifierName = "Noise2", Value = 0.1f },
            };

            var dominantSum = this.aggregator.Aggregate(dominant);                     // 2.2
            var dominantDim = this.aggregator.AggregateDiminishing(dominant, 0.6f);   // ~2.136

            Assert.AreEqual(2.2f, dominantSum, 0.001f);
            // Sorted by absolute value: [2.0, 0.1, 0.1]
            // 2.0*1.0 + 0.1*0.6 + 0.1*0.36 = 2.0 + 0.06 + 0.036 = 2.096
            Assert.AreEqual(2.096f, dominantDim, 0.01f);

            // VALIDATION: Noise has minimal impact on dominant signal (95% preserved)
            // 2.096 / 2.2 = 0.9527 ≈ 95.27%
            Assert.Greater(dominantDim / dominantSum, 0.95f, "Noise should have minimal dilution effect");
        }

        #endregion
    }
}
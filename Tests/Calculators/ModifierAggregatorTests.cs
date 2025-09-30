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
    }
}
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Calculators
{
    /// <summary>
    /// Aggregates multiple modifier results into a single difficulty adjustment
    /// </summary>
    public class ModifierAggregator
    {
        /// <summary>
        /// Aggregates multiple modifier results using sum strategy
        /// </summary>
        /// <param name="results">Collection of modifier results</param>
        /// <returns>Total difficulty adjustment</returns>
        public virtual float Aggregate(List<ModifierResult> results)
        {
            if (results == null || results.Count == 0)
                return DifficultyConstants.ZERO_VALUE;

            // Default strategy: Sum all modifier values
            return results.Sum(r => r.Value);
        }

        /// <summary>
        /// Alternative aggregation using weighted average
        /// </summary>
        public float AggregateWeighted(List<ModifierResult> results, Dictionary<string, float> weights)
        {
            if (results == null || results.Count == 0)
                return DifficultyConstants.ZERO_VALUE;

            float totalValue = DifficultyConstants.ZERO_VALUE;
            float totalWeight = DifficultyConstants.ZERO_VALUE;

            foreach (var result in results)
            {
                float weight = weights?.GetValueOrDefault(result.ModifierName, DifficultyConstants.DEFAULT_AGGREGATION_WEIGHT) 
                              ?? DifficultyConstants.DEFAULT_AGGREGATION_WEIGHT;
                totalValue += result.Value * weight;
                totalWeight += weight;
            }

            return totalWeight > DifficultyConstants.ZERO_VALUE ? totalValue / totalWeight : DifficultyConstants.ZERO_VALUE;
        }

        /// <summary>
        /// Alternative aggregation using maximum absolute value
        /// </summary>
        public float AggregateMax(List<ModifierResult> results)
        {
            if (results == null || results.Count == 0)
                return DifficultyConstants.ZERO_VALUE;

            var maxResult = results.OrderByDescending(r => Mathf.Abs(r.Value)).FirstOrDefault();
            return maxResult?.Value ?? DifficultyConstants.ZERO_VALUE;
        }

        /// <summary>
        /// Alternative aggregation with diminishing returns
        /// </summary>
        public float AggregateDiminishing(List<ModifierResult> results, float diminishFactor = DifficultyConstants.DEFAULT_DIMINISHING_FACTOR)
        {
            if (results == null || results.Count == 0)
                return DifficultyConstants.ZERO_VALUE;

            // Sort by absolute value (largest first)
            var sortedResults = results.OrderByDescending(r => Mathf.Abs(r.Value)).ToList();

            float total = DifficultyConstants.ZERO_VALUE;
            float currentFactor = DifficultyConstants.DEFAULT_AGGREGATION_WEIGHT;

            foreach (var result in sortedResults)
            {
                total += result.Value * currentFactor;
                currentFactor *= diminishFactor; // Each subsequent modifier has less impact
            }

            return total;
        }
    }
}
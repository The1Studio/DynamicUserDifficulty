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
                return 0f;

            // Default strategy: Sum all modifier values
            return results.Sum(r => r.Value);
        }

        /// <summary>
        /// Alternative aggregation using weighted average
        /// </summary>
        public float AggregateWeighted(List<ModifierResult> results, Dictionary<string, float> weights)
        {
            if (results == null || results.Count == 0)
                return 0f;

            float totalValue = 0f;
            float totalWeight = 0f;

            foreach (var result in results)
            {
                float weight = weights?.GetValueOrDefault(result.ModifierName, 1f) ?? 1f;
                totalValue += result.Value * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? totalValue / totalWeight : 0f;
        }

        /// <summary>
        /// Alternative aggregation using maximum absolute value
        /// </summary>
        public float AggregateMax(List<ModifierResult> results)
        {
            if (results == null || results.Count == 0)
                return 0f;

            var maxResult = results.OrderByDescending(r => Mathf.Abs(r.Value)).FirstOrDefault();
            return maxResult?.Value ?? 0f;
        }

        /// <summary>
        /// Alternative aggregation with diminishing returns
        /// </summary>
        public float AggregateDiminishing(List<ModifierResult> results, float diminishFactor = 0.5f)
        {
            if (results == null || results.Count == 0)
                return 0f;

            // Sort by absolute value (largest first)
            var sortedResults = results.OrderByDescending(r => Mathf.Abs(r.Value)).ToList();

            float total = 0f;
            float currentFactor = 1f;

            foreach (var result in sortedResults)
            {
                total += result.Value * currentFactor;
                currentFactor *= diminishFactor; // Each subsequent modifier has less impact
            }

            return total;
        }
    }
}
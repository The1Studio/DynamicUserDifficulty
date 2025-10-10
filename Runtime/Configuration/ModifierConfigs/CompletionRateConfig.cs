using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Completion Rate modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class CompletionRateConfig : BaseModifierConfig
    {
        [Header("Completion Rate Settings")]

        [SerializeField][Range(0f, 1f)]
        [Tooltip("Minimum completion rate before difficulty decreases (0-1)")]
        private float lowCompletionThreshold = 0.4f;

        [SerializeField][Range(0f, 1f)]
        [Tooltip("Maximum completion rate before difficulty increases (0-1)")]
        private float highCompletionThreshold = 0.7f;

        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty decrease when below low threshold")]
        private float lowCompletionDecrease = 0.5f;

        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty increase when above high threshold")]
        private float highCompletionIncrease = 0.5f;

        [SerializeField][Range(5, 50)]
        [Tooltip("Minimum number of attempts before this modifier activates")]
        private int minAttemptsRequired = 10;

        [SerializeField][Range(0f, 1f)]
        [Tooltip("Weight factor for total wins/losses consideration")]
        private float totalStatsWeight = 0.3f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_COMPLETION_RATE;

        // Type-safe properties
        public float LowCompletionThreshold => this.lowCompletionThreshold;
        public float HighCompletionThreshold => this.highCompletionThreshold;
        public float LowCompletionDecrease => this.lowCompletionDecrease;
        public float HighCompletionIncrease => this.highCompletionIncrease;
        public int MinAttemptsRequired => this.minAttemptsRequired;
        public float TotalStatsWeight => this.totalStatsWeight;

        public override IModifierConfig CreateDefault()
        {
            var config = new CompletionRateConfig
            {
                lowCompletionThreshold = 0.4f,
                highCompletionThreshold = 0.7f,
                lowCompletionDecrease = 0.5f,
                highCompletionIncrease = 0.5f,
                minAttemptsRequired = 10,
                totalStatsWeight = 0.3f,
            };
            config.SetEnabled(true);
            config.SetPriority(4);
            return config;
        }

        public override void GenerateFromStats(GameStats stats)
        {
            // lowCompletionThreshold = winRate - 20% (trigger help mode early)
            float winRateNormalized = stats.winRatePercentage / 100f;
            this.lowCompletionThreshold = winRateNormalized - 0.2f;
            this.lowCompletionThreshold = Mathf.Clamp(this.lowCompletionThreshold, 0f, 1f);

            // highCompletionThreshold = winRate + 10% (challenge mode)
            this.highCompletionThreshold = winRateNormalized + 0.1f;
            this.highCompletionThreshold = Mathf.Clamp(this.highCompletionThreshold, 0f, 1f);

            // Ensure low < high
            if (this.lowCompletionThreshold >= this.highCompletionThreshold)
            {
                this.highCompletionThreshold = this.lowCompletionThreshold + 0.2f;
            }

            // lowCompletionDecrease = maxChange / 4 (moderate help)
            this.lowCompletionDecrease = stats.maxDifficultyChangePerSession / 4f;
            this.lowCompletionDecrease = Mathf.Clamp(this.lowCompletionDecrease, 0.1f, 2f);

            // highCompletionIncrease = lowCompletionDecrease (symmetrical)
            this.highCompletionIncrease = this.lowCompletionDecrease;

            // minAttemptsRequired = avgAttemptsPerLevel * 4 (reasonable sample)
            this.minAttemptsRequired = Mathf.RoundToInt(stats.avgAttemptsPerLevel * 4f);
            this.minAttemptsRequired = Mathf.Clamp(this.minAttemptsRequired, 5, 50);

            // totalStatsWeight = 30% (balance recent vs overall)
            this.totalStatsWeight = 0.3f;
        }
    }
}
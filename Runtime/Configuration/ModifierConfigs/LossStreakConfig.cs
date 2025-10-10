using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Loss Streak modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class LossStreakConfig : BaseModifierConfig
    {

        [Header("Loss Streak Settings")]
        [SerializeField][Range(1, 10)]
        [Tooltip("Number of consecutive losses needed to trigger difficulty decrease")]
        private float lossThreshold = 2f;

        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty decrease per threshold exceeded")]
        private float stepSize = 0.3f;

        [SerializeField][Range(0.5f, 5f)]
        [Tooltip("Maximum difficulty reduction from loss streaks")]
        private float maxReduction = 1.5f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK;

        // Type-safe properties
        public float LossThreshold => this.lossThreshold;
        public float StepSize      => this.stepSize;
        public float MaxReduction  => this.maxReduction;

        public override IModifierConfig CreateDefault()
        {
            var config = new LossStreakConfig();
            config.SetEnabled(true);
            config.SetPriority(2);
            // Field values are already set to defaults via DifficultyConstants
            return config;
        }

        public override void GenerateFromStats(GameStats stats)
        {
            // lossThreshold = avgConsecutiveLosses * 0.8 (trigger slightly before average)
            this.lossThreshold = Mathf.Max(2f, Mathf.Round(stats.avgConsecutiveLosses * 0.8f));

            // stepSize = range / (avgConsecutiveLosses * 3) (gentler decrease than win streak)
            float diffRange = stats.difficultyMax - stats.difficultyMin;
            float divisor = Mathf.Max(0.1f, stats.avgConsecutiveLosses * 3f);
            this.stepSize = diffRange / divisor;
            this.stepSize = Mathf.Clamp(this.stepSize, 0.1f, 2f);

            // maxReduction = 25% of range (less aggressive than win streak bonus)
            this.maxReduction = diffRange * 0.25f;
            this.maxReduction = Mathf.Clamp(this.maxReduction, 0.5f, 5f);
        }
    }
}
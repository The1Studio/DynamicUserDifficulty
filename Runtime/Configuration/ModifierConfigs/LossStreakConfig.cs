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
    }
}
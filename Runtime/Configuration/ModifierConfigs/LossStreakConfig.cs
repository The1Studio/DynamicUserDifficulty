using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Loss Streak modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class LossStreakConfig : IModifierConfig
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority = 2;

        [Header("Loss Streak Settings")]
        [SerializeField, Range(1, 10)]
        [Tooltip("Number of consecutive losses needed to trigger difficulty decrease")]
        private float lossThreshold = DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("Difficulty decrease per threshold exceeded")]
        private float stepSize = DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE;

        [SerializeField, Range(0.5f, 5f)]
        [Tooltip("Maximum difficulty reduction from loss streaks")]
        private float maxReduction = DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION;

        // IModifierConfig implementation
        public string ModifierType => DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK;
        public bool IsEnabled => this.isEnabled;
        public int Priority => this.priority;

        // Type-safe properties
        public float LossThreshold => this.lossThreshold;
        public float StepSize => this.stepSize;
        public float MaxReduction => this.maxReduction;

        public IModifierConfig CreateDefault()
        {
            return new LossStreakConfig
            {
                isEnabled = true,
                priority = 2,
                lossThreshold = DifficultyConstants.LOSS_STREAK_DEFAULT_THRESHOLD,
                stepSize = DifficultyConstants.LOSS_STREAK_DEFAULT_STEP_SIZE,
                maxReduction = DifficultyConstants.LOSS_STREAK_DEFAULT_MAX_REDUCTION
            };
        }
    }
}
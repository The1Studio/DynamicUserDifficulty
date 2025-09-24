using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Level Progress modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class LevelProgressConfig : BaseModifierConfig
    {
        [Header("Attempts Settings")]

        [SerializeField][Range(3, 10)]
        [Tooltip("Number of attempts indicating player is struggling")]
        private int highAttemptsThreshold = 5;

        [SerializeField][Range(0.1f, 0.5f)]
        [Tooltip("Difficulty decrease per attempt over threshold")]
        private float difficultyDecreasePerAttempt = 0.2f;

        [Header("Completion Time Settings")]

        [SerializeField][Range(0.3f, 0.9f)]
        [Tooltip("Percentage of average time considered fast (0.5 = 50% of average)")]
        private float fastCompletionRatio = 0.7f;

        [SerializeField][Range(1.1f, 2.0f)]
        [Tooltip("Percentage of average time considered slow (1.5 = 150% of average)")]
        private float slowCompletionRatio = 1.5f;

        [SerializeField][Range(0.1f, 1f)]
        [Tooltip("Difficulty increase for fast completion")]
        private float fastCompletionBonus = 0.3f;

        [SerializeField][Range(0.1f, 1f)]
        [Tooltip("Difficulty decrease for slow completion")]
        private float slowCompletionPenalty = 0.3f;

        [Header("Level Progression Settings")]

        [SerializeField][Range(5, 30)]
        [Tooltip("Expected levels per hour for normal progression")]
        private int expectedLevelsPerHour = 15;

        [SerializeField][Range(0.05f, 0.2f)]
        [Tooltip("Difficulty adjustment based on level difference from expected")]
        private float levelProgressionFactor = 0.1f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_LEVEL_PROGRESS;

        // Type-safe properties
        public int HighAttemptsThreshold => this.highAttemptsThreshold;
        public float DifficultyDecreasePerAttempt => this.difficultyDecreasePerAttempt;
        public float FastCompletionRatio => this.fastCompletionRatio;
        public float SlowCompletionRatio => this.slowCompletionRatio;
        public float FastCompletionBonus => this.fastCompletionBonus;
        public float SlowCompletionPenalty => this.slowCompletionPenalty;
        public int ExpectedLevelsPerHour => this.expectedLevelsPerHour;
        public float LevelProgressionFactor => this.levelProgressionFactor;

        public override IModifierConfig CreateDefault()
        {
            var config = new LevelProgressConfig
            {
                highAttemptsThreshold = 5,
                difficultyDecreasePerAttempt = 0.2f,
                fastCompletionRatio = 0.7f,
                slowCompletionRatio = 1.5f,
                fastCompletionBonus = 0.3f,
                slowCompletionPenalty = 0.3f,
                expectedLevelsPerHour = 15,
                levelProgressionFactor = 0.1f,
            };
            config.SetEnabled(true);
            config.SetPriority(4);
            return config;
        }
    }
}
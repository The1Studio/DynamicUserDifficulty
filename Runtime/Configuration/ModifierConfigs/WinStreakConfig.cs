using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Win Streak modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class WinStreakConfig : BaseModifierConfig
    {

        [Header("Win Streak Settings")]
        [SerializeField][Range(1, 10)]
        [Tooltip("Number of consecutive wins needed to trigger difficulty increase")]
        private float winThreshold = 3f;

        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty increase per threshold exceeded")]
        private float stepSize = 0.5f;

        [SerializeField][Range(0.5f, 5f)]
        [Tooltip("Maximum difficulty increase from win streaks")]
        private float maxBonus = 2f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_WIN_STREAK;

        // Type-safe properties
        public float WinThreshold => this.winThreshold;
        public float StepSize     => this.stepSize;
        public float MaxBonus     => this.maxBonus;

        public override IModifierConfig CreateDefault()
        {
            var config = new WinStreakConfig();
            config.SetEnabled(true);
            config.SetPriority(1);
            // Field values are already set to defaults via DifficultyConstants
            return config;
        }

        public override void GenerateFromStats(GameStats stats)
        {
            // winThreshold = avgConsecutiveWins * 0.75 (trigger before average ends)
            this.winThreshold = Mathf.Max(2f, Mathf.Round(stats.avgConsecutiveWins * 0.75f));

            // stepSize = range / (avgConsecutiveWins * 2) (gradual scaling)
            // SAFE: Prevent division by zero
            float diffRange = stats.difficultyMax - stats.difficultyMin;
            float divisor = Mathf.Max(0.1f, stats.avgConsecutiveWins * 2f);
            this.stepSize = diffRange / divisor;
            this.stepSize = Mathf.Clamp(this.stepSize, 0.1f, 2f);

            // maxBonus = 30% of range (prevent extremes)
            this.maxBonus = diffRange * 0.3f;
            this.maxBonus = Mathf.Clamp(this.maxBonus, 0.5f, 5f);
        }
    }
}
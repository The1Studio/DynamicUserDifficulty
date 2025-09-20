using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Win Streak modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class WinStreakConfig : IModifierConfig
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority = 1;

        [Header("Win Streak Settings")]
        [SerializeField, Range(1, 10)]
        [Tooltip("Number of consecutive wins needed to trigger difficulty increase")]
        private float winThreshold = DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("Difficulty increase per threshold exceeded")]
        private float stepSize = DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE;

        [SerializeField, Range(0.5f, 5f)]
        [Tooltip("Maximum difficulty increase from win streaks")]
        private float maxBonus = DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS;

        // IModifierConfig implementation
        public string ModifierType => DifficultyConstants.MODIFIER_TYPE_WIN_STREAK;
        public bool IsEnabled => this.isEnabled;
        public int Priority => this.priority;

        // Type-safe properties
        public float WinThreshold => this.winThreshold;
        public float StepSize => this.stepSize;
        public float MaxBonus => this.maxBonus;

        public IModifierConfig CreateDefault()
        {
            return new WinStreakConfig
            {
                isEnabled = true,
                priority = 1,
                winThreshold = DifficultyConstants.WIN_STREAK_DEFAULT_THRESHOLD,
                stepSize = DifficultyConstants.WIN_STREAK_DEFAULT_STEP_SIZE,
                maxBonus = DifficultyConstants.WIN_STREAK_DEFAULT_MAX_BONUS
            };
        }
    }
}
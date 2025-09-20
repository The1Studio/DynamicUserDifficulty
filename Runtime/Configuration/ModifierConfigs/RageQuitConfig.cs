using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Rage Quit modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class RageQuitConfig : IModifierConfig
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority = 4;

        [Header("Rage Quit Settings")]
        [SerializeField, Range(5f, 120f)]
        [Tooltip("Time threshold in seconds to consider as rage quit")]
        private float rageQuitThreshold = DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD;

        [SerializeField, Range(0.5f, 3f)]
        [Tooltip("Difficulty reduction for rage quit")]
        private float rageQuitReduction = DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("Difficulty reduction for normal quit")]
        private float quitReduction = DifficultyConstants.QUIT_DEFAULT_REDUCTION;

        [SerializeField, Range(0.1f, 1f)]
        [Tooltip("Difficulty reduction for mid-play quit")]
        private float midPlayReduction = DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION;

        // IModifierConfig implementation
        public string ModifierType => DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT;
        public bool IsEnabled => this.isEnabled;
        public int Priority => this.priority;

        // Type-safe properties
        public float RageQuitThreshold => this.rageQuitThreshold;
        public float RageQuitReduction => this.rageQuitReduction;
        public float QuitReduction => this.quitReduction;
        public float MidPlayReduction => this.midPlayReduction;

        public IModifierConfig CreateDefault()
        {
            return new RageQuitConfig
            {
                isEnabled = true,
                priority = 4,
                rageQuitThreshold = DifficultyConstants.RAGE_QUIT_TIME_THRESHOLD,
                rageQuitReduction = DifficultyConstants.RAGE_QUIT_DEFAULT_REDUCTION,
                quitReduction = DifficultyConstants.QUIT_DEFAULT_REDUCTION,
                midPlayReduction = DifficultyConstants.MID_PLAY_DEFAULT_REDUCTION
            };
        }
    }
}
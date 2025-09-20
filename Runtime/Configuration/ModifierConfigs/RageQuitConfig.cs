using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Rage Quit modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class RageQuitConfig : BaseModifierConfig
    {

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

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT;

        // Type-safe properties
        public float RageQuitThreshold => this.rageQuitThreshold;
        public float RageQuitReduction => this.rageQuitReduction;
        public float QuitReduction => this.quitReduction;
        public float MidPlayReduction => this.midPlayReduction;

        public override IModifierConfig CreateDefault()
        {
            var config = new RageQuitConfig();
            config.SetEnabled(true);
            config.SetPriority(4);
            // Field values are already set to defaults via DifficultyConstants
            return config;
        }
    }
}
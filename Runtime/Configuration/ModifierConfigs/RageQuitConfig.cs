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
        [SerializeField][Range(5f, 120f)]
        [Tooltip("Time threshold in seconds to consider as rage quit")]
        private float rageQuitThreshold = 30f;

        [SerializeField][Range(0.5f, 3f)]
        [Tooltip("Difficulty reduction for rage quit")]
        private float rageQuitReduction = 1f;

        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty reduction for normal quit")]
        private float quitReduction = 0.5f;

        [SerializeField][Range(0.1f, 1f)]
        [Tooltip("Difficulty reduction for mid-play quit")]
        private float midPlayReduction = 0.3f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT;

        // Type-safe properties
        public float RageQuitThreshold => this.rageQuitThreshold;
        public float RageQuitReduction => this.rageQuitReduction;
        public float QuitReduction     => this.quitReduction;
        public float MidPlayReduction  => this.midPlayReduction;

        public override IModifierConfig CreateDefault()
        {
            var config = new RageQuitConfig();
            config.SetEnabled(true);
            config.SetPriority(5);
            // Field values are already set to defaults via DifficultyConstants
            return config;
        }

        public override void GenerateFromStats(GameStats stats)
        {
            // rageQuitThreshold = avgLevelCompletionTime / 2 (less than half expected time)
            this.rageQuitThreshold = stats.avgLevelCompletionTimeSeconds / 2f;
            this.rageQuitThreshold = Mathf.Clamp(this.rageQuitThreshold, 5f, 120f);

            // rageQuitReduction = maxChange / 2 (significant but not max)
            this.rageQuitReduction = stats.maxDifficultyChangePerSession / 2f;
            this.rageQuitReduction = Mathf.Clamp(this.rageQuitReduction, 0.5f, 3f);

            // quitReduction = rageQuitReduction / 2 (half of rage quit)
            this.quitReduction = this.rageQuitReduction / 2f;
            this.quitReduction = Mathf.Clamp(this.quitReduction, 0.1f, 2f);

            // midPlayReduction = quitReduction / 2 (least severe)
            this.midPlayReduction = this.quitReduction / 2f;
            this.midPlayReduction = Mathf.Clamp(this.midPlayReduction, 0.1f, 1f);
        }
    }
}
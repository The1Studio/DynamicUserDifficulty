using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Time Decay modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class TimeDecayConfig : BaseModifierConfig
    {

        [Header("Time Decay Settings")]
        [SerializeField][Range(0.1f, 2f)]
        [Tooltip("Difficulty reduction per day of inactivity")]
        private float decayPerDay = 0.5f;

        [SerializeField][Range(0.5f, 5f)]
        [Tooltip("Maximum total difficulty reduction from time decay")]
        private float maxDecay = 2f;

        [SerializeField][Range(0f, 48f)]
        [Tooltip("Hours before decay starts (grace period)")]
        private float graceHours = 6f;

        // BaseModifierConfig implementation
        public override string ModifierType => DifficultyConstants.MODIFIER_TYPE_TIME_DECAY;

        // Type-safe properties
        public float DecayPerDay => this.decayPerDay;
        public float MaxDecay    => this.maxDecay;
        public float GraceHours  => this.graceHours;

        public override IModifierConfig CreateDefault()
        {
            var config = new TimeDecayConfig();
            config.SetEnabled(true);
            config.SetPriority(3);
            // Field values are already set to defaults via DifficultyConstants
            return config;
        }

        public override void GenerateFromStats(GameStats stats)
        {
            // decayPerDay = maxChange / targetRetentionDays (reach maxChange over retention period)
            this.decayPerDay = stats.maxDifficultyChangePerSession / stats.targetRetentionDays;
            this.decayPerDay = Mathf.Clamp(this.decayPerDay, 0.1f, 2f);

            // maxDecay = maxChange (align with session limits)
            this.maxDecay = stats.maxDifficultyChangePerSession;
            this.maxDecay = Mathf.Clamp(this.maxDecay, 0.5f, 5f);

            // graceHours = avgHoursBetweenSessions (no decay for regular players)
            this.graceHours = stats.avgHoursBetweenSessions;
            this.graceHours = Mathf.Clamp(this.graceHours, 0f, 48f);
        }
    }
}
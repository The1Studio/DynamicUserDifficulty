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
        private float decayPerDay = DifficultyConstants.TIME_DECAY_DEFAULT_DECAY_PER_DAY;

        [SerializeField][Range(0.5f, 5f)]
        [Tooltip("Maximum total difficulty reduction from time decay")]
        private float maxDecay = DifficultyConstants.TIME_DECAY_DEFAULT_MAX_DECAY;

        [SerializeField][Range(0f, 48f)]
        [Tooltip("Hours before decay starts (grace period)")]
        private float graceHours = DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS;

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
    }
}
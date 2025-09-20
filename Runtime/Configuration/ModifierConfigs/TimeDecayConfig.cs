using System;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs
{
    /// <summary>
    /// Configuration for Time Decay modifier with type-safe properties
    /// </summary>
    [Serializable]
    public class TimeDecayConfig : IModifierConfig
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority = 3;

        [Header("Time Decay Settings")]
        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("Difficulty reduction per day of inactivity")]
        private float decayPerDay = DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY;

        [SerializeField, Range(0.5f, 5f)]
        [Tooltip("Maximum total difficulty reduction from time decay")]
        private float maxDecay = DifficultyConstants.TIME_DECAY_DEFAULT_MAX;

        [SerializeField, Range(0f, 48f)]
        [Tooltip("Hours before decay starts (grace period)")]
        private float graceHours = DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS;

        // IModifierConfig implementation
        public string ModifierType => DifficultyConstants.MODIFIER_TYPE_TIME_DECAY;
        public bool IsEnabled => this.isEnabled;
        public int Priority => this.priority;

        // Type-safe properties
        public float DecayPerDay => this.decayPerDay;
        public float MaxDecay => this.maxDecay;
        public float GraceHours => this.graceHours;

        public IModifierConfig CreateDefault()
        {
            return new TimeDecayConfig
            {
                isEnabled = true,
                priority = 3,
                decayPerDay = DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY,
                maxDecay = DifficultyConstants.TIME_DECAY_DEFAULT_MAX,
                graceHours = DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS
            };
        }
    }
}
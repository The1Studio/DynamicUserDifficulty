#nullable enable

using System;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Abstract base class for modifier configurations.
    /// Concrete implementation for Unity serialization support.
    /// </summary>
    [Serializable]
    public abstract class BaseModifierConfig : IModifierConfig
    {
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private int priority = 0;

        public abstract string ModifierType { get; }
        public bool IsEnabled => this.isEnabled;
        public int Priority => this.priority;

        public abstract IModifierConfig CreateDefault();

        /// <summary>
        /// Generates optimal configuration values based on game statistics.
        /// Each modifier implements its own calculation logic.
        /// </summary>
        public abstract void GenerateFromStats(GameStats stats);

        /// <summary>
        /// Sets the enabled state (for Unity Inspector)
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            this.isEnabled = enabled;
        }

        /// <summary>
        /// Sets the priority (for Unity Inspector)
        /// </summary>
        public void SetPriority(int priority)
        {
            this.priority = priority;
        }
    }
}
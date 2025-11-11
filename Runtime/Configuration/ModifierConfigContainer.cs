#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Container for modifier configurations with polymorphic serialization support.
    /// Uses SerializeReference to support different config types.
    /// </summary>
    [Serializable]
    public sealed class ModifierConfigContainer : IEnumerable<IModifierConfig>
    {
        [SerializeReference]
        [Tooltip("List of modifier configurations. Use + to add new configs.")]
        private List<BaseModifierConfig> configs = new();

        /// <summary>
        /// Gets a strongly-typed configuration for a specific modifier type
        /// </summary>
        public T GetConfig<T>(string modifierType) where T : class, IModifierConfig
        {
            var config = this.configs?.FirstOrDefault(c => c?.ModifierType == modifierType);
            return config as T;
        }

        /// <summary>
        /// Gets configuration by modifier type
        /// </summary>
        public IModifierConfig GetConfig(string modifierType)
        {
            return this.configs?.FirstOrDefault(c => c?.ModifierType == modifierType);
        }

        /// <summary>
        /// Adds or updates a modifier configuration
        /// </summary>
        public void SetConfig(IModifierConfig config)
        {
            if (config == null || !(config is BaseModifierConfig baseConfig)) return;

            // Remove existing config of the same type
            this.configs?.RemoveAll(c => c?.ModifierType == config.ModifierType);

            // Add new config
            if (this.configs == null)
            {
                this.configs = new();
            }
            this.configs.Add(baseConfig);
        }

        /// <summary>
        /// Checks if a modifier configuration exists and is enabled
        /// </summary>
        public bool IsModifierEnabled(string modifierType)
        {
            var config = this.GetConfig(modifierType);
            return config?.IsEnabled ?? false;
        }

        /// <summary>
        /// Gets all enabled modifier configurations sorted by priority
        /// </summary>
        public IEnumerable<IModifierConfig> GetEnabledConfigs()
        {
            if (this.configs == null) return Enumerable.Empty<IModifierConfig>();

            return this.configs
                .Where(c => c is { IsEnabled: true })
                .OrderBy(c => c.Priority);
        }

        /// <summary>
        /// Gets all configurations
        /// </summary>
        public IReadOnlyList<IModifierConfig> AllConfigs => this.configs?.Cast<IModifierConfig>().ToList() ?? new List<IModifierConfig>();

        /// <summary>
        /// Initializes with default configurations
        /// </summary>
        public void InitializeDefaults()
        {
            this.configs = new()
            {
                (WinStreakConfig)new WinStreakConfig().CreateDefault(),
                (LossStreakConfig)new LossStreakConfig().CreateDefault(),
                (TimeDecayConfig)new TimeDecayConfig().CreateDefault(),
                (RageQuitConfig)new RageQuitConfig().CreateDefault(),
                (CompletionRateConfig)new CompletionRateConfig().CreateDefault(),
                (LevelProgressConfig)new LevelProgressConfig().CreateDefault(),
                (SessionPatternConfig)new SessionPatternConfig().CreateDefault(),
            };
        }

        /// <summary>
        /// Clears all configurations
        /// </summary>
        public void Clear()
        {
            this.configs?.Clear();
        }

        /// <summary>
        /// Gets the number of configurations
        /// </summary>
        public int Count => this.configs?.Count ?? 0;

        // IEnumerable<IModifierConfig> implementation
        public IEnumerator<IModifierConfig> GetEnumerator()
        {
            if (this.configs == null)
                return Enumerable.Empty<IModifierConfig>().GetEnumerator();

            return this.configs.Cast<IModifierConfig>().GetEnumerator();
        }

        // IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
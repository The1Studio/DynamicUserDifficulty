using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Abstract base class for all difficulty modifiers
    /// </summary>
    public abstract class BaseDifficultyModifier : IDifficultyModifier
    {
        protected readonly ModifierConfig config;

        public abstract string ModifierName { get; }
        public virtual  int    Priority     => this.config?.Priority ?? 0;
        public          bool   IsEnabled    { get; set; }

        protected BaseDifficultyModifier(ModifierConfig config)
        {
            this.config = config;
            this.IsEnabled = config?.Enabled ?? true;
        }

        public abstract ModifierResult Calculate(PlayerSessionData sessionData);

        public virtual void OnApplied(DifficultyResult result)
        {
            // Optional hook for post-application logic
            // Can be overridden by specific modifiers
        }

        /// <summary>
        /// Gets a configuration parameter value
        /// </summary>
        protected float GetParameter(string key, float defaultValue = 0f)
        {
            return this.config?.GetParameter(key, defaultValue) ?? defaultValue;
        }

        /// <summary>
        /// Applies the response curve to a value
        /// </summary>
        protected float ApplyCurve(float input)
        {
            if (this.config?.ResponseCurve != null)
                return this.config.ResponseCurve.Evaluate(Mathf.Clamp01(input));
            return input;
        }

        /// <summary>
        /// Logs debug information if debug mode is enabled
        /// </summary>
        protected void LogDebug(string message)
        {
            if (this.config != null && Application.isEditor)
            {
                Debug.Log($"[{this.ModifierName}] {message}");
            }
        }
    }
}
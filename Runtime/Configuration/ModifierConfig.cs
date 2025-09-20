using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Core;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Configuration
{
    /// <summary>
    /// Configuration for individual difficulty modifiers
    /// DEPRECATED: Use typed configuration classes (WinStreakConfig, LossStreakConfig, etc.) instead
    /// </summary>
    [System.Obsolete("Use typed configuration classes like WinStreakConfig, LossStreakConfig, etc. instead")]
    [Serializable]
    public class ModifierConfig
    {
        [Header("Basic Settings")]
        [SerializeField] private string modifierType;
        [SerializeField] private bool enabled = true;
        [SerializeField] private int priority = DifficultyConstants.DEFAULT_MODIFIER_PRIORITY;

        [Header("Response Curve")]
        [SerializeField] private AnimationCurve responseCurve = AnimationCurve.Linear(
            DifficultyConstants.CURVE_START_TIME,
            DifficultyConstants.CURVE_START_VALUE,
            DifficultyConstants.CURVE_END_TIME,
            DifficultyConstants.CURVE_END_VALUE);

        [Header("Parameters")]
        [SerializeField] private List<ModifierParameter> parameters = new List<ModifierParameter>();

        public string                  ModifierType  => this.modifierType;
        public bool                    Enabled       => this.enabled;
        public int                     Priority      => this.priority;
        public AnimationCurve          ResponseCurve => this.responseCurve;
        public List<ModifierParameter> Parameters    => this.parameters;

        /// <summary>
        /// Gets a parameter value by key
        /// </summary>
        public float GetParameter(string key, float defaultValue = DifficultyConstants.ZERO_VALUE)
        {
            var param = this.parameters?.FirstOrDefault(p => p.Key == key);
            return param?.Value ?? defaultValue;
        }

        /// <summary>
        /// Sets a parameter value by key
        /// </summary>
        public void SetParameter(string key, float value)
        {
            if (this.parameters == null) this.parameters = new List<ModifierParameter>();

            var param = this.parameters.FirstOrDefault(p => p.Key == key);
            if (param != null)
            {
                param.Value = value;
            }
            else
            {
                this.parameters.Add(new ModifierParameter { Key = key, Value = value });
            }
        }

        /// <summary>
        /// Sets the modifier type
        /// </summary>
        public void SetModifierType(string type)
        {
            this.modifierType = type;
        }
    }

    /// <summary>
    /// A single configuration parameter
    /// </summary>
    [Serializable]
    public class ModifierParameter
    {
        [SerializeField] private string key;
        [SerializeField] private float value;

        public string Key
        {
            get => this.key;
            set => this.key = value;
        }

        public float Value
        {
            get => this.value;
            set => this.value = value;
        }

        public ModifierParameter() { }

        public ModifierParameter(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
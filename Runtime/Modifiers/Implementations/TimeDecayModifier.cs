using System;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty based on time since last play
    /// </summary>
    public class TimeDecayModifier : BaseDifficultyModifier
    {
        public override string ModifierName => "TimeDecay";

        public TimeDecayModifier(ModifierConfig config) : base(config) { }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            if (sessionData == null)
                return ModifierResult.NoChange();

            var hoursSincePlay = (DateTime.Now - sessionData.LastPlayTime).TotalHours;
            var decayPerDay = GetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY, DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY);
            var maxDecay = GetParameter(DifficultyConstants.PARAM_MAX_DECAY, DifficultyConstants.TIME_DECAY_DEFAULT_MAX);
            var graceHours = GetParameter(DifficultyConstants.PARAM_GRACE_HOURS, DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS);

            float value = DifficultyConstants.ZERO_VALUE;
            string reason = "Recently played";

            if (hoursSincePlay > graceHours)
            {
                // Calculate days away after grace period
                var effectiveHours = hoursSincePlay - graceHours;
                var daysAway = effectiveHours / DifficultyConstants.HOURS_IN_DAY;

                // Calculate decay
                value = -(float)(daysAway * decayPerDay);

                // Apply maximum decay limit
                value = Mathf.Max(value, -maxDecay);

                // Apply response curve if configured
                if (maxDecay > DifficultyConstants.ZERO_VALUE)
                {
                    var normalizedValue = Mathf.Abs(value) / maxDecay;
                    value = -ApplyCurve(normalizedValue) * maxDecay;
                }

                // Format reason based on duration
                if (daysAway < 1f)
                {
                    reason = $"Away for {hoursSincePlay:F1} hours";
                }
                else if (daysAway < DifficultyConstants.TIME_DECAY_WEEK_THRESHOLD)
                {
                    reason = $"Away for {daysAway:F1} days";
                }
                else
                {
                    var weeks = daysAway / DifficultyConstants.DAYS_IN_WEEK;
                    reason = $"Away for {weeks:F1} weeks";
                }

                LogDebug($"Time decay: {hoursSincePlay:F1} hours -> {value:F2} adjustment");
            }

            return new ModifierResult
            {
                ModifierName = ModifierName,
                Value = value,
                Reason = reason,
                Metadata =
                {
                    ["hours_away"] = hoursSincePlay,
                    ["days_away"] = hoursSincePlay / DifficultyConstants.HOURS_IN_DAY,
                    ["grace_hours"] = graceHours,
                    ["applied"] = value < DifficultyConstants.ZERO_VALUE
                }
            };
        }
    }
}
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
            var decayPerDay = GetParameter("DecayPerDay", 0.5f);
            var maxDecay = GetParameter("MaxDecay", 2f);
            var graceHours = GetParameter("GraceHours", DifficultyConstants.DEFAULT_GRACE_HOURS);

            float value = 0f;
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
                if (maxDecay > 0)
                {
                    var normalizedValue = Mathf.Abs(value) / maxDecay;
                    value = -ApplyCurve(normalizedValue) * maxDecay;
                }

                // Format reason based on duration
                if (daysAway < 1)
                {
                    reason = $"Away for {hoursSincePlay:F1} hours";
                }
                else if (daysAway < 7)
                {
                    reason = $"Away for {daysAway:F1} days";
                }
                else
                {
                    var weeks = daysAway / 7;
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
                    ["applied"] = value < 0
                }
            };
        }
    }
}
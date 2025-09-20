using System;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty based on time since last play
    /// Requires ITimeDecayProvider to be implemented by the game
    /// </summary>
    public class TimeDecayModifier : BaseDifficultyModifier
    {
        private readonly ITimeDecayProvider timeDecayProvider;

        public override string ModifierName => "TimeDecay";

        public TimeDecayModifier(ModifierConfig config, ITimeDecayProvider timeDecayProvider) : base(config)
        {
            this.timeDecayProvider = timeDecayProvider ?? throw new ArgumentNullException(nameof(timeDecayProvider));
        }

        public override ModifierResult Calculate(PlayerSessionData sessionData)
        {
            try
            {
                // Return NoChange if session data is null (convention for null handling)
                if (sessionData == null)
                {
                    return ModifierResult.NoChange();
                }

                var timeSincePlay = this.timeDecayProvider.GetTimeSinceLastPlay();
                var hoursSincePlay = timeSincePlay.TotalHours;
            var decayPerDay    = this.GetParameter(DifficultyConstants.PARAM_DECAY_PER_DAY, DifficultyConstants.TIME_DECAY_DEFAULT_PER_DAY);
            var maxDecay       = this.GetParameter(DifficultyConstants.PARAM_MAX_DECAY, DifficultyConstants.TIME_DECAY_DEFAULT_MAX);
            var graceHours     = this.GetParameter(DifficultyConstants.PARAM_GRACE_HOURS, DifficultyConstants.TIME_DECAY_DEFAULT_GRACE_HOURS);

            var value = DifficultyConstants.ZERO_VALUE;
            var reason = "Recently played";

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
                    value = -this.ApplyCurve(normalizedValue) * maxDecay;
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

                this.LogDebug($"Time decay: {hoursSincePlay:F1} hours -> {value:F2} adjustment");
            }

                return new ModifierResult
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["hours_away"] = hoursSincePlay,
                        ["days_away"] = hoursSincePlay / DifficultyConstants.HOURS_IN_DAY,
                        ["grace_hours"] = graceHours,
                        ["applied"] = value < DifficultyConstants.ZERO_VALUE
                    }
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"[TimeDecayModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }
    }
}
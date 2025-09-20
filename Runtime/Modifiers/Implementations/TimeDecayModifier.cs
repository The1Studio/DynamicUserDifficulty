using System;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
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
    public class TimeDecayModifier : BaseDifficultyModifier<TimeDecayConfig>
    {
        private readonly ITimeDecayProvider timeDecayProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_TIME_DECAY;

        // Constructor for typed config
        public TimeDecayModifier(TimeDecayConfig config, ITimeDecayProvider timeDecayProvider, ILoggerManager loggerManager = null) : base(config, loggerManager)
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
                // Use strongly-typed properties instead of string parameters
                var decayPerDay = this.config.DecayPerDay;
                var maxDecay = this.config.MaxDecay;
                var graceHours = this.config.GraceHours;

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

                // Response curve logic removed from typed config
                // Can be re-added if needed

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

                return new()
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
                this.logger?.Error($"[TimeDecayModifier] Error calculating: {e.Message}");
                return ModifierResult.NoChange();
            }
        }

    }
}
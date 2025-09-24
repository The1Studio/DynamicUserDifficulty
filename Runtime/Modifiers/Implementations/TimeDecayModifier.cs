using System;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Configuration.ModifierConfigs;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine.Scripting;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Modifiers
{
    /// <summary>
    /// Reduces difficulty based on time since last play
    /// Requires ITimeDecayProvider to be implemented by the game
    /// </summary>
    [Preserve]
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

                // Use all three provider methods for comprehensive tracking
                var lastPlayTime = this.timeDecayProvider.GetLastPlayTime();
                var timeSincePlay = this.timeDecayProvider.GetTimeSinceLastPlay();
                var daysAway = this.timeDecayProvider.GetDaysAwayFromGame();

                var hoursSincePlay = timeSincePlay.TotalHours;
                // Use strongly-typed properties instead of string parameters
                var decayPerDay = this.config.DecayPerDay;
                var maxDecay = this.config.MaxDecay;
                var graceHours = this.config.GraceHours;

            var value = DifficultyConstants.ZERO_VALUE;
            var reason = "Recently played";

            if (hoursSincePlay > graceHours)
            {
                // Use provider's daysAway value for more accurate calculation
                // Provider might have custom logic for counting days
                float effectiveDays = daysAway;

                // If grace period hasn't passed for a full day, adjust
                if (daysAway == 0 && hoursSincePlay > graceHours)
                {
                    var effectiveHours = hoursSincePlay - graceHours;
                    effectiveDays = (float)(effectiveHours / DifficultyConstants.HOURS_IN_DAY);
                }

                // Calculate decay
                value = -(float)(effectiveDays * decayPerDay);

                // Apply maximum decay limit
                value = Mathf.Max(value, -maxDecay);

                // Response curve logic removed from typed config
                // Can be re-added if needed

                // Format reason based on duration using provider's daysAway
                if (daysAway < 1)
                {
                    reason = $"Away for {hoursSincePlay:F1} hours (last play: {lastPlayTime:MMM dd HH:mm})";
                }
                else if (daysAway < DifficultyConstants.TIME_DECAY_WEEK_THRESHOLD)
                {
                    reason = $"Away for {daysAway} days (last play: {lastPlayTime:MMM dd})";
                }
                else
                {
                    var weeks = daysAway / DifficultyConstants.DAYS_IN_WEEK;
                    reason = $"Away for {weeks:F1} weeks (last play: {lastPlayTime:MMM dd})";
                }

                this.LogDebug($"Time decay: {daysAway} days ({hoursSincePlay:F1} hours) -> {value:F2} adjustment");
            }

                return new()
                {
                    ModifierName = this.ModifierName,
                    Value        = value,
                    Reason       = reason,
                    Metadata =
                    {
                        ["last_play_time"] = lastPlayTime.ToString("O"), // Using GetLastPlayTime()
                        ["time_since_play"] = timeSincePlay.ToString(), // Using GetTimeSinceLastPlay()
                        ["days_away_provider"] = daysAway,  // Using GetDaysAwayFromGame()
                        ["hours_away"] = hoursSincePlay,
                        ["days_away_calculated"] = hoursSincePlay / DifficultyConstants.HOURS_IN_DAY,
                        ["grace_hours"] = graceHours,
                        ["applied"] = value < DifficultyConstants.ZERO_VALUE,
                    },
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
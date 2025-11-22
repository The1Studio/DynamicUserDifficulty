#nullable enable

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
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Reduces difficulty based on time since last play
    /// Requires ITimeDecayProvider to be implemented by the game
    /// </summary>
    [Preserve]
    public sealed class TimeDecayModifier : BaseDifficultyModifier<TimeDecayConfig>
    {
        private readonly ITimeDecayProvider timeDecayProvider;

        public override string ModifierName => DifficultyConstants.MODIFIER_TYPE_TIME_DECAY;

        // Constructor for typed config
        public TimeDecayModifier(TimeDecayConfig config, ITimeDecayProvider timeDecayProvider, ILogger logger) : base(config, logger)
        {
            this.timeDecayProvider = timeDecayProvider ?? throw new ArgumentNullException(nameof(timeDecayProvider));
        }


        public override ModifierResult Calculate()
        {
            try
            {
                // Get data from providers - stateless approach
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
                // FIX: Use provider's daysAway directly if available (provider already considers business logic)
                // Only calculate from hours if provider returns 0 days but we're past grace period
                float effectiveDays;

                if (daysAway > 0)
                {
                    // Provider has already determined we're away for full days
                    effectiveDays = daysAway;
                }
                else
                {
                    // Less than a full day away, but past grace period
                    // Calculate fractional days from hours beyond grace period
                    var hoursAfterGrace = hoursSincePlay - graceHours;
                    effectiveDays = (float)(hoursAfterGrace / DifficultyConstants.HOURS_IN_DAY);
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
                else if (daysAway < 7) // Week threshold
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
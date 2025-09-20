using System;
using System.Collections.Generic;
using System.Linq;
using TheOne.Logging;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Core
{
    using ILogger = TheOne.Logging.ILogger;

    /// <summary>
    /// Main service implementation for dynamic difficulty management using provider pattern
    /// </summary>
    public class DynamicDifficultyService : IDynamicDifficultyService, IDisposable
    {
        private readonly IDifficultyDataProvider dataProvider;
        private readonly IDifficultyCalculator calculator;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ILogger logger;

        public float CurrentDifficulty => this.dataProvider?.GetCurrentDifficulty() ?? DifficultyConstants.DEFAULT_DIFFICULTY;

        public DynamicDifficultyService(
            IDifficultyDataProvider dataProvider,
            IDifficultyCalculator calculator,
            DifficultyConfig config,
            IEnumerable<IDifficultyModifier> modifiers,
            ILoggerManager loggerManager)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers = modifiers?.ToList() ?? new List<IDifficultyModifier>();
            this.logger = loggerManager?.GetLogger(this);

            this.logger.Info($"[DynamicDifficultyService] Initialized with {this.modifiers.Count} modifiers");
        }

        public void Initialize()
        {
            try
            {
                var currentDiff = this.dataProvider.GetCurrentDifficulty();
                if (currentDiff <= 0)
                {
                    this.dataProvider.SaveDifficulty(this.config.DefaultDifficulty);
                }

                this.logger.Info($"[DynamicDifficultyService] Initialized with difficulty: {this.CurrentDifficulty:F2}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Failed to initialize: {e.Message}");
                this.dataProvider?.SaveDifficulty(this.config.DefaultDifficulty);
            }
        }

        public DifficultyResult CalculateDifficulty()
        {
            try
            {
                var sessionData = this.dataProvider.GetSessionData();

                // Get enabled modifiers sorted by priority
                var enabledModifiers = this.modifiers
                    .Where(m => m != null && m.IsEnabled)
                    .OrderBy(m => m.Priority);

                // Calculate new difficulty
                var result = this.calculator.Calculate(sessionData, enabledModifiers);

                this.logger.Info($"[DynamicDifficultyService] Calculated difficulty: " +
                         $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");

                return result;
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error calculating difficulty: {e.Message}");

                // Return no change on error
                return new()
                {
                    PreviousDifficulty = this.CurrentDifficulty,
                    NewDifficulty      = this.CurrentDifficulty,
                    PrimaryReason      = "Calculation error"
                };
            }
        }

        public void ApplyDifficulty(DifficultyResult result)
        {
            if (result == null)
            {
                this.logger.Warning("[DynamicDifficultyService] Cannot apply null difficulty result");
                return;
            }

            try
            {
                // Update difficulty in data provider
                this.dataProvider.SaveDifficulty(result.NewDifficulty);

                // Update session data
                var sessionData = this.dataProvider.GetSessionData();
                sessionData.CurrentDifficulty = result.NewDifficulty;
                this.dataProvider.SaveSessionData(sessionData);

                // Notify all modifiers
                foreach (var modifier in this.modifiers)
                {
                    try
                    {
                        modifier?.OnApplied(result);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error($"[DynamicDifficultyService] Error in modifier OnApplied: {e.Message}");
                    }
                }

                this.logger.Info($"[DynamicDifficultyService] Applied difficulty: {result.NewDifficulty:F2}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error applying difficulty: {e.Message}");
            }
        }

        public void RegisterModifier(IDifficultyModifier modifier)
        {
            if (modifier == null)
            {
                this.logger.Warning("[DynamicDifficultyService] Cannot register null modifier");
                return;
            }

            if (!this.modifiers.Contains(modifier))
            {
                this.modifiers.Add(modifier);
                this.logger.Info($"[DynamicDifficultyService] Registered modifier: {modifier.ModifierName}");
            }
        }

        public void UnregisterModifier(IDifficultyModifier modifier)
        {
            if (modifier != null && this.modifiers.Remove(modifier))
            {
                this.logger.Info($"[DynamicDifficultyService] Unregistered modifier: {modifier.ModifierName}");
            }
        }

        public void OnSessionStart()
        {
            try
            {
                // Calculate difficulty at session start
                var result = this.CalculateDifficulty();
                this.ApplyDifficulty(result);

                this.logger.Info("[DynamicDifficultyService] Session started");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on session start: {e.Message}");
            }
        }

        public void OnLevelStart(int levelId)
        {
            try
            {
                this.logger.Info($"[DynamicDifficultyService] Level {levelId} started at difficulty {this.CurrentDifficulty:F2}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on level start: {e.Message}");
            }
        }

        public void OnLevelComplete(bool won, float completionTime)
        {
            try
            {
                // This is handled by the providers now
                // Games implement provider interfaces to record wins/losses
                this.logger.Info($"[DynamicDifficultyService] Level completed: {(won ? "won" : "lost")} in {completionTime:F1}s");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on level complete: {e.Message}");
            }
        }

        public void OnSessionEnd(SessionEndType endType)
        {
            try
            {
                this.logger.Info($"[DynamicDifficultyService] Session ended: {endType}");
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on session end: {e.Message}");
            }
        }

        public void Dispose()
        {
            this.modifiers?.Clear();
        }
    }
}
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
    /// <summary>
    /// Main service implementation for dynamic difficulty management
    /// </summary>
    public class DynamicDifficultyService : IDynamicDifficultyService, IDisposable
    {
        private readonly ISessionDataProvider dataProvider;
        private readonly IDifficultyCalculator calculator;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ILogger logger;

        private int currentLevelId;
        private DateTime levelStartTime;

        public float CurrentDifficulty { get; private set; }

        public DynamicDifficultyService(
            ISessionDataProvider dataProvider,
            IDifficultyCalculator calculator,
            DifficultyConfig config,
            ILoggerManager loggerManager)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers = new List<IDifficultyModifier>();
            this.logger = loggerManager?.GetLogger(this) ?? throw new ArgumentNullException(nameof(loggerManager));
        }

        public void Initialize()
        {
            try
            {
                var sessionData = this.dataProvider.GetCurrentSession();
                CurrentDifficulty = sessionData?.CurrentDifficulty ?? this.config.DefaultDifficulty;

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Initialized with difficulty: {CurrentDifficulty:F2}");
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Failed to initialize: {e.Message}");
                CurrentDifficulty = this.config.DefaultDifficulty;
            }
        }

        public DifficultyResult CalculateDifficulty()
        {
            try
            {
                var sessionData = this.dataProvider.GetCurrentSession();

                // Get enabled modifiers sorted by priority
                var enabledModifiers = modifiers
                    .Where(m => m != null && m.IsEnabled)
                    .OrderBy(m => m.Priority);

                // Calculate new difficulty
                var result = this.calculator.Calculate(sessionData, enabledModifiers);

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Calculated difficulty: " +
                             $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");
                }

                return result;
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error calculating difficulty: {e.Message}");

                // Return no change on error
                return new DifficultyResult
                {
                    PreviousDifficulty = CurrentDifficulty,
                    NewDifficulty = CurrentDifficulty,
                    PrimaryReason = "Calculation error"
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
                CurrentDifficulty = result.NewDifficulty;

                // Update session data
                var sessionData = this.dataProvider.GetCurrentSession();
                sessionData.CurrentDifficulty = result.NewDifficulty;
                this.dataProvider.SaveSession(sessionData);

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

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Applied difficulty: {result.NewDifficulty:F2}");
                }
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

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Registered modifier: {modifier.ModifierName}");
                }
            }
        }

        public void UnregisterModifier(IDifficultyModifier modifier)
        {
            if (modifier != null && this.modifiers.Remove(modifier))
            {
                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Unregistered modifier: {modifier.ModifierName}");
                }
            }
        }

        public void OnSessionStart()
        {
            try
            {
                // Calculate difficulty at session start
                var result = CalculateDifficulty();
                ApplyDifficulty(result);

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info("[DynamicDifficultyService] Session started");
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on session start: {e.Message}");
            }
        }

        public void OnLevelStart(int levelId)
        {
            this.currentLevelId = levelId;
            this.levelStartTime = DateTime.Now;

            if (this.config.EnableDebugLogs)
            {
                this.logger.Info($"[DynamicDifficultyService] Level {levelId} started at difficulty {CurrentDifficulty:F2}");
            }
        }

        public void OnLevelComplete(bool won, float completionTime)
        {
            try
            {
                var sessionData = this.dataProvider.GetCurrentSession();

                if (won)
                {
                    sessionData.RecordWin(this.currentLevelId, completionTime);
                    this.dataProvider.UpdateWinStreak(sessionData.WinStreak);
                }
                else
                {
                    sessionData.RecordLoss(this.currentLevelId, completionTime);
                    this.dataProvider.UpdateLossStreak(sessionData.LossStreak);
                }

                this.dataProvider.SaveSession(sessionData);

                if (this.config.EnableDebugLogs)
                {
                    var result = won ? "won" : "lost";
                    this.logger.Info($"[DynamicDifficultyService] Level {this.currentLevelId} {result} " +
                             $"in {completionTime:F1}s (Streaks: W{sessionData.WinStreak}/L{sessionData.LossStreak})");
                }
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
                this.dataProvider.RecordSessionEnd(endType);

                if (this.config.EnableDebugLogs)
                {
                    this.logger.Info($"[DynamicDifficultyService] Session ended: {endType}");
                }
            }
            catch (Exception e)
            {
                this.logger.Error($"[DynamicDifficultyService] Error on session end: {e.Message}");
            }
        }

        public void Dispose()
        {
            this.modifiers?.Clear();

            if (this.dataProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
}
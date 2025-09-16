using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;
using UnityEngine;

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

        private int currentLevelId;
        private DateTime levelStartTime;

        public float CurrentDifficulty { get; private set; }

        public DynamicDifficultyService(
            ISessionDataProvider dataProvider,
            IDifficultyCalculator calculator,
            DifficultyConfig config)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.modifiers = new List<IDifficultyModifier>();
        }

        public void Initialize()
        {
            try
            {
                var sessionData = dataProvider.GetCurrentSession();
                CurrentDifficulty = sessionData?.CurrentDifficulty ?? config.DefaultDifficulty;

                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Initialized with difficulty: {CurrentDifficulty:F2}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Failed to initialize: {e.Message}");
                CurrentDifficulty = config.DefaultDifficulty;
            }
        }

        public DifficultyResult CalculateDifficulty()
        {
            try
            {
                var sessionData = dataProvider.GetCurrentSession();

                // Get enabled modifiers sorted by priority
                var enabledModifiers = modifiers
                    .Where(m => m != null && m.IsEnabled)
                    .OrderBy(m => m.Priority);

                // Calculate new difficulty
                var result = calculator.Calculate(sessionData, enabledModifiers);

                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Calculated difficulty: " +
                             $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error calculating difficulty: {e.Message}");

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
                Debug.LogWarning("[DynamicDifficultyService] Cannot apply null difficulty result");
                return;
            }

            try
            {
                CurrentDifficulty = result.NewDifficulty;

                // Update session data
                var sessionData = dataProvider.GetCurrentSession();
                sessionData.CurrentDifficulty = result.NewDifficulty;
                dataProvider.SaveSession(sessionData);

                // Notify all modifiers
                foreach (var modifier in modifiers)
                {
                    try
                    {
                        modifier?.OnApplied(result);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DynamicDifficultyService] Error in modifier OnApplied: {e.Message}");
                    }
                }

                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Applied difficulty: {result.NewDifficulty:F2}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error applying difficulty: {e.Message}");
            }
        }

        public void RegisterModifier(IDifficultyModifier modifier)
        {
            if (modifier == null)
            {
                Debug.LogWarning("[DynamicDifficultyService] Cannot register null modifier");
                return;
            }

            if (!modifiers.Contains(modifier))
            {
                modifiers.Add(modifier);

                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Registered modifier: {modifier.ModifierName}");
                }
            }
        }

        public void UnregisterModifier(IDifficultyModifier modifier)
        {
            if (modifier != null && modifiers.Remove(modifier))
            {
                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Unregistered modifier: {modifier.ModifierName}");
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

                if (config.EnableDebugLogs)
                {
                    Debug.Log("[DynamicDifficultyService] Session started");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on session start: {e.Message}");
            }
        }

        public void OnLevelStart(int levelId)
        {
            currentLevelId = levelId;
            levelStartTime = DateTime.Now;

            if (config.EnableDebugLogs)
            {
                Debug.Log($"[DynamicDifficultyService] Level {levelId} started at difficulty {CurrentDifficulty:F2}");
            }
        }

        public void OnLevelComplete(bool won, float completionTime)
        {
            try
            {
                var sessionData = dataProvider.GetCurrentSession();

                if (won)
                {
                    sessionData.RecordWin(currentLevelId, completionTime);
                    dataProvider.UpdateWinStreak(sessionData.WinStreak);
                }
                else
                {
                    sessionData.RecordLoss(currentLevelId, completionTime);
                    dataProvider.UpdateLossStreak(sessionData.LossStreak);
                }

                dataProvider.SaveSession(sessionData);

                if (config.EnableDebugLogs)
                {
                    var result = won ? "won" : "lost";
                    Debug.Log($"[DynamicDifficultyService] Level {currentLevelId} {result} " +
                             $"in {completionTime:F1}s (Streaks: W{sessionData.WinStreak}/L{sessionData.LossStreak})");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on level complete: {e.Message}");
            }
        }

        public void OnSessionEnd(SessionEndType endType)
        {
            try
            {
                dataProvider.RecordSessionEnd(endType);

                if (config.EnableDebugLogs)
                {
                    Debug.Log($"[DynamicDifficultyService] Session ended: {endType}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on session end: {e.Message}");
            }
        }

        public void Dispose()
        {
            modifiers?.Clear();

            if (dataProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
}
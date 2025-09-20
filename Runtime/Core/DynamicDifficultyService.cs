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
    /// Main service implementation for dynamic difficulty management using provider pattern
    /// </summary>
    public class DynamicDifficultyService : IDynamicDifficultyService, IDisposable
    {
        private readonly IDifficultyDataProvider dataProvider;
        private readonly IDifficultyCalculator calculator;
        private readonly DifficultyConfig config;
        private readonly List<IDifficultyModifier> modifiers;

        public float CurrentDifficulty => this.dataProvider?.GetCurrentDifficulty() ?? DifficultyConstants.DEFAULT_DIFFICULTY;

        public DynamicDifficultyService(
            IDifficultyDataProvider dataProvider,
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
                var currentDiff = this.dataProvider.GetCurrentDifficulty();
                if (currentDiff <= 0)
                {
                    this.dataProvider.SaveDifficulty(this.config.DefaultDifficulty);
                }

                Debug.Log($"[DynamicDifficultyService] Initialized with difficulty: {this.CurrentDifficulty:F2}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Failed to initialize: {e.Message}");
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

                Debug.Log($"[DynamicDifficultyService] Calculated difficulty: " +
                         $"{result.PreviousDifficulty:F2} -> {result.NewDifficulty:F2}");

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error calculating difficulty: {e.Message}");

                // Return no change on error
                return new DifficultyResult
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
                Debug.LogWarning("[DynamicDifficultyService] Cannot apply null difficulty result");
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
                        Debug.LogError($"[DynamicDifficultyService] Error in modifier OnApplied: {e.Message}");
                    }
                }

                Debug.Log($"[DynamicDifficultyService] Applied difficulty: {result.NewDifficulty:F2}");
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

            if (!this.modifiers.Contains(modifier))
            {
                this.modifiers.Add(modifier);
                Debug.Log($"[DynamicDifficultyService] Registered modifier: {modifier.ModifierName}");
            }
        }

        public void UnregisterModifier(IDifficultyModifier modifier)
        {
            if (modifier != null && this.modifiers.Remove(modifier))
            {
                Debug.Log($"[DynamicDifficultyService] Unregistered modifier: {modifier.ModifierName}");
            }
        }

        public void OnSessionStart()
        {
            try
            {
                // Calculate difficulty at session start
                var result = this.CalculateDifficulty();
                this.ApplyDifficulty(result);

                Debug.Log("[DynamicDifficultyService] Session started");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on session start: {e.Message}");
            }
        }

        public void OnLevelStart(int levelId)
        {
            try
            {
                Debug.Log($"[DynamicDifficultyService] Level {levelId} started at difficulty {this.CurrentDifficulty:F2}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on level start: {e.Message}");
            }
        }

        public void OnLevelComplete(bool won, float completionTime)
        {
            try
            {
                // This is handled by the providers now
                // Games implement provider interfaces to record wins/losses
                Debug.Log($"[DynamicDifficultyService] Level completed: {(won ? "won" : "lost")} in {completionTime:F1}s");
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
                Debug.Log($"[DynamicDifficultyService] Session ended: {endType}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DynamicDifficultyService] Error on session end: {e.Message}");
            }
        }

        public void Dispose()
        {
            this.modifiers?.Clear();
        }
    }
}
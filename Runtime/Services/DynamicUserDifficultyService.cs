using System;
using System.Collections.Generic;
using System.Linq;
using TheOneStudio.DynamicUserDifficulty.Configuration;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Modifiers;
using TheOneStudio.DynamicUserDifficulty.Providers;
using TheOneStudio.DynamicUserDifficulty.Calculators;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Services
{
    /// <summary>
    /// Main service for managing dynamic difficulty adjustments
    /// </summary>
    public class DynamicUserDifficultyService : IDynamicUserDifficultyService
    {
        private readonly DifficultyConfig config;
        private readonly IPlayerDataProvider dataProvider;
        private readonly DifficultyManager difficultyManager;
        private readonly List<IDifficultyModifier> modifiers;
        private readonly ModifierAggregator aggregator;

        private PlayerSessionData sessionData;
        private DateTime sessionStartTime;

        public float CurrentDifficulty => this.dataProvider?.LoadDifficulty() ?? DifficultyConstants.DEFAULT_DIFFICULTY;

        public DynamicUserDifficultyService(DifficultyConfig config, IPlayerDataProvider dataProvider)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));

            this.difficultyManager = new DifficultyManager();
            this.aggregator        = new ModifierAggregator();
            this.modifiers            = new List<IDifficultyModifier>();

            this.Initialize();
        }

        private void Initialize()
        {
            // Load session data
            this.sessionData = this.dataProvider.LoadSessionData() ?? new PlayerSessionData();

            // Initialize modifiers based on config
            this.InitializeModifiers();

            // Load current difficulty
            var currentDiff = this.dataProvider.LoadDifficulty();
            if (currentDiff <= 0)
            {
                this.dataProvider.SaveDifficulty(DifficultyConstants.DEFAULT_DIFFICULTY);
            }
        }

        private void InitializeModifiers()
        {
            this.modifiers.Clear();

            foreach (var modifierConfig in this.config.ModifierConfigs)
            {
                if (!modifierConfig.Enabled) continue;

                IDifficultyModifier modifier = null;

                switch (modifierConfig.ModifierType)
                {
                    case DifficultyConstants.MODIFIER_TYPE_WIN_STREAK:
                        modifier = new WinStreakModifier(modifierConfig);
                        break;
                    case DifficultyConstants.MODIFIER_TYPE_LOSS_STREAK:
                        modifier = new LossStreakModifier(modifierConfig);
                        break;
                    case DifficultyConstants.MODIFIER_TYPE_TIME_DECAY:
                        modifier = new TimeDecayModifier(modifierConfig);
                        break;
                    case DifficultyConstants.MODIFIER_TYPE_RAGE_QUIT:
                        modifier = new RageQuitModifier(modifierConfig);
                        break;
                }

                if (modifier != null)
                {
                    this.modifiers.Add(modifier);
                }
            }
        }

        public void RecordWin()
        {
            this.sessionData.WinStreak++;
            this.sessionData.LossStreak = 0;
            this.sessionData.TotalWins++;
            this.dataProvider.SaveSessionData(this.sessionData);
        }

        public void RecordLoss()
        {
            this.sessionData.LossStreak++;
            this.sessionData.WinStreak = 0;
            this.sessionData.TotalLosses++;
            this.dataProvider.SaveSessionData(this.sessionData);
        }

        public void RecordQuit(QuitType quitType)
        {
            this.sessionData.QuitType = quitType;
            this.dataProvider.SaveSessionData(this.sessionData);
        }

        public void RecordSessionStart()
        {
            this.sessionStartTime = DateTime.Now;
            this.sessionData.SessionCount++;
            this.sessionData.LastSessionTime = this.sessionStartTime;
            this.dataProvider.SaveSessionData(this.sessionData);
        }

        public void RecordSessionEnd()
        {
            if (this.sessionStartTime != default)
            {
                this.sessionData.SessionLength = (float)(DateTime.Now - this.sessionStartTime).TotalSeconds;
                this.dataProvider.SaveSessionData(this.sessionData);
            }
        }

        public float UpdateDifficulty()
        {
            var currentDifficulty = this.CurrentDifficulty;

            // Calculate modifier results
            var modifierResults = new List<ModifierResult>();
            foreach (var modifier in this.modifiers)
            {
                var result = modifier.Calculate(this.sessionData);
                modifierResults.Add(result);
            }

            // Aggregate adjustments
            float totalAdjustment = this.aggregator.Aggregate(modifierResults);

            // Apply adjustment
            float newDifficulty = this.difficultyManager.AdjustDifficulty(
                currentDifficulty,
                totalAdjustment, this.config.MinDifficulty, this.config.MaxDifficulty
            );

            // Apply max change per session constraint
            float maxChange    = this.config.MaxChangePerSession;
            float actualChange = newDifficulty - currentDifficulty;
            if (Mathf.Abs(actualChange) > maxChange)
            {
                newDifficulty = currentDifficulty + Mathf.Sign(actualChange) * maxChange;
            }

            // Save new difficulty
            this.dataProvider.SaveDifficulty(newDifficulty);

            return newDifficulty;
        }

        public void ResetDifficulty()
        {
            this.dataProvider.SaveDifficulty(this.config.DefaultDifficulty);
            this.sessionData = new PlayerSessionData();
            this.dataProvider.SaveSessionData(this.sessionData);
        }

        public DifficultyLevel GetDifficultyLevel()
        {
            return this.difficultyManager.GetDifficultyLevel(this.CurrentDifficulty);
        }

        public PlayerSessionData GetSessionData()
        {
            return this.sessionData;
        }

        public void SaveData()
        {
            this.dataProvider.SaveSessionData(this.sessionData);
            this.dataProvider.SaveDifficulty(this.CurrentDifficulty);
        }

        public void LoadData()
        {
            this.sessionData = this.dataProvider.LoadSessionData() ?? new PlayerSessionData();
        }

        public Dictionary<string, float> GetDifficultyStats()
        {
            return new Dictionary<string, float>
            {
                ["CurrentDifficulty"] = this.CurrentDifficulty,
                ["WinStreak"]         = this.sessionData.WinStreak,
                ["LossStreak"]        = this.sessionData.LossStreak,
                ["SessionCount"]      = this.sessionData.SessionCount,
                ["TotalWins"]         = this.sessionData.TotalWins,
                ["TotalLosses"]       = this.sessionData.TotalLosses
            };
        }
    }

    /// <summary>
    /// Interface for the dynamic difficulty service
    /// </summary>
    public interface IDynamicUserDifficultyService
    {
        float CurrentDifficulty { get; }
        void RecordWin();
        void RecordLoss();
        void RecordQuit(QuitType quitType);
        void RecordSessionStart();
        void RecordSessionEnd();
        float UpdateDifficulty();
        void ResetDifficulty();
        DifficultyLevel GetDifficultyLevel();
        PlayerSessionData GetSessionData();
        void SaveData();
        void LoadData();
        Dictionary<string, float> GetDifficultyStats();
    }
}
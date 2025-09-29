using TheOneStudio.DynamicUserDifficulty.Models;
using TheOneStudio.DynamicUserDifficulty.Providers;

namespace TheOneStudio.DynamicUserDifficulty.Tests.Utilities
{
    /// <summary>
    /// Shared mock provider implementations for testing.
    /// Eliminates duplication across test files.
    /// </summary>
    public class MockWinStreakProvider : IWinStreakProvider
    {
        public int WinStreak { get; set; } = 0;
        public int LossStreak { get; set; } = 0;
        public int TotalWins { get; set; } = 5;
        public int TotalLosses { get; set; } = 3;

        public int GetWinStreak() => this.WinStreak;
        public int GetLossStreak() => this.LossStreak;
        public int GetTotalWins() => this.TotalWins;
        public int GetTotalLosses() => this.TotalLosses;

        public void RecordWin()
        {
            this.WinStreak++;
            this.LossStreak = 0;
            this.TotalWins++;
        }

        public void RecordLoss()
        {
            this.LossStreak++;
            this.WinStreak = 0;
            this.TotalLosses++;
        }

        public void Reset()
        {
            this.WinStreak = 0;
            this.LossStreak = 0;
        }
    }

    public class MockTimeDecayProvider : ITimeDecayProvider
    {
        public System.DateTime LastPlayTime { get; set; } = System.DateTime.Now.AddHours(-2);
        public System.TimeSpan TimeSinceLastPlay { get; set; } = System.TimeSpan.FromHours(2);
        public int DaysAwayFromGame { get; set; } = 0;

        public System.DateTime GetLastPlayTime() => this.LastPlayTime;
        public System.TimeSpan GetTimeSinceLastPlay() => this.TimeSinceLastPlay;
        public int GetDaysAwayFromGame() => this.DaysAwayFromGame;

        public void UpdateLastPlayTime()
        {
            this.LastPlayTime = System.DateTime.Now;
            this.TimeSinceLastPlay = System.TimeSpan.Zero;
            this.DaysAwayFromGame = 0;
        }
    }

    public class MockRageQuitProvider : IRageQuitProvider
    {
        public QuitType LastQuitType { get; set; } = QuitType.Normal;
        public float AverageSessionDuration { get; set; } = 300f; // 5 minutes
        public float CurrentSessionDuration { get; set; } = 180f; // 3 minutes
        public int RecentRageQuitCount { get; set; } = 0;

        public QuitType GetLastQuitType() => this.LastQuitType;
        public float GetAverageSessionDuration() => this.AverageSessionDuration;
        public float GetCurrentSessionDuration() => this.CurrentSessionDuration;
        public int GetRecentRageQuitCount() => this.RecentRageQuitCount;

        public void RecordSessionEnd(QuitType quitType, float durationSeconds)
        {
            this.LastQuitType = quitType;
            this.CurrentSessionDuration = durationSeconds;
        }

        public void RecordSessionStart()
        {
            this.CurrentSessionDuration = 0f;
        }
    }

    public class MockLevelProgressProvider : ILevelProgressProvider
    {
        public int AttemptsOnCurrentLevel { get; set; } = 3;
        public int CurrentLevel { get; set; } = 10;
        public float CurrentLevelDifficulty { get; set; } = 3f;
        public float CompletionRate { get; set; } = 0.75f;
        public float AverageCompletionTime { get; set; } = 120f; // 2 minutes
        public float CurrentLevelTimePercentage { get; set; } = 1.0f; // 100% of expected time

        public int GetAttemptsOnCurrentLevel() => this.AttemptsOnCurrentLevel;
        public int GetCurrentLevel() => this.CurrentLevel;
        public float GetCurrentLevelDifficulty() => this.CurrentLevelDifficulty;
        public float GetCompletionRate() => this.CompletionRate;
        public float GetAverageCompletionTime() => this.AverageCompletionTime;
        public float GetCurrentLevelTimePercentage() => this.CurrentLevelTimePercentage;

        public void IncrementAttempts()
        {
            this.AttemptsOnCurrentLevel++;
        }

        public void CompleteLevel()
        {
            this.CurrentLevel++;
            this.AttemptsOnCurrentLevel = 0;
        }
    }

    public class MockDifficultyDataProvider : IDifficultyDataProvider
    {
        public float CurrentDifficulty { get; set; } = 3f;

        public float GetCurrentDifficulty() => this.CurrentDifficulty;

        public void SetCurrentDifficulty(float newDifficulty)
        {
            this.CurrentDifficulty = newDifficulty;
        }
    }
}
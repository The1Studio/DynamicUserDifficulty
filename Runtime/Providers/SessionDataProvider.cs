using System;
using Newtonsoft.Json;
using TheOneStudio.DynamicUserDifficulty.Core;
using TheOneStudio.DynamicUserDifficulty.Models;
using UnityEngine;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Default implementation of session data provider using PlayerPrefs
    /// </summary>
    public class SessionDataProvider : ISessionDataProvider, IDisposable
    {
        private PlayerSessionData cachedData;
        private DateTime cacheTime;
        private readonly TimeSpan cacheExpiry = TimeSpan.FromMinutes(DifficultyConstants.CACHE_EXPIRY_MINUTES);

        public PlayerSessionData GetCurrentSession()
        {
            // Check cache
            if (cachedData != null && DateTime.Now - cacheTime < cacheExpiry)
            {
                return cachedData;
            }

            // Load from PlayerPrefs
            cachedData = LoadFromPlayerPrefs();
            cacheTime = DateTime.Now;

            return cachedData;
        }

        public void SaveSession(PlayerSessionData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Attempted to save null session data");
                return;
            }

            // Update cache
            cachedData = data;
            cacheTime = DateTime.Now;

            // Save to PlayerPrefs
            SaveToPlayerPrefs(data);
        }

        public void UpdateWinStreak(int streak)
        {
            var data = GetCurrentSession();
            data.WinStreak = streak;
            data.LossStreak = 0; // Reset loss streak on win
            SaveSession(data);
        }

        public void UpdateLossStreak(int streak)
        {
            var data = GetCurrentSession();
            data.LossStreak = streak;
            data.WinStreak = 0; // Reset win streak on loss
            SaveSession(data);
        }

        public void RecordSessionEnd(SessionEndType endType)
        {
            var data = GetCurrentSession();

            if (data.LastSession != null)
            {
                data.LastSession.EndType = endType;
                data.LastSession.EndTime = DateTime.Now;
                data.LastSession.PlayDuration = (float)(data.LastSession.EndTime - data.LastSession.StartTime).TotalSeconds;
            }

            data.LastPlayTime = DateTime.Now;
            SaveSession(data);
        }

        public void ClearData()
        {
            // Clear cache
            cachedData = null;

            // Clear PlayerPrefs
            PlayerPrefs.DeleteKey(DifficultyConstants.PREFS_CURRENT_DIFFICULTY);
            PlayerPrefs.DeleteKey(DifficultyConstants.PREFS_WIN_STREAK);
            PlayerPrefs.DeleteKey(DifficultyConstants.PREFS_LOSS_STREAK);
            PlayerPrefs.DeleteKey(DifficultyConstants.PREFS_LAST_PLAY_TIME);
            PlayerPrefs.DeleteKey(DifficultyConstants.PREFS_SESSION_DATA);
            PlayerPrefs.Save();
        }

        private PlayerSessionData LoadFromPlayerPrefs()
        {
            try
            {
                // Try to load serialized data first
                if (PlayerPrefs.HasKey(DifficultyConstants.PREFS_SESSION_DATA))
                {
                    var json = PlayerPrefs.GetString(DifficultyConstants.PREFS_SESSION_DATA);
                    var data = JsonConvert.DeserializeObject<PlayerSessionData>(json);
                    if (data != null)
                        return data;
                }

                // Fall back to individual values
                var sessionData = new PlayerSessionData
                {
                    CurrentDifficulty = PlayerPrefs.GetFloat(
                        DifficultyConstants.PREFS_CURRENT_DIFFICULTY,
                        DifficultyConstants.DEFAULT_DIFFICULTY),

                    WinStreak = PlayerPrefs.GetInt(DifficultyConstants.PREFS_WIN_STREAK, 0),
                    LossStreak = PlayerPrefs.GetInt(DifficultyConstants.PREFS_LOSS_STREAK, 0),
                };

                // Parse last play time
                if (PlayerPrefs.HasKey(DifficultyConstants.PREFS_LAST_PLAY_TIME))
                {
                    var timeString = PlayerPrefs.GetString(DifficultyConstants.PREFS_LAST_PLAY_TIME);
                    if (DateTime.TryParse(timeString, out var lastPlayTime))
                    {
                        sessionData.LastPlayTime = lastPlayTime;
                    }
                }

                return sessionData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading session data: {e.Message}");
                return new PlayerSessionData();
            }
        }

        private void SaveToPlayerPrefs(PlayerSessionData data)
        {
            try
            {
                // Save as JSON for complete data
                var json = JsonConvert.SerializeObject(data);
                PlayerPrefs.SetString(DifficultyConstants.PREFS_SESSION_DATA, json);

                // Also save individual values for backward compatibility
                PlayerPrefs.SetFloat(DifficultyConstants.PREFS_CURRENT_DIFFICULTY, data.CurrentDifficulty);
                PlayerPrefs.SetInt(DifficultyConstants.PREFS_WIN_STREAK, data.WinStreak);
                PlayerPrefs.SetInt(DifficultyConstants.PREFS_LOSS_STREAK, data.LossStreak);
                PlayerPrefs.SetString(DifficultyConstants.PREFS_LAST_PLAY_TIME, data.LastPlayTime.ToString("O"));

                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving session data: {e.Message}");
            }
        }

        public void Dispose()
        {
            // Clear cache on dispose
            cachedData = null;
        }
    }
}
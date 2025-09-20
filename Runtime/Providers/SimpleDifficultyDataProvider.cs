using UnityEngine;
using TheOneStudio.DynamicUserDifficulty.Core;

namespace TheOneStudio.DynamicUserDifficulty.Providers
{
    /// <summary>
    /// Simple implementation of IDifficultyDataProvider using Unity's PlayerPrefs.
    /// This is suitable for prototypes and simple games without complex data systems.
    /// </summary>
    public class SimpleDifficultyDataProvider : IDifficultyDataProvider
    {
        private const string DIFFICULTY_KEY = "DUD_CurrentDifficulty";
        private float cachedDifficulty = -1f;

        public float GetCurrentDifficulty()
        {
            // Use cache to avoid frequent PlayerPrefs access
            if (this.cachedDifficulty < 0)
            {
                this.cachedDifficulty = PlayerPrefs.GetFloat(DIFFICULTY_KEY, DifficultyConstants.DEFAULT_DIFFICULTY);
            }
            return this.cachedDifficulty;
        }

        public void SetCurrentDifficulty(float newDifficulty)
        {
            this.cachedDifficulty = newDifficulty;
            PlayerPrefs.SetFloat(DIFFICULTY_KEY, newDifficulty);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clears the cached value, forcing a fresh read from PlayerPrefs
        /// </summary>
        public void ClearCache()
        {
            this.cachedDifficulty = -1f;
        }

        /// <summary>
        /// Resets difficulty to default value
        /// </summary>
        public void ResetToDefault()
        {
            this.SetCurrentDifficulty(DifficultyConstants.DEFAULT_DIFFICULTY);
        }
    }
}
using System;
using UnityEngine;

namespace Drift
{
    public sealed class ProgressionService
    {
        public const string HighestUnlockedLevelKey = "Drift.HighestUnlockedLevel";
        public const string CompletedLevelsKey = "Drift.CompletedLevels";

        public int GetHighestUnlockedLevel()
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1), 1, LevelCatalog.GetLevels().Count);
        }

        public bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= GetHighestUnlockedLevel();
        }

        public bool IsLevelCompleted(int levelNumber)
        {
            return GetCompletedString().IndexOf(Token(levelNumber), StringComparison.Ordinal) >= 0;
        }

        public void MarkLevelComplete(int levelNumber)
        {
            int clampedLevel = Mathf.Clamp(levelNumber, 1, LevelCatalog.GetLevels().Count);
            string completed = GetCompletedString();
            string token = Token(clampedLevel);

            if (completed.IndexOf(token, StringComparison.Ordinal) < 0)
            {
                completed += token;
                PlayerPrefs.SetString(CompletedLevelsKey, completed);
            }

            int nextUnlocked = Mathf.Min(clampedLevel + 1, LevelCatalog.GetLevels().Count);
            if (nextUnlocked > GetHighestUnlockedLevel())
            {
                PlayerPrefs.SetInt(HighestUnlockedLevelKey, nextUnlocked);
            }

            PlayerPrefs.Save();
        }

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(HighestUnlockedLevelKey);
            PlayerPrefs.DeleteKey(CompletedLevelsKey);
            PlayerPrefs.Save();
        }

        private static string Token(int levelNumber)
        {
            return $"|{levelNumber}|";
        }

        private static string GetCompletedString()
        {
            return PlayerPrefs.GetString(CompletedLevelsKey, string.Empty);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Drift
{
    public readonly struct LevelSelectEntry
    {
        public LevelSelectEntry(int levelNumber, string label, bool unlocked, bool completed)
        {
            LevelNumber = levelNumber;
            Label = label;
            Unlocked = unlocked;
            Completed = completed;
        }

        public int LevelNumber { get; }
        public string Label { get; }
        public bool Unlocked { get; }
        public bool Completed { get; }
    }

    public sealed class LevelSelectManager : MonoBehaviour
    {
        public IReadOnlyList<LevelSelectEntry> GetEntries(IReadOnlyList<LevelDefinition> levels, ProgressionService progression)
        {
            List<LevelSelectEntry> entries = new List<LevelSelectEntry>();
            if (levels == null || progression == null)
            {
                return entries;
            }

            foreach (LevelDefinition level in levels)
            {
                bool unlocked = progression.IsLevelUnlocked(level.LevelNumber);
                bool completed = progression.IsLevelCompleted(level.LevelNumber);
                string suffix = completed ? "  Completed" : level.IsTutorial ? "  Tutorial" : string.Empty;
                entries.Add(new LevelSelectEntry(level.LevelNumber, $"{level.LevelNumber}. {level.DisplayName}{suffix}", unlocked, completed));
            }

            return entries;
        }
    }
}

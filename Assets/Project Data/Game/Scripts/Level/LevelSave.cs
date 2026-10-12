using System.Collections.Generic;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class LevelSave : ISaveObject
    {
        public int RealLevelNumber = 0;
        public int DisplayLevelNumber = 0;
        public bool ReplayingLevelAgain = false;

        public int selectedLevelIndex = 0;
        public bool isPlayingFromLevelSelection = false;

        public List<LevelProgressData> levelProgress = new List<LevelProgressData>();

        // NEW: Property to get the correct level number to display
        public int GetCurrentDisplayLevel()
        {
            if (isPlayingFromLevelSelection)
            {
                return selectedLevelIndex + 1; // Convert to 1-based
            }
            else
            {
                return DisplayLevelNumber;
            }
        }

        public void Flush()
        {

        }

        // Fast lookup by level index (1050 levels; every map screen asks for many of them).
        // Not saved; rebuilt whenever the saved list changes size.
        [System.NonSerialized] private Dictionary<int, LevelProgressData> lookup;
        [System.NonSerialized] private int lookupCount = -1;

        public LevelProgressData GetLevelProgress(int levelIndex)
        {
            if (levelProgress == null)
                return null;

            if (lookup == null || lookupCount != levelProgress.Count)
            {
                lookup = new Dictionary<int, LevelProgressData>(levelProgress.Count);
                for (int i = 0; i < levelProgress.Count; i++)
                {
                    LevelProgressData d = levelProgress[i];
                    if (d != null && !lookup.ContainsKey(d.levelIndex))
                        lookup.Add(d.levelIndex, d);
                }
                lookupCount = levelProgress.Count;
            }

            return lookup.TryGetValue(levelIndex, out LevelProgressData data) ? data : null;
        }

        /// <summary>Clears all level progress (used once when the world moved to 30 levels per country).</summary>
        public void ResetLevelProgress()
        {
            levelProgress.Clear();
            lookup = null;
            lookupCount = -1;
            RealLevelNumber = 0;
            DisplayLevelNumber = 0;
            selectedLevelIndex = 0;
            ReplayingLevelAgain = false;
            isPlayingFromLevelSelection = false;
        }

        public int GetTotalStars()
        {
            int total = 0;

            if (levelProgress == null)
                return total;

            for (int i = 0; i < levelProgress.Count; i++)
            {
                LevelProgressData data = levelProgress[i];
                if (data == null)
                    continue;

                total += System.Math.Max(0, data.starsEarned);
            }

            return total;
        }

        public void SetLevelProgress(int levelIndex, bool isCompleted, int starsEarned)
        {
            LevelProgressData data = GetLevelProgress(levelIndex);

            if (data == null)
            {
                data = new LevelProgressData();
                data.levelIndex = levelIndex;
                data.isCompleted = isCompleted;
                data.starsEarned = starsEarned;
                levelProgress.Add(data);
            }
            else
            {
                data.isCompleted = isCompleted;
                data.starsEarned = starsEarned;
            }
        }
    }

    [System.Serializable]
    public class LevelProgressData
    {
        public int levelIndex;
        public bool isCompleted = false;
        public int starsEarned = 0;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// Everything that makes up a player's progress, as one piece of text for the cloud: the
    /// save file (levels, stars, coins, gems, trays, power-ups...) plus the progress kept in
    /// PlayerPrefs (chef XP and outfit, stats, collection, achievements, plate and lid
    /// styles, tutorial done). Screen-only prefs (last selected country, daily counters) are
    /// left out.
    /// </summary>
    [Serializable]
    public sealed class CloudSnapshot
    {
        private const string SaveFile = "save";               // SaveController's file name
        private const string ExportFile = "cloud_snapshot";   // a copy written just for the upload

        public int version = 1;
        public int levels;
        public int stars;
        public int xp;
        public long savedAt;
        public string device;
        public string save;       // the save file, base64
        public List<Pref> prefs = new List<Pref>();

        [Serializable]
        public sealed class Pref
        {
            public string k;
            public int i;
            public string s;
        }

        // ------------------------------------------------------------------ keys

        private static readonly string[] IntKeys =
        {
            "CC_Chef_XP", "CC_Stats_DishesServed", "CC_Stats_PowerUpsUsed", "CC_Stats_LevelsWon",
            "CC_Style_Plate", "CC_Style_Lid", "CC_Diamonds", "CC_DiamondCurrencyMigrated",
            "CC_Progress_LegacyMigrated", "CC_LocalBestScore", "CC_WorldMap_DragHintSeen",
            "CC_WorldMap_ContinentOrderVersion",
            "CC_Tutorial_Level1", "CC_Tutorial_Skipped", "CC_Tutorial_Hidden", "CC_Tutorial_Spawner",
            "CC_Tutorial_PowerUp_" + nameof(PUUndoSettings), "CC_Tutorial_PowerUp_" + nameof(PUHintSettings),
            "CC_Tutorial_PowerUp_" + nameof(PUShuffleSettings),
        };

        private static readonly string[] StringKeys = { "CC_Chef_Outfit", "CC_PlayerName" };

        private static IEnumerable<string> ProgressIntKeys()
        {
            foreach (string key in IntKeys)
                yield return key;
            for (int dish = 0; dish < PlayerStats.TotalDishes; dish++)
                yield return "CC_Stats_Dish_" + dish;
            for (int country = 0; country < CountryCatalog.CountryCount; country++)
            {
                yield return "CC_CountryComplete_Shown_" + country;
                yield return "CC_Flag_country_master_" + country;
            }
            foreach (string id in AchievementsPanel.Ids)
                yield return "CC_Flag_achievement_" + id;
            for (int style = 1; style < CountryFoodStyle.PlateNames.Length; style++)
                yield return "CC_Style_Plate_Owned_" + style;
            for (int style = 1; style < CountryFoodStyle.LidNames.Length; style++)
                yield return "CC_Style_Lid_Owned_" + style;
        }

        // ------------------------------------------------------------------ capture / restore

        /// <summary>This phone's progress right now (null if the save is not loaded yet).</summary>
        public static CloudSnapshot Capture()
        {
            if (!SaveController.IsSaveLoaded)
                return null;
            try
            {
                SaveController.PresetsSave(ExportFile);
                string path = Serializer.persistentDataPath + ExportFile;
                byte[] bytes = File.ReadAllBytes(path);

                var snapshot = new CloudSnapshot
                {
                    save = Convert.ToBase64String(bytes),
                    device = SystemInfo.deviceUniqueIdentifier,
                    xp = ChefProgress.TotalXP,
                };
                PlayerStats.GetProgress(out snapshot.levels, out snapshot.stars, out _);

                foreach (string key in ProgressIntKeys())
                    if (PlayerPrefs.HasKey(key))
                        snapshot.prefs.Add(new Pref { k = key, i = PlayerPrefs.GetInt(key) });
                foreach (string key in StringKeys)
                    if (PlayerPrefs.HasKey(key))
                        snapshot.prefs.Add(new Pref { k = key, s = PlayerPrefs.GetString(key) });
                return snapshot;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cloud] Could not read the local progress: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Writes this progress over the phone's own (the caller restarts the game next).
        /// Saving is locked first, so the old progress in memory can't overwrite it.
        /// </summary>
        public void Restore()
        {
            SaveController.Lock();
            File.WriteAllBytes(Serializer.persistentDataPath + SaveFile, Convert.FromBase64String(save));
            foreach (Pref pref in prefs)
            {
                if (string.IsNullOrEmpty(pref.k))
                    continue;
                if (pref.s != null && Array.IndexOf(StringKeys, pref.k) >= 0)
                    PlayerPrefs.SetString(pref.k, pref.s);
                else
                    PlayerPrefs.SetInt(pref.k, pref.i);
            }
            PlayerPrefs.Save();
        }

        // ------------------------------------------------------------------ text

        /// <summary>The same progress, ignoring when and where it was saved.</summary>
        public bool SameProgress(CloudSnapshot other) =>
            other != null && other.save == save && JsonUtility.ToJson(new PrefList(prefs)) == JsonUtility.ToJson(new PrefList(other.prefs));

        /// <summary>Further along: more levels, then more stars, then more chef XP.</summary>
        public bool IsAheadOf(CloudSnapshot other)
        {
            if (other == null)
                return true;
            if (levels != other.levels)
                return levels > other.levels;
            if (stars != other.stars)
                return stars > other.stars;
            return xp > other.xp;
        }

        public string ToJson()
        {
            savedAt = DateTime.UtcNow.Ticks;
            return JsonUtility.ToJson(this);
        }

        public static CloudSnapshot FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;
            try
            {
                CloudSnapshot snapshot = JsonUtility.FromJson<CloudSnapshot>(json);
                return snapshot != null && !string.IsNullOrEmpty(snapshot.save) ? snapshot : null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cloud] Unreadable cloud save: " + ex.Message);
                return null;
            }
        }

        [Serializable]
        private sealed class PrefList
        {
            public List<Pref> items;
            public PrefList(List<Pref> items) { this.items = items; }
        }
    }
}

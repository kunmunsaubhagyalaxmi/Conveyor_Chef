using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Finds the art for a continent or country by name, so new art only needs to be dropped
    /// into the right folder with the right file name:
    ///
    ///   Resources/World/&lt;continent id&gt;/&lt;key&gt;.png                 (continent art)
    ///   Resources/World/&lt;continent id&gt;/&lt;country id&gt;/&lt;key&gt;.png    (country art)
    ///
    /// e.g. Resources/World/europe/italy/levelselect_hero.png. Until a file exists, the
    /// existing Asian art stands in (WorldArtLibrary): a country uses its own entry, then the
    /// Asian country in the same position (Italy uses China's art, France Japan's, ...).
    /// See Images/World/ASSET_LIST.md for every key.
    /// </summary>
    public static class WorldArt
    {
        public const string Root = "World/";

        // Continent art.
        public const string ContinentMap = "continent_map";
        public const string ProgressGlobe = "progress_globe";
        /// <summary>World Map header: logo + "CHAPTER n • NAME" + "UNLOCKED • 15 LEVELS" in one picture.</summary>
        public const string ChapterBanner = "chapter_banner";
        /// <summary>The same banner without the "UNLOCKED" line, for continents still locked.</summary>
        public const string ChapterBannerLocked = "chapter_banner_locked";

        // Country art.
        public const string FlagBadge = "flag_badge";
        public const string FlagRound = "flag_round";
        public const string MapDiorama = "map_diorama";
        public const string Landmark = "landmark";
        public const string Decor = "decor";
        public const string LevelSelectHero = "levelselect_hero";
        public const string LevelSelectIcon = "levelselect_icon";

        public static string LevelThumbnail(int slot) => "level_" + (slot + 1);
        public static string Dish(int colour) => "dish_" + WorldCatalog.DishColours[Mathf.Clamp(colour, 0, WorldCatalog.DishColours.Length - 1)];

        public static readonly string[] ContinentKeys = { ContinentMap, ProgressGlobe, ChapterBanner, ChapterBannerLocked };

        public static readonly string[] CountryKeys =
        {
            FlagBadge, FlagRound, MapDiorama, Landmark, Decor, LevelSelectHero, LevelSelectIcon,
            "level_1", "level_2", "level_3",
            "dish_red", "dish_green", "dish_pink", "dish_blue", "dish_yellow", "dish_teal", "dish_purple",
        };

        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        private static WorldArtLibrary library;
        private static bool libraryLoaded;

        private static WorldArtLibrary Library
        {
            get
            {
                if (!libraryLoaded)
                {
                    library = Resources.Load<WorldArtLibrary>(WorldArtLibrary.ResourcePath);
                    libraryLoaded = true;
                }
                return library;
            }
        }

        public static string ContinentPath(int continent, string key)
        {
            WorldCatalog.Continent c = WorldCatalog.GetContinent(continent);
            return c != null ? Root + c.Id + "/" + key : null;
        }

        public static string CountryPath(int country, string key)
        {
            WorldCatalog.Country c = WorldCatalog.GetCountry(country);
            return c != null ? Root + WorldCatalog.Continents[c.Continent].Id + "/" + c.Id + "/" + key : null;
        }

        /// <summary>Country art: its own file, then its library entry, then the Asian stand-in.</summary>
        public static Sprite ForCountry(int country, string key, bool allowStandIn = true)
        {
            Sprite own = Find(CountryPath(country, key));
            if (own != null)
                return own;

            if (!allowStandIn)
                return null;

            int standIn = WorldCatalog.StandInCountry(country);
            return standIn != country ? Find(CountryPath(standIn, key)) : null;
        }

        /// <summary>Continent art: its own file, then its library entry, then Asia's.</summary>
        public static Sprite ForContinent(int continent, string key, bool allowStandIn = true)
        {
            Sprite own = Find(ContinentPath(continent, key));
            if (own != null)
                return own;

            return allowStandIn && continent != 0 ? Find(ContinentPath(0, key)) : null;
        }

        private static string[] cardTitles;

        /// <summary>
        /// Title of a level's Level Selection card (its own place, e.g. "Great Wall Snacks"), from
        /// Resources/World/card_titles.txt (line n = game level n, written by
        /// Tools/LevelPipeline/card_art_list.py). Null when the file has no title for it.
        /// </summary>
        public static string CardTitle(int levelIndex)
        {
            if (cardTitles == null)
            {
                TextAsset file = Resources.Load<TextAsset>(Root + "card_titles");
                cardTitles = file != null ? file.text.Replace("\r", string.Empty).Split('\n') : new string[0];
            }

            if (levelIndex < 0 || levelIndex >= cardTitles.Length)
                return null;
            string title = cardTitles[levelIndex].Trim();
            return title.Length > 0 ? title : null;
        }

        /// <summary>Art shared by every continent: Resources/World/&lt;key&gt;.png.</summary>
        public static Sprite Shared(string key) => Find(Root + key);

        /// <summary>"LOCKED" status picture shown under a locked continent's chapter banner.</summary>
        public const string LockedStatus = "locked_status";

        /// <summary>True when the country has its own art for this key (not a stand-in).</summary>
        public static bool HasOwn(int country, string key) => Find(CountryPath(country, key)) != null;

        private static Sprite Find(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (cache.TryGetValue(path, out Sprite cached))
                return cached;

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null && Library != null)
                sprite = Library.Get(path);

            cache[path] = sprite;
            return sprite;
        }
    }
}

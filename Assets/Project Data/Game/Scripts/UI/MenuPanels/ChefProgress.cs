using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Chef level and outfits.
    /// - Every won level gives XP (<see cref="AwardWin"/>, called from GameController.WinGame).
    ///   Each chef level needs a bit more XP than the last; reaching a level pays coins.
    /// - Outfits unlock at chef levels and are picked in Customize > CHEF. The chosen outfit's
    ///   pictures replace the chef in the menus (<see cref="Avatar"/>, <see cref="FullBody"/>).
    ///   Pictures: Resources/Chef/&lt;id&gt;_avatar.png (square face) and &lt;id&gt;_full.png
    ///   (standing chef). The Classic outfit uses the original chef art. An outfit whose
    ///   pictures are not there yet shows as "coming soon" and cannot be picked.
    /// Saved in PlayerPrefs.
    /// </summary>
    public static class ChefProgress
    {
        private const string XPKey = "CC_Chef_XP";
        private const string OutfitKey = "CC_Chef_Outfit";
        public const string ArtFolder = "Chef/";

        public sealed class Outfit
        {
            public readonly string Id, Name, Description;
            public readonly int UnlockLevel;

            public Outfit(string id, string name, int unlockLevel, string description)
            {
                Id = id;
                Name = name;
                UnlockLevel = unlockLevel;
                Description = description;
            }
        }

        public static readonly Outfit[] Outfits =
        {
            new Outfit("classic", "Classic Chef", 1, "The trusty white jacket"),
            new Outfit("street_food", "Street Food Chef", 3, "Bandana and a busy apron"),
            new Outfit("pastry", "Pastry Chef", 6, "Pink apron dusted with flour"),
            new Outfit("sushi", "Sushi Master", 10, "Hachimaki headband and a sharp knife"),
            new Outfit("pizza", "Pizza Maestro", 14, "Red scarf and a pizza peel"),
            new Outfit("bbq", "BBQ Pitmaster", 18, "Leather apron and big tongs"),
            new Outfit("royal", "Royal Chef", 24, "Gold-trimmed jacket fit for a palace"),
            new Outfit("golden", "Golden Chef", 30, "A legend in a shining gold jacket"),
        };

        // ------------------------------------------------------------ XP and level
        public const int XPPerWin = 30;
        public const int XPPerStar = 10;

        public static int TotalXP => PlayerPrefs.GetInt(XPKey, 0);

        /// <summary>XP needed to go from <paramref name="level"/> to the next one.</summary>
        public static int XPToNext(int level) => 100 + 50 * (Mathf.Max(1, level) - 1);

        /// <summary>Coins paid when the chef reaches <paramref name="level"/>.</summary>
        public static int LevelUpCoins(int level) => 50 + 25 * level;

        public static int Level => LevelAt(TotalXP, out _, out _);

        /// <summary>Chef level for a total XP, with the XP into that level and the XP it needs.</summary>
        public static int LevelAt(int xp, out int into, out int need)
        {
            int level = 1;
            int left = Mathf.Max(0, xp);
            while (left >= XPToNext(level))
            {
                left -= XPToNext(level);
                level++;
            }
            into = left;
            need = XPToNext(level);
            return level;
        }

        public struct Gain
        {
            public int XP, Before, After, OldLevel, NewLevel, Coins;
            public bool LevelUp => NewLevel > OldLevel;
        }

        /// <summary>The XP from the last won level (the win screen shows it).</summary>
        public static Gain LastGain { get; private set; }

        public static Gain AwardWin(int stars)
        {
            var gain = new Gain { XP = XPPerWin + XPPerStar * Mathf.Clamp(stars, 0, 3), Before = TotalXP };
            gain.After = gain.Before + gain.XP;
            gain.OldLevel = LevelAt(gain.Before, out _, out _);
            gain.NewLevel = LevelAt(gain.After, out _, out _);
            PlayerPrefs.SetInt(XPKey, gain.After);

            for (int level = gain.OldLevel + 1; level <= gain.NewLevel; level++)
                gain.Coins += LevelUpCoins(level);
            if (gain.Coins > 0)
            {
                try { CurrenciesController.Add(CurrencyType.Coins, gain.Coins); }
                catch (System.Exception ex) { Debug.LogWarning("[Chef] Could not add level-up coins: " + ex.Message); }
            }
            PlayerPrefs.Save();
            LastGain = gain;
            return gain;
        }

        // ------------------------------------------------------------ outfits
        public static bool IsUnlocked(Outfit outfit) => outfit != null && Level >= outfit.UnlockLevel;

        public static bool HasArt(Outfit outfit) => outfit != null && (outfit.Id == "classic" || LoadAvatar(outfit) != null);

        public static Outfit Selected
        {
            get
            {
                string id = PlayerPrefs.GetString(OutfitKey, "classic");
                foreach (Outfit o in Outfits)
                    if (o.Id == id && IsUnlocked(o) && HasArt(o))
                        return o;
                return Outfits[0];
            }
        }

        /// <summary>Raised after the player picks another outfit (menu chef pictures follow it).</summary>
        public static event System.Action OutfitChanged;

        public static void Select(Outfit outfit)
        {
            if (outfit == null || !IsUnlocked(outfit) || !HasArt(outfit))
                return;
            PlayerPrefs.SetString(OutfitKey, outfit.Id);
            PlayerPrefs.Save();
            OutfitChanged?.Invoke();
        }

        /// <summary>The first outfit still locked (for "next outfit at level N").</summary>
        public static Outfit NextLocked
        {
            get
            {
                foreach (Outfit o in Outfits)
                    if (!IsUnlocked(o))
                        return o;
                return null;
            }
        }

        public static Sprite LoadAvatar(Outfit outfit) => outfit == null ? null : Resources.Load<Sprite>(ArtFolder + outfit.Id + "_avatar");
        public static Sprite LoadFull(Outfit outfit) => outfit == null ? null : Resources.Load<Sprite>(ArtFolder + outfit.Id + "_full");
        /// <summary>The avatar inside the classic gold square, for the main menu HUD (made by Tools/make_chef_hud_avatars.py).</summary>
        public static Sprite LoadHud(Outfit outfit) => outfit == null ? null : Resources.Load<Sprite>(ArtFolder + outfit.Id + "_hud");

        /// <summary>Face of the chosen outfit (the original chef when it has none).</summary>
        public static Sprite Avatar(MenuPanelArt art)
        {
            Outfit o = Selected;
            Sprite s = o.Id == "classic" ? null : LoadAvatar(o);
            return s != null ? s : (art != null ? art.chefAvatar : null);
        }

        /// <summary>Whole chef in the chosen outfit (the original chef when it has none).</summary>
        public static Sprite FullBody(MenuPanelArt art)
        {
            Outfit o = Selected;
            Sprite s = o.Id == "classic" ? null : LoadFull(o);
            if (s == null && o.Id != "classic")
                s = LoadAvatar(o);
            return s != null ? s : (art != null ? art.chefFull : null);
        }

        public static Sprite AvatarOf(Outfit outfit, MenuPanelArt art)
        {
            if (outfit == null || outfit.Id == "classic")
                return art != null ? art.chefAvatar : null;
            return LoadAvatar(outfit);
        }
    }
}
